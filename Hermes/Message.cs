using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hermes
{
    public class Message
    {
        public string SenderIP { get; set; }
        public int SenderPort { get; set; }
        public string Date { get; private set; }
        public string MessageId { get; private set; }
        public string ClientId { get; set; }

        public string TopicId { get; set; }
        public string Text { get; set; }
        public object Bag { get; set; }
        public string Token { get; set; }
        public Message()
        {
            this.Date = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            this.MessageId = Guid.NewGuid().ToString();
            this.Text = "";
            this.Bag = null;
        }
        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

    public class ComunicationTCPEvent : EventArgs
    {
        public EndPoint RemoteAddress { get; set; }
        public string RerceivedData { get; set; }
        public string ReturnString { get; set; }

        public ComunicationTCPEvent(EndPoint endP, string received)
        {
            this.RemoteAddress = endP;
            this.RerceivedData = received;
        }
    }
}
