﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using XpressionMapper.Extensions;
using XpressionMapper.Structures;

namespace XpressionMapper
{
    public class MapIncludesVisitor : XpressionMapperVisitor
    {
        public MapIncludesVisitor(Dictionary<Type, Type> typeMappings)
            : base(typeMappings)
        {
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            MemberExpression me;
            switch (node.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:

                    me = ((node != null) ? node.Operand : null) as MemberExpression;
                    ParameterExpression parameterExpression = node.GetParameterExpression();
                    Type sType = parameterExpression == null ? null : parameterExpression.Type;
                    if (sType != null && me.Expression.NodeType == ExpressionType.MemberAccess && (me.Type == typeof(string) || me.Type.IsValueType || (me.Type.IsGenericType
                                                                                                                                    && me.Type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))
                                                                                                                                    && Nullable.GetUnderlyingType(me.Type).IsValueType)))
                    {
                        //ParameterExpression parameter = me.Expression.GetParameter();
                        //string fullName = me.Expression.GetPropertyFullName();
                        //return parameter.BuildExpression(sType, fullName);
                        return this.Visit(me.Expression);
                    }
                    else
                    {
                        return base.VisitUnary(node);
                    }
                default:
                    return base.VisitUnary(node);
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.NodeType == ExpressionType.Constant)
                return base.VisitMember(node);

            string sourcePath = null;

            ParameterExpression parameterExpression = node.GetParameterExpression();
            if (parameterExpression == null)
                return base.VisitMember(node);

            InfoDictionary.Add(parameterExpression, this.TypeMappings);
            Type sType = parameterExpression.Type;
            if (sType != null && InfoDictionary.ContainsKey(parameterExpression) && node.IsMemberExpression())
            {
                sourcePath = node.GetPropertyFullName();
            }
            else
            {
                return base.VisitMember(node);
            }

            List<PropertyMapInfo> propertyMapInfoList = new List<PropertyMapInfo>();
            FindDestinationFullName(sType, InfoDictionary[parameterExpression].DestType, sourcePath, propertyMapInfoList);
            string fullName = null;

            if (propertyMapInfoList[propertyMapInfoList.Count - 1].CustomExpression != null)//CustomExpression takes precedence over DestinationPropertyInfo
            {
                PropertyMapInfo last = propertyMapInfoList[propertyMapInfoList.Count - 1];
                propertyMapInfoList.Remove(last);

                FindMemberExpressionsVisitor v = new FindMemberExpressionsVisitor(last.CustomExpression.Parameters[0].Type/*Parent type of current property*/);
                v.Visit(last.CustomExpression.Body);

                fullName = BuildFullName(propertyMapInfoList);
                PrependParentNameVisitor visitor = new PrependParentNameVisitor(InfoDictionary[parameterExpression].DestType, last.CustomExpression.Parameters[0].Type/*Parent type of current property*/, fullName, InfoDictionary[parameterExpression].NewParameter);
                Expression ex = visitor.Visit(v.Result);
                return ex;
            }
            else
            {
                fullName = BuildFullName(propertyMapInfoList);
                MemberExpression me = InfoDictionary[parameterExpression].NewParameter.BuildExpression(InfoDictionary[parameterExpression].DestType, fullName);
                if (me.Expression.NodeType == ExpressionType.MemberAccess && (me.Type == typeof(string) || me.Type.IsValueType || (me.Type.IsGenericType
                                                                                                                            && me.Type.GetGenericTypeDefinition().Equals(typeof(Nullable<>))
                                                                                                                            && Nullable.GetUnderlyingType(me.Type).IsValueType)))
                {
                    return me.Expression;
                }

                return me;
            }
        }
    }
}
