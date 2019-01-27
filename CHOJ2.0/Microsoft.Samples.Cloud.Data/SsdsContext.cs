namespace Microsoft.Samples.Cloud.Data
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using System.Globalization;

    /// <summary>
    /// Represents the SsdsAuthority context. Allows users
    /// to retrieve strongly typed objects while making queries 
    /// to a container inside it.
    /// </summary>
    public class SsdsContext:IDisposable
    {
        /// <summary>
        /// Holds a reference to the connection string used during the object life-cycle.
        /// </summary>
        private SsdsConnectionString connectionString;
        
        /// <summary>
        /// Creates a new instance of SsdsContext.
        /// </summary>
        /// <param name="connectionString">The connection string pointing to the authority.</param>
        public SsdsContext(string connectionString)
        {
            this.connectionString = new SsdsConnectionString(connectionString);
        }

        /// <summary>
        /// Creates an authority with given name.
        /// </summary>
        /// <param name="name">The authority name.</param>
        /// <returns>The URI of the created authority.</returns>
        public string CreateAuthority(string name)
        {
            SsdsRestFacade facade = this.CreateFacade(true);
            var authority = new XElement(Constants.ns + "Authority",
                new XElement(Constants.ns + "Id", name)
                );

            return facade.Insert(authority.ToString());
        }

        /// <summary>
        /// Creates a container on the given authority.
        /// </summary>
        /// <param name="name">The containers name.</param>
        /// <returns>The URI of the created container.</returns>
        public string CreateContainer(string name)
        {
            SsdsRestFacade facade = this.CreateFacade();
            var container = new XElement(Constants.ns + "Container",
                new XElement(Constants.ns + "Id", name)
                );

            return facade.Insert(container.ToString());
        }

        /// <summary>
        /// Deletes a container on the given authority.
        /// </summary>
        /// <param name="name">The container's name.</param>
        public void DeleteContainer(string name)
        {
            SsdsRestFacade facade = this.CreateFacade();
            facade.Delete(name, ConcurrencyPattern.Always, null);
        }

        /// <summary>
        /// Returns a value indicating whether the container passed as parameter
        /// exists or not.
        /// </summary>
        /// <param name="name">The container name.</param>
        /// <returns>A value indicating whether the container exists.</returns>
        public bool ContainerExists(string name)
        {
            SsdsRestFacade facade = this.CreateFacade();
            return facade.EntityExists(name);
        }

        /// <summary>
        /// Retrieves an strongly type query facade to interact with Ssds.
        /// </summary>
        /// <param name="scope">Container name where the operations will be performed.</param>
        /// <returns>A SsdsTable object.</returns>
        public SsdsContainer OpenContainer(string scope)
        {
            return new SsdsContainer(this.connectionString.UserName, this.connectionString.Password, this.connectionString.ScopeUri, scope);
        }

        /// <summary>
        /// Creates a SsdsRestFacade used to send REST messages
        /// from and to the container/authority location.
        /// </summary>
        /// <returns>A new SsdsRestFacade instance.</returns>
        private SsdsRestFacade CreateFacade()
        {
            SsdsRestFacade facade = new SsdsRestFacade(this.connectionString.UserName, this.connectionString.Password, this.connectionString.ScopeUri);
            return facade;
        }

        private SsdsRestFacade CreateFacade(bool authority)
        {
            var service = String.Join(".",
                this.connectionString.ScopeUri.Authority.Split('.').Skip(1).ToArray()
                    );

            var uri = new Uri(String.Format(CultureInfo.InvariantCulture, "{0}://{1}{2}",
                this.connectionString.ScopeUri.Scheme,
                service,
                this.connectionString.ScopeUri.PathAndQuery
                ));

            return new SsdsRestFacade(this.connectionString.UserName, this.connectionString.Password, uri);
        }

        public void Dispose()
        {
            
        }
    }
}
