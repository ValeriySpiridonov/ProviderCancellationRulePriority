namespace ProviderCancellationRule.Entities
{
    class SpecialOffer
    {
        public int Id { get; set; }
        public int CancellationRuleId { get; set; }
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }
}
