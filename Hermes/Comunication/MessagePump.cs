using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hermes.Comunication
{
    public delegate void InternalMessageDelegate(object sender, MessageEventArgs args);
  
    internal class MessagePump : IDisposable
    {
        private static readonly Lazy<MessagePump> lazy = new Lazy<MessagePump>(() => new MessagePump());

        private object lockQueue;
        private object lockSubcribers;

        private Queue<InternalMessage> queueMessage;

        private Dictionary<Guid, List<InternalMessageDelegate>> subscribers;

        public static MessagePump Instance { get { return lazy.Value; } }

        private List<InternalTopic> Topics { get; set; }

        private CancellationTokenSource ts;

        private MessagePump()
        {
            this.subscribers = new Dictionary<Guid, List<InternalMessageDelegate>>();
            this.lockQueue = new object();
            this.lockSubcribers = new object();
            this.queueMessage = new Queue<InternalMessage>();

            this.Topics = new List<InternalTopic>();

            var ts = new CancellationTokenSource();
            CancellationToken ct = ts.Token;

            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (ct.IsCancellationRequested)
                    {
                        Log.LogEngine.Instance.Engine.Debug("Task canceled");
                        break;
                    }
                    InternalMessage m = null;
                    lock (this.lockQueue)
                    {
                        if (this.queueMessage.Count > 0)
                        {
                            //     LogEngine.Instance.Engine.Debug("Queue Dequeue");
                            m = this.queueMessage.Dequeue();
                        }
                    }
                    if (m != null)
                    {
                        // LogEngine.Instance.Engine.Debug("Queue Messaggio trovato");
                        lock (this.lockSubcribers)
                        {
                            InternalTopic t = new InternalTopic(m.TopicGuid, m.TopicName);

                            List<InternalMessageDelegate> list = this.getListDelegate(t);
                            if (list != null && list.Count > 0)
                            {
                                //                      LogEngine.Instance.Engine.Debug("Queue Messaggio in trasmissione");
                                list.ForEach(c => c(m.Sender, m.Argument));
                            }
                        }
                    }

                    Thread.Sleep(20);
                }
            }, ct);

        }

        public Guid GetTopic(string topicName)
        {
            return this.getTopicByName(topicName).Guid;
        }

        private InternalTopic getTopicByName(string topicName)
        {
            InternalTopic t = null;
            if (this.Topics == null)
                this.Topics = new List<InternalTopic>();
            if (this.Topics.Count > 0)
            {
                t = this.Topics.Find(to => to.Name == topicName.ToUpper());
            }
            if (t == null)
            {
                t = new InternalTopic(topicName.ToUpper());
                this.Topics.Add(t);
            }
            return t;
        }

        public void Subscribe(Guid topic, InternalMessageDelegate callBack)
        {
            lock (this.lockSubcribers)
            {

                InternalTopic internalTopic = this.getTopicByGuid(topic);

                if (internalTopic != null)
                {
                    InternalMessageDelegate dele = this.getCallBack(internalTopic, callBack);
                    if (dele == null)
                    {
                        List<InternalMessageDelegate> list = this.getListDelegate(internalTopic);
                        if (list != null && list.Count > 0)
                        {
                            list.Add(callBack);
                        }
                        else
                        {
                            list = new List<InternalMessageDelegate>();
                            list.Add(callBack);
                            this.subscribers.Add(internalTopic.Guid, list);
                        }

                    }
                }
            }
        }

        public void UnSubscribe(Guid topic, InternalMessageDelegate callBack)
        {
            lock (this.lockSubcribers)
            {
                InternalTopic internalTopic = this.getTopicByGuid(topic);

                if (internalTopic != null)
                {
                    InternalMessageDelegate dele = this.getCallBack(internalTopic, callBack);
                    if (dele != null)
                    {
                        this.getListDelegate(internalTopic).Remove(dele);
                    }
                }

            }
        }

        public void Publish(Guid topic, object sender, MessageEventArgs args)
        {
            lock (this.lockQueue)
            {

                InternalTopic internalTopic = this.getTopicByGuid(topic);

                if (internalTopic != null)
                {
                    // LogEngine.Instance.Engine.Debug("Publish topic " + internalTopic.Name);
                    this.queueMessage.Enqueue(new InternalMessage(internalTopic.Name, internalTopic.Guid, sender, args));
                }

            }
        }

        private List<InternalMessageDelegate> getListDelegate(InternalTopic topic)
        {
            if (this.subscribers != null && this.subscribers.Count > 0)
            {
                if (this.subscribers.ContainsKey(topic.Guid))
                {
                    return this.subscribers[topic.Guid];
                }
                else return null;
            }
            else
                return null;
        }

        private InternalMessageDelegate getCallBack(InternalTopic topic, InternalMessageDelegate callBack)
        {
            if (this.subscribers != null && this.subscribers.Count > 0)
            {
                if (this.subscribers.ContainsKey(topic.Guid) && this.subscribers[topic.Guid].Contains(callBack))
                {
                    return this.subscribers[topic.Guid].Find(i => i.Equals(callBack));
                }
                else return null;
            }
            else
                return null;
        }

        public void Dispose()
        {
            this.ts.Cancel();
            this.subscribers.Clear();
            this.subscribers = null;
        }

        private InternalTopic getTopicByGuid(Guid topicGuid)
        {
            InternalTopic t = null;
            if (this.Topics == null)
                this.Topics = new List<InternalTopic>();
            if (this.Topics.Count > 0)
            {
                t = this.Topics.Find(to => to.Guid.ToString() == topicGuid.ToString());
            }

            return t;
        }

    }

    public class InternalMessage
    {
        public string TopicName { get; set; }
        public Guid TopicGuid { get; set; }
        public object Sender { get; set; }
        public MessageEventArgs Argument { get; set; }

        public InternalMessage(string topicName, Guid topicGuid, object sender, MessageEventArgs arg)
        {
            this.Argument = arg;
            this.Sender = sender;
            this.TopicName = topicName;
            this.TopicGuid = topicGuid;
        }
    }

    internal class InternalTopic
    {
        public Guid Guid { get; private set; }
        public string Name { get; set; }
        public InternalTopic(string name)
        {
            this.Guid = new Guid();
            this.Guid = Guid.NewGuid();
            this.Name = name;
        }
        public InternalTopic(Guid guid, string name) : this(name)
        {
            this.Guid = guid;
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public Message Message { get; set; }
    }
}
