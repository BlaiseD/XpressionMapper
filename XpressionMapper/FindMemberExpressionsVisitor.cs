using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using XpressionMapper.Extensions;

namespace XpressionMapper
{
    internal class FindMemberExpressionsVisitor : ExpressionVisitor
    {
        internal FindMemberExpressionsVisitor(Type parameterType)
        {
            this.parameterType = parameterType;
            this.newParameter = Expression.Parameter(parameterType);
        }

        #region Fields
        private Type parameterType;
        private ParameterExpression newParameter;
        private List<MemberExpression> memberExpressions = new List<MemberExpression>();
        #endregion Fields

        public MemberExpression Result
        {
            get
            {
                const string PERIOD = ".";
                List<string> fullNamesGrouped = memberExpressions.ConvertAll<string>(m => m.GetPropertyFullName())
                    .GroupBy(n => n)
                    .Select(grp => grp.Key)
                    .OrderBy(a => a.Length).ToList();

                string member = fullNamesGrouped.Aggregate(string.Empty, (result, next) =>
                {
                    if (string.IsNullOrEmpty(result) || next.Contains(result))
                        result = next;
                    else throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture,
                        Properties.Resources.includeExpressionTooComplex,
                        string.Concat(this.parameterType.Name, PERIOD, result),
                        string.Concat(this.parameterType.Name, PERIOD, next)));

                    return result;
                });

                return this.newParameter.BuildExpression(this.parameterType, member);
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.NodeType == ExpressionType.Constant)
                return base.VisitMember(node);

            ParameterExpression parameterExpression = node.GetParameterExpression();
            Type sType = parameterExpression == null ? null : parameterExpression.Type;
            if (sType != null && this.parameterType == sType && node.IsMemberExpression())
            {
                if (node.Expression.NodeType == ExpressionType.MemberAccess && (node.Type == typeof(string) 
                                                                                    || node.Type.IsValueType
                                                                                    || (node.Type.IsGenericType 
                                                                                        && node.Type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))
                                                                                        && Nullable.GetUnderlyingType(node.Type).IsValueType)))
                    memberExpressions.Add((MemberExpression)node.Expression);
                else
                    memberExpressions.Add(node);
            }

            return base.VisitMember(node);
        }
    }
}
