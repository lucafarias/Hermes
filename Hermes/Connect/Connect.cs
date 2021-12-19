using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Connect
{
    public class Connect
    {
        public enum enumState
        {
            AUTH,
            CONNECT
        }
        public EndPoint RemoteAddress { get; set; }
        public int Port { get; set; }

        public string Guid { get; set; }

        public enumState State { get; set; }
        public string Token { get; set; }

        public bool IsExpired
        {
            get
            {
                if(this.State == enumState.AUTH)
                {
                    int deltaTime = Convert.ToInt32((DateTime.Now - this.CreationTime).TotalSeconds);
                    if (deltaTime > Manager.Instance.GetExpiredSec) return true;
                    
                }
                return false;
            }
        }

        public DateTime CreationTime { get; private  set; }
        public Connect()
        {
            this.State = enumState.AUTH;
            this.CreationTime = DateTime.Now;
        }
    }
}
