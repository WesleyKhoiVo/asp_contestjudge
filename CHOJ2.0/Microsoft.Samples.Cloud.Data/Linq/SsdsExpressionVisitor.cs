namespace Microsoft.Samples.Cloud.Data.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;

    internal enum OperandType : int
    {
        Logic = 0,
        Comparation = 1,
        None = 2
    }

    /// <summary>
    /// Contains the logic to translate from a lambda expression to 
    /// Sql Server Data Services query expression.
    /// </summary>
    public class SsdsExpressionVisitor : ExpressionVisitor
    {
        private Stack<OperandType> stackOperands = new Stack<OperandType>();        

        /// <summary>
        /// Holds an instance to a System.Text.StringBuilder used to built the query expression.
        /// </summary>
        private StringBuilder expressionBuilder = new StringBuilder();

        /// <summary>
        /// Converts the given expression into a SQL Server Data Services
        /// query expression.
        /// </summary>
        /// <param name="expression">The expression to be translated.</param>
        /// <returns>A System.String with the translated expression.</returns>
        public string Translate(Expression expression)
        {
            Expression partialEvaluatedExpression = Evaluator.PartialEvaluate(expression);
            this.expressionBuilder.Append("from e in entities where ");
            this.Visit(partialEvaluatedExpression);
            this.expressionBuilder.Append(" select e");

            return this.expressionBuilder.ToString();
        }
        
        /// <summary>
        /// Analyzes the method call and generates the appropiated stream 
        /// on the expression builder.
        /// </summary>
        /// <param name="m">The method call expression to analyze.</param>
        /// <returns>An System.Linq.Expressions.Expression.</returns>
        protected override Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            if (methodCall.Method.DeclaringType == typeof(Queryable) && methodCall.Method.Name == "Where")
            {
                //this.expressionBuilder.Append("where ");                
                LambdaExpression lambda = (LambdaExpression)StripQuotes(methodCall.Arguments[1]);
                this.Visit(lambda.Body);
                return methodCall;
            }

            if (methodCall.Method.DeclaringType == typeof(Dictionary<string, object>) && methodCall.Method.Name == "get_Item")
            {
                //the dictionary already has the quotes around it...
                this.expressionBuilder.AppendFormat("e[{0}]", methodCall.Arguments[0].ToString());
                this.CheckBoolExpression();                
                return methodCall;
            }

            if (methodCall.Method.Name == "ToString")
            {
                this.Visit(methodCall.Object);
                return methodCall;
            }

            //since string operations using > are invalid
            if (methodCall.Method.Name == "GreaterThan")
            {
                this.stackOperands.Push(OperandType.Comparation);

                this.expressionBuilder.Append("(");
                this.Visit(methodCall.Arguments[0]);
                this.expressionBuilder.Append(" > ");
                this.Visit(methodCall.Arguments[1]);
                this.expressionBuilder.Append(")");

                this.stackOperands.Pop();

                return methodCall;
            }

            //since string operations using < are invalid
            if (methodCall.Method.Name == "LessThan")
            {
                this.stackOperands.Push(OperandType.Comparation);

                this.expressionBuilder.Append("(");
                this.Visit(methodCall.Arguments[0]);
                this.expressionBuilder.Append(" < ");
                this.Visit(methodCall.Arguments[1]);
                this.expressionBuilder.Append(")");

                this.stackOperands.Pop();

                return methodCall;
            }            

            throw new NotSupportedException(String.Format(CultureInfo.InvariantCulture, "No support for '{0}' operation", methodCall.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression unary)
        {
            if (unary.NodeType == ExpressionType.Not)
            {
                this.expressionBuilder.Append("!");
                this.expressionBuilder.Append("(");
                this.Visit(unary.Operand);
                this.expressionBuilder.Append(")");

                return unary;
            }
            else
            {
                return base.VisitUnary(unary);
            }
        }

        /// <summary>
        /// Analyze a BinaryExpression an genereates the appropiated stream 
        /// on the expression builder.
        /// </summary>
        /// <param name="b">The BinaryExpression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            switch (binaryExpression.NodeType)
            {
                case ExpressionType.AndAlso:                                        
                case ExpressionType.OrElse:
                    this.stackOperands.Push(OperandType.Logic);
                    break;
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    this.stackOperands.Push(OperandType.Comparation);
                    break;
                default:
                    throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The operator '{0}' is not supported", binaryExpression.NodeType));
            }

            this.expressionBuilder.Append("(");
            this.Visit(binaryExpression.Left);
            this.AppendOperand(binaryExpression.NodeType);
            this.Visit(binaryExpression.Right);

            this.expressionBuilder.Append(")");

            this.stackOperands.Pop();

            return binaryExpression;
        }

        /// <summary>
        /// Analyze a ConstantExpression an generates the appropiated stream 
        /// on the expression builder.
        /// </summary>
        /// <param name="c">The ConstantExpression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            IQueryable q = constantExpression.Value as IQueryable;

            if (q == null) // Debugger breaks without this check
            {
                if (constantExpression.Value == null)
                {
                    this.expressionBuilder.Append("null");
                }
                else
                {
                    switch (Type.GetTypeCode(constantExpression.Value.GetType()))
                    {
                        case TypeCode.String:
                            this.expressionBuilder.Append("\"");
                            this.expressionBuilder.Append(constantExpression.Value);
                            this.expressionBuilder.Append("\"");
                            break;
                        case TypeCode.Boolean:
                            this.expressionBuilder.Append(constantExpression.Value.ToString().ToLowerInvariant());
                            break;
                        case TypeCode.DateTime:
                            this.expressionBuilder.Append("DateTime(\"");
                            this.expressionBuilder.Append(((DateTime)constantExpression.Value).ToUniversalTime().ToString("u"));
                            this.expressionBuilder.Append("\")");
                            break;
                        default:
                            this.expressionBuilder.Append(constantExpression.Value);
                            break;
                    }
                }
            }

            return constantExpression;
        }

        /// <summary>
        /// Analyze a MemberExpression and generates the appropiated stream
        /// on the command builder.
        /// </summary>
        /// <param name="m">The MemberExpression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected override Expression VisitMemberAccess(MemberExpression member)
        {
            if (member.Expression != null 
                && (member.Expression.NodeType == ExpressionType.Parameter || member.Expression.NodeType == ExpressionType.MemberAccess))
            {
                var template = "e[\"{0}\"]";
                
                //we handle the built-in system attributes slightly differently                
                if (member.Member.Name == "Id" || member.Member.Name == "Kind")
                {
                    template = "e.{0}";
                }
                
                this.expressionBuilder.AppendFormat(template, member.Member.Name);
                this.CheckBoolExpression();
                return member;
            }

            throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "The member '{0}' is not supported", member.Member.Name));
        }

        /// <summary>
        /// Removes the quotes from the given expression.
        /// </summary>
        /// <param name="e">The Expression to be quote stripped.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        private void AppendOperand(ExpressionType operand)
        {
            switch (operand)
            {
                case ExpressionType.AndAlso:
                    this.expressionBuilder.Append(" && ");
                    break;
                case ExpressionType.OrElse:
                    this.expressionBuilder.Append(" || ");
                    break;
                case ExpressionType.Equal:
                    this.expressionBuilder.Append(" == ");
                    break;
                case ExpressionType.NotEqual:
                    this.expressionBuilder.Append(" != ");
                    break;
                case ExpressionType.LessThan:
                    this.expressionBuilder.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    this.expressionBuilder.Append(" <= ");
                    break;
                case ExpressionType.GreaterThan:
                    this.expressionBuilder.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    this.expressionBuilder.Append(" >= ");
                    break;
            }
        }

        private void CheckBoolExpression()
        {            
            if (this.stackOperands.Count > 0)
            {
                OperandType operand = this.stackOperands.Pop();
                if (operand == OperandType.Logic)
                {
                    this.expressionBuilder.Append(" == true ");
                }
                this.stackOperands.Push(operand);
            }
            else
            {
                this.expressionBuilder.Append(" == true ");
            }
        }

    }
}
