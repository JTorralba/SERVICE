using Standard;
using System;
using System.IO;
using System.Reflection;

namespace MULTIMODE
{
    public class Service
    {
        private FIFO<Data<object>> logQueue;

        private FIFO<Data<object>> dataQueue;

        private StringGenerator dataProducer;

        public void Initialize()
        {

            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            string assemblyLogPath = $"{assemblyPath}\\log";

            string assemblyLogName = $"{assemblyLogPath}\\{assemblyName}.log";

            Directory.CreateDirectory(@assemblyLogPath);

            Console.SetOut(new DualWriter($"{assemblyLogName}"));

            logQueue = new FIFO<Data<object>>(LogQueueItem, LogQueueStatus);

            dataQueue = new FIFO<Data<object>>(DataQueueItem, DataQueueStatus);

            dataProducer = new StringGenerator(DataProducerItem, DataProducerStatus);
        }

        public void DeInitialize()
        {
            if (dataProducer != null)
                dataProducer.Dispose();

            if (dataProducer != null)
                dataQueue.Dispose();

            if (dataProducer != null)
                logQueue.Dispose();
        }

        private void DataProducerItem(string data)
        {
            Data<object> item;

            item = new Data<object>($"{data}");

            dataQueue.Enqueue(item);
        }

        private void DataProducerStatus(string status)
        {
            string timeStamp;

            timeStamp = TimeStamp.Get();

            Console.WriteLine($"{timeStamp} data producer {status}");
        }

        private void DataQueueItem(Data<object> data)
        {
            Data<object> item;

            item = new Data<object>($"{data.TimeStamp:yyyy-MM-dd HH:mm:ss.fff} {data.Value}");

            logQueue.Enqueue(item);
        }

        private void DataQueueStatus(string status)
        {
            string timeStamp;

            timeStamp = TimeStamp.Get();

            Console.WriteLine($"{timeStamp} data queue {status}");
        }

        private void LogQueueItem(Data<object> data)
        {
            Console.WriteLine($"{data.Value} {data.TimeStamp:yyyy-MM-dd HH:mm:ss.fff}");
        }

        private void LogQueueStatus(string data)
        {
            string timeStamp;

            timeStamp = TimeStamp.Get();

            Console.WriteLine($"{timeStamp} log queue {data}");
        }
    }
}
