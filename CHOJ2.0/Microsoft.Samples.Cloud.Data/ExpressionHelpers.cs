namespace Microsoft.Samples.Cloud.Data
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Extension method to compare one string to another.  This is only used to 
        /// build expressions that later get coverted to LINQ syntax for SSDS.  The 
        /// implementation is not useful.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool GreaterThan(this string first, string second)
        {
            return true;
        }

        /// <summary>
        /// Extension method to compare one string to another.  This is only used to 
        /// build expressions that later get coverted to LINQ syntax for SSDS.  The 
        /// implementation is not useful.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool LessThan(this string first, string second)
        {
            return true;
        }
    }
}
