using ProviderCancellationRule.Entities.Enums;

namespace ProviderCancellationRule.Entities
{
    public class CancellationRule
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }

        public string Name { get; set; }
        public CancellationRuleDisplayStatus DisplayStatus { get; set; }

        public string CustomText { get; set; }

        public CancellationReferencePointKind ReferencePointKind { get; set; }

        private Time _referencePointTime = Time.Null;

        public Time ReferencePointTime
        {
            get => ( ReferencePointKind == CancellationReferencePointKind.CustomArrivalTime )
                ? _referencePointTime
                : Time.Null;
            set => _referencePointTime = value;
        }

        public CancellationRule()
        {
            DisplayStatus = CancellationRuleDisplayStatus.OnBookingForm;
        }

        public override string ToString()
        {
            return $"id={Id}, name={Name}";
        }
    }
}
