using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniAccomodation.Models;

namespace UniAccomodation.Data
{
    public interface IAdvertRepository
    {
        DbSet<Advert> Adverts { get; }
        void SaveAdvert(Advert advert);
    }
}
