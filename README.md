<!DOCTYPE html><html><head><meta charset="utf-8"><title>Read_Me.md</title><style></style></head><body>
<h3 id="what-is-xpressionmapper-">What is XpressionMapper?</h3>
<p>XpressionMapper leverages AutoMapper to transform business model expressions into data model expressions.</p>
<h1 id="how-it-works-">How it works?</h1>
<p>The service layer uses the business model classes and has no knowledge of the data model (POCOs) or the EF (Entity Framework) layer</p>
<pre><code>public class PersonService : IPersonService
{
    private IPersonRepository repository;

    public ICollection&lt;PersonModel&gt; GetList(Expression&lt;Func&lt;PersonModel, bool&gt;&gt; filter = null, Expression&lt;Func&lt;IQueryable&lt;PersonModel&gt;, IQueryable&lt;PersonModel&gt;&gt;&gt; orderBy = null, ICollection&lt;Expression&lt;Func&lt;PersonModel, object&gt;&gt;&gt; includeProperties = null)
    {
        ICollection&lt;PersonModel&gt; list = repository.GetList(filter, orderBy, includeProperties);
        return list.ToList();
    }
}
</code></pre><p>The repository layer uses the business model and data model (POCOs)classes and has no knowledge of the EF layer.
    public class PersonRepository : IPersonRepository
    {
        private IPersonStore store;</p>
<pre><code>    public ICollection&lt;PersonModel&gt; GetList(Expression&lt;Func&lt;PersonModel, bool&gt;&gt; filter = null, Expression&lt;Func&lt;IQueryable&lt;PersonModel&gt;, IQueryable&lt;PersonModel&gt;&gt;&gt; orderBy = null, ICollection&lt;Expression&lt;Func&lt;PersonModel, object&gt;&gt;&gt; includeProperties = null)
    {
        Expression&lt;Func&lt;Person, bool&gt;&gt; f = filter.MapExpression&lt;PersonModel, Person, bool&gt;();
        Expression&lt;Func&lt;IQueryable&lt;Person&gt;, IQueryable&lt;Person&gt;&gt;&gt; mappedOrderBy = orderBy.MapOrderByExpression&lt;PersonModel, Person&gt;();
        ICollection&lt;Expression&lt;Func&lt;Person, object&gt;&gt;&gt; includes = includeProperties.MapExpressionList&lt;PersonModel, Person, object&gt;();

        ICollection&lt;Person&gt; list = store.Get(f, mappedOrderBy == null ? null : mappedOrderBy.Compile(), includes);
        return Mapper.Map&lt;IEnumerable&lt;Person&gt;, IEnumerable&lt;PersonModel&gt;&gt;(list).ToList();
    }
}
</code></pre><p>The EF layer uses the data model (POCOs) and has no knowledge of the business model.</p>
<pre><code>public class PersonStore : IPersonStore
{
    public IList&lt;Person&gt; Get(Expression&lt;Func&lt;Person, bool&gt;&gt; filter = null, Func&lt;IQueryable&lt;Person&gt;, IQueryable&lt;Person&gt;&gt; orderBy = null, ICollection&lt;Expression&lt;Func&lt;Person, object&gt;&gt;&gt; includeProperties = null)
    {
        IList&lt;Person&gt; list = null;
        using (IPersonUnitOfWork unitOfWork = new PersonUnitOfWork())
        {
            list = new PersonDbMapper(unitOfWork).Get(filter, orderBy, includeProperties);
        }

        return list;
    }
}
</code></pre>
</body></html>
