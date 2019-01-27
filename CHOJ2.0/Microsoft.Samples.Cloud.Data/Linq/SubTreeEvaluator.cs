namespace Microsoft.Samples.Cloud.Data.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Evaluates &amp; replaces sub-trees when first candidate is reached (top-down).
    /// taken from LINQ Series here: http://blogs.msdn.com/mattwar/archive/2007/07/30/linq-building-an-iqueryable-provider-part-i.aspx
    /// </summary>
    internal class SubtreeEvaluator : ExpressionVisitor
    {
        /// <summary>
        /// Holds a reference to the available candidates.
        /// </summary>
        private HashSet<Expression> candidates;

        /// <summary>
        /// Creates an instance of SubtreeEvaluator.
        /// </summary>
        /// <param name="candidates">The candidate expressions to be evaluated.</param>
        internal SubtreeEvaluator(HashSet<Expression> candidates)
        {
            this.candidates = candidates;
        }

        /// <summary>
        /// Evaluates the given expression.
        /// </summary>
        /// <param name="exp">The expression to evaluate.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        internal Expression Eval(Expression exp)
        {
            return this.Visit(exp);
        }

        /// <summary>
        /// Evaluates the given expression.
        /// </summary>
        /// <param name="exp">The expression to evaluate.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected override Expression Visit(Expression exp)
        {
            if (exp == null)
                return null;

            if (this.candidates.Contains(exp))
                return Evaluate(exp);

            return base.Visit(exp);
        }

        /// <summary>
        /// Evaluates the given expression by compiling the lambda expression and executing it.
        /// </summary>
        /// <param name="e">The expression to evaluate.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        private static Expression Evaluate(Expression e)
        {
            if (e.NodeType == ExpressionType.Constant)
                return e;

            LambdaExpression lambda = Expression.Lambda(e);
            Delegate fn = lambda.Compile();
            return Expression.Constant(fn.DynamicInvoke(null), e.Type);
        }
    }
}
