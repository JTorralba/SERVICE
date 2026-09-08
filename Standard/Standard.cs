using System;
using System.Collections.Concurrent;
using System.IO;
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
    }

    public class DualWriter : TextWriter
    {
        private readonly TextWriter _consoleWriter;
        private readonly StreamWriter _fileWriter;

        public DualWriter(string logFilePath)
        {
            // Keep original console output.
            _consoleWriter = Console.Out;

            var fileStream = new FileStream(
                logFilePath,
                FileMode.Append,
                FileAccess.Write,
                FileShare.ReadWrite
            );

            _fileWriter = new StreamWriter(fileStream, Encoding.UTF8)
            {
                AutoFlush = true // Ensure immediate write(s).
            };
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
                _fileWriter?.Dispose();
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

            _status($"started");

            while (!_cts.IsCancellationRequested)
            {
                data = $"{DateTime.Now:ss.fff}";

                data = $"{i.ToString("D6")}-{data}";

                _item($"{data}");

                Thread.Sleep(250 * 1 * 1);

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

            _status($"stopped");
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
            _status($"started");

            while (!_cts.IsCancellationRequested)
            {
                _signal.WaitOne();

                while (_queue.TryDequeue(out var item))
                {
                    try
                    {
                        _item(item);
                    }
                    catch (Exception x)
                    {
                        _status($"{x.Message}");
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

            _status($"stopped");
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
