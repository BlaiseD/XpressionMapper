﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using XpressionMapper.Extensions;

namespace XpressionMapper.ArgumentMappers
{
    internal class LambdaArgumentMapper : ArgumentMapper
    {
        public LambdaArgumentMapper(XpressionMapperVisitor expressionVisitor, Expression argument)
            : base(expressionVisitor, argument)
        {
        }

        public override Expression MappedArgumentExpression
        {
            get
            {
                LambdaExpression lambdaExpression = (LambdaExpression)this.argument;
                Expression ex = this.ExpressionVisitor.Visit(lambdaExpression.Body);

                LambdaExpression mapped = Expression.Lambda(ex, lambdaExpression.Parameters.GetDestinationParameterExpressions(this.ExpressionVisitor.InfoDictionary));
                return mapped;
            }
        }
    }
}
