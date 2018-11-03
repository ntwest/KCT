using Expressive.Expressions;
using System;

namespace Expressive.Functions.Mathematical
{
    internal class LnFunction : FunctionBase
    {
        #region FunctionBase Members

        public override string Name { get { return "Log"; } }

        public override object Evaluate(IExpression[] parameters, ExpressiveOptions options)
        {
            this.ValidateParameterCount(parameters, 1, 1);

            return Math.Log( Convert.ToDouble( parameters[0].Evaluate( Variables ) ) );
        }

        #endregion
    }
}
