namespace Microsoft.Samples.Cloud.Data.Linq
{
    using System;
    using System.Linq.Expressions;

    /// <summary>
    /// Provides partial evaluation of expressions.
    /// taken from LINQ Series here: http://blogs.msdn.com/mattwar/archive/2007/07/30/linq-building-an-iqueryable-provider-part-i.aspx
    /// </summary>
    public static class Evaluator
    {
        /// <summary>
        /// Performs evaluation &amp; replacement of independent sub-trees.
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <param name="funcCanBeEvaluated">A function that decides whether a given expression node can be part of the local function.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEvaluate(Expression expression, Func<Expression, bool> funcCanBeEvaluated)
        {
            return new SubtreeEvaluator(new Nominator(funcCanBeEvaluated).Nominate(expression)).Eval(expression);
        }

        /// <summary>
        /// Performs evaluation &amp; replacement of independent sub-trees.
        /// </summary>
        /// <param name="expression">The root of the expression tree.</param>
        /// <returns>A new tree with sub-trees evaluated and replaced.</returns>
        public static Expression PartialEvaluate(Expression expression)
        {
            return PartialEvaluate(expression, Evaluator.CanBeEvaluatedLocally);
        }

        /// <summary>
        /// Retuns a value indicating whether the current value can be locally evaluated.
        /// </summary>
        /// <param name="expression">The expresion to evaluate.</param>
        /// <returns>A value indicating whether the current value can be locally evaluated.</returns>
        private static bool CanBeEvaluatedLocally(Expression expression)
        {
            return expression.NodeType != ExpressionType.Parameter;
        }
    }
}
