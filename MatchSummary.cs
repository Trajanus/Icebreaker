using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Icebreaker
{
    public class MatchSummary
    {
        public class Userinfo
        {
            public object realname { get; set; }
            public string gender_letter { get; set; }
            public string gender { get; set; }
            public int age { get; set; }
            public string rel_status { get; set; }
            public string location { get; set; }
            public string orientation { get; set; }
        }

        public class Likes
        {
            public object you_like { get; set; }
            public int mutual_like { get; set; }
            public object they_like { get; set; }
        }

        public class Percentages
        {
            public int match { get; set; }
            public int enemy { get; set; }
        }

        public class Info
        {
            public string path { get; set; }
            public int when_taken { get; set; }
            public int type { get; set; }
            public int lower_right_x { get; set; }
            public int lower_right_y { get; set; }
            public int ordinal { get; set; }
            public string caption { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public int when_uploaded { get; set; }
            public int upper_left_x { get; set; }
            public int upper_left_y { get; set; }
            public string thumbnail { get; set; }
            public string picid { get; set; }
        }

        public class Thumb
        {
            public Info info { get; set; }
        }

        public class LastContacts
        {
            public int forward { get; set; }
            public int reverse { get; set; }
        }

        public class Datum
        {
            public bool online { get; set; }
            public Userinfo userinfo { get; set; }
            public Likes likes { get; set; }
            public Percentages percentages { get; set; }
            public bool inactive { get; set; }
            public string userid { get; set; }
            public string username { get; set; }
            public bool staff { get; set; }
            public List<Thumb> thumbs { get; set; }
            public LastContacts last_contacts { get; set; }
        }

        public List<Datum> data { get; set; }
    }
}
