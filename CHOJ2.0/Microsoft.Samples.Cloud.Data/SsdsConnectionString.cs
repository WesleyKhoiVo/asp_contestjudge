namespace Microsoft.Samples.Cloud.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the Connection String used to interact with Ssds.
    /// </summary>
    public class SsdsConnectionString
    {
        /// <summary>
        /// Creates an instance of SsdsConnectionString.
        /// </summary>
        /// <param name="connectionString">The connection string as string.</param>
        public SsdsConnectionString(string connectionString)
        {
            Dictionary<string, string> components = FactorizeComponents(connectionString);

            this.Authority = components["authority"];
            this.UserName = components["username"];
            this.Password = components["password"];
        }

        /// <summary>
        /// Gets or sets the authority Uri.
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Gets or sets the user name used to connect to Ssds.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the password used to connect to Ssds.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets the current scope Uri.
        /// </summary>
        public Uri ScopeUri
        {
            get
            {
                Uri baseUri = new Uri(this.Authority);
                return baseUri;
            }
        }

        /// <summary>
        /// Parses the connection string provided as a plain string and 
        /// returns its components contained on K,V dictionary.
        /// </summary>
        /// <param name="connectionString">The connection string as plain string.</param>
        /// <returns>A Dictionary containing on the components of the connection string splitted in K,V pairs.</returns>
        private static Dictionary<string, string> FactorizeComponents(string connectionString)
        {
            Dictionary<string, string> components = new Dictionary<string, string>();
            
            string[] chunks = connectionString.Split(';');
            
            for (int i = 0; i < chunks.Count(); i++)
            {
                string[] keyValuePair = chunks[i].Split('=');
                components[keyValuePair[0]] = keyValuePair[1];
            }

            return components;
        }
    }
}
