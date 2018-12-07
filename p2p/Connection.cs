using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p2p
{
    public class Connection
    {
        private string UsernameValue;

        public string Username
        {
            get { return UsernameValue; }
            set { UsernameValue = value; }
        }

        private string ConnectorIPValue;

        public string ConnectorIP
        {
            get { return ConnectorIPValue; }
            set { ConnectorIPValue = value; }
        }

        private string ConnectorPortValue;

        public string ConnectorPort
        {
            get { return ConnectorPortValue; }

            set { ConnectorPortValue = value; }
        }
        private string ListenerPortValue;

        public string ListenerPort
        {
            get { return ListenerPortValue; }

            set { ListenerPortValue = value; }

        }
    }
}
