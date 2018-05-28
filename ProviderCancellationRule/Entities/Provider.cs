using System;

namespace ProviderCancellationRule.Entities
{
    public class Provider
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Time ArrivalTime { get; set; }
        public Time DepartureTime { get; set; }
        private readonly string _tzid;

        public Provider(string tzid)
        {
            _tzid = tzid;
        }

        public TimeZoneInfo TimeZoneInfo => TimeZoneInfo.FindSystemTimeZoneById(_tzid ?? TimeZoneInfo.Local.ToString());

        public override string ToString()
        {
            return $"id={Id}, name={Name}";
        }
    }
}
