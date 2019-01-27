namespace Microsoft.Samples.Cloud.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Resolves which XSD type corresponds to each CLR type.
    /// </summary>
    public static class XsdTypeResolver
    {
        /// <summary>
        /// Resolves which XSD type corresponds to a given CLR type.
        /// </summary>
        /// <param name="type">The type that will be mapped to XSD types.</param>
        /// <returns>An string with the XSD type name. If type does not have
        /// an specific type corresponding it will return string.</returns>
        public static string Solve(Type type)
        {
            if (!xsdTypes.ContainsKey(type))
            {
                return xsdTypes[typeof(string)];
            }

            return xsdTypes[type];
        }

        /// <summary>
        /// Contains the static mapping type between CLR types and XSD types.
        /// </summary>
        static Dictionary<Type, string> xsdTypes = new Dictionary<Type, string>()
        {
            {typeof(string), "x:string"},
            {typeof(int), "x:decimal"},
            {typeof(long), "x:decimal"},
            {typeof(float), "x:decimal"},
            {typeof(decimal), "x:decimal"},
            {typeof(short), "x:decimal"},
            {typeof(double), "x:decimal"},
            {typeof(DateTime), "x:dateTime"},
            {typeof(bool), "x:boolean"},
            {typeof(byte[]), "x:base64Binary"}
        };
    }
}
