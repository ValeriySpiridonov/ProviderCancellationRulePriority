using System;

namespace ProviderCancellationRule.Entities.Enums
{
    [Flags]
    public enum CancellationRuleDisplayStatus
    {
        None = 0,
        /// <summary>
        ///   Показывать на 2 и 3 шаге бронирования
        /// </summary>
        OnBookingForm = 1
    }
}