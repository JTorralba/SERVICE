using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Standard
{
    public static class TimeStamp
    {
        public static string Get()
        {
            return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
        }

        public static string Get(DateTime dateTime)
        {
            return $"{dateTime:yyyy-MM-dd HH:mm:ss.fff}";
        }
    }

    public class FileWriter
    {
        private readonly string fileDirectory;

        private readonly string fileName;

        private readonly string file;

        private readonly long fileSize;

        private readonly int fileRetention;

        private Mutex fileMutex;

        //private FileStream _fileStream;

        private readonly ConcurrentQueue<string> _queue = new ConcurrentQueue<string>();

        private readonly AutoResetEvent _signal = new AutoResetEvent(false);

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private readonly Task _task;

        public FileWriter(string file, long fileSize, int fileRetention)
        {
            this.file = file;

            this.fileDirectory = Path.GetDirectoryName(file);

            this.fileName = Path.GetFileName(file);

            this.fileSize = fileSize;

            this.fileRetention = fileRetention;

            this.fileMutex = new Mutex(false, this.fileName);

            Prune();

            _task = Task.Run(ProcessTask);
        }

        public void Write(char data)
        {
            Write($"{data.ToString().Trim()}");
        }

        public void WriteLine(string data)
        {
            Write($"{data.Trim()}{Environment.NewLine}");
        }

        public void Write(string data)
        {
            _queue.Enqueue(data);

            _signal.Set();
        }

        private void ProcessTask()
        {
            while (!_cts.IsCancellationRequested)
            {
                _signal.WaitOne();

                while (_queue.TryDequeue(out var item))
                {
                    fileMutex.WaitOne();

                    Rotate(file);

                    byte[] utf8Bytes = Encoding.UTF8.GetBytes(item);

                    bool Wrote = false;

                    while (!Wrote)
                    {
                        try
                        {
                            //_fileStream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, bufferSize: 32768, useAsync: false);

                            //_fileStream.Lock(0, 0);
                            //_fileStream.Position = _fileStream.Length;
                            //_fileStream.Write(utf8Bytes, 0, utf8Bytes.Length);
                            //_fileStream.Flush();
                            //_fileStream.Unlock(0, 0);
                            //_fileStream.Close();
                            //_fileStream.Dispose();

                            File.AppendAllText(file, item);

                            Wrote = true;
                        }
                        catch (Exception e)
                        {
                            Thread.Sleep(250);
                        }
                    }

                    fileMutex.ReleaseMutex();
                }
            }
        }

        private void Rotate(string file)
        {
            try
            {
                if (File.Exists(file) && new FileInfo(file).Length >= (fileSize))
                {
                    string archiveFile = Path.Combine(fileDirectory, $"{Path.GetFileNameWithoutExtension(fileName)}_{File.GetCreationTime(file):yyyyMMdd_HHmmss}.log");

                    bool Moved = false;

                    while (!Moved)
                    {
                        try
                        {
                            File.Move(file, archiveFile);

                            Moved = true;
                        }
                        catch (Exception e)
                        {
                            Thread.Sleep(250);
                        }                      
                    }

                    Compress(archiveFile);

                    Prune();
                }
            }
            catch (Exception e)
            {
                Thread.Sleep(250);
            }
        }

        private void Compress(string file)
        {
            string zipFile = file + ".zip";

            bool Compressed = false;

            while (!Compressed)
            {
                try
                {
                    using (FileStream fileStream = new(zipFile, FileMode.Create))

                    using (ZipArchive zipArchive = new(fileStream, ZipArchiveMode.Create))
                    {
                        zipArchive.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                    }

                    File.SetCreationTime(zipFile, File.GetCreationTime(file));

                    while (File.Exists(file))
                    {
                        File.Delete(file);

                        Thread.Sleep(250);
                    }

                    Compressed = true;
                }
                catch (Exception e)
                {
                    Thread.Sleep(250);
                }
            }           
        }

        private void Prune()
        {
            bool Pruned = false;

            while (!Pruned)
            {
                try
                {
                    foreach (var zipFile in Directory.GetFiles(fileDirectory, "*.zip"))
                    {
                        if (File.GetCreationTime(zipFile) < DateTime.Now.AddMinutes(-fileRetention))
                        {
                            while (File.Exists(zipFile))
                            {
                                File.Delete(zipFile);

                                Thread.Sleep(250);
                            }
                        }
                    }

                    Pruned = true;
                }
                catch (Exception e)
                {
                    Thread.Sleep(250);
                }
            }           
        }

        public void Dispose()
        {
            //_fileStream.Flush();
            //_fileStream.Close();
            //_fileStream.Dispose();

            _cts.Cancel();
            _signal.Set();
            _task.Wait();
            _signal.Dispose();
            _cts.Dispose();
        }
    }

    public class DualWriter : TextWriter
    {
        private readonly TextWriter _consoleWriter;

        private readonly FileWriter _fileWriter;

        public DualWriter(string file)
        {
            // Keep original console output.
            _consoleWriter = Console.Out;

            int megaBytes = 0;
            int kiloBytes = 16;
            int bytes = 0;

            int days = 365;
            int hours = 0;
            int minutes = 0;

            _fileWriter = new FileWriter(
                file: file,
                fileSize: (bytes) + (1024 * kiloBytes) + (1024 * 1024 * megaBytes),
                fileRetention: (minutes) + (60 * hours) + (60 * 24 * days));
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            _fileWriter.Write(value);
            _consoleWriter.Write(value);
        }

        public override void Write(string value)
        {
            _fileWriter.Write(value);
            _consoleWriter.Write(value);
        }

        public override void WriteLine(string value)
        {
            _fileWriter.WriteLine(value);
            _consoleWriter.WriteLine(value);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileWriter.Dispose();
                _consoleWriter?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class StringGenerator : IDisposable
    {
        private readonly AutoResetEvent _signal = new AutoResetEvent(false);

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public delegate void ProcessItem(string item);

        public delegate void ProcessStatus(string status);

        private readonly StringGenerator.ProcessItem _item;

        private readonly StringGenerator.ProcessStatus _status;

        private readonly Task _task;

        public StringGenerator(StringGenerator.ProcessItem itemHandlerMethodName, StringGenerator.ProcessStatus statusHandlerMethodName)
        {
            _item = itemHandlerMethodName ?? throw new ArgumentNullException(nameof(itemHandlerMethodName));

            _status = statusHandlerMethodName ?? throw new ArgumentNullException(nameof(statusHandlerMethodName));

            _task = Task.Run(ProcessTask);
        }

        private void ProcessTask()
        {
            string data = string.Empty;

            int i = 0;

            _status("started");

            while (!_cts.IsCancellationRequested)
            {
                data = $"{DateTime.Now:ss.fff}";

                data = $"{i.ToString("D6")}-{data}";

                _item($"{data}");

                Thread.Sleep(250);

                i++;
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _signal.Set();
            _task.Wait();
            _signal.Dispose();
            _cts.Dispose();

            _status("stopped");
        }
    }

    public class FIFO<T> : IDisposable
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();

        private readonly AutoResetEvent _signal = new AutoResetEvent(false);

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public delegate void ProcessItem(T item);

        public delegate void ProcessStatus(string status);

        private readonly FIFO<T>.ProcessItem _item;

        private readonly FIFO<T>.ProcessStatus _status;

        private readonly Task _task;

        public FIFO(FIFO<T>.ProcessItem itemHandlerMethodName, FIFO<T>.ProcessStatus statusHandlerMethodName)
        {
            _item = itemHandlerMethodName ?? throw new ArgumentNullException(nameof(itemHandlerMethodName));

            _status = statusHandlerMethodName ?? throw new ArgumentNullException(nameof(statusHandlerMethodName));

            _task = Task.Run(ProcessTask);
        }

        public void Enqueue(T item)
        {
            _queue.Enqueue(item);

            _signal.Set();
        }

        private void ProcessTask()
        {
            _status("started");

            while (!_cts.IsCancellationRequested)
            {
                _signal.WaitOne();

                while (_queue.TryDequeue(out var data))
                {
                    try
                    {
                        _item(data);
                    }
                    catch (Exception x)
                    {
                        _status(x.Message);
                    }
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _signal.Set();
            _task.Wait();
            _signal.Dispose();
            _cts.Dispose();

            _status("stopped");
        }
    }

    public class Data<T>
    {
        public DateTime TimeStamp { get; }

        public T Value { get; }

        public Data(T value)
        {
            TimeStamp = DateTime.Now;

            Value = value;
        }
    }
}
