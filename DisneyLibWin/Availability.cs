using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisneyLibWin

{
    public class Availability
    {
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
                var offerDate = Offers.First().DateAndTime.Date.ToString("MM-dd-yy");
                var times = string.Join(", ", Offers.OrderBy(x=>x.DateAndTime).Select(x => x.DateAndTime.ToString("h:mmtt")).ToArray());
                offersAsString = $"{offerDate} @ {times}";

                }
                catch (Exception ex)
                {
                    var x = ex;
                    throw;
                }
            }
            return $"{Restaurant.Name} - Offers: {offersAsString}";
        }
    }
}
