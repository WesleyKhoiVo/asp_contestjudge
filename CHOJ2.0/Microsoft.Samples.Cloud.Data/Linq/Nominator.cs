namespace Microsoft.Samples.Cloud.Data.Linq
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    /// <summary>
    /// Performs bottom-up analysis to determine which nodes can possibly
    /// be part of an evaluated sub-tree.
    /// taken from LINQ Series here: http://blogs.msdn.com/mattwar/archive/2007/07/30/linq-building-an-iqueryable-provider-part-i.aspx
    /// </summary>
    internal class Nominator : ExpressionVisitor
    {
        /// <summary>
        /// Holds a reference to the Fun&lt;Expression,Bool&gt; to be evaluated.
        /// </summary>
        private Func<Expression, bool> funcCanBeEvaluated;

        /// <summary>
        /// Holds a reference to the candidate expressions that can be evaluated as sub-tree expressions.
        /// </summary>
        private HashSet<Expression> candidates;

        /// <summary>
        /// Holds a value indicating whether the expression can be evaluated.
        /// </summary>
        private bool cannotBeEvaluated;

        /// <summary>
        /// Creates an instance of Nominator.
        /// </summary>
        /// <param name="funcCanBeEvaluated">The function to be determined if it's evaluable.</param>
        internal Nominator(Func<Expression, bool> funcCanBeEvaluated)
        {
            this.funcCanBeEvaluated = funcCanBeEvaluated;
        }

        /// <summary>
        /// Returns the expressions that can be evaluated a sub-tree expressions.
        /// </summary>
        /// <param name="expression">The expression to extract the candidates from.</param>
        /// <returns>A System.Collection.Generics.HashSet.</returns>
        internal HashSet<Expression> Nominate(Expression expression)
        {
            this.candidates = new HashSet<Expression>();
            this.Visit(expression);
            return this.candidates;
        }

        /// <summary>
        /// Analyze if the given expression can be evaluated.
        /// </summary>
        /// <param name="expression">The expression to analyze.</param>
        /// <returns>A System.Linq.Expressions.Expression.</returns>
        protected override Expression Visit(Expression expression)
        {
            if (expression != null)
            {
                bool saveCannotBeEvaluated = this.cannotBeEvaluated;
                this.cannotBeEvaluated = false;
                base.Visit(expression);
                if (!this.cannotBeEvaluated)
                {
                    if (this.funcCanBeEvaluated(expression))
                    {
                        this.candidates.Add(expression);
                    }
                    else
                    {
                        this.cannotBeEvaluated = true;
                    }
                }

                this.cannotBeEvaluated |= saveCannotBeEvaluated;
            }

            return expression;
        }
    }
}
