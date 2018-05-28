using System;

namespace ProviderCancellationRule.Entities
{
    class CancellationRuleConditionPeriod
    {
        public int Id { get; set; }
        public int CancellationRuleConditionId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsEndless { get; set; }
        public bool GetIsActualForDate( DateTime date )
        {
            return ( date.Date >= StartDate ) && ( IsEndless || ( date.Date <= EndDate ) );
        }
    }
}
