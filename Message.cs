using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Icebreaker
{
    public class Message
    {
        protected string _messageText;
        public string MessageText
        {
            get { return _messageText; }
        }

        public Message(string messageText)
        {
            _messageText = messageText;
        }

        public Message()
        {
            _messageText = string.Empty;
        }
    }
}
