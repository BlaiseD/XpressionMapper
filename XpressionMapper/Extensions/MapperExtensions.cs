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
        /// Maps an expression given a dictionary of types where the source type is the key and the destination ttype is the value.
        /// </summary>
        /// <typeparam name="TSourceDelegate"></typeparam>
        /// <typeparam name="TDestDelegate"></typeparam>
        /// <param name="expression"></param>
        /// <param name="infoDictionary"></param>
        /// <returns></returns>
        public static Expression<TDestDelegate> MapExpression<TSourceDelegate, TDestDelegate>(this Expression<TSourceDelegate> expression, IDictionary<Type, Type> typeMappings = null)
        {
            if (expression == null)
                return null;

            Dictionary<Type, Type> alltypeMappings = new Dictionary<Type, Type>()
                                                .AddTypeMappingsFromDelegates<TSourceDelegate, TDestDelegate>()
                                                .AddTypeMappingRange(typeMappings);

            XpressionMapperVisitor visitor = new XpressionMapperVisitor(alltypeMappings);
            Expression remappedBody = visitor.Visit(expression.Body);
            if (remappedBody == null)
                throw new InvalidOperationException(Properties.Resources.cantRemapExpression);

            return Expression.Lambda<TDestDelegate>(remappedBody, expression.GetParameterExpressions(visitor.InfoDictionary));
        }

        /// <summary>
        /// Maps an expression to be used as an "Include" given a dictionary of types where the source type is the key and the destination ttype is the value.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <typeparam name="TDestinationResult"></typeparam>
        /// <param name="expression"></param>
        /// <param name="infoDictionary"></param>
        /// <returns></returns>
        public static Expression<TDestDelegate> MapExpressionAsInclude<TSourceDelegate, TDestDelegate>(this Expression<TSourceDelegate> expression, IDictionary<Type, Type> typeMappings = null)
        {
            if (expression == null)
                return null;

            Dictionary<Type, Type> alltypeMappings = new Dictionary<Type, Type>()
                                                .AddTypeMappingsFromDelegates<TSourceDelegate, TDestDelegate>()
                                                .AddTypeMappingRange(typeMappings);

            XpressionMapperVisitor visitor = new MapIncludesVisitor(alltypeMappings);
            Expression remappedBody = visitor.Visit(expression.Body);
            if (remappedBody == null)
                throw new InvalidOperationException(Properties.Resources.cantRemapExpression);

            return Expression.Lambda<TDestDelegate>(remappedBody, expression.GetParameterExpressions(visitor.InfoDictionary));
        }

        /// <summary>
        /// Maps a collection of expressions given a dictionary of types where the source type is the key and the destination ttype is the value.
        /// </summary>
        /// <typeparam name="TSourceDelegate"></typeparam>
        /// <typeparam name="TDestDelagate"></typeparam>
        /// <param name="collection"></param>
        /// <param name="infoDictionary"></param>
        /// <returns></returns>
        public static ICollection<Expression<TDestDelagate>> MapExpressionList<TSourceDelegate, TDestDelagate>(this ICollection<Expression<TSourceDelegate>> collection, IDictionary<Type, Type> typeMappings = null)
        {
            if (collection == null)
                return null;

            return collection.ToList().ConvertAll<Expression<TDestDelagate>>(item => item.MapExpression<TSourceDelegate, TDestDelagate>(typeMappings));
        }

        /// <summary>
        /// Maps a collection of expressions to be used as a "Includes" given a dictionary of types where the source type is the key and the destination ttype is the value.
        /// </summary>
        /// <typeparam name="TSourceDelegate"></typeparam>
        /// <typeparam name="TDestDelagate"></typeparam>
        /// <param name="collection"></param>
        /// <param name="infoDictionary"></param>
        /// <returns></returns>
        public static ICollection<Expression<TDestDelagate>> MapIncludesList<TSourceDelegate, TDestDelagate>(this ICollection<Expression<TSourceDelegate>> collection, IDictionary<Type, Type> typeMappings = null)
        {
            if (collection == null)
                return null;

            return collection.ToList().ConvertAll<Expression<TDestDelagate>>(item => item.MapExpressionAsInclude<TSourceDelegate, TDestDelagate>(typeMappings));
        }

        /// <summary>
        /// Takes a list of parameters from the source lamda expression and returns a list of parameters for the destination lambda expression.
        /// </summary>
        /// <param name="sourceExpressions"></param>
        /// <param name="infoDictionary"></param>
        /// <returns></returns>
        public static List<ParameterExpression> GetDestinationParameterExpressions(this IEnumerable<ParameterExpression> sourceExpressions, Dictionary<ParameterExpression, MapperInfo> infoDictionary)
        {
            return sourceExpressions.ToList().ConvertAll<ParameterExpression>(p => infoDictionary[p].NewParameter);
        }

        /// <summary>
        /// Adds a new source and destination key-value pair to a dictionary of type mappings based on the generic arguments.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDest"></typeparam>
        /// <param name="typeMappings"></param>
        /// <returns></returns>
        public static Dictionary<Type, Type> AddTypeMapping<TSource, TDest>(this Dictionary<Type, Type> typeMappings)
        {
            if (typeMappings == null)
                throw new ArgumentException(Properties.Resources.typeMappingsDictionaryIsNull);

            Type sourceType = typeof(TSource);
            Type destType = typeof(TDest);

            if (!typeMappings.ContainsKey(sourceType) && sourceType != destType)
                typeMappings.Add(sourceType, destType);

            return typeMappings;
        }

        /// <summary>
        /// Adds a new source and destination key-value pair to a dictionary of type mappings based on the arguments.
        /// </summary>
        /// <param name="typeMappings"></param>
        /// <param name="sourceType"></param>
        /// <param name="destType"></param>
        /// <returns></returns>
        public static Dictionary<Type, Type> AddTypeMapping(this Dictionary<Type, Type> typeMappings, Type sourceType, Type destType)
        {
            if (typeMappings == null)
                throw new ArgumentException(Properties.Resources.typeMappingsDictionaryIsNull);

            if (!typeMappings.ContainsKey(sourceType) && sourceType != destType)
                typeMappings.Add(sourceType, destType);

            return typeMappings;
        }

        /// <summary>
        /// Adds a range of new source and destination key-value pairs to an existing dictionary.
        /// </summary>
        /// <param name="typeMappings"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static Dictionary<Type, Type> AddTypeMappingRange(this Dictionary<Type, Type> typeMappings, IDictionary<Type, Type> range)
        {
            if (typeMappings == null)
                throw new ArgumentException(Properties.Resources.typeMappingsDictionaryIsNull);

            if (range == null)
                return typeMappings;

            return range.Aggregate(typeMappings, (dic, next) =>
            {
                if (!dic.ContainsKey(next.Key) && next.Key != next.Value)
                    dic.Add(next.Key, next.Value);

                return dic;
            });
        }

        #region Private Methods
        private static Dictionary<Type, Type> AddTypeMappingsFromDelegates<TSourceDelegate, TDestDelegate>(this Dictionary<Type, Type> typeMappings)
        {
            if (typeMappings == null)
                throw new ArgumentException(Properties.Resources.typeMappingsDictionaryIsNull);

            List<Type> sourceArguments = typeof(TSourceDelegate).GetGenericArguments().ToList();
            List<Type> destArguments = typeof(TDestDelegate).GetGenericArguments().ToList();

            if (sourceArguments.Count != destArguments.Count)
                throw new ArgumentException(Properties.Resources.invalidArgumentCount);

            return sourceArguments.Aggregate(typeMappings, (dic, next) =>
            {
                if (!dic.ContainsKey(next) && next != destArguments[sourceArguments.IndexOf(next)])
                    dic.AddTypeMapping(next, destArguments[sourceArguments.IndexOf(next)]);

                return dic;
            });
        }

        private static List<ParameterExpression> GetParameterExpressions(this LambdaExpression expression, Dictionary<ParameterExpression, MapperInfo> infoDictionary)
        {
            return expression.Parameters.ToList().ConvertAll<ParameterExpression>(p => infoDictionary[p].NewParameter);
        }
        #endregion Private Methods
    }
}
