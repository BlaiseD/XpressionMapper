# XpressionMapper
<h2>
<a id="user-content-what-is-xmapper" class="anchor" href="#what-is-xmapper" aria-hidden="true"><span class="octicon octicon-link"></span></a>What is XpressionMapper?</h2>

<p>XpressionMapper leverages AutoMapper to transform business model expressions into data model expressions.  Expression mapping provides a couple of advantages:
<ol class="list">
            <li>Improved separation of concerns:  The Entity Framework (or other ORM) layer has no knowledge of the business model.</li>
            <li>Removes the need for projection in the data layer or the need to return an IQueryable from the data layer.</li>
        </ol>
        </p>
<h2>
<a id="user-content-how-it-works" class="anchor" href="#how-it-works" aria-hidden="true"><span class="octicon octicon-link"></span></a>How does it work?</h2>


<p>The service layer uses the business model classes and has no knowledge of the data model (POCOs) or the EF (Entity Framework) layer.
<code>
    public class PersonService : IPersonService
    {

        private IPersonRepository repository;

        public ICollection<PersonModel> GetList(Expression<Func<PersonModel, bool>> filter = null, Expression<Func<IQueryable<PersonModel>, IQueryable<PersonModel>>> orderBy = null, ICollection<Expression<Func<PersonModel, object>>> includeProperties = null)
        {
            ICollection<PersonModel> list = repository.GetList(filter, orderBy, includeProperties);
            return list.ToList();
        }
    }
</code>
</p>

<p>The repository layer uses the business model and data model (POCOs)classes and has no knowledge of the EF layer.
<code>
    public class PersonRepository : IPersonRepository
    {
            private IPersonStore store;

            public ICollection<PersonModel> GetList(Expression<Func<PersonModel, bool>> filter = null, Expression<Func<IQueryable<PersonModel>, IQueryable<PersonModel>>> orderBy = null, ICollection<Expression<Func<PersonModel, object>>> includeProperties = null)        {
            Expression<Func<Person, bool>> f = filter.MapExpression<PersonModel, Person, bool>();
            Expression<Func<IQueryable<Person>, IQueryable<Person>>> mappedOrderBy = orderBy.MapOrderByExpression<PersonModel, Person>();
            ICollection<Expression<Func<Person, object>>> includes = includeProperties.MapExpressionList<PersonModel, Person, object>();

            ICollection<Person> list = store.Get(f, mappedOrderBy == null ? null : mappedOrderBy.Compile(), includes);
            return Mapper.Map<IEnumerable<Person>, IEnumerable<PersonModel>>(list).ToList();
        }
    }
</code>
</p>

<p>The EF layer uses the data model (POCOs) and has no knowledge of the business model.
<code>
    public class PersonStore : IPersonStore
    {
        public IList<Person> Get(Expression<Func<Person, bool>> filter = null, Func<IQueryable<Person>, IQueryable<Person>> orderBy = null, ICollection<Expression<Func<Person, object>>> includeProperties = null)
        {
            IList<Person> list = null;
            using (IPersonUnitOfWork unitOfWork = new PersonUnitOfWork())
            {
                list = new PersonDbMapper(unitOfWork).Get(filter, orderBy, includeProperties);
            }

            return list;
        }
    }
</code>
</p>
