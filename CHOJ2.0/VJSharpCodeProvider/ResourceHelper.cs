using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Microsoft.VJSharp
{
    internal class ResourceHelper
    {
        // Fields
        private static ResourceManager resourceManager;
        private const string resourceName = "Microsoft.VJSharp.VJSharpCodeProvider";

        // Methods
        private ResourceHelper()
        {
        }

        public static ResourceManager GetLoader()
        {
            if (resourceManager == null)
            {
                lock (typeof(ResourceHelper))
                {
                    if (resourceManager == null)
                    {
                        resourceManager = new ResourceManager("Microsoft.VJSharp.VJSharpCodeProvider", Assembly.GetCallingAssembly());
                    }
                }
            }
            return resourceManager;
        }

        public static string GetString(string resourceID)
        {
            return GetLoader().GetString(resourceID, CultureInfo.InvariantCulture);
        }
    }

 

}