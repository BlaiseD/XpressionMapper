﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSample.Domain
{
    public enum EntityStateType
    {
        DontCare,
        Unchanged,
        Added,
        Modified,
        Deleted
    }
}
