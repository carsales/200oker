using System.Collections.Generic;
using System.Net;

namespace _200oker
{
    public class Check
    {
        /// <summary>
        /// Entry URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Used to select children for checking - uses JQuery/Sizzle syntax
        /// </summary>
        public string ChildSelector { get; set; }

        /// <summary>
        /// Child requests, if applicable
        /// </summary>
        public List<Check> Children { get; set; }

        /// <summary>
        /// The result of the check
        /// </summary>
        public HttpStatusCode Result { get; set; }

        public Check()
        {
            Children = new List<Check>();
        }
    }
}
