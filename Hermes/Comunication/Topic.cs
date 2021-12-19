using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hermes.Comunication
{
    internal class Topic : IDisposable
    {
        
        private CancellationTokenSource tokenSource;
        private CancellationToken cancelToken;

        private object lockQueue;
        public Guid Guid { get; private set; }
        public string Name { get; set; }
        public Topic(string name)
        {
            this.Guid = new Guid();
            this.Guid = Guid.NewGuid();
            this.Name = name;
            this.lockQueue = new object();

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.cancelToken.ThrowIfCancellationRequested();

                while (true)
                {
                    if (this.cancelToken.IsCancellationRequested)
                    {
                        Log.LogEngine.Instance.Engine.Debug("IsCancellationRequested ");
                        return;
                    }
                    if (this.subscriber != null && this.subscriber.Count > 0)
                        while (this.queue.Count > 0)
                        {
                            Message msg = null;
                            lock (this.lockQueue)
                            {
                                msg = this.queue.Dequeue();
                            }
                            foreach (var item in this.subscriber)
                            {
                                Channel.Tcp.Client clientTCP = new Channel.Tcp.Client(((IPEndPoint)item.RemoteAddress).Address.ToString(), ((IPEndPoint)item.RemoteAddress).Port);
                                clientTCP.Send(msg.ToString());
                                clientTCP = null;
                            }
                        }
                }


            });

        }
        public Topic(Guid guid, string name) : this(name)
        {
            this.Guid = guid;
        }

        private List<Connect.Connect> subscriber;

        public List<Connect.Connect> Subscriber
        {
            get => subscriber;
            set => subscriber = value;
        }

        private Queue<Message> queue;
        public void MessageEqueue(Message msg)
        {
            lock (this.lockQueue)
            {
                this.queue.Enqueue(msg);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
