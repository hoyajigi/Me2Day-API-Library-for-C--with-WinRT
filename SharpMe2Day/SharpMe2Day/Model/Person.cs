using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMe2Day.Model
{
    public class Person
    {
        public string id { get; set; }
        public string openid { get; set; }
        public string nickname { get; set; }
        public string face { get; set; }
        public string description { get; set; }
        public string homepage { get; set; }
        public string me2dayHome { get; set; }
        public string rssDaily { get; set; }
        public string invitedBy { get; set; }
        public string request_message { get; set; }
        public string request_id { get; set; }
        public int friendsCount { get; set; }
        public DateTime updated { get; set; }

        public string realname { get; set; }
        public string birthday { get; set; }
    }
}
