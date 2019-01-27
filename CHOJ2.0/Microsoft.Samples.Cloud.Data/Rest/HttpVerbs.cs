namespace Microsoft.Samples.Cloud.Data
{
    /// <summary>
    /// Contains the diferent types of HttpVerbs used to interact with Ssds.
    /// </summary>
    internal static class HttpVerbs
    {
        /// <summary>
        /// Returns the HttpVerb.Get used to query Ssds.
        /// </summary>
        public const string Get = "GET";

        /// <summary>
        /// Returns the HttpVerb.Post used to insert data on Ssds.
        /// </summary>
        public const string Post = "POST";

        /// <summary>
        /// Returns the HttpVerb.Put used to update data on Ssds.
        /// </summary>
        public const string Put = "PUT";

        /// <summary>
        /// Returns the HttpVerb.Post used to delete data from Ssds.
        /// </summary>
        public const string Delete = "DELETE";
    }
}

