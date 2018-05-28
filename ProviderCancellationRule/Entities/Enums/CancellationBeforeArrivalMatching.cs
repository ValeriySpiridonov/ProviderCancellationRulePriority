namespace ProviderCancellationRule.Entities.Enums
{
    public enum CancellationBeforeArrivalMatching
    {
        NoMatter = 0,
        AtLeast = 1,
        //[Obsolete( "Удалено за ненадобностью в рамках задачи TRAVELLINE-8525" )]
        //Equals = 2,
        NoMoreThan = 3,
        Between = 4
    }
}