using System;

using Standard;

namespace MULTIMODE
{
    public class Service
    {
        private FIFO<Data<object>> dataQueue;

        private StringGenerator dataProducer;

        public void Initialize()
        {
            dataQueue = new FIFO<Data<object>>(DataQueueItem, DataQueueStatus);

            dataProducer = new StringGenerator(DataProducerItem, DataProducerStatus);
        }

        public void DeInitialize()
        {
            if (dataProducer != null)
                dataProducer.Dispose();

            if (dataQueue != null)
                dataQueue.Dispose();
        }

        private void DataProducerItem(string data)
        {
            Data<object> item;

            item = new Data<object>($"{data}");

            dataQueue.Enqueue(item);
        }

        private void DataProducerStatus(string status)
        {
            Console.WriteLine($"{TimeStamp.Get()} data producer {status}");
        }

        private void DataQueueItem(Data<object> data)
        {
            Console.WriteLine($"{TimeStamp.Get(data.TimeStamp)} {data.Value}");
        }

        private void DataQueueStatus(string status)
        {
            Console.WriteLine($"{TimeStamp.Get()} data queue {status}");
        }
    }
}
