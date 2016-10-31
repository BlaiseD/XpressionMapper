using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using XpressionMapper.Extensions;

namespace XpressionMapper.ArgumentMappers
{
    internal class QuoteArgumentMapper : ArgumentMapper
    {
        public QuoteArgumentMapper(XpressionMapperVisitor expressionVisitor, Expression argument)
            : base(expressionVisitor, argument)
        {
        }

        public override Expression MappedArgumentExpression
        {
            get
            {
                LambdaExpression lambdaExpression = (LambdaExpression)((UnaryExpression)this.argument).Operand;
                Expression ex = this.ExpressionVisitor.Visit(lambdaExpression.Body);

                LambdaExpression mapped = Expression.Lambda(ex, lambdaExpression.Parameters.GetDestinationParameterExpressions(this.ExpressionVisitor.InfoDictionary));
                return Expression.Quote(mapped);
            }
        }
    }
}
