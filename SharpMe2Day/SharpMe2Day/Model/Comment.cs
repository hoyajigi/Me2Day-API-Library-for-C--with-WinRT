using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMe2Day.Model
{
    public class Comment
    {
        public string body { get; set; }
        public DateTime pubDate { get; set; }
        public Person Author { get; set; }
    }
}
