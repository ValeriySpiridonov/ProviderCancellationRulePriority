using System;

namespace ProviderCancellationRule
{
    public static class DateTimeUtil
    {
        private const int HoursPerDay = 24;

        public static DateTime AddTime( this DateTime date, Time time )
        {
            TimeSpan ts = time.IsNull ? TimeSpan.Zero : new TimeSpan( time.Hours, time.Minutes, 0 );
            return date.Add( ts );
        }

        public static DateTime AddWorkDays( DateTime date, int workDays )
        {
            while ( date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday )
            {
                date = date.AddDays( 1 );
            }

            while ( workDays > 0 )
            {
                date = date.AddDays( 1 );
                if ( date.DayOfWeek < DayOfWeek.Saturday && date.DayOfWeek > DayOfWeek.Sunday )
                {
                    workDays--;
                }
            }
            return date;
        }

        public static int GetQuarterNumber( DateTime date )
        {
            return ( date.Month - 1 ) / 3 + 1;
        }

        public static DateTime GetQuarterFirstDay( DateTime date )
        {
            int quarterNumber = GetQuarterNumber( date );
            return new DateTime( date.Year, ( quarterNumber - 1 ) * 3 + 1, 1 );
        }

    }
}
