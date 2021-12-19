using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {

            Hermes.Comunication.Server.Server server = new Hermes.Comunication.Server.Server(34000);
            server.Start();
            while(true)
            {
                Thread.Sleep(200);
            }
 }
    }
}
