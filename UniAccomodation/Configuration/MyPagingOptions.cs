using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniAccomodation.Configuration
{
    public class MyPagingOptions
    {
        public virtual int PageSize { get; set; } = 2;

        public MyPagingOptions()
        {
            PageSize = 2;
        }
    }
}
