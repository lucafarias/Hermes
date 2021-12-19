using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Hermes.Channel.Tcp
{
    public class Server : IDisposable
    {
        TcpListener server = null;
        private CancellationTokenSource tokenSource ;
        private CancellationToken cancelToken;

        public Action<ComunicationTCPEvent> DataReceived { get; set; }

        public int Port { get; set; }
        public string LocalIp { get; set; }
        public Server() { }

        public Server(string localIp, int port)
        {
            this.Port = port;
            this.LocalIp = localIp;
            this.tokenSource = new CancellationTokenSource();
            this.cancelToken = tokenSource.Token;
        }

        public void Start()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(this.LocalIp);
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, this.Port);
            Log.LogEngine.Instance.Engine.Info($" Server start in { localEndPoint.ToString()}" );
            this.server = new TcpListener(localEndPoint);
            this.server.Start();
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.cancelToken.ThrowIfCancellationRequested();
                while(true)
                {
                    Log.LogEngine.Instance.Engine.Info(" Sono in ascolto ");
                    if (this.cancelToken.IsCancellationRequested)
                    {
                        Log.LogEngine.Instance.Engine.Debug("IsCancellationRequested ");
                        return;
                    }
                    TcpClient client = server.AcceptTcpClient();
                    Log.LogEngine.Instance.Engine.Debug("Connected!");

                    Thread t = new Thread(new ParameterizedThreadStart(HandleConnection));
                    t.Start(client);
               
                }
            });

        }

        public void HandleConnection(Object obj)
        {
            TcpClient client = (TcpClient)obj;
            var stream = client.GetStream();
            string imei = String.Empty;

            string dataReceived = null;
            Byte[] bytes = new Byte[1048576];
            int i;
            try
            {
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    string hex = BitConverter.ToString(bytes);
                    dataReceived = Encoding.ASCII.GetString(bytes, 0, i);

                    Log.LogEngine.Instance.Engine.Debug("Data Received: {0}", dataReceived);

                    string messageToReturn= "ERROR";

                    if (this.DataReceived != null)
                    {
                        ComunicationTCPEvent ev = new ComunicationTCPEvent(client.Client.RemoteEndPoint, dataReceived);
                        this.DataReceived(ev);
                        messageToReturn = ev.ReturnString;

                    }
                    
                    Byte[] reply = System.Text.Encoding.ASCII.GetBytes(messageToReturn);
                    stream.Write(reply, 0, reply.Length);
                    Log.LogEngine.Instance.Engine.Debug("Sent: {0}", messageToReturn);
                }
            }
            catch (Exception e)
            {
                Log.LogEngine.Instance.Engine.Error(e);
                client.Close();
            }
        }

        public void Stop()
        {
            this.tokenSource.Cancel();
        }

        private Message getMessage(string dataReceived)
        {
            if(!string.IsNullOrEmpty(dataReceived))
            {
                try
                {
                    Message mes = Newtonsoft.Json.JsonConvert.DeserializeObject<Message>(dataReceived);
                    return mes;
                }
                catch (Exception ex)
                {
                    Log.LogEngine.Instance.Engine.Error(ex);
                }
            }
            return null;
        }
        public void Dispose()
        {
            this.tokenSource.Cancel();
        }
    }
}
