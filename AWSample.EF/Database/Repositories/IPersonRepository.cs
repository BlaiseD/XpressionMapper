﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AWSample.EF.POCO.Person;

namespace AWSample.EF.Database.Repositories
{
    interface IPersonRepository : IBusinessEntityContactRepository, IDbContext
    {
        GenericRepository<AWSample.EF.POCO.Person.Person> PersonRepository { get; }
    }
}
