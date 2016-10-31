using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace XpressionMapper
{
    public class ParameterExpressionEqualityComparer : IEqualityComparer<ParameterExpression>
    {
        public bool Equals(ParameterExpression x, ParameterExpression y)
        {
            return ParameterExpression.ReferenceEquals(x, y);
        }

        public int GetHashCode(ParameterExpression obj)
        {
            return obj.GetHashCode();
        }
    }
}
