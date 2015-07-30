using System.Net;

namespace _200oker
{
    public class CheckResult
    {
        /// <summary>
        /// Entry URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The result of the check
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }
    }
}
