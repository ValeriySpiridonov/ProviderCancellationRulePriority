using System;

namespace ProviderCancellationRule
{
    [Serializable]
    public struct Time : IComparable, IComparable<Time>
    {
        public static readonly Time Null;

        private ushort _Value;

        private ushort Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        public int Hours
        {
            get
            {
                if ( IsNull )
                {
                    new ConsoleLogger().Error( "Invalid use of struct Time. Required check IsNull() before calling getters." );
                    // throw TimeNullReferenceException();
                }
                return Value >> 8;
            }
            set
            {
                if ( value < 0 || value > 23 )
                {
                    throw new OverflowException();
                }
                Value = ( ushort )( ( value << 8 ) | ( Value & 0xFF ) );
            }
        }

        public int Minutes
        {
            get
            {
                if ( IsNull )
                {
                    new ConsoleLogger().Error( "Invalid use of struct Time. Required check IsNull() before calling getters." );
                    // throw TimeNullReferenceException();
                }
                return Value & 0xFF;
            }
            set
            {
                if ( value < 0 || value > 59 )
                {
                    throw new OverflowException();
                }
                Value = ( ushort )( ( Value & 0xFF00 ) | value );
            }
        }

        public bool IsNull
        {
            get { return _Value == Null.Value; }
        }

        public bool ShouldSerializeHours()
        {
            return !IsNull;
        }

        public bool ShouldSerializeMinutes()
        {
            return !IsNull;
        }

        private int GetModule( int value, int mod )
        {
            if ( value < 0 )
            {
                return mod - ( mod - value ) % mod;
            }
            else
            {
                return value % mod;
            }
        }

        static Time()
        {
            Null = new Time();
            Null.Value = ushort.MaxValue;
        }

        public Time( int hours, int minutes )
        {
            _Value = 0;
            Hours = hours;
            Minutes = minutes;
        }

        public override string ToString()
        {
            return _Value == Null.Value ? String.Empty : String.Format( "{0:D}:{1:D2}", Hours, Minutes );
        }

        public override int GetHashCode()
        {
            return _Value.GetHashCode();
        }

        public override bool Equals( object obj )
        {
            if ( !( obj is Time ) ) return false;
            return ( Time )obj == this;
        }

        #region operators

        public static Time operator +( Time t1, Time t2 )
        {
            int hours = t1.Hours + t2.Hours;
            int minutes = t1.Minutes + t2.Minutes;
            if ( minutes >= 60 )
            {
                hours += minutes / 60;
                minutes %= 60;
            }

            Time t = new Time();
            t.Hours = hours;
            t.Minutes = minutes;
            return t;
        }

        public static explicit operator UInt16( Time time )
        {
            return time.Value;
        }

        public static explicit operator Time( UInt16 value )
        {
            return value == Null.Value ? Null : new Time( value >> 8, value & 0xFF );
        }

        public static DateTime operator +( Time time, DateTime date )
        {
            return date + time;
        }

        public static DateTime operator +( DateTime date, Time time )
        {
            return time.IsNull ? date : date.AddHours( time.Hours ).AddMinutes( time.Minutes );
        }

        public static bool operator ==( Time t1, Time t2 )
        {
            return t1.Value == t2.Value;
        }

        public static bool operator !=( Time t1, Time t2 )
        {
            return !( t1 == t2 );
        }

        public static bool operator <( Time t1, Time t2 )
        {
            return t1 == Null ? true : ( t1.Value < t2.Value );
        }

        public static bool operator <=( Time t1, Time t2 )
        {
            return t1 == Null ? true : ( t1.Value <= t2.Value );
        }

        public static bool operator >( Time t1, Time t2 )
        {
            return t1 == Null ? true : ( t1.Value > t2.Value );
        }

        public static bool operator >=( Time t1, Time t2 )
        {
            return t1 == Null ? true : ( t1.Value >= t2.Value );
        }

        public static Time Parse( string str )
        {
            Time t = new Time();
            if ( !String.IsNullOrEmpty( str ) )
            {
                string[] parts = str.Split( ':' );
                if ( parts.Length > 0 )
                {
                    t.Hours = Convert.ToInt32( parts[ 0 ] );
                }
                if ( parts.Length > 1 )
                {
                    t.Minutes = Convert.ToInt32( parts[ 1 ] );
                }

            }
            return t;
        }

        public static bool TryParse( string str, out Time time )
        {
            bool success = false;
            time = Time.Null;
            try
            {
                time = Time.Parse( str );
                success = true;
            }
            catch
            { }
            return success;
        }

        public TimeSpan ToTimeSpan()
        {
            return new TimeSpan( Hours, Minutes, 0 );
        }

        #endregion

        #region IComparable Members

        public int CompareTo( object obj )
        {
            if ( obj is Time )
                return CompareTo( ( Time )obj );
            else
                throw new ArgumentException( "Object is not a Time." );
        }

        #endregion

        #region IComparable<Time> Members

        public int CompareTo( Time other )
        {
            if ( this > other )
                return 1;
            if ( this < other )
                return -1;

            return 0;
        }

        #endregion
    }
}
