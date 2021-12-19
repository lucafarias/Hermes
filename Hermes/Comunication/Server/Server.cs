using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Comunication.Server
{
    public class Server
    {
       
        private Channel.Tcp.Server serverTCP;

        private List<Topic> topics;

        public int Port { get; private set; }

        public Server(int port)
        {
            this.Port = port;
            this.serverTCP = new Channel.Tcp.Server("127.0.0.1", this.Port);
            this.serverTCP.DataReceived = (Action<ComunicationTCPEvent>)delegate (ComunicationTCPEvent dataReceive)
            {
                Log.LogEngine.Instance.Engine.Debug("RerceivedData =", dataReceive.RerceivedData);
                Message received = Utility.Functions.GetMessage(dataReceive.RerceivedData);
               
                dataReceive.ReturnString = "ERROR";
                if (received != null)
                {
                    Log.LogEngine.Instance.Engine.Debug("received =", received.ToString());
                    if (received.Text.ToUpper().StartsWith(Command.GetCommand(Command.enumTCPCommand.AUTH0)))
                    {
                        if (!Connect.Manager.Instance.IsConnect(received))
                        {

                            Connect.Connect con = new Hermes.Connect.Connect();
                            con.Token = Security.Token.GetToken();
                            con.Guid = received.ClientId;
                            con.RemoteAddress = dataReceive.RemoteAddress;
                            Hermes.Connect.Manager.Instance.Add(con);
                            received.Token = Hermes.Security.Token.GetToken();
                            dataReceive.ReturnString = Newtonsoft.Json.JsonConvert.SerializeObject(received);
                        }
                    }
                    else if (received.Text.ToUpper().StartsWith(Command.GetCommand(Command.enumTCPCommand.AUTH1)) && Connect.Manager.Instance.IsConnect(received))
                    {
                        string[] part = received.Text.Split(':');
                        if(!string.IsNullOrEmpty(part[1]) && Security.Token.GetVerifyToken(received) == part[1])
                        {
                            dataReceive.ReturnString = part[1];
                            Connect.Connect con = Connect.Manager.Instance.GetConnection(received);
                            con.Token = part[1];
                            con.State = Connect.Connect.enumState.CONNECT;
                        }
                    }
                    else if (received.Text.ToUpper().StartsWith(Command.GetCommand(Command.enumTCPCommand.GETTOPIC)))
                    {
                        string[] part = received.Text.Split(':');
                        if (!Connect.Manager.Instance.IsGrant(received))
                        {
                            Connect.Manager.Instance.Remove(received);
                        }
                        if (!string.IsNullOrEmpty(part[1]))
                        {
                            received.Bag = this.GetTopic(part[1]);
                            dataReceive.ReturnString = Newtonsoft.Json.JsonConvert.SerializeObject(received);
                        }
                        
                    }
                    else if (received.Text.ToUpper().StartsWith(Command.GetCommand(Command.enumTCPCommand.SUBSCRIBE)))
                    {
                        if (!Connect.Manager.Instance.IsGrant(received))
                        {
                            Connect.Manager.Instance.Remove(received);
                        }
                        Connect.Connect con = Connect.Manager.Instance.GetConnection(received);
                        Topic top = this.getTopicByGuid(new Guid( received.TopicId));
                        top.Subscriber.Add(con);
                        received.TopicId = top.Guid.ToString();
                        dataReceive.ReturnString = Newtonsoft.Json.JsonConvert.SerializeObject(received);
                    }
                    else if (received.Text.ToUpper().StartsWith(Command.GetCommand(Command.enumTCPCommand.UNSUBSCRIBE)))
                    {
                        if (!Connect.Manager.Instance.IsGrant(received))
                        {
                            Connect.Manager.Instance.Remove(received);
                        }
                        if(!string.IsNullOrEmpty(received.TopicId)) 
                        {
                            Connect.Connect con = Connect.Manager.Instance.GetConnection(received);
                            Topic top = this.getTopicByGuid(new Guid( received.TopicId));
                            top.Subscriber.Remove(con);
                            received.TopicId = "";
                            dataReceive.ReturnString = "DONE";
                        }                        
                    }
                    else if (!string.IsNullOrEmpty(received.TopicId))
                    {
                        if (!Connect.Manager.Instance.IsGrant(received))
                        {
                            Connect.Manager.Instance.Remove(received);
                        }
                        Topic top = this.getTopicByGuid(new Guid(received.TopicId));
                        top.MessageEqueue(received);
                        dataReceive.ReturnString = "DONE";
                    }
                    
                }
            };

            this.topics = new List<Topic>();
        }

        public void Start()
        {
            this.serverTCP.Start();
        }

        public Guid GetTopic(string topicName)
        {
            return this.getTopicByName(topicName).Guid;
        }

        private Topic getTopicByName(string topicName)
        {
            Topic t = null;
            if (this.topics == null)
                this.topics = new List<Topic>();
            if (this.topics.Count > 0)
            {
                t = this.topics.Find(to => to.Name == topicName.ToUpper());
            }
            if (t == null)
            {
                t = new Topic(topicName.ToUpper());
                this.topics.Add(t);
            }
            return t;
        }

        private Topic getTopicByGuid(Guid topicGuid)
        {
            Topic t = null;
            if (this.topics == null)
                this.topics = new List<Topic>();
            if (this.topics.Count > 0)
            {
                t = this.topics.Find(to => to.Guid.ToString() == topicGuid.ToString());
            }

            return t;
        }
    }
}
