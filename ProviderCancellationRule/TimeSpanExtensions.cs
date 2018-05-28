using System;

namespace ProviderCancellationRule
{
    public static class TimeSpanExtensions
    {
        public static Time ToTime( this TimeSpan timeSpan )
        {
            return new Time( timeSpan.Hours, timeSpan.Minutes );
        }
    }
}
