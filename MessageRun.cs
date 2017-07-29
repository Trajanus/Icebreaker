using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Icebreaker
{
    
    public abstract class MessageRun
    {
        protected int _numMessagesToSend;
        private bool _ready;

        public int NumMessagesToSend
        {
            set { _numMessagesToSend = value; }
            get { return _numMessagesToSend; }
        }

        public bool Ready
        {
            get { return _ready; }
            set { _ready = value; }
        }
    }
}
