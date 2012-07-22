using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMe2Day
{
    public class Me2Exception : Exception
    {
        /// <summary>
        /// me2DAY Error
        /// </summary>
        public Me2Error Error { get; set; }

        public Me2Exception(Me2Error error)
        {
            Error = error;
        }

    }
}
