using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Hermes.Channel.Tcp
{
    public class Client
    {
        public int Port { get; set; }
        public string Remote { get; set; }

        public string Id { get; private set; }


        public Client() 
        {
            this.Port = 0;
            this.Id = Guid.NewGuid().ToString(); 
        }

        public Client(string remoteAddress, int port) : this()
        {
            this.Remote = remoteAddress;
            this.Port = port;
            
        }



        public string  Send(string message)
        {
            if (string.IsNullOrEmpty(this.Remote) || this.Port == 0 || string.IsNullOrEmpty(message)) return "No server data";

            Log.LogEngine.Instance.Engine.Debug($"Send Message = {message}" );
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];
            string response = "";
            // Connect to a remote device.  
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry(this.Remote);
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, this.Port);

                // Create a TCP/IP  socket.  
                Socket sender = new Socket(ipAddress.AddressFamily,SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    Log.LogEngine.Instance.Engine.Debug("Socket connected to {0}",sender.RemoteEndPoint.ToString());

                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes(message);

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.  
                    int bytesRec = sender.Receive(bytes);
                    response = Encoding.ASCII.GetString(bytes).Substring(0,bytesRec);

                    Log.LogEngine.Instance.Engine.Debug("Response = {0}",Encoding.ASCII.GetString(bytes, 0, bytesRec));

                    // Release the socket.  
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Log.LogEngine.Instance.Engine.Error("ArgumentNullException : " + ane.ToString(), ane);
                }
                catch (SocketException se)
                {
                    Log.LogEngine.Instance.Engine.Error("SocketException : " +se.ToString(), se);
                }
                catch (Exception e)
                {
                    Log.LogEngine.Instance.Engine.Error(e);
                }

            }
            catch (Exception e)
            {
                Log.LogEngine.Instance.Engine.Error(e.ToString());
            }
            return response;
        }
    }
}
