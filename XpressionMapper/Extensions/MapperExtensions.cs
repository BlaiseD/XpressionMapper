using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using XpressionMapper.Structures;

namespace XpressionMapper.Extensions
{
    public static class MapperExtensions
    {
        /// <summary>
        /// Maps an expression given a source type, destination type and result type.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="expression"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static Expression<Func<TDestination, TResult>> MapExpression<TSource, TDestination, TResult>(this Expression<Func<TSource, TResult>> expression, string parameterName = "item")
            where TSource : class
            where TDestination : class
        {
            if (expression == null)
                return null;

            Dictionary<Type, MapperInfo> infoDictionary = new List<MapperInfo>
            {
                expression.CreateMapperInfo<TSource, TDestination>(parameterName)
            }.ToDictionary(i => i.SourceType);

            XpressionMapperVisitor visitor = new XpressionMapperVisitor(infoDictionary);
            var remappedBody = visitor.Visit(expression.Body);
            if (remappedBody == null)
                throw new InvalidOperationException(Properties.Resources.cantRemapExpression);

            return Expression.Lambda<Func<TDestination, TResult>>(remappedBody, infoDictionary[typeof(TSource)].NewParameter);
        }

        /// <summary>
        /// Maps an expression given a dictionary of MapperInfo items with the source type as the key.
        /// </summary>
        /// <typeparam name="TDestination">Destination parameter type</typeparam>
        /// <typeparam name="TDestinationResult">Result of the destination lambda function</typeparam>
        /// <param name="expression">the expression</param>
        /// <param name="infoDictionary">A dictionary containing the list of paramters to be mapped</param>
        /// <returns></returns>
        public static Expression<Func<TDestination, TDestinationResult>> MapExpression<TSource, TDestination, TDestinationResult>(this LambdaExpression expression, Dictionary<Type, MapperInfo> infoDictionary)
        {
            if (expression == null)
                return null;

            XpressionMapperVisitor visitor = new XpressionMapperVisitor(infoDictionary);
            Expression remappedBody = visitor.Visit(expression.Body);
            if (remappedBody == null)
                throw new InvalidOperationException(Properties.Resources.cantRemapExpression);

            return Expression.Lambda<Func<TDestination, TDestinationResult>>(remappedBody, infoDictionary[typeof(TSource)].NewParameter);
        }

        /// <summary>
        /// Maps a collection of expressions given a source type, destination type and result type.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="collection"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static ICollection<Expression<Func<TDestination, TResult>>> MapExpressionList<TSource, TDestination, TResult>(this ICollection<Expression<Func<TSource, TResult>>> collection, string parameterName = "item")
            where TSource : class
            where TDestination : class
        {
            if (collection == null)
                return null;

            return collection.ToList().ConvertAll<Expression<Func<TDestination, TResult>>>(item => item.MapExpression<TSource, TDestination, TResult>(parameterName));
        }

        /// <summary>
        /// Maps a collection of expressions given a dictionary of MapperInfo items with the source type as the key.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="collection"></param>
        /// <param name="infoDictionary"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static ICollection<Expression<Func<TDestination, TResult>>> MapExpressionList<TSource, TDestination, TResult>(this ICollection<Expression<Func<TSource, TResult>>> collection, Dictionary<Type, MapperInfo> infoDictionary)
            where TDestination : class
        {
            if (collection == null)
                return null;

            return collection.ToList().ConvertAll<Expression<Func<TDestination, TResult>>>(item => item.MapExpression<TSource, TDestination, TResult>(infoDictionary));
        }

        /// <summary>
        /// Initializes a MapperInfo object given the parameter name with the source and destination types as generic arguments.
        /// </summary>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <typeparam name="TDest">Destination type</typeparam>
        /// <param name="expression">The expression</param>
        /// <param name="parameterName">The parameter name.</param>
        /// <returns></returns>
        public static MapperInfo CreateMapperInfo<TSource, TDest>(this LambdaExpression expression, string parameterName)
            where TSource : class
            where TDest : class
        {
            ParameterExpression newParameter = Expression.Parameter(typeof(TDest), parameterName);
            return new MapperInfo(newParameter, typeof(TSource), typeof(TDest));
        }

        /// <summary>
        /// Initializes a MapperInfo object given the parameter name and source and destination types.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="sourceType"></param>
        /// <param name="destType"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public static MapperInfo CreateMapperInfo(this LambdaExpression expression, Type sourceType, Type destType, string parameterName)
        {
            ParameterExpression newParameter = Expression.Parameter(destType, parameterName);
            return new MapperInfo(newParameter, sourceType, destType);
        }

        /// <summary>
        /// Returns a dictionary of mapper info objects for mapping expressions.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="sourceDestTypes">A Collection of tupless where the source type is Items, the destination type is Item2 and the parameter name is Item3.</param>
        /// <returns></returns>
        public static Dictionary<Type, MapperInfo> CreateMapperInfoDictionary(this LambdaExpression expression, ICollection<Tuple<Type, Type, string>> sourceDestTypes)
        {
            return sourceDestTypes.Aggregate(new Dictionary<Type, MapperInfo>(), (dic, next) =>
            {
                dic.Add(next.Item1, expression.CreateMapperInfo(next.Item1, next.Item2, next.Item3));
                return dic;
            });
        }

        /// <summary>
        /// Returns an OrderBy Func for type TDest given an OrderBy Expression of type TSource
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="orderByExpression"></param>
        /// <param name="innerParameterName"></param>
        /// <param name="outerParameterName"></param>
        /// <returns></returns>
        public static Expression<Func<IQueryable<TDest>, IQueryable<TDest>>> MapOrderByExpression<TSource, TDest>(this Expression<Func<IQueryable<TSource>, IQueryable<TSource>>> orderByExpression, string innerParameterName = "p", string outerParameterName = "q")
            where TSource : class
            where TDest : class
        {
            if (orderByExpression == null)
                return null;

            Dictionary<Type, MapperInfo> infoDictionary = new List<MapperInfo>
                {
                    orderByExpression.CreateMapperInfo<IQueryable<TSource>, IQueryable<TDest>>(outerParameterName),//mapping for outer expression must come first
                    orderByExpression.CreateMapperInfo<TSource, TDest>(innerParameterName)
                }.ToDictionary(i => i.SourceType);

            Expression<Func<IQueryable<TDest>, IQueryable<TDest>>> mappedOrderBy = orderByExpression.MapExpression<IQueryable<TSource>, IQueryable<TDest>, IQueryable<TDest>>(infoDictionary);

            return mappedOrderBy;
        }
    }
}
