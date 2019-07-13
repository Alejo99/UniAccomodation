using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UniAccomodation.Models;

namespace UniAccomodation.Data
{
    public class EFAdvertRepository : IAdvertRepository
    {
        private UniAccomodationDbContext context;

        public EFAdvertRepository(UniAccomodationDbContext dbContext)
        {
            context = dbContext;
        }

        public DbSet<Advert> Adverts => context.Adverts;

        public void SaveAdvert(Advert advert)
        {
            if (advert.Id == 0)
            {
                context.Adverts.Add(advert);
            }
            else
            {
                Advert dbEntry = context.Adverts.FirstOrDefault(a => a.Id == advert.Id);
                if (dbEntry != null)
                {
                    dbEntry.Title = advert.Title;
                    dbEntry.Description = advert.Description;
                    dbEntry.MonthlyPrice = advert.MonthlyPrice;
                    dbEntry.PhotoUrl = advert.PhotoUrl;
                    dbEntry.Status = advert.Status;
                    dbEntry.Comments = advert.Comments;
                }
            }
            context.SaveChanges();
        }
    }
}
