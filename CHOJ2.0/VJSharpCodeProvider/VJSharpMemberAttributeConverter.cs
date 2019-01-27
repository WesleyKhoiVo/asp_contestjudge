using System.CodeDom;

namespace Microsoft.VJSharp
{
    internal class VJSharpMemberAttributeConverter : VJSharpModifierAttributeConverter
    {
        // Fields
        private static VJSharpMemberAttributeConverter defaultConverter;
        private static string[] names;
        private static object[] values;

        // Methods
        private VJSharpMemberAttributeConverter()
        {
        }

        // Properties
        public static VJSharpMemberAttributeConverter Default
        {
            get
            {
                if (defaultConverter == null)
                {
                    defaultConverter = new VJSharpMemberAttributeConverter();
                }
                return defaultConverter;
            }
        }

        protected override object DefaultValue
        {
            get
            {
                return MemberAttributes.Private;
            }
        }

        protected override string[] Names
        {
            get
            {
                if (names == null)
                {
                    names = new string[] { "Public", "Protected", "Package", "Private" };
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
                    values = new object[] { MemberAttributes.Public, MemberAttributes.Family, MemberAttributes.Assembly, MemberAttributes.Private };
                }
                return values;
            }
        }
    }

 

}