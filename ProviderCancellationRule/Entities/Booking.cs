using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProviderCancellationRule.Entities
{
    class Booking
    {
        public decimal AmountBeforeTax { get; set; }
        public decimal PrepaySum { get; set; }
        public decimal RoomTypeCount { get; set; }

        public override string ToString()
        {
            return $"amountAfterTax: {AmountBeforeTax}, prepaySum: {PrepaySum}, roomTypeCount: {RoomTypeCount}";
        }

        public bool IsEmpty()
        {
            return AmountBeforeTax == 0 && PrepaySum == 0;
        }
    }
}
