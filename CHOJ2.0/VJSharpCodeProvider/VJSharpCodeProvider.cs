using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

namespace Microsoft.VJSharp
{
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    public class VJSharpCodeProvider : CodeDomProvider
    {
        // Fields
        private VJSharpCodeGenerator generator = new VJSharpCodeGenerator();

        // Methods
        [Obsolete("Callers should not use the ICodeCompiler interface and should instead use the methods directly on the CodeDomProvider class.")]
        public override ICodeCompiler CreateCompiler()
        {
            return this.generator;
        }

        [Obsolete("Callers should not use the ICodeGenerator interface and should instead use the methods directly on the CodeDomProvider class.")]
        public override ICodeGenerator CreateGenerator()
        {
            return this.generator;
        }

        public override void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
        {
            this.generator.GenerateCodeFromMember(member, writer, options);
        }

        public override TypeConverter GetConverter(Type type)
        {
            if (type == typeof(MemberAttributes))
            {
                return VJSharpMemberAttributeConverter.Default;
            }
            if (type == typeof(TypeAttributes))
            {
                return VJSharpTypeAttributeConverter.Default;
            }
            return base.GetConverter(type);
        }

        // Properties
        public override string FileExtension
        {
            get
            {
                return ".jsl";
            }
        }
    }


}