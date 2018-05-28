using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using ProviderCancellationRule.Entities;
using ProviderCancellationRule.Entities.Enums;

namespace ProviderCancellationRule
{
    class CancellationRulePenalty
    {
        public int CancellationRuleId { get; set; }
        public decimal Penalty { get; set; }
    }

    internal class ProviderCancellationRulePriorityUpdater
    {
        private readonly Provider _provider;
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private List<SpecialOffer> _specialOffers;
        private Booking _avgBooking;

        private const int LastPeriodInMonth = 3;

        public ProviderCancellationRulePriorityUpdater(Provider provider, string connectionString, ILogger logger)
        {
            _provider = provider;
            _connectionString = connectionString;
            _logger = logger;
        }


        public void Execute()
        {
            _avgBooking = GetAvgBooking();
            if (_avgBooking.IsEmpty())
            {
                _logger.Warning( $"provider: {_provider}. No Data");
                return;
            }

            _specialOffers = GetSpecialOffers(_provider.Id);
            _logger.Info($"provider: {_provider}. avg_booking: {_avgBooking}");
            List<CancellationRule> cancellationRules = GetCancellationRules();

            List<CancellationRulePenalty> result = new List<CancellationRulePenalty>();
            int maxCancellationBeforeArrivalValue = GetMaxCancellationBeforeArrivalValue() + 1;
            _logger.Info( $"MaxCancellationBeforeArrivalValue={maxCancellationBeforeArrivalValue}" );
            CancellationRulePenaltyCalculator cancellationRulePenaltyCalculator = new CancellationRulePenaltyCalculator(_avgBooking, maxCancellationBeforeArrivalValue, _specialOffers, _logger, _connectionString );
            foreach ( CancellationRule cancellationRule in cancellationRules)
            {
                _logger.Info( $"cancellation_rule: {cancellationRule}" );
                decimal penalty = cancellationRulePenaltyCalculator.Calculate( cancellationRule);
                result.Add(new CancellationRulePenalty {CancellationRuleId = cancellationRule.Id, Penalty = penalty});
            }

            List<int> sortedCancellationRuleIds = result.OrderByDescending(penalty => penalty.Penalty).Select(penalty => penalty.CancellationRuleId).ToList();
            _logger.Info( string.Empty );
            _logger.Info($"set priority for provider: {_provider}");
            for (int index = 0; index < sortedCancellationRuleIds.Count; index++)
            {
                _logger.Info($"cancellationRule: {cancellationRules.Find(rule => rule.Id == sortedCancellationRuleIds[ index ] )}, penalty={result.Find(cancellationRulePenalty => cancellationRulePenalty.CancellationRuleId == sortedCancellationRuleIds[ index ] ).Penalty}, priority: {index}");
                SetCancellationRulePriority(sortedCancellationRuleIds[index], index);
            }
        }

        private void SetCancellationRulePriority(int cancellationRuleId, int priority)
        {
            //string queryString = $"UPDATE cancellation_rule SET priority={priority} WHERE id_cancellation_rule={cancellationRuleId}";

            //using ( SqlConnection connection = new SqlConnection( _connectionString ) )
            //{
            //    SqlCommand command = new SqlCommand( queryString, connection );
            //    connection.Open();
            //    try
            //    {
            //        command.ExecuteNonQuery();
            //    }
            //    finally
            //    {
            //        connection.Close();
            //    }
            //}
        }

        /// <summary>
        /// Получение максимального CancellationBeforeArrivalValue в разрезе провайдера
        /// Необходимо для вычисление максимального коэфициента
        /// </summary>
        /// <returns></returns>
        private int GetMaxCancellationBeforeArrivalValue()
        {
            int result = 0;
            string queryString = @"select MAX(t.max_value), MAX(t.max_value_max) from cancellation_rule cr
            left join
            ( select
                    id_cancellation_rule,
                MAX((case cancellation_before_arrival_unit
                when 2 then
                cancellation_before_arrival_value

            when 1 then

            cancellation_before_arrival_value * 24
            else 0

            end)) as max_value,

            MAX( (case cancellation_before_arrival_unit

                when 2 then

                cancellation_before_arrival_value_max

            when 1 then

            cancellation_before_arrival_value_max * 24
            else 0

            end )) as max_value_max

                from cancellation_rule_condition group by id_cancellation_rule) t on t.id_cancellation_rule = cr.id_cancellation_rule
            where cr.id_provider = " + _provider.Id;
            using ( SqlConnection connection = new SqlConnection( _connectionString ) )
            {
                SqlCommand command = new SqlCommand( queryString, connection );
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while ( reader.Read() )
                    {
                        result = Math.Max(Convert.ToInt32(reader[0]), Convert.ToInt32( reader[1]));
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            return result;
        }

        private Booking GetAvgBooking()
        {
            Booking booking = null;
            string queryString = "SELECT ISNULL(AVG(b.amount_before_tax),0) amount_before_tax, ISNULL(AVG(b.prepay_sum),0) prepay_sum, ISNULL(AVG(brt.count),0) booking_room_type_count FROM booking b " + // $"SELECT AVG(amount_after_tax/brt.count) FROM booking b " +
                                 "LEFT JOIN (SELECT id_booking, COUNT(*) [count] FROM booking_room_type GROUP BY id_booking ) brt ON b.id_booking = brt.id_booking " +
                                 $"WHERE id_provider={_provider.Id} and creation_date >= DATEADD(month, -{LastPeriodInMonth}, GETDATE())";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while ( reader.Read() )
                    {
                        booking = new Booking()
                        {
                            AmountBeforeTax = Convert.ToDecimal( reader[ "amount_before_tax" ] ),
                            PrepaySum = Convert.ToDecimal( reader[ "prepay_sum" ] ),
                            RoomTypeCount = Convert.ToDecimal( reader[ "booking_room_type_count" ] )
                        };
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            return booking;
        }

        private List<CancellationRule> GetCancellationRules()
        {
            List<CancellationRule> result = new List<CancellationRule>();
            string queryString = $"SELECT * FROM cancellation_rule WHERE id_provider = {_provider.Id};";
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        result.Add(new CancellationRule
                        {
                            Id = Convert.ToInt32(reader["id_cancellation_rule"]),
                            ProviderId = Convert.ToInt32(reader["id_provider"]),
                            CustomText = reader["custom_text"] as string,
                            ReferencePointTime = ( reader[ "reference_point_time" ] is DBNull ) ? Time.Null : ( Time )Convert.ToInt32( reader[ "reference_point_time" ] ),
                            DisplayStatus = (CancellationRuleDisplayStatus) Convert.ToInt32(reader["display_status"]),
                            Name = reader["name"] as string,
                            ReferencePointKind = (CancellationReferencePointKind) Convert.ToByte(reader["reference_point"])
                        });
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            return result;
        }

        private List<SpecialOffer> GetSpecialOffers(int providerId)
        {
            List<SpecialOffer> result = new List<SpecialOffer>();
            string queryString = $"SELECT * FROM [special_offer] WHERE [id_provider] = {providerId}";
            using ( SqlConnection connection = new SqlConnection( _connectionString ) )
            {
                SqlCommand command = new SqlCommand( queryString, connection );
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while ( reader.Read() )
                    {
                        result.Add( new SpecialOffer()
                        {
                            Id = Convert.ToInt32( reader[ "id_special_offer" ] ),
                            Name = Convert.ToString( reader[ "name" ] ),
                            CancellationRuleId = Convert.ToInt32( reader[ "id_cancellation_rule" ] ),
                            IsEnabled = Convert.ToBoolean( reader[ "is_enabled" ] )
                        } );
                    }
                }
                finally
                {
                    reader.Close();
                }
            }

            return result;
        }
    }
}