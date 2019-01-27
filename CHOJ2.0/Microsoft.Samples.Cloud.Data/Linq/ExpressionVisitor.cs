namespace Microsoft.Samples.Cloud.Data.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq.Expressions;

    /// <summary>
    /// Base for the lambda expression visitor pattern implementation.
    /// taken from LINQ Series here: http://blogs.msdn.com/mattwar/archive/2007/07/30/linq-building-an-iqueryable-provider-part-i.aspx
    /// </summary>
    public abstract class ExpressionVisitor
    {
        /// <summary>
        /// Creates an instance of ExpressionVisitor.
        /// </summary>
        protected ExpressionVisitor()
        {
        }

        /// <summary>
        /// Analyzes the expression and returns it converted to the 
        /// appropiated type.
        /// </summary>
        /// <param name="exp">The expression to be analyzed.</param>
        /// <returns>A System.Linq.Expressions.Expression of the real expression type.</returns>
        protected virtual Expression Visit(Expression exp)
        {
            if (exp == null)
                return exp;
            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return this.VisitUnary((UnaryExpression)exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return this.VisitBinary((BinaryExpression)exp);
                case ExpressionType.TypeIs:
                    return this.VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    return this.VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return this.VisitConstant((ConstantExpression)exp);
                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return this.VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return this.VisitNewExpression((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return this.VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                    return this.VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                    return this.VisitListInit((ListInitExpression)exp);
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        /// <summary>
        /// Analyzes the member binding provided as parameter and calls
        /// the appropiated visitor according to the definition type.
        /// </summary>
        /// <param name="binding">The binding to analyze.</param>
        /// <returns>A System.Linq.Expressions.MemberBinding.</returns>
        protected virtual MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return this.VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return this.VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return this.VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        /// <summary>
        /// Analyzes the element initializer provided as parameter and calls
        /// the appropiated visitor according to the definition type.
        /// </summary>
        /// <param name="initializer">The initializer to analyze.</param>
        /// <returns>A System.Linq.Expressions.ElementInit.</returns>
        protected virtual ElementInit VisitElementInitializer(ElementInit initializer)
        {
            ReadOnlyCollection<Expression> arguments = this.VisitExpressionList(initializer.Arguments);
            if (arguments != initializer.Arguments)
            {
                return Expression.ElementInit(initializer.AddMethod, arguments);
            }
            return initializer;
        }

        /// <summary>
        /// Analyzes the unary expression provided as parameter and
        /// returns an appropiated unariy expression.
        /// </summary>
        /// <param name="unary">The unary expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitUnary(UnaryExpression unary)
        {
            Expression operand = this.Visit(unary.Operand);
            if (operand != unary.Operand)
            {
                return Expression.MakeUnary(unary.NodeType, operand, unary.Type, unary.Method);
            }
            return unary;
        }

        /// <summary>
        /// Analyzes the binary expression provided as parameter and
        /// returns an appropiated binary expression.
        /// </summary>
        /// <param name="binaryExpression">The binary expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitBinary(BinaryExpression binaryExpression)
        {
            Expression left = this.Visit(binaryExpression.Left);
            Expression right = this.Visit(binaryExpression.Right);
            Expression conversion = this.Visit(binaryExpression.Conversion);
            if (left != binaryExpression.Left || right != binaryExpression.Right || conversion != binaryExpression.Conversion)
            {
                if (binaryExpression.NodeType == ExpressionType.Coalesce && binaryExpression.Conversion != null)
                {
                    return Expression.Coalesce(left, right, conversion as LambdaExpression);
                }
                else
                {
                    return Expression.MakeBinary(binaryExpression.NodeType, left, right, binaryExpression.IsLiftedToNull, binaryExpression.Method);
                }
            }

            return binaryExpression;
        }

        /// <summary>
        /// Analyzes the type binary expression provided as parameter and
        /// returns an appropiated type binary expression.
        /// </summary>
        /// <param name="typeBinary">The type binary expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitTypeIs(TypeBinaryExpression typeBinary)
        {
            Expression expr = this.Visit(typeBinary.Expression);
            if (expr != typeBinary.Expression)
            {
                return Expression.TypeIs(expr, typeBinary.TypeOperand);
            }

            return typeBinary;
        }

        /// <summary>
        /// Analyzes the constant expression provided as parameter and
        /// returns an appropiated constant expression.
        /// </summary>
        /// <param name="constantExpression">The constant expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitConstant(ConstantExpression constantExpression)
        {
            return constantExpression;
        }

        /// <summary>
        /// Analyzes the conditional expression provided as parameter and
        /// returns an appropiated binary expression.
        /// </summary>
        /// <param name="conditionalExpression">The conditional expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            Expression test = this.Visit(conditionalExpression.Test);
            Expression testTrue = this.Visit(conditionalExpression.IfTrue);
            Expression testFalse = this.Visit(conditionalExpression.IfFalse);
            if (test != conditionalExpression.Test || testTrue != conditionalExpression.IfTrue || testFalse != conditionalExpression.IfFalse)
            {
                return Expression.Condition(test, testTrue, testFalse);
            }

            return conditionalExpression;
        }

        /// <summary>
        /// Analyzes the parameter expression provided as parameter and
        /// returns an appropiated parameter expression.
        /// </summary>
        /// <param name="parameter">The parameter expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitParameter(ParameterExpression parameter)
        {
            return parameter;
        }

        /// <summary>
        /// Analyzes the member access expression provided as parameter and
        /// returns an appropiated member access.
        /// </summary>
        /// <param name="member">The member access to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitMemberAccess(MemberExpression member)
        {
            Expression exp = this.Visit(member.Expression);
            if (exp != member.Expression)
            {
                return Expression.MakeMemberAccess(exp, member.Member);
            }

            return member;
        }

        /// <summary>
        /// Analyzes the method call expression provided as parameter and
        /// returns an appropiated member access.
        /// </summary>
        /// <param name="methodCall">The method call to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitMethodCall(MethodCallExpression methodCall)
        {
            Expression obj = this.Visit(methodCall.Object);
            IEnumerable<Expression> args = this.VisitExpressionList(methodCall.Arguments);
            if (obj != methodCall.Object || args != methodCall.Arguments)
            {
                return Expression.Call(obj, methodCall.Method, args);
            }

            return methodCall;
        }

        /// <summary>
        /// Analyzes many expressions provided as parameter and returns 
        /// the collection of the analyzed expressions.
        /// </summary>
        /// <param name="original">The expressions to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
        {
            List<Expression> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                Expression p = this.Visit(original[i]);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != original[i])
                {
                    list = new List<Expression>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(p);
                }
            }

            if (list != null)
            {
                return list.AsReadOnly();
            }

            return original;
        }

        /// <summary>
        /// Analyzes member assignment expression provided as parameter and returns 
        /// the analyzed expression.
        /// </summary>
        /// <param name="assignment">The expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual MemberAssignment VisitMemberAssignment(MemberAssignment assignment)
        {
            Expression e = this.Visit(assignment.Expression);
            if (e != assignment.Expression)
            {
                return Expression.Bind(assignment.Member, e);
            }

            return assignment;
        }

        /// <summary>
        /// Analyzes member member binding expression provided as parameter and returns 
        /// the analyzed expression.
        /// </summary>
        /// <param name="binding">The expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            IEnumerable<MemberBinding> bindings = this.VisitBindingList(binding.Bindings);
            if (bindings != binding.Bindings)
            {
                return Expression.MemberBind(binding.Member, bindings);
            }

            return binding;
        }

        /// <summary>
        /// Analyzes member binding expressions provided as parameter and returns 
        /// the analyzed expressions.
        /// </summary>
        /// <param name="binding">The expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual MemberListBinding VisitMemberListBinding(MemberListBinding binding)
        {
            IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(binding.Initializers);
            if (initializers != binding.Initializers)
            {
                return Expression.ListBind(binding.Member, initializers);
            }

            return binding;
        }

        /// <summary>
        /// Analyzes the binding expression list provided as parameter and returns 
        /// the analyzed expressions.
        /// </summary>
        /// <param name="original">The expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual IEnumerable<MemberBinding> VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                MemberBinding b = this.VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }

            if (list != null)
            {
                return list;
            }

            return original;
        }

        /// <summary>
        /// Analyzes element initializer list expression provided as parameter and returns 
        /// the analyzed expression.
        /// </summary>
        /// <param name="original">The expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual IEnumerable<ElementInit> VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                ElementInit init = this.VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (int j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }

            if (list != null)
            {
                return list;
            }

            return original;
        }

        /// <summary>
        /// Analyzes labda expression expression provided as parameter and returns 
        /// the analyzed expression.
        /// </summary>
        /// <param name="lambda">The expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitLambda(LambdaExpression lambda)
        {
            Expression body = this.Visit(lambda.Body);
            if (body != lambda.Body)
            {
                return Expression.Lambda(lambda.Type, body, lambda.Parameters);
            }

            return lambda;
        }

        /// <summary>
        /// Analyzes new expression provided as parameter and returns 
        /// the analyzed expression.
        /// </summary>
        /// <param name="newExpression">The expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual NewExpression VisitNewExpression(NewExpression newExpression)
        {
            IEnumerable<Expression> args = this.VisitExpressionList(newExpression.Arguments);
            if (args != newExpression.Arguments)
            {
                if (newExpression.Members != null)
                {
                    return Expression.New(newExpression.Constructor, args, newExpression.Members);
                }
                else
                {
                    return Expression.New(newExpression.Constructor, args);
                }
            }

            return newExpression;
        }

        /// <summary>
        /// Analyzes member initialization expression provided as parameter and returns 
        /// the analyzed expression.
        /// </summary>
        /// <param name="init">The expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitMemberInit(MemberInitExpression init)
        {
            NewExpression n = this.VisitNewExpression(init.NewExpression);
            IEnumerable<MemberBinding> bindings = this.VisitBindingList(init.Bindings);
            if (n != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(n, bindings);
            }

            return init;
        }

        /// <summary>
        /// Analyzes list initialization expression provided as parameter and returns 
        /// the analyzed expression.
        /// </summary>
        /// <param name="init">The expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitListInit(ListInitExpression init)
        {
            NewExpression n = this.VisitNewExpression(init.NewExpression);
            IEnumerable<ElementInit> initializers = this.VisitElementInitializerList(init.Initializers);
            if (n != init.NewExpression || initializers != init.Initializers)
            {
                return Expression.ListInit(n, initializers);
            }

            return init;
        }

        /// <summary>
        /// Analyzes new array expression provided as parameter and returns 
        /// the analyzed expression.
        /// </summary>
        /// <param name="na">The expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitNewArray(NewArrayExpression na)
        {
            IEnumerable<Expression> exprs = this.VisitExpressionList(na.Expressions);
            if (exprs != na.Expressions)
            {
                if (na.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(na.Type.GetElementType(), exprs);
                }
                else
                {
                    return Expression.NewArrayBounds(na.Type.GetElementType(), exprs);
                }
            }

            return na;
        }

        /// <summary>
        /// Analyzes invocation expression provided as parameter and returns 
        /// the analyzed expression.
        /// </summary>
        /// <param name="iv">The expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected virtual Expression VisitInvocation(InvocationExpression iv)
        {
            IEnumerable<Expression> args = this.VisitExpressionList(iv.Arguments);
            Expression expr = this.Visit(iv.Expression);
            if (args != iv.Arguments || expr != iv.Expression)
            {
                return Expression.Invoke(expr, args);
            }

            return iv;
        }
    }
}