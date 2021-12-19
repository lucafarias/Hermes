using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Comunication
{
    internal static class Command
    {
        public enum enumTCPCommand
        {
            AUTH0,
            AUTH1,
            GETTOPIC,
            SUBSCRIBE,
            UNSUBSCRIBE
        }

        internal static string  GetCommand(enumTCPCommand command)
        {
            return command.ToString() + ":";
        }
    }
}
