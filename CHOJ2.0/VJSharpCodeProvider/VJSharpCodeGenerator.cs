using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Microsoft.VJSharp
{
    internal class VJSharpCodeGenerator : CodeCompiler
    {
        // Fields
        private CodeAttributeDeclarationCollection assemblyAttributes;
        private const string BYTE_FORMAT = "((ubyte){0})";
        private CodeParameterDeclarationExpressionCollection currentMethodParameters;
        private int EnumRemainingMemberCount;
        private bool fInArrayInitializer;
        private bool fInMethodReferenceExpression;
        private bool fInSetStatement;
        private bool fIsAttributeArg;
        private bool generatingForLoop;
        private const string INT16_FORMAT = "((short){0})";
        private const string INT64_FORMAT = "{0}L";
        private static readonly string[][] keywords;
        private const GeneratorSupport LanguageSupport = (GeneratorSupport.DeclareIndexerProperties | GeneratorSupport.GenericTypeReference | GeneratorSupport.Resources | GeneratorSupport.Win32Resources | GeneratorSupport.ComplexExpressions | GeneratorSupport.PublicStaticMembers | GeneratorSupport.NestedTypes | GeneratorSupport.ReferenceParameters | GeneratorSupport.ParameterAttributes | GeneratorSupport.AssemblyAttributes | GeneratorSupport.DeclareEvents | GeneratorSupport.DeclareInterfaces | GeneratorSupport.DeclareDelegates | GeneratorSupport.DeclareEnums | GeneratorSupport.DeclareValueTypes | GeneratorSupport.ReturnTypeAttributes | GeneratorSupport.TryCatchStatements | GeneratorSupport.StaticConstructors | GeneratorSupport.MultidimensionalArrays | GeneratorSupport.EntryPointMethod | GeneratorSupport.ArraysOfArrays);
        private const int MaxLineLength = 80;
        private int NestingLevel;
        private static Regex outputReg;
        private static readonly string[][] primitiveTypes;
        private const string SBYTE_FORMAT = "((byte){0})";
        private CodeTypeReference setCodeTypeReference;
        private static readonly string[][] vjsAttributes;

        // Methods
        static VJSharpCodeGenerator()
        {
            string[][] strArray = new string[14][];
            strArray[10] = new string[] { "System.Byte", "System.Char" };
            strArray[11] = new string[] { "System.Int16", "System.Int32", "System.Int64", "System.SByte" };
            strArray[12] = new string[] { "System.Double", "System.Single" };
            strArray[13] = new string[] { "System.Boolean" };
            primitiveTypes = strArray;
            string[][] strArray6 = new string[12][];
            strArray6[1] = new string[] { "do", "if" };
            strArray6[2] = new string[] { "for", "int", "new", "try" };
            strArray6[3] = new string[] { "byte", "case", "char", "else", "enum", "goto", "long", "null", "this", "true", "void" };
            strArray6[4] = new string[] { "break", "catch", "class", "const", "false", "final", "float", "short", "super", "throw", "ubyte", "while" };
            strArray6[5] = new string[] { "double", "import", "native", "public", "return", "static", "string", "struct", "switch", "throws" };
            strArray6[6] = new string[] { "boolean", "default", "extends", "finally", "package", "private" };
            strArray6[7] = new string[] { "abstract", "continue", "delegate", "strictfp", "volatile" };
            strArray6[8] = new string[] { "interface", "multicast", "protected", "transient" };
            strArray6[9] = new string[] { "implements", "instanceof" };
            strArray6[11] = new string[] { "synchronized" };
            keywords = strArray6;
            string[][] strArray17 = new string[11][];
            strArray17[2] = new string[] { "com", "dll" };
            strArray17[5] = new string[] { "hidden", "module" };
            strArray17[7] = new string[] { "security", "assembly" };
            strArray17[8] = new string[] { "attribute" };
            strArray17[10] = new string[] { "conditional" };
            vjsAttributes = strArray17;
        }

        private void AppendEscapedChar(StringBuilder b, char value)
        {
            if (b == null)
            {
                base.Output.Write(@"\u");
                int num = value;
                base.Output.Write(num.ToString("X4", CultureInfo.InvariantCulture));
            }
            else
            {
                b.Append(@"\u");
                b.Append(((int)value).ToString("X4", CultureInfo.InvariantCulture));
            }
        }

        private bool castRequiredForParam(string paramName, ref string paramType)
        {
            if (this.currentMethodParameters != null)
            {
                foreach (CodeParameterDeclarationExpression expression in this.currentMethodParameters)
                {
                    if ((string.Compare(expression.Name, paramName, false, CultureInfo.InvariantCulture) == 0) && IsPrimitiveType(expression.Type))
                    {
                        paramType = expression.Type.BaseType;
                        return true;
                    }
                }
            }
            return false;
        }

        protected override string CmdArgsFromParameters(CompilerParameters options)
        {
            StringBuilder builder = new StringBuilder(0x80);
            if (options.GenerateExecutable)
            {
                builder.Append("/t:exe ");
                if ((options.MainClass != null) && (options.MainClass.Length > 0))
                {
                    builder.Append("/main:");
                    builder.Append(options.MainClass);
                    builder.Append(" ");
                }
            }
            else
            {
                builder.Append("/t:library ");
            }
            builder.Append("/utf8output ");
            foreach (string str in options.ReferencedAssemblies)
            {
                if (!IsFilterableAssembly(str))
                {
                    builder.Append("/r:");
                    builder.Append("\"");
                    builder.Append(str);
                    builder.Append("\"");
                    builder.Append(" ");
                }
            }
            builder.Append("/out:");
            builder.Append("\"");
            builder.Append(options.OutputAssembly);
            builder.Append("\"");
            builder.Append(" ");
            if (options.IncludeDebugInformation)
            {
                builder.Append("/d:DEBUG ");
                builder.Append("/debug ");
            }
            else
            {
                builder.Append("/optimize ");
            }
            if (options.Win32Resource != null)
            {
                builder.Append("/win32res:\"" + options.Win32Resource + "\" ");
            }
            foreach (string str2 in options.EmbeddedResources)
            {
                builder.Append("/res:\"");
                builder.Append(str2);
                builder.Append("\" ");
            }
            foreach (string str3 in options.LinkedResources)
            {
                builder.Append("/linkres:\"");
                builder.Append(str3);
                builder.Append("\" ");
            }
            if (options.TreatWarningsAsErrors)
            {
                builder.Append("/warnaserror ");
            }
            if (options.WarningLevel >= 0)
            {
                builder.Append("/w:" + options.WarningLevel + " ");
            }
            if (options.CompilerOptions != null)
            {
                builder.Append(options.CompilerOptions + " ");
            }
            return builder.ToString();
        }

        protected override string CreateEscapedIdentifier(string name)
        {
            if (IsKeyword(name))
            {
                return ("@" + name);
            }
            return name;
        }

        protected override string CreateValidIdentifier(string name)
        {
            if (IsKeyword(name))
            {
                return ("_" + name);
            }
            return name;
        }

        private void GatherThrowStmts(CodeStatementCollection stms, ArrayList arr)
        {
            foreach (CodeStatement statement in stms)
            {
                if (statement is CodeThrowExceptionStatement)
                {
                    arr.Add(((CodeThrowExceptionStatement)statement).ToThrow);
                    continue;
                }
                if (statement is CodeConditionStatement)
                {
                    this.GatherThrowStmts(((CodeConditionStatement)statement).TrueStatements, arr);
                    this.GatherThrowStmts(((CodeConditionStatement)statement).FalseStatements, arr);
                    continue;
                }
                if (statement is CodeTryCatchFinallyStatement)
                {
                    CodeCatchClauseCollection catchClauses = ((CodeTryCatchFinallyStatement)statement).CatchClauses;
                    if (catchClauses.Count > 0)
                    {
                        foreach (CodeCatchClause clause in catchClauses)
                        {
                            this.GatherThrowStmts(clause.Statements, arr);
                        }
                    }
                    else
                    {
                        this.GatherThrowStmts(((CodeTryCatchFinallyStatement)statement).TryStatements, arr);
                    }
                    this.GatherThrowStmts(((CodeTryCatchFinallyStatement)statement).FinallyStatements, arr);
                    continue;
                }
                if (statement is CodeIterationStatement)
                {
                    this.GatherThrowStmts(((CodeIterationStatement)statement).Statements, arr);
                }
            }
        }

        protected override void GenerateArgumentReferenceExpression(CodeArgumentReferenceExpression e)
        {
            if (this.fInArrayInitializer)
            {
                string paramType = null;
                if (this.castRequiredForParam(e.ParameterName, ref paramType))
                {
                    base.Output.Write("(");
                    base.Output.Write(paramType);
                    base.Output.Write(")");
                }
            }
            this.OutputIdentifier(e.ParameterName);
        }

        protected override void GenerateArrayCreateExpression(CodeArrayCreateExpression e)
        {
            base.Output.Write("new ");
            CodeExpressionCollection initializers = e.Initializers;
            if (initializers.Count > 0)
            {
                this.OutputType(e.CreateType);
                if (e.CreateType.ArrayRank == 0)
                {
                    base.Output.Write("[]");
                }
                this.OutputStartingBrace();
                if (string.Compare(e.CreateType.BaseType, "System.Object", false, CultureInfo.InvariantCulture) == 0)
                {
                    this.fInArrayInitializer = true;
                }
                this.OutputExpressionList(initializers, true);
                if (string.Compare(e.CreateType.BaseType, "System.Object", false, CultureInfo.InvariantCulture) == 0)
                {
                    this.fInArrayInitializer = false;
                }
                this.OutputEndingBrace();
            }
            else
            {
                base.Output.Write(this.GetBaseTypeOutput(e.CreateType));
                base.Output.Write("[");
                if (e.SizeExpression != null)
                {
                    base.GenerateExpression(e.SizeExpression);
                }
                else
                {
                    base.Output.Write(e.Size);
                }
                base.Output.Write("]");
            }
        }

        protected override void GenerateArrayIndexerExpression(CodeArrayIndexerExpression e)
        {
            base.GenerateExpression(e.TargetObject);
            base.Output.Write("[");
            bool flag = true;
            foreach (CodeExpression expression in e.Indices)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    base.Output.Write(", ");
                }
                base.GenerateExpression(expression);
            }
            base.Output.Write("]");
        }

        protected override void GenerateAssignStatement(CodeAssignStatement e)
        {
            if (e.Left is CodeIndexerExpression)
            {
                this.GenerateJavaIndexerReferenceExpression((CodeIndexerExpression)e.Left, "set");
                base.Output.Write("(");
                bool flag = true;
                foreach (CodeExpression expression in ((CodeIndexerExpression)e.Left).Indices)
                {
                    if (flag)
                    {
                        flag = false;
                    }
                    else
                    {
                        base.Output.Write(",");
                    }
                    base.GenerateExpression(expression);
                }
                if (!flag)
                {
                    base.Output.Write(",");
                }
                if (this.fInSetStatement && IsPrimitiveType(this.setCodeTypeReference))
                {
                    base.Output.Write("(" + this.setCodeTypeReference.BaseType + ")");
                }
                base.GenerateExpression(e.Right);
                base.Output.Write(")");
            }
            else if (e.Left is CodePropertyReferenceExpression)
            {
                this.GenerateJavaPropertyReferenceExpression((CodePropertyReferenceExpression)e.Left, "set");
                base.Output.Write("(");
                base.GenerateExpression(e.Right);
                base.Output.Write(")");
            }
            else
            {
                base.GenerateExpression(e.Left);
                base.Output.Write(" = ");
                base.GenerateExpression(e.Right);
            }
            if (!this.generatingForLoop)
            {
                base.Output.WriteLine(";");
            }
        }

        protected override void GenerateAttachEventStatement(CodeAttachEventStatement e)
        {
            this.GenerateJavaEventReferenceExpression(e.Event, "add");
            base.Output.Write("( ");
            base.GenerateExpression(e.Listener);
            base.Output.WriteLine(");");
        }

        protected override void GenerateAttributeDeclarationsEnd(CodeAttributeDeclarationCollection attributes)
        {
            base.Output.Write("*/");
        }

        protected override void GenerateAttributeDeclarationsStart(CodeAttributeDeclarationCollection attributes)
        {
            base.Output.Write("/** ");
        }

        private void GenerateAttributes(CodeAttributeDeclarationCollection attributes)
        {
            this.GenerateAttributes(attributes, null, false);
        }

        private void GenerateAttributes(CodeAttributeDeclarationCollection attributes, string prefix)
        {
            this.GenerateAttributes(attributes, prefix, false);
        }

        private void GenerateAttributes(CodeAttributeDeclarationCollection attributes, string prefix, bool inLine)
        {
            if (attributes.Count != 0)
            {
                IEnumerator enumerator = attributes.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    CodeAttributeDeclaration current = (CodeAttributeDeclaration)enumerator.Current;
                    this.GenerateAttributeDeclarationsStart(attributes);
                    if (prefix != null)
                    {
                        if (string.Compare(prefix, "assembly: ", false, CultureInfo.InvariantCulture) == 0)
                        {
                            base.Output.Write("@assembly ");
                        }
                        else if (string.Compare(prefix, "return: ", false, CultureInfo.InvariantCulture) == 0)
                        {
                            base.Output.Write("@attribute.return ");
                        }
                        else
                        {
                            base.Output.Write("@attribute ");
                        }
                    }
                    else
                    {
                        base.Output.Write("@attribute ");
                    }
                    if (current.AttributeType != null)
                    {
                        base.Output.Write(this.GetTypeOutput(current.AttributeType));
                    }
                    base.Output.Write("(");
                    bool flag = true;
                    foreach (CodeAttributeArgument argument in current.Arguments)
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            base.Output.Write(", ");
                        }
                        this.fIsAttributeArg = true;
                        this.OutputAttributeArgument(argument);
                        this.fIsAttributeArg = false;
                    }
                    base.Output.Write(")");
                    this.GenerateAttributeDeclarationsEnd(attributes);
                    if (inLine)
                    {
                        base.Output.Write(" ");
                    }
                    else
                    {
                        base.Output.WriteLine();
                    }
                }
            }
        }

        protected override void GenerateBaseReferenceExpression(CodeBaseReferenceExpression e)
        {
            base.Output.Write("super");
        }

        protected override void GenerateCastExpression(CodeCastExpression e)
        {
            if (IsPrimitiveType(e.TargetType))
            {
                bool flag = false;
                bool flag2 = true;
                bool flag3 = !HasPrimitiveSubExpressionOfSameType(e);
                if (e.UserData["CastIsBoxing"] is bool)
                {
                    flag = (bool)e.UserData["CastIsBoxing"];
                }
                if (e.UserData["DoubleCast"] is bool)
                {
                    flag2 = (bool)e.UserData["DoubleCast"];
                }
                if (flag)
                {
                    base.Output.Write("((");
                    base.Output.Write(e.TargetType.BaseType);
                    if (flag3 && flag2)
                    {
                        base.Output.Write(")(");
                        this.OutputType(e.TargetType);
                    }
                    base.Output.Write(")(");
                    base.GenerateExpression(e.Expression);
                    base.Output.Write("))");
                }
                else if (flag3)
                {
                    base.Output.Write("((");
                    this.OutputType(e.TargetType);
                    if (flag2)
                    {
                        base.Output.Write(")(");
                        base.Output.Write(e.TargetType.BaseType);
                    }
                    base.Output.Write(")(");
                    base.GenerateExpression(e.Expression);
                    base.Output.Write("))");
                }
                else
                {
                    base.GenerateExpression(e.Expression);
                }
            }
            else
            {
                base.Output.Write("((");
                this.OutputType(e.TargetType);
                base.Output.Write(")(");
                base.GenerateExpression(e.Expression);
                base.Output.Write("))");
            }
        }

        private void GenerateChecksumPragma(CodeChecksumPragma checksumPragma)
        {
            base.Output.Write("#pragma checksum ");
            base.Output.Write(this.QuoteSnippetString(checksumPragma.FileName, false));
            base.Output.Write(" \"");
            base.Output.Write(checksumPragma.ChecksumAlgorithmId.ToString("B", CultureInfo.InvariantCulture));
            base.Output.Write("\" \"");
            if (checksumPragma.ChecksumData != null)
            {
                foreach (byte num in checksumPragma.ChecksumData)
                {
                    base.Output.Write(num.ToString("X2", CultureInfo.InvariantCulture));
                }
            }
            base.Output.WriteLine("\"");
        }

        private void GenerateCodeRegionDirective(CodeRegionDirective regionDirective)
        {
            if (regionDirective.RegionMode == CodeRegionMode.Start)
            {
                base.Output.Write("#region ");
                base.Output.WriteLine(regionDirective.RegionText);
            }
            else if (regionDirective.RegionMode == CodeRegionMode.End)
            {
                base.Output.WriteLine("#endregion");
            }
        }

        protected override void GenerateComment(CodeComment e)
        {
            bool fMultiline = false;
            string escapedComment = this.GetEscapedComment(e.Text, ref fMultiline);
            if (e.DocComment)
            {
                this.GenerateDocComment(escapedComment);
            }
            else if (fMultiline)
            {
                base.Output.Write("/* ");
                base.Output.Write(escapedComment);
                base.Output.WriteLine(" */");
            }
            else
            {
                base.Output.Write("// ");
                base.Output.WriteLine(escapedComment);
            }
        }

        protected override void GenerateCompileUnit(CodeCompileUnit e)
        {
            if ((e.AssemblyCustomAttributes != null) && (e.AssemblyCustomAttributes.Count > 0))
            {
                this.assemblyAttributes = e.AssemblyCustomAttributes;
            }
            this.GenerateCompileUnitStart(e);
            if (((e.Namespaces != null) && (e.Namespaces.Count > 0)) && (e.Namespaces[0] != null))
            {
                this.GenerateNamespace(e.Namespaces[0]);
            }
            else if (this.assemblyAttributes != null)
            {
                this.GenerateAttributes(this.assemblyAttributes, "assembly: ");
                this.assemblyAttributes = null;
            }
            this.GenerateCompileUnitEnd(e);
        }

        protected override void GenerateCompileUnitStart(CodeCompileUnit e)
        {
            base.GenerateCompileUnitStart(e);
            base.Output.WriteLine("/*******************************************************************************");
            base.Output.WriteLine(" *");
            base.Output.Write(" *     ");
            base.Output.WriteLine(ResourceHelper.GetString("AutoGen_Comment_Line2"));
            base.Output.Write(" *     ");
            base.Output.Write(ResourceHelper.GetString("AutoGen_Comment_Line3"));
            base.Output.WriteLine(Environment.Version.ToString());
            base.Output.WriteLine(" *");
            base.Output.Write(" *     ");
            base.Output.WriteLine(ResourceHelper.GetString("AutoGen_Comment_Line4"));
            base.Output.Write(" *     ");
            base.Output.WriteLine(ResourceHelper.GetString("AutoGen_Comment_Line5"));
            base.Output.WriteLine(" *");
            base.Output.WriteLine(" ******************************************************************************/");
            base.Output.WriteLine("");
        }

        protected override void GenerateConditionStatement(CodeConditionStatement e)
        {
            base.Output.Write("if (");
            base.GenerateExpression(e.Condition);
            base.Output.Write(")");
            this.OutputStartingBrace();
            base.GenerateStatements(e.TrueStatements);
            if (e.FalseStatements.Count > 0)
            {
                this.OutputEndingBraceElseStyle();
                base.Output.Write("else");
                this.OutputStartingBrace();
                base.GenerateStatements(e.FalseStatements);
            }
            this.OutputEndingBrace();
        }

        protected override void GenerateConstructor(CodeConstructor e, CodeTypeDeclaration c)
        {
            if (base.IsCurrentClass || base.IsCurrentStruct)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.GenerateAttributes(e.CustomAttributes);
                }
                this.OutputMemberAccessModifier(e.Attributes);
                this.OutputIdentifier(base.CurrentTypeName);
                base.Output.Write("(");
                this.OutputParameters(e.Parameters);
                base.Output.Write(")");
                if (!this.GenerateThrowsClause(e))
                {
                    this.GenerateThrowsClause(e.Statements);
                }
                CodeExpressionCollection baseConstructorArgs = e.BaseConstructorArgs;
                CodeExpressionCollection chainedConstructorArgs = e.ChainedConstructorArgs;
                this.OutputStartingBrace();
                if (baseConstructorArgs.Count > 0)
                {
                    base.Output.Write("super(");
                    this.OutputExpressionList(baseConstructorArgs);
                    base.Output.WriteLine(");");
                }
                else if (chainedConstructorArgs.Count > 0)
                {
                    base.Output.Write("this(");
                    this.OutputExpressionList(chainedConstructorArgs);
                    base.Output.WriteLine(");");
                }
                this.GenerateMethodStatements(e.Statements);
                this.OutputEndingBrace();
            }
        }

        protected override void GenerateDefaultValueExpression(CodeDefaultValueExpression e)
        {
        }

        protected override void GenerateDelegateCreateExpression(CodeDelegateCreateExpression e)
        {
            base.Output.Write("new ");
            this.OutputType(e.DelegateType);
            base.Output.Write("(");
            base.GenerateExpression(e.TargetObject);
            base.Output.Write(".");
            this.OutputIdentifier(e.MethodName);
            base.Output.Write(")");
        }

        protected override void GenerateDelegateInvokeExpression(CodeDelegateInvokeExpression e)
        {
            if (e.TargetObject != null)
            {
                base.GenerateExpression(e.TargetObject);
                base.Output.Write(".");
            }
            base.Output.Write("Invoke(");
            this.OutputExpressionList(e.Parameters);
            base.Output.Write(")");
        }

        protected override void GenerateDirectives(CodeDirectiveCollection directives)
        {
            for (int i = 0; i < directives.Count; i++)
            {
                CodeDirective directive = directives[i];
                if (directive is CodeChecksumPragma)
                {
                    this.GenerateChecksumPragma((CodeChecksumPragma)directive);
                }
                else if (directive is CodeRegionDirective)
                {
                    this.GenerateCodeRegionDirective((CodeRegionDirective)directive);
                }
            }
        }

        private void GenerateDocComment(string docComment)
        {
            if (!"<summary>".Equals(docComment) && !"</summary>".Equals(docComment))
            {
                base.Output.Write("/** ");
                base.Output.Write(docComment);
                base.Output.WriteLine(" */");
            }
        }

        protected override void GenerateDoubleValue(double d)
        {
            if (double.IsNaN(d))
            {
                base.Output.Write("Double.NaN");
            }
            else if (double.IsNegativeInfinity(d))
            {
                base.Output.Write("Double.NEGATIVE_INFINITY");
            }
            else if (double.IsPositiveInfinity(d))
            {
                base.Output.Write("Double.POSITIVE_INFINITY");
            }
            else
            {
                base.Output.Write(d.ToString("R", CultureInfo.InvariantCulture));
            }
        }

        protected override void GenerateEntryPointMethod(CodeEntryPointMethod e, CodeTypeDeclaration c)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.GenerateAttributes(e.CustomAttributes);
            }
            base.Output.Write("public static void main(String[] args)");
            if (!this.GenerateThrowsClause(e))
            {
                this.GenerateThrowsClause(e.Statements);
            }
            this.OutputStartingBrace();
            this.GenerateMethodStatements(e.Statements);
            this.OutputEndingBrace();
        }

        protected override void GenerateEvent(CodeMemberEvent e, CodeTypeDeclaration c)
        {
            if (!base.IsCurrentDelegate && !base.IsCurrentEnum)
            {
                base.Output.Write("private ");
                string name = e.Name;
                string str2 = e.Name;
                this.OutputTypeNamePair(e.Type, str2);
                base.Output.WriteLine(";");
                base.Output.WriteLine("");
                if (e.CustomAttributes.Count > 0)
                {
                    this.GenerateAttributes(e.CustomAttributes);
                }
                base.Output.WriteLine("/** @event */");
                this.OutputMemberAccessModifier(e.Attributes);
                base.Output.Write(" void add_" + name + "(");
                this.OutputTypeNamePair(e.Type, "e");
                base.Output.Write(")");
                this.OutputStartingBrace();
                base.Output.Write("this." + str2 + " = (");
                this.OutputType(e.Type);
                base.Output.WriteLine(")System.Delegate.Combine(this." + str2 + ",e);");
                this.OutputEndingBrace();
                base.Output.WriteLine("");
                base.Output.WriteLine("/** @event */");
                this.OutputMemberAccessModifier(e.Attributes);
                base.Output.Write(" void remove_" + name + "(");
                this.OutputTypeNamePair(e.Type, "e");
                base.Output.Write(")");
                this.OutputStartingBrace();
                base.Output.Write("this." + str2 + " = (");
                this.OutputType(e.Type);
                base.Output.WriteLine(")System.Delegate.Remove(this." + str2 + ",e);");
                this.OutputEndingBrace();
            }
        }

        protected override void GenerateEventReferenceExpression(CodeEventReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                base.GenerateExpression(e.TargetObject);
                base.Output.Write(".");
            }
            this.OutputIdentifier(e.EventName);
        }

        protected override void GenerateExpressionStatement(CodeExpressionStatement e)
        {
            base.GenerateExpression(e.Expression);
            if (!this.generatingForLoop)
            {
                base.Output.WriteLine(";");
            }
        }

        protected override void GenerateField(CodeMemberField e)
        {
            if (!base.IsCurrentDelegate && !base.IsCurrentInterface)
            {
                if (base.IsCurrentEnum)
                {
                    if (e.CustomAttributes.Count > 0)
                    {
                        this.GenerateAttributes(e.CustomAttributes);
                    }
                    this.OutputIdentifier(e.Name);
                    if (e.InitExpression != null)
                    {
                        base.Output.Write("(");
                        base.GenerateExpression(e.InitExpression);
                        base.Output.Write(")");
                    }
                    this.EnumRemainingMemberCount--;
                    if (this.EnumRemainingMemberCount != 0)
                    {
                        base.Output.WriteLine(",");
                    }
                    else
                    {
                        base.Output.WriteLine("");
                    }
                }
                else
                {
                    if (e.CustomAttributes.Count > 0)
                    {
                        this.GenerateAttributes(e.CustomAttributes);
                    }
                    this.OutputMemberAccessModifier(e.Attributes);
                    this.OutputFieldScopeModifier(e.Attributes);
                    this.OutputTypeNamePair(e.Type, e.Name);
                    if (e.InitExpression != null)
                    {
                        base.Output.Write(" = ");
                        base.GenerateExpression(e.InitExpression);
                    }
                    base.Output.WriteLine(";");
                }
            }
        }

        protected override void GenerateFieldReferenceExpression(CodeFieldReferenceExpression e)
        {
            if (this.fInArrayInitializer && (e.TargetObject == null))
            {
                string paramType = null;
                if (this.castRequiredForParam(e.FieldName, ref paramType))
                {
                    base.Output.Write("(");
                    base.Output.Write(paramType);
                    base.Output.Write(")");
                }
            }
            if (e.TargetObject != null)
            {
                base.GenerateExpression(e.TargetObject);
                base.Output.Write(".");
            }
            this.OutputIdentifier(e.FieldName);
        }

        protected override void GenerateGotoStatement(CodeGotoStatement e)
        {
            throw new NotSupportedException("goto");
        }

        protected override void GenerateIndexerExpression(CodeIndexerExpression e)
        {
            this.GenerateJavaIndexerReferenceExpression(e, "get");
            base.Output.Write("(");
            bool flag = true;
            foreach (CodeExpression expression in e.Indices)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    base.Output.Write(",");
                }
                base.GenerateExpression(expression);
            }
            base.Output.Write(")");
        }

        protected override void GenerateIterationStatement(CodeIterationStatement e)
        {
            this.generatingForLoop = true;
            base.Output.Write("for (");
            base.GenerateStatement(e.InitStatement);
            base.Output.Write("; ");
            base.GenerateExpression(e.TestExpression);
            base.Output.Write("; ");
            base.GenerateStatement(e.IncrementStatement);
            base.Output.Write(")");
            this.OutputStartingBrace();
            this.generatingForLoop = false;
            base.GenerateStatements(e.Statements);
            this.OutputEndingBrace();
        }

        private void GenerateJavaEventReferenceExpression(CodeEventReferenceExpression e, string prefix)
        {
            if (e.TargetObject != null)
            {
                base.GenerateExpression(e.TargetObject);
                base.Output.Write(".");
            }
            base.Output.Write(prefix + "_" + e.EventName);
        }

        private void GenerateJavaIndexerReferenceExpression(CodeIndexerExpression e, string prefix)
        {
            if (e.TargetObject != null)
            {
                base.GenerateExpression(e.TargetObject);
                base.Output.Write(".");
            }
            base.Output.Write(prefix + "_Item");
        }

        private void GenerateJavaPropertyReferenceExpression(CodePropertyReferenceExpression e, string prefix)
        {
            if (e.TargetObject != null)
            {
                base.GenerateExpression(e.TargetObject);
                base.Output.Write(".");
            }
            base.Output.Write(prefix + "_" + e.PropertyName);
        }

        protected override void GenerateLabeledStatement(CodeLabeledStatement e)
        {
            base.Indent--;
            base.Output.Write(e.Label);
            base.Output.WriteLine(":");
            base.Indent++;
            if (e.Statement != null)
            {
                base.GenerateStatement(e.Statement);
            }
        }

        protected override void GenerateLinePragmaEnd(CodeLinePragma e)
        {
            base.Output.WriteLine();
            base.Output.WriteLine("#line default");
            base.Output.WriteLine("#line hidden");
        }

        protected override void GenerateLinePragmaStart(CodeLinePragma e)
        {
            base.Output.WriteLine("");
            base.Output.Write("#line ");
            base.Output.Write(e.LineNumber);
            base.Output.Write(" ");
            base.Output.Write(this.QuoteSnippetString(e.FileName, false));
            base.Output.WriteLine("");
        }

        protected override void GenerateMethod(CodeMemberMethod e, CodeTypeDeclaration c)
        {
            if ((base.IsCurrentClass || base.IsCurrentStruct) || base.IsCurrentInterface)
            {
                this.currentMethodParameters = e.Parameters;
                if (e.CustomAttributes.Count > 0)
                {
                    this.GenerateAttributes(e.CustomAttributes);
                }
                if (e.ReturnTypeCustomAttributes.Count > 0)
                {
                    this.GenerateAttributes(e.ReturnTypeCustomAttributes, "return: ");
                }
                if (!base.IsCurrentInterface)
                {
                    this.OutputMemberAccessModifier(e.Attributes);
                    this.OutputMemberScopeModifier(e.Attributes);
                }
                this.OutputType(e.ReturnType);
                base.Output.Write(" ");
                this.OutputIdentifier(e.Name);
                base.Output.Write("(");
                this.OutputParameters(e.Parameters);
                base.Output.Write(")");
                bool flag = this.GenerateThrowsClause(e);
                if (!base.IsCurrentInterface && ((e.Attributes & MemberAttributes.ScopeMask) != MemberAttributes.Abstract))
                {
                    if (!flag)
                    {
                        this.GenerateThrowsClause(e.Statements);
                    }
                    this.OutputStartingBrace();
                    this.GenerateMethodStatements(e.Statements);
                    this.OutputEndingBrace();
                }
                else
                {
                    base.Output.WriteLine(";");
                }
                this.currentMethodParameters = null;
            }
        }

        protected override void GenerateMethodInvokeExpression(CodeMethodInvokeExpression e)
        {
            this.GenerateMethodReferenceExpression(e.Method);
            base.Output.Write("(");
            this.OutputExpressionList(e.Parameters);
            base.Output.Write(")");
        }

        protected override void GenerateMethodReferenceExpression(CodeMethodReferenceExpression e)
        {
            if (e.TargetObject != null)
            {
                if (e.TargetObject is CodeBinaryOperatorExpression)
                {
                    base.Output.Write("(");
                    base.GenerateExpression(e.TargetObject);
                    base.Output.Write(")");
                }
                else
                {
                    this.fInMethodReferenceExpression = true;
                    base.GenerateExpression(e.TargetObject);
                    this.fInMethodReferenceExpression = false;
                }
                base.Output.Write(".");
            }
            this.OutputIdentifier(e.MethodName);
            if (e.TypeArguments.Count > 0)
            {
                base.Output.Write(this.GetTypeArgumentsOutput(e.TypeArguments));
            }
        }

        protected override void GenerateMethodReturnStatement(CodeMethodReturnStatement e)
        {
            base.Output.Write("return");
            if (e.Expression != null)
            {
                base.Output.Write(" ");
                base.GenerateExpression(e.Expression);
            }
            base.Output.WriteLine(";");
        }

        private void GenerateMethodStatements(CodeStatementCollection stms)
        {
            base.GenerateStatements(stms);
        }

        protected override void GenerateNamespace(CodeNamespace e)
        {
            this.GenerateCommentStatements(e.Comments);
            this.GenerateNamespaceStart(e);
            base.GenerateNamespaceImports(e);
            base.Output.WriteLine("");
            if ((this.assemblyAttributes != null) && (this.assemblyAttributes.Count > 0))
            {
                base.Output.WriteLine("");
                this.GenerateAttributes(this.assemblyAttributes, "assembly: ");
                this.assemblyAttributes = null;
            }
            base.GenerateTypes(e);
            this.GenerateNamespaceEnd(e);
        }

        protected override void GenerateNamespaceEnd(CodeNamespace e)
        {
        }

        protected override void GenerateNamespaceImport(CodeNamespaceImport e)
        {
            base.Output.Write("import ");
            this.OutputIdentifier(e.Namespace);
            base.Output.WriteLine(".*;");
        }

        protected override void GenerateNamespaceStart(CodeNamespace e)
        {
            if ((e.Name != null) && (e.Name.Length > 0))
            {
                base.Output.Write("package ");
                this.OutputIdentifier(e.Name);
                base.Output.WriteLine(";");
            }
        }

        protected override void GenerateObjectCreateExpression(CodeObjectCreateExpression e)
        {
            base.Output.Write("new ");
            this.OutputType(e.CreateType);
            base.Output.Write("(");
            this.OutputExpressionList(e.Parameters);
            base.Output.Write(")");
        }

        protected override void GenerateParameterDeclarationExpression(CodeParameterDeclarationExpression e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.GenerateAttributes(e.CustomAttributes, null, true);
            }
            this.OutputDirection(e.Direction);
            this.OutputTypeNamePair(e.Type, e.Name);
        }

        private void GeneratePrimitiveChar(char c)
        {
            base.Output.Write('\'');
            switch (c)
            {
                case '\t':
                    base.Output.Write(@"\t");
                    break;

                case '\n':
                    base.Output.Write(@"\n");
                    break;

                case '\r':
                    base.Output.Write(@"\r");
                    break;

                case '"':
                    base.Output.Write("\\\"");
                    break;

                case '\0':
                    base.Output.Write(@"\0");
                    break;

                case '\'':
                    base.Output.Write(@"\'");
                    break;

                case '\\':
                    base.Output.Write(@"\\");
                    break;

                case '\x0084':
                case '\x0085':
                case '\u2028':
                case '\u2029':
                    this.AppendEscapedChar(null, c);
                    break;

                default:
                    if (char.IsSurrogate(c))
                    {
                        this.AppendEscapedChar(null, c);
                    }
                    else
                    {
                        base.Output.Write(c);
                    }
                    break;
            }
            base.Output.Write('\'');
        }

        protected override void GeneratePrimitiveExpression(CodePrimitiveExpression e)
        {
            if (e.Value is char)
            {
                this.GeneratePrimitiveChar((char)e.Value);
            }
            else if (e.Value is sbyte)
            {
                object[] args = new object[] { ((sbyte)e.Value).ToString(CultureInfo.InvariantCulture) };
                base.Output.Write(string.Format(CultureInfo.InvariantCulture, "((byte){0})", args));
            }
            else if (e.Value is byte)
            {
                object[] objArray2 = new object[] { ((byte)e.Value).ToString(CultureInfo.InvariantCulture) };
                base.Output.Write(string.Format(CultureInfo.InvariantCulture, "((ubyte){0})", objArray2));
            }
            else if (e.Value is short)
            {
                object[] objArray3 = new object[] { ((short)e.Value).ToString(CultureInfo.InvariantCulture) };
                base.Output.Write(string.Format(CultureInfo.InvariantCulture, "((short){0})", objArray3));
            }
            else if (e.Value is long)
            {
                object[] objArray4 = new object[] { ((long)e.Value).ToString(CultureInfo.InvariantCulture) };
                base.Output.Write(string.Format(CultureInfo.InvariantCulture, "{0}L", objArray4));
            }
            else
            {
                base.GeneratePrimitiveExpression(e);
            }
        }

        protected override void GenerateProperty(CodeMemberProperty e, CodeTypeDeclaration c)
        {
            if ((base.IsCurrentClass || base.IsCurrentStruct) || base.IsCurrentInterface)
            {
                bool flag = false;
                bool flag2 = base.IsCurrentInterface || ((e.Attributes & MemberAttributes.ScopeMask) == MemberAttributes.Abstract);
                bool flag3 = (e.Parameters.Count > 0) && (string.Compare(e.Name, "Item", true, CultureInfo.InvariantCulture) == 0);
                if (e.HasGet)
                {
                    if (e.CustomAttributes.Count > 0)
                    {
                        this.GenerateAttributes(e.CustomAttributes);
                        flag = true;
                    }
                    base.Output.WriteLine("/** @property */");
                    if (!base.IsCurrentInterface)
                    {
                        this.OutputMemberAccessModifier(e.Attributes);
                        this.OutputMemberScopeModifier(e.Attributes);
                    }
                    this.OutputType(e.Type);
                    base.Output.Write(" ");
                    base.Output.Write("get_" + e.Name);
                    base.Output.Write("(");
                    if (flag3)
                    {
                        this.OutputParameters(e.Parameters);
                    }
                    base.Output.Write(")");
                    this.GenerateThrowsClause(e.GetStatements);
                    if (flag2)
                    {
                        base.Output.WriteLine(";");
                    }
                    else
                    {
                        this.OutputStartingBrace();
                        this.GenerateMethodStatements(e.GetStatements);
                        this.OutputEndingBrace();
                    }
                }
                if (e.HasSet)
                {
                    if ((e.CustomAttributes.Count > 0) && !flag)
                    {
                        this.GenerateAttributes(e.CustomAttributes);
                    }
                    base.Output.WriteLine("/** @property */");
                    if (!base.IsCurrentInterface)
                    {
                        this.OutputMemberAccessModifier(e.Attributes);
                        this.OutputMemberScopeModifier(e.Attributes);
                    }
                    base.Output.Write("void ");
                    base.Output.Write("set_" + e.Name);
                    base.Output.Write("(");
                    if (flag3)
                    {
                        this.OutputParameters(e.Parameters);
                        base.Output.Write(", ");
                    }
                    this.OutputType(e.Type);
                    base.Output.Write(" ");
                    this.GeneratePropertySetValueReferenceExpression(null);
                    base.Output.Write(")");
                    this.GenerateThrowsClause(e.SetStatements);
                    if (flag2)
                    {
                        base.Output.WriteLine(";");
                    }
                    else
                    {
                        this.OutputStartingBrace();
                        this.fInSetStatement = true;
                        this.setCodeTypeReference = e.Type;
                        this.GenerateMethodStatements(e.SetStatements);
                        this.setCodeTypeReference = null;
                        this.fInSetStatement = false;
                        this.OutputEndingBrace();
                    }
                }
            }
        }

        protected override void GeneratePropertyReferenceExpression(CodePropertyReferenceExpression e)
        {
            this.GenerateJavaPropertyReferenceExpression(e, "get");
            base.Output.Write("(");
            base.Output.Write(")");
        }

        protected override void GeneratePropertySetValueReferenceExpression(CodePropertySetValueReferenceExpression e)
        {
            base.Output.Write("value");
        }

        protected override void GenerateRemoveEventStatement(CodeRemoveEventStatement e)
        {
            this.GenerateJavaEventReferenceExpression(e.Event, "remove");
            base.Output.Write("( ");
            base.GenerateExpression(e.Listener);
            base.Output.WriteLine(");");
        }

        protected override void GenerateSingleFloatValue(float s)
        {
            if (float.IsNaN(s))
            {
                base.Output.Write("Float.NaN");
            }
            else if (float.IsNegativeInfinity(s))
            {
                base.Output.Write("Float.NEGATIVE_INFINITY");
            }
            else if (float.IsPositiveInfinity(s))
            {
                base.Output.Write("Float.POSITIVE_INFINITY");
            }
            else
            {
                base.Output.Write(s.ToString(CultureInfo.InvariantCulture));
                base.Output.Write('F');
            }
        }

        protected override void GenerateSnippetExpression(CodeSnippetExpression e)
        {
            base.Output.Write(e.Value);
        }

        protected override void GenerateSnippetMember(CodeSnippetTypeMember e)
        {
            base.Output.Write(e.Text);
        }

        protected override void GenerateSnippetStatement(CodeSnippetStatement e)
        {
            base.Output.WriteLine(e.Value);
        }

        protected override void GenerateThisReferenceExpression(CodeThisReferenceExpression e)
        {
            base.Output.Write("this");
        }

        protected override void GenerateThrowExceptionStatement(CodeThrowExceptionStatement e)
        {
            if (e.ToThrow != null)
            {
                base.Output.Write("throw");
                base.Output.Write(" ");
                base.GenerateExpression(e.ToThrow);
                base.Output.WriteLine(";");
            }
        }

        private bool GenerateThrowsClause(CodeMemberMethod cmm)
        {
            CodeTypeReferenceCollection references = cmm.UserData["throwsCollection"] as CodeTypeReferenceCollection;
            if (references == null)
            {
                return false;
            }
            if (references.Count != 0)
            {
                base.Output.Write(" throws ");
                bool flag = false;
                foreach (CodeTypeReference reference in references)
                {
                    if (flag)
                    {
                        base.Output.Write(", ");
                    }
                    else
                    {
                        flag = true;
                    }
                    this.OutputType(reference);
                }
            }
            return true;
        }

        private void GenerateThrowsClause(CodeStatementCollection stms)
        {
            ArrayList arr = new ArrayList();
            this.GatherThrowStmts(stms, arr);
            if (arr.Count >= 1)
            {
                base.Output.Write(" throws ");
                bool flag = true;
                for (int i = 0; i < arr.Count; i++)
                {
                    CodeExpression expression = (CodeExpression)arr[i];
                    if (expression is CodeObjectCreateExpression)
                    {
                        if (i == 0)
                        {
                            continue;
                        }
                        CodeObjectCreateExpression expression2 = (CodeObjectCreateExpression)expression;
                        CodeObjectCreateExpression expression3 = (CodeObjectCreateExpression)arr[i - 1];
                        if (expression2.CreateType.BaseType == expression3.CreateType.BaseType)
                        {
                            continue;
                        }
                    }
                    flag = false;
                    break;
                }
                if (flag)
                {
                    this.OutputType(((CodeObjectCreateExpression)arr[0]).CreateType);
                }
                else
                {
                    base.Output.Write("System.Exception");
                }
            }
        }

        protected override void GenerateTryCatchFinallyStatement(CodeTryCatchFinallyStatement e)
        {
            base.Output.Write("try");
            this.OutputStartingBrace();
            base.GenerateStatements(e.TryStatements);
            CodeCatchClauseCollection catchClauses = e.CatchClauses;
            if (catchClauses.Count > 0)
            {
                IEnumerator enumerator = catchClauses.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    this.OutputEndingBraceElseStyle();
                    CodeCatchClause current = (CodeCatchClause)enumerator.Current;
                    base.Output.Write("catch (");
                    this.OutputType(current.CatchExceptionType);
                    base.Output.Write(" ");
                    this.OutputIdentifier(current.LocalName);
                    base.Output.Write(")");
                    this.OutputStartingBrace();
                    base.GenerateStatements(current.Statements);
                }
            }
            CodeStatementCollection finallyStatements = e.FinallyStatements;
            if (finallyStatements.Count > 0)
            {
                this.OutputEndingBraceElseStyle();
                base.Output.Write("finally");
                this.OutputStartingBrace();
                base.GenerateStatements(finallyStatements);
            }
            this.OutputEndingBrace();
        }

        protected override void GenerateTypeConstructor(CodeTypeConstructor e)
        {
            if (base.IsCurrentClass || base.IsCurrentStruct)
            {
                if (e.CustomAttributes.Count > 0)
                {
                    this.GenerateAttributes(e.CustomAttributes);
                }
                base.Output.Write("static ");
                this.OutputStartingBrace();
                this.GenerateMethodStatements(e.Statements);
                this.OutputEndingBrace();
            }
        }

        protected override void GenerateTypeEnd(CodeTypeDeclaration e)
        {
            this.NestingLevel--;
            if (!base.IsCurrentDelegate)
            {
                this.OutputEndingBrace();
            }
        }

        protected override void GenerateTypeOfExpression(CodeTypeOfExpression e)
        {
            this.OutputType(e.Type);
            base.Output.Write(".class");
            if (!this.fIsAttributeArg)
            {
                base.Output.Write(".ToType()");
            }
        }

        protected override void GenerateTypeStart(CodeTypeDeclaration e)
        {
            if (e.CustomAttributes.Count > 0)
            {
                this.GenerateAttributes(e.CustomAttributes);
            }
            if (!base.IsCurrentDelegate)
            {
                this.OutputTypeAttributes(e);
                this.OutputIdentifier(e.Name);
                if (base.IsCurrentStruct)
                {
                    base.Output.Write(" extends System.ValueType ");
                }
                else if (base.IsCurrentEnum)
                {
                    this.EnumRemainingMemberCount = e.Members.Count;
                }
                if (!base.IsCurrentEnum)
                {
                    bool flag = true;
                    bool flag2 = false;
                    if (base.IsCurrentStruct || ((e.UserData["hasExtendsClause"] is bool) && !((bool)e.UserData["hasExtendsClause"])))
                    {
                        flag = false;
                        flag2 = true;
                    }
                    foreach (CodeTypeReference reference in e.BaseTypes)
                    {
                        if (flag)
                        {
                            base.Output.Write(" extends ");
                            flag = false;
                            flag2 = true;
                        }
                        else if (flag2)
                        {
                            base.Output.Write(" implements ");
                            flag2 = false;
                        }
                        else
                        {
                            base.Output.Write(", ");
                        }
                        this.OutputType(reference);
                    }
                }
                this.OutputStartingBrace();
            }
            else
            {
                base.Output.WriteLine("/** @delegate */");
                switch ((e.TypeAttributes & TypeAttributes.VisibilityMask))
                {
                    case TypeAttributes.Public:
                        base.Output.Write("public ");
                        break;
                }
                CodeTypeDelegate delegate2 = (CodeTypeDelegate)e;
                base.Output.Write("delegate ");
                this.OutputType(delegate2.ReturnType);
                base.Output.Write(" ");
                this.OutputIdentifier(e.Name);
                base.Output.Write("(");
                this.OutputParameters(delegate2.Parameters);
                base.Output.WriteLine(");");
            }
            this.NestingLevel++;
        }

        protected override void GenerateVariableDeclarationStatement(CodeVariableDeclarationStatement e)
        {
            this.OutputTypeNamePair(e.Type, e.Name);
            if (e.InitExpression != null)
            {
                base.Output.Write(" = ");
                base.GenerateExpression(e.InitExpression);
            }
            if (!this.generatingForLoop)
            {
                base.Output.WriteLine(";");
            }
        }

        protected override void GenerateVariableReferenceExpression(CodeVariableReferenceExpression e)
        {
            this.OutputIdentifier(e.VariableName);
        }

        private string GetBaseTypeOutput(CodeTypeReference typeRef)
        {
            string baseType = typeRef.BaseType;
            if (this.fInMethodReferenceExpression)
            {
                return baseType;
            }
            if (baseType.Length == 0)
            {
                return "void";
            }
            switch (baseType.ToLower(CultureInfo.InvariantCulture))
            {
                case "system.sbyte":
                    return "byte";

                case "system.byte":
                    return "ubyte";

                case "system.int16":
                    return "short";

                case "system.int32":
                    return "int";

                case "system.int64":
                    return "long";

                case "system.string":
                    return "String";

                case "system.object":
                    return "Object";

                case "system.boolean":
                    return "boolean";

                case "system.void":
                    return "void";

                case "system.char":
                    return "char";

                case "system.single":
                    return "float";

                case "system.double":
                    return "double";
            }
            StringBuilder sb = new StringBuilder(baseType.Length + 10);
            string str3 = typeRef.BaseType;
            int startIndex = 0;
            int start = 0;
            for (int i = 0; i < str3.Length; i++)
            {
                switch (str3[i])
                {
                    case '+':
                    case '.':
                        sb.Append(this.CreateEscapedIdentifier(str3.Substring(startIndex, i - startIndex)));
                        sb.Append('.');
                        i++;
                        startIndex = i;
                        break;

                    case '`':
                        {
                            sb.Append(this.CreateEscapedIdentifier(str3.Substring(startIndex, i - startIndex)));
                            i++;
                            int length = 0;
                            while (((i < str3.Length) && (str3[i] >= '0')) && (str3[i] <= '9'))
                            {
                                length = (length * 10) + (str3[i] - '0');
                                i++;
                            }
                            this.GetTypeArgumentsOutput(typeRef.TypeArguments, start, length, sb);
                            start += length;
                            if ((i < str3.Length) && ((str3[i] == '+') || (str3[i] == '.')))
                            {
                                sb.Append('.');
                                i++;
                            }
                            startIndex = i;
                            break;
                        }
                }
            }
            if (startIndex < str3.Length)
            {
                sb.Append(this.CreateEscapedIdentifier(str3.Substring(startIndex)));
            }
            return sb.ToString();
        }

        private string GetEscapedComment(string commentString, ref bool fMultiline)
        {
            StringBuilder b = new StringBuilder(commentString.Length);
            BitArray isEscaped = new BitArray(commentString.Length);
            string str = LexUnicodeEscapeSequence(commentString, isEscaped);
            fMultiline = false;
            for (int i = 0; i < str.Length; i++)
            {
                switch (str[i])
                {
                    case '\u2028':
                    case '\u2029':
                    case '\n':
                    case '\r':
                        fMultiline = true;
                        break;

                    case '@':
                        if ((i < (str.Length - 1)) && IsAttribute(str.Substring(i + 1)))
                        {
                            b.Append("@");
                        }
                        break;

                    case '*':
                        if ((i < (str.Length - 1)) && (str[i + 1] == '/'))
                        {
                            goto Label_00F0;
                        }
                        break;
                }
                if (str[i] != '\0')
                {
                    if (isEscaped.Get(i))
                    {
                        this.AppendEscapedChar(b, str[i]);
                    }
                    else
                    {
                        b.Append(str[i]);
                    }
                }
            }
        Label_00F0:
            return b.ToString();
        }

        protected override string GetResponseFileCmdArgs(CompilerParameters options, string cmdArgs)
        {
            return base.GetResponseFileCmdArgs(options, cmdArgs);
        }

        private string GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments)
        {
            StringBuilder sb = new StringBuilder(0x80);
            this.GetTypeArgumentsOutput(typeArguments, 0, typeArguments.Count, sb);
            return sb.ToString();
        }

        private void GetTypeArgumentsOutput(CodeTypeReferenceCollection typeArguments, int start, int length, StringBuilder sb)
        {
            sb.Append('<');
            bool flag = true;
            for (int i = start; i < (start + length); i++)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    sb.Append(", ");
                }
                if (i < typeArguments.Count)
                {
                    sb.Append(this.GetTypeOutput(typeArguments[i]));
                }
            }
            sb.Append('>');
        }

        protected override string GetTypeOutput(CodeTypeReference typeRef)
        {
            string str = string.Empty;
            CodeTypeReference arrayElementType = typeRef;
            while (arrayElementType.ArrayElementType != null)
            {
                arrayElementType = arrayElementType.ArrayElementType;
            }
            str = str + this.GetBaseTypeOutput(arrayElementType);
            while ((typeRef != null) && (typeRef.ArrayRank > 0))
            {
                char[] chArray = new char[typeRef.ArrayRank + 1];
                chArray[0] = '[';
                chArray[typeRef.ArrayRank] = ']';
                for (int i = 1; i < typeRef.ArrayRank; i++)
                {
                    chArray[i] = ',';
                }
                str = str + new string(chArray);
                typeRef = typeRef.ArrayElementType;
            }
            return str;
        }

        private static bool HasPrimitiveSubExpressionOfSameType(CodeCastExpression e)
        {
            if (e.Expression is CodePrimitiveExpression)
            {
                CodePrimitiveExpression expression = e.Expression as CodePrimitiveExpression;
                if (expression.Value.GetType() == Type.GetType(e.TargetType.BaseType))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsAttribute(string val)
        {
            int length = 0;
            while (length < val.Length)
            {
                if (!char.IsLetterOrDigit(val[length]))
                {
                    break;
                }
                length++;
            }
            if (length == 0)
            {
                return false;
            }
            return FixedStringLookup.Contains(vjsAttributes, val.Substring(0, length), false);
        }

        private static bool IsFilterableAssembly(string assemblyName)
        {
            string str = assemblyName.ToLower(CultureInfo.InvariantCulture);
            if ((!str.EndsWith(@"\system.dll") && !str.EndsWith(@"\mscorlib.dll")) && (!str.EndsWith(@"\vjscor.dll") && !str.EndsWith(@"\vjslibcw.dll")))
            {
                return str.EndsWith(@"\vjslib.dll");
            }
            return true;
        }

        private static bool IsKeyword(string value)
        {
            return FixedStringLookup.Contains(keywords, value, false);
        }

        private static bool IsPrimitiveType(CodeTypeReference type)
        {
            if (type.ArrayRank > 0)
            {
                return false;
            }
            return FixedStringLookup.Contains(primitiveTypes, type.BaseType, false);
        }

        protected override bool IsValidIdentifier(string value)
        {
            if ((value == null) || (value.Length == 0))
            {
                return false;
            }
            if (value[0] != '@')
            {
                if (IsKeyword(value))
                {
                    return false;
                }
            }
            else
            {
                value = value.Substring(1);
                if (!IsKeyword(value))
                {
                    return false;
                }
            }
            return CodeGenerator.IsValidLanguageIndependentIdentifier(value);
        }

        private static string LexUnicodeEscapeSequence(string text, BitArray isEscaped)
        {
            int length = text.Length;
            StringBuilder builder = new StringBuilder(length);
            int startIndex = 0;
            int index = 0;
            bool flag = false;
            while (index != -1)
            {
                index = text.IndexOf(@"\u", startIndex);
                if ((index == -1) || ((length - index) < 6))
                {
                    builder.Append(text.Substring(startIndex));
                    break;
                }
                builder.Append(text.Substring(startIndex, index - startIndex));
                try
                {
                    int num4 = int.Parse(text.Substring(index + 2, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                    builder.Append((char)num4);
                    isEscaped.Set(builder.Length - 1, true);
                    startIndex = index + 6;
                }
                catch (ArgumentOutOfRangeException)
                {
                    flag = true;
                }
                catch (ArgumentNullException)
                {
                    flag = true;
                }
                catch (OverflowException)
                {
                    flag = true;
                }
                if (flag)
                {
                    builder.Append(@"\u");
                    startIndex = index + 2;
                }
            }
            return builder.ToString();
        }

        protected override void OutputDirection(FieldDirection dir)
        {
            switch (dir)
            {
                case FieldDirection.In:
                    break;

                case FieldDirection.Out:
                    base.Output.Write("/** @ref *//** @attribute System.Runtime.InteropServices.Out() */");
                    return;

                case FieldDirection.Ref:
                    base.Output.Write("/** @ref */");
                    break;

                default:
                    return;
            }
        }

        protected virtual void OutputEndingBrace()
        {
            base.Indent--;
            base.Output.WriteLine("}");
        }

        protected virtual void OutputEndingBraceElseStyle()
        {
            base.Indent--;
            if (base.Options.ElseOnClosing)
            {
                base.Output.Write("} ");
            }
            else
            {
                base.Output.WriteLine("}");
            }
        }

        protected override void OutputFieldScopeModifier(MemberAttributes attributes)
        {
            switch ((attributes & MemberAttributes.ScopeMask))
            {
                case MemberAttributes.Static:
                    base.Output.Write("static ");
                    break;

                case MemberAttributes.Override:
                    break;

                case MemberAttributes.Const:
                    base.Output.Write("static final ");
                    return;

                default:
                    return;
            }
        }

        protected override void OutputIdentifier(string ident)
        {
            base.Output.Write(this.CreateEscapedIdentifier(ident));
        }

        protected override void OutputMemberAccessModifier(MemberAttributes attributes)
        {
            MemberAttributes attributes2 = attributes & MemberAttributes.AccessMask;
            if (attributes2 <= MemberAttributes.Family)
            {
                if (((attributes2 != MemberAttributes.Assembly) && (attributes2 != MemberAttributes.FamilyAndAssembly)) && (attributes2 == MemberAttributes.Family))
                {
                    base.Output.Write("protected ");
                }
            }
            else
            {
                switch (attributes2)
                {
                    case MemberAttributes.FamilyOrAssembly:
                        return;

                    case MemberAttributes.Private:
                        base.Output.Write("private ");
                        return;

                    case MemberAttributes.Public:
                        base.Output.Write("public ");
                        return;

                    default:
                        return;
                }
            }
        }

        protected override void OutputMemberScopeModifier(MemberAttributes attributes)
        {
            switch ((attributes & MemberAttributes.ScopeMask))
            {
                case MemberAttributes.Abstract:
                    base.Output.Write("abstract ");
                    return;

                case MemberAttributes.Final:
                    break;

                case MemberAttributes.Static:
                    base.Output.Write("static ");
                    break;

                default:
                    return;
            }
        }

        protected virtual void OutputStartingBrace()
        {
            if (base.Options.BracingStyle == "C")
            {
                base.Output.WriteLine("");
                base.Output.WriteLine("{");
            }
            else
            {
                base.Output.WriteLine(" {");
            }
            base.Indent++;
        }

        protected override void OutputType(CodeTypeReference typeRef)
        {
            base.Output.Write(this.GetTypeOutput(typeRef));
        }

        private void OutputTypeAttributes(CodeTypeDeclaration e)
        {
            if ((e.Attributes & MemberAttributes.New) != ((MemberAttributes)0))
            {
                base.Output.Write("new ");
            }
            TypeAttributes typeAttributes = e.TypeAttributes;
            switch ((typeAttributes & TypeAttributes.VisibilityMask))
            {
                case TypeAttributes.Public:
                case TypeAttributes.NestedPublic:
                    base.Output.Write("public ");
                    break;

                case TypeAttributes.NestedPrivate:
                    base.Output.Write("private ");
                    break;

                case TypeAttributes.NestedFamily:
                    base.Output.Write("protected ");
                    break;

                case TypeAttributes.VisibilityMask:
                    base.Output.Write("protected ");
                    break;
            }
            if ((this.NestingLevel > 0) && (base.IsCurrentClass || base.IsCurrentEnum))
            {
                base.Output.Write("static ");
            }
            if (e.IsStruct)
            {
                base.Output.Write("final class ");
            }
            else if (e.IsEnum)
            {
                base.Output.Write("enum ");
            }
            else
            {
                TypeAttributes attributes3 = typeAttributes & TypeAttributes.Interface;
                if (attributes3 != TypeAttributes.AutoLayout)
                {
                    if (attributes3 != TypeAttributes.Interface)
                    {
                        return;
                    }
                }
                else
                {
                    if ((typeAttributes & TypeAttributes.Sealed) == TypeAttributes.Sealed)
                    {
                        base.Output.Write("final ");
                    }
                    if ((typeAttributes & TypeAttributes.Abstract) == TypeAttributes.Abstract)
                    {
                        base.Output.Write("abstract ");
                    }
                    base.Output.Write("class ");
                    return;
                }
                base.Output.Write("interface ");
            }
        }

        protected override void ProcessCompilerOutputLine(CompilerResults results, string line)
        {
            if (outputReg == null)
            {
                outputReg = new Regex(@"(^([^(]+)(\(([0-9]+),([0-9]+)\))?: )?(error|warning) ([A-Z]+[0-9]+) ?: (.*)");
            }
            Match match = outputReg.Match(line);
            if (match.Success)
            {
                CompilerError error = new CompilerError();
                if (match.Groups[3].Success)
                {
                    error.FileName = match.Groups[2].Value;
                    error.Line = int.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);
                    error.Column = int.Parse(match.Groups[5].Value, CultureInfo.InvariantCulture);
                }
                if (string.Compare(match.Groups[6].Value, "warning", true, CultureInfo.InvariantCulture) == 0)
                {
                    error.IsWarning = true;
                }
                error.ErrorNumber = match.Groups[7].Value;
                error.ErrorText = match.Groups[8].Value;
                results.Errors.Add(error);
            }
        }

        protected override string QuoteSnippetString(string value)
        {
            return this.QuoteSnippetString(value, true);
        }

        private string QuoteSnippetString(string value, bool fMultiLine)
        {
            return this.QuoteSnippetStringCStyle(value, fMultiLine);
        }

        protected string QuoteSnippetStringCStyle(string value, bool fMultiLine)
        {
            StringBuilder b = new StringBuilder(value.Length + 5);
            b.Append("\"");
            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case '\u2028':
                    case '\u2029':
                        this.AppendEscapedChar(b, value[i]);
                        break;

                    case '\\':
                        b.Append(@"\\");
                        break;

                    case '\'':
                        b.Append(@"\'");
                        break;

                    case '\t':
                        b.Append(@"\t");
                        break;

                    case '\n':
                        b.Append(@"\n");
                        break;

                    case '\r':
                        b.Append(@"\r");
                        break;

                    case '"':
                        b.Append("\\\"");
                        break;

                    case '\0':
                        b.Append(@"\0");
                        break;

                    default:
                        b.Append(value[i]);
                        break;
                }
                if ((fMultiLine && (i > 0)) && ((i % 80) == 0))
                {
                    if ((char.IsHighSurrogate(value[i]) && (i < (value.Length - 1))) && char.IsLowSurrogate(value[i + 1]))
                    {
                        b.Append(value[++i]);
                    }
                    b.Append("\" +\r\n");
                    b.Append('"');
                }
            }
            b.Append("\"");
            return b.ToString();
        }

        protected override bool Supports(GeneratorSupport support)
        {
            return ((support & (GeneratorSupport.DeclareIndexerProperties | GeneratorSupport.GenericTypeReference | GeneratorSupport.Resources | GeneratorSupport.Win32Resources | GeneratorSupport.ComplexExpressions | GeneratorSupport.PublicStaticMembers | GeneratorSupport.NestedTypes | GeneratorSupport.ReferenceParameters | GeneratorSupport.ParameterAttributes | GeneratorSupport.AssemblyAttributes | GeneratorSupport.DeclareEvents | GeneratorSupport.DeclareInterfaces | GeneratorSupport.DeclareDelegates | GeneratorSupport.DeclareEnums | GeneratorSupport.DeclareValueTypes | GeneratorSupport.ReturnTypeAttributes | GeneratorSupport.TryCatchStatements | GeneratorSupport.StaticConstructors | GeneratorSupport.MultidimensionalArrays | GeneratorSupport.EntryPointMethod | GeneratorSupport.ArraysOfArrays)) == support);
        }

        // Properties
        protected override string CompilerName
        {
            get
            {
                return "xvjc.exe";
            }
        }

        protected override string FileExtension
        {
            get
            {
                return ".jsl";
            }
        }

        protected override string NullToken
        {
            get
            {
                return "null";
            }
        }
    }

 

}