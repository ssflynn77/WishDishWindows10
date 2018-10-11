using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisneyLib
{
    public partial class RestaurantAvail
    {
        public Restaurant Restaurant { get; set; }
        public List<TimesAvail> TimesAvailabile { get; set; } = new List<TimesAvail>();

        public string InfoText { get; set; }

        public string InfoTitle { get; set; }


        public override string ToString()
        {
            string offersAsString;
            if (TimesAvailabile == null || TimesAvailabile.Count == 0) { offersAsString = "None"; }
            else
            {
                try
                {
                    offersAsString = string.Join(", ", TimesAvailabile.Select(x => x.Time).ToArray());
                }
                catch (Exception ex)
                {
                    var x = ex;
                    throw;
                }
            }
            return $"{Restaurant.Name} @ {offersAsString}";
        }

    }
}
