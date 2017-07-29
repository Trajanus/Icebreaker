using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Icebreaker
{
    public class SingleMessageRun : MessageRun
    {
        private string _messageText;
        public string MessageText
        {
            set { _messageText = value; }
            get { return _messageText; }
        }

        public int MessagesToSend
        {
            get { return Usernames.Count(); }
        }

        public List<string> _usernames;
        public List<string> Usernames
        {
            get { return _usernames; }
        }

        public SingleMessageRun(List<string> usernames, Message message)
        {
            _usernames = usernames;
            _messageText = message.MessageText;
        }
    }
}
