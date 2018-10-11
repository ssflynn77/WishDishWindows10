using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisneyLib
{
   public class Offer
    {
        public string TimeString { get; set; }
        public DateTime DateAndTime { get; set; }

        public override string ToString()
        {
            return $"{DateAndTime.ToString()}";
        }
    }
}
