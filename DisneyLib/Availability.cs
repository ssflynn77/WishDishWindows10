using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisneyLib
{
    public class Availability
    {
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
        public List<Offer> Offers { get; set; }

        public override string ToString()
        {
            string offersAsString;
            if (Offers == null || Offers.Count == 0) { offersAsString = "None"; }
            else
            {
                try
                {
                    offersAsString = string.Join(", ", Offers.OrderBy(x => x.DateAndTime).Select(x => x.DateAndTime.ToString("h:mmtt")).ToArray());
                }
                catch (Exception ex)
                {
                    var x = ex;
                    throw;
                }
            }
            return $"{Restaurant.Name} @ {offersAsString}";
        }

        public string Times { get
            {
                string offersAsString;
                if (Offers == null || Offers.Count == 0) { offersAsString = "None"; }
                else
                {
                    try
                    {
                        offersAsString = string.Join(", ", Offers.OrderBy(x => x.DateAndTime).Select(x => x.DateAndTime.ToString("h:mmtt")).ToArray());
                    }
                    catch (Exception ex)
                    {
                        var x = ex;
                        throw;
                    }
                }
                return offersAsString;
            }
        }
    }
}
