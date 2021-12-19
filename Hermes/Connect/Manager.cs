using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hermes.Connect
{
    public class Manager:IDisposable
    {
        private CancellationTokenSource tokenSource;
        private CancellationToken cancelToken;

        private object lockInternal;

        private List<Connect> connect;
        private static readonly Lazy<Manager> lazy = new Lazy<Manager>(() => new Manager());

        public static Manager Instance { get { return lazy.Value; } }
        public Manager()
        {
            this.connect = new List<Connect>();
            this.lockInternal = new object();
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.cancelToken.ThrowIfCancellationRequested();
                while (true)
                {
                    Thread.Sleep(400);
                    if (this.cancelToken.IsCancellationRequested)
                    {
                        return;
                    }
                    lock (this.lockInternal)
                    {
                        if (this.connect != null && this.connect.Count > 0)
                        {
                            List<Connect> toremove = new List<Connect>();
                            this.connect.ForEach(c => { if (c.IsExpired) toremove.Add(c); });
                            toremove.ForEach(r => this.connect.Remove(r));
                        }
                    }
                }
            });

        }
        private const int EXPIREDTIMEOUTSEC = 60;
        public int GetExpiredSec { get { return EXPIREDTIMEOUTSEC; } }
     
        public bool IsConnect(Message msg)
        {
            if (this.connect == null || (this.connect != null && this.connect.Count >= 0))
            {
                Connect con = this.GetConnection(msg);
                if (con != null) return true;
            }
            return false;
        }
        
        public bool IsGrant(Message msg)
        {
            if (this.connect == null || (this.connect != null && this.connect.Count >= 0))
            {
                Connect con = this.GetConnection(msg);
                if (con != null && con.Token == msg.Token && con.State == Connect.enumState.CONNECT) return true;
            }
            return false;
        }

        public Connect GetConnection(Message msg)
        {
            return this.connect.Find(c => c.Guid == msg.ClientId);
        }

        public void Add( Connect con)
        {
            lock(this.lockInternal)
            {
                this.connect.Add(con);
            }
        }

        public void Remove(Connect con)
        {
            lock (this.lockInternal)
            {
                this.connect.Remove(con);
            }
        }
        public void Remove (Message msg )
        {
            Connect con = this.GetConnection(msg);
            if(con != null )
            {
                this.Remove(con);
            }

        }
        public void Dispose()
        {
            this.tokenSource.Cancel();
        }
    }
}
