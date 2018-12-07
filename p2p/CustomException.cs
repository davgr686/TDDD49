using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p2p
{
    public class CustomException : Exception
    {
        public string message { get; set; }

        public CustomException(string msg)
        {
           message = msg; 
        }
    }
}
