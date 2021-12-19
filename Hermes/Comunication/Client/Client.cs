using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Comunication.Client
{
    public class Client
    {
        private Channel.Tcp.Client clientTCP;
        private Channel.Tcp.Server serverTCP;

        private Dictionary<string, string> topics;



        private string token { get; set; }

        private bool isConnect { get; set; }
        public int Port { get; set; }
        public string Remote { get; set; }

        public Client(string remote, int port)
        {
            this.Port = port;
            this.Remote = remote;
            this.isConnect = false;

            this.topics = new Dictionary<string, string>();

            this.clientTCP = new Channel.Tcp.Client(this.Remote, this.Port);
            this.serverTCP = new Channel.Tcp.Server("127.0.0.1", Utility.Functions.GetAvailablePort(10000));
            this.serverTCP.DataReceived = (Action<ComunicationTCPEvent>)delegate (ComunicationTCPEvent dataReceive)
            {
                Log.LogEngine.Instance.Engine.Debug("RerceivedData =", dataReceive.RerceivedData);
                Message received = Utility.Functions.GetMessage(dataReceive.RerceivedData);

                dataReceive.ReturnString = "ERROR";
                if (received != null)
                {
                    Log.LogEngine.Instance.Engine.Debug("received =", received.ToString());
                    if(this.topics.ContainsKey(received.TopicId))
                    {
                        MessageEventArgs arg = new MessageEventArgs();
                        arg.Message = received;
                        MessagePump.Instance.Publish(new Guid(received.ToString()), this, arg);
                    }


                }
            };
        }

        private Message getMessage()
        {
            Message msg = new Message();
            msg.ClientId = this.clientTCP.Id;
            msg.Token = this.token;
            return msg;
        }

        public bool Connect()
        {
            if (string.IsNullOrEmpty(this.Remote)) return false;
            if (this.isConnect) return true;

            if (this.clientTCP == null) return false;

            /* Prima fase AUTH */
            Hermes.Message msg = new Hermes.Message();
            msg.ClientId = this.clientTCP.Id;
            msg.Text = Command.GetCommand(Command.enumTCPCommand.AUTH0);
            string dataReturn = this.clientTCP.Send(msg.ToString());
            if (!string.IsNullOrEmpty(dataReturn))
            {
                Message retMsg = Utility.Functions.GetMessage(dataReturn);
                if (retMsg != null)
                {
                    Log.LogEngine.Instance.Engine.Debug("Messagio Ricevuto =" + retMsg.ToString());

                    string sCode = Security.Token.GetVerifyToken(retMsg);
                    retMsg.Text = Command.GetCommand(Command.enumTCPCommand.AUTH1) + sCode;
                    dataReturn = this.clientTCP.Send(retMsg.ToString());
                    Log.LogEngine.Instance.Engine.Debug("dataReturn =" + dataReturn);
                    if (!string.IsNullOrEmpty(dataReturn) && dataReturn == sCode)
                    {
                        this.token = dataReturn;
                        this.isConnect = true;
                        this.serverTCP.Start();
                        return true;
                    }
                }
            }
            return false;
        }

        public Guid? GetTopic(string topicName)
        {
            if (this.isConnect)
            {
                Log.LogEngine.Instance.Engine.Debug(" GETTOPIC =" + topicName);
                Message msg = this.getMessage();
                msg.Text = Command.GetCommand(Command.enumTCPCommand.GETTOPIC) + topicName;
                string dataReturn = this.clientTCP.Send(msg.ToString());
                if (!string.IsNullOrEmpty(dataReturn))
                {
                    Message retMsg = Utility.Functions.GetMessage(dataReturn);
                    if (retMsg != null && retMsg.Bag != null)
                    {

                        return new Guid(retMsg.Bag.ToString());
                    }
                }
            }
            return null;
        }

        public bool SubScribe(Guid topic, InternalMessageDelegate callBack)
        {
            if (this.isConnect)
            {
                Log.LogEngine.Instance.Engine.Debug(" SUBSCRIBE =" + topic.ToString());
                Message msg = this.getMessage();
                msg.Text = Command.GetCommand(Command.enumTCPCommand.SUBSCRIBE);
                msg.TopicId = topic.ToString();
                string dataReturn = this.clientTCP.Send(msg.ToString());
                if (!string.IsNullOrEmpty(dataReturn))
                {
                    Message retMsg = Utility.Functions.GetMessage(dataReturn);
                    if (retMsg != null && retMsg.Bag != null)
                    {
                        Guid iTop = MessagePump.Instance.GetTopic(retMsg.Bag.ToString());
                        MessagePump.Instance.Subscribe(iTop, callBack);
                        this.topics.Add(topic.ToString(), iTop.ToString());
                        return true;
                    }

                }
            }
            return false;
        }

        public bool UnsubScribe(Guid topic, InternalMessageDelegate callBack)
        {
            if (this.isConnect)
            {
                Log.LogEngine.Instance.Engine.Debug(" UNSUBSCRIBE =" + topic.ToString());
                Message msg = this.getMessage();
                msg.Text = Command.GetCommand(Command.enumTCPCommand.UNSUBSCRIBE);
                msg.TopicId = topic.ToString();
                string dataReturn = this.clientTCP.Send(msg.ToString());
                if (!string.IsNullOrEmpty(dataReturn))
                {
                    if (dataReturn == "DONE")
                    {
                        MessagePump.Instance.UnSubscribe(topic, callBack);
                        this.topics.Remove(topic.ToString());
                        return true;
                    }
                }
            }
            return false;
        }

        public bool SendTopic(Guid topic, string msgText, object msgBag)
        {
            if (this.isConnect)
            {
                Log.LogEngine.Instance.Engine.Debug(" SENDTOPIC =" + topic.ToString());
                Message msg = this.getMessage();
                msg.Text = Command.GetCommand(Command.enumTCPCommand.UNSUBSCRIBE);
                msg.TopicId = topic.ToString();
                msg.Text = msgText;
                msg.Bag = msgBag;
                string dataReturn = this.clientTCP.Send(msg.ToString());
                if (!string.IsNullOrEmpty(dataReturn))
                {
                    if (dataReturn == "DONE")
                        return true;
                }
            }
            return false;
        }

    }


}
