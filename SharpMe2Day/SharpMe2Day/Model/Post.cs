using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpMe2Day.Model;


namespace SharpMe2Day.Model
{
    public class Post
    {
        public string post_id { get; set; }
        public string permalink { get; set; }
        public string body { get; set; }
        public string kind { get; set; }
        public string icon { get; set; }
        public string me2dayPage { get; set; }
        public DateTime pubDate { get; set; }
        public Int32 commentsCount { get; set; }
        public Int32 metooCount { get; set; }
        public Person Author { get; set; }
        public List<Tag> Tags { get; set; }

        public Post()
        {
            Tags = new List<Tag>();
        }

    }
}
