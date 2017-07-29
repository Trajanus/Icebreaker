using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Icebreaker
{
    public class QuestionBasedMessage : Message
    {
        private string _question;
        public string Question
        {
            get { return _question; }
        }

        private string _answer;
        public string Answer
        {
            get { return _answer; }
        }

        private int _questionId;
        public int QuestionId
        {
            get { return _questionId; }
        }

        public string AnswerHtmlId_XpathQuery
        {
            get { return String.Format(Constants.XPathSelector.AnswerHtmlId, QuestionId.ToString()); }
        }

        public QuestionBasedMessage(int questionId, string question, string answer, string messageText)
        {
            _questionId = questionId;
            _question = question;
            _answer = answer;
            _messageText = messageText;
        }
    }
}
