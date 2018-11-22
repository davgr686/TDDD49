using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace _DataProtocol
{
    public class DataProtocol
    {
        private String _type;
        private String _username;
        private DateTime _timestamp = DateTime.Now;
        private String _message;

        public DataProtocol(string type, string username, string message)
        {
            _type = type;
            _username = username;
            _message = message;
        }

        public String Type
        {
            get
            {
                return _type;
            }
            set
            {
                //Maybe add checks for invalid input
                _type = value;
            }
        }

        public String Username
        {
            get
            {
                return _username;
            }
            set
            {
                //Maybe add checks for invalid input
                _username = value;
            }
        }

        public DateTime Timestamp
        {
            get
            {
                return _timestamp;
            }
            set { }
        }

        public String Message
        {
            get
            {
                return _message;
            }
            set
            {
                //Maybe add checks for invalid input
                _message = value;
            }
        }

        public String CreateJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}