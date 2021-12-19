using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Security
{
    public class Token
    {
        public const int TokenLenght = 128;
        public static string GetToken()
        {
            var allChar = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-";
            var random = new Random();
            var resultToken = new string(
               Enumerable.Repeat(allChar, TokenLenght)
               .Select(token => token[random.Next(token.Length)]).ToArray());

           return  resultToken.ToString();
        }

        public static string GetVerifyToken(Message msg)
        {
            return Utility.Functions.EncryptString(Utility.Functions.MixString(msg.Token, msg.ClientId), msg.ClientId).Substring(0,TokenLenght);
        }
    }
}
