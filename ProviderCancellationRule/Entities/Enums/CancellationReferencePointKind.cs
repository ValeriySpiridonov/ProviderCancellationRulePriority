namespace ProviderCancellationRule.Entities.Enums
{
    public enum CancellationReferencePointKind : byte
    {
        ProviderArrivalTime = 0,
        ProviderDepartureTime = 1,
        GuestArrivalTime = 2,
        CustomArrivalTime = 3,
        BookingCreationTime = 4
    }
}