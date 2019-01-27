using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace LinqToTwitter
{
    class PublicStatusRequestProcessor : StatusRequestProcessor, IRequestProcessor
    {
        /// <summary>
        /// extracts parameters from lambda
        /// </summary>
        /// <param name="lambdaExpression">lambda expression with where clause</param>
        /// <returns>dictionary of parameter name/value pairs</returns>
        public override Dictionary<string, string> GetParameters(LambdaExpression lambdaExpression)
        {
            var paramFinder =
               new ParameterFinder<Status>(
                   lambdaExpression.Body,
                   new List<string> { 
                       "Type",
                       "ID",
                       "Since",
                       "SinceID",
                       "Count",
                       "Page"
                   });

            var parameters = paramFinder.Parameters;

            return parameters;
        }

        /// <summary>
        /// builds url based on input parameters
        /// </summary>
        /// <param name="parameters">criteria for url segments and parameters</param>
        /// <returns>URL conforming to Twitter API</returns>
        public override string BuildURL(Dictionary<string, string> parameters)
        {
            string url = null;

            if (parameters == null ||
                !parameters.ContainsKey("Type"))
            {
                url = BaseUrl + "statuses/public_timeline.xml";
                return url;
            }

            switch (parameters["Type"])
            {
                case "Public":
                    url = BaseUrl + "statuses/public_timeline.xml";
                    break;
                case "Friends":
                    url = BaseUrl + "statuses/friends_timeline.xml";
                    break;
                case "User":
                    url = BaseUrl + "statuses/user_timeline.xml";
                    break;
                default:
                    url = BaseUrl + "statuses/public_timeline.xml";
                    break;
            }

            if (parameters.ContainsKey("ID"))
            {
                url = url.Replace(".xml", "/" + parameters["ID"] + ".xml");
            }

            var urlParams = new List<string>();

            if (parameters.ContainsKey("Since"))
            {
                var sinceDateLocal = DateTime.Parse(parameters["Since"]);
                var sinceDateUtc = new DateTimeOffset(sinceDateLocal,
                            TimeZoneInfo.Local.GetUtcOffset(sinceDateLocal));

                urlParams.Add("since=" + sinceDateUtc.ToUniversalTime().ToString("r"));
            }

            if (parameters.ContainsKey("SinceID"))
            {
                urlParams.Add("since_id=" + parameters["SinceID"]);
            }

            if (parameters.ContainsKey("Count"))
            {
                urlParams.Add("count=" + parameters["Count"]);
            }

            if (parameters.ContainsKey("Page"))
            {
                urlParams.Add("page=" + parameters["Page"]);
            }

            if (urlParams.Count > 0)
            {
                url += "?" + string.Join("&", urlParams.ToArray());
            }

            return url;
        }
    }
}
