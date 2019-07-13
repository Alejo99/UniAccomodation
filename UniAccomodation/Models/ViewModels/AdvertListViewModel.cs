using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniAccomodation.Models.ViewModels
{
    public class AdvertListViewModel
    {
        public IEnumerable<Advert> Adverts { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public AdvertStatus? Status { get; set; }
    }
}
