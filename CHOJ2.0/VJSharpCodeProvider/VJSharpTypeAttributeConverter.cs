using System.Reflection;

namespace Microsoft.VJSharp
{
    internal class VJSharpTypeAttributeConverter : VJSharpModifierAttributeConverter
    {
        // Fields
        private static VJSharpTypeAttributeConverter defaultConverter;
        private static string[] names;
        private static object[] values;

        // Methods
        private VJSharpTypeAttributeConverter()
        {
        }

        // Properties
        public static VJSharpTypeAttributeConverter Default
        {
            get
            {
                if (defaultConverter == null)
                {
                    defaultConverter = new VJSharpTypeAttributeConverter();
                }
                return defaultConverter;
            }
        }

        protected override object DefaultValue
        {
            get
            {
                return TypeAttributes.AutoLayout;
            }
        }

        protected override string[] Names
        {
            get
            {
                if (names == null)
                {
                    names = new string[] { "Public", "Package" };
                }
                return names;
            }
        }

        protected override object[] Values
        {
            get
            {
                if (values == null)
                {
                    values = new object[] { TypeAttributes.Public, TypeAttributes.AutoLayout };
                }
                return values;
            }
        }
    }

 

}