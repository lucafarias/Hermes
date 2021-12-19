using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Hermes.Comunication.Client.Client cli = new Hermes.Comunication.Client.Client("127.0.0.1", 34000);
            cli.Connect();
            Guid? top = cli.GetTopic("TOPIC 001");

            while (true)
            {
                System.Threading.Thread.Sleep(200);
            }
        }
    }
}
