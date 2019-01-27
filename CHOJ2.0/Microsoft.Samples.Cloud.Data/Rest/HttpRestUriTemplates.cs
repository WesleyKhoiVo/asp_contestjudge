namespace Microsoft.Samples.Cloud.Data
{
    using System;

    /// <summary>
    /// Contains the uri templates for Rest interactions.
    /// </summary>
    internal static class HttpRestUriTemplates
    {
        /// <summary>
        /// Staticaly initiates the class members.
        /// </summary>
        static HttpRestUriTemplates()
        {
            EntityTemplate = new UriTemplate("{entityId}");
            UpdateTemplate = new UriTemplate("{containerId}/{entityId}");
            ContainerTemplate = new UriTemplate("{containerId}");
        }

        /// <summary>
        /// Gets or sets the UriTemplate for a Delete operation.
        /// </summary>
        internal static UriTemplate EntityTemplate { get; set; }

        /// <summary>
        /// Gets or sets the UriTemplate for an Update operation.
        /// </summary>
        internal static UriTemplate UpdateTemplate { get; set; }

        /// <summary>
        /// Gets or sets the UriTemplate for a Container.
        /// </summary>
        internal static UriTemplate ContainerTemplate { get; set; }
    }
}