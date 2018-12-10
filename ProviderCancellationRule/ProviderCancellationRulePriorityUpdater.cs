using System;
using System.Collections.Generic;
using System.Configuration;
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
        private Booking _avgBooking;

        public ProviderCancellationRulePriorityUpdater(Provider provider, string connectionString, ILogger logger)
        {
            _provider = provider;
            _connectionString = connectionString;
            _logger = logger;
        }


        public void Execute()
        {
            _logger.Warning( $"{_provider.Id}" );
            _avgBooking = GetAvgBooking();
            if (_avgBooking.IsEmpty())
            {
//                _logger.Warning( $"provider: {_provider}. No Data");
                _avgBooking = new Booking
                {
                    AmountBeforeTax = 19820,
                    PrepaySum = 2644,
                    RoomTypeCount = 1
                };
            }

            _logger.Info($"{_avgBooking}");
            List<CancellationRule> cancellationRules = GetCancellationRules();

            List<CancellationRulePenalty> result = new List<CancellationRulePenalty>();
            int maxCancellationBeforeArrivalValue = GetMaxCancellationBeforeArrivalValue() + 1;
//            _logger.Info( $"MaxCancellationBeforeArrivalValue={maxCancellationBeforeArrivalValue}" );
            CancellationRulePenaltyCalculator cancellationRulePenaltyCalculator = new CancellationRulePenaltyCalculator(
                _avgBooking, 
                maxCancellationBeforeArrivalValue, 
                _logger, 
                _connectionString );
            foreach ( CancellationRule cancellationRule in cancellationRules)
            {
//                _logger.Info( $"cancellation_rule: {cancellationRule}" );
                decimal penalty = cancellationRulePenaltyCalculator.Calculate( cancellationRule);
                result.Add(new CancellationRulePenalty {CancellationRuleId = cancellationRule.Id, Penalty = penalty});
            }

            List<int> sortedCancellationRuleIds = result.OrderByDescending(penalty => penalty.Penalty).Select(penalty => penalty.CancellationRuleId).ToList();
            string rules= String.Empty;
            for (int index = 0; index < sortedCancellationRuleIds.Count; index++)
            {
//                _logger.Info($"cancellationRule: {cancellationRules.Find(rule => rule.Id == sortedCancellationRuleIds[ index ] )}, penalty={result.Find(cancellationRulePenalty => cancellationRulePenalty.CancellationRuleId == sortedCancellationRuleIds[ index ] ).Penalty}, priority: {index}");
                SetCancellationRulePriority(sortedCancellationRuleIds[index], index);
                rules += $"{sortedCancellationRuleIds[index]} = {index} ";
            }
            _logger.Info( rules );
        }

        private void SetCancellationRulePriority(int cancellationRuleId, int priority)
        {
            bool needUpdate = Boolean.Parse(ConfigurationManager.AppSettings["NeedUpdate"]);
            if (needUpdate)
            {
                string queryString = $"UPDATE cancellation_rule SET priority={priority} WHERE id_cancellation_rule={cancellationRuleId}";

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    connection.Open();
                    try
                    {
                        command.ExecuteNonQuery();
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
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

            string queryString =
                @"SELECT 
	ISNULL(AVG(b.amount_before_tax),0) amount_before_tax, 
	ISNULL(AVG(b.prepay_sum),0) prepay_sum, 
	1 booking_room_type_count
FROM booking b 
WHERE 
	" + $"id_provider={_provider.Id} " +
	@"AND id_booking > 32000000
	AND b.status in (1,2) 
	AND b.permanent_number is null 
	AND b.source<>'BS-CHANNEL_MANAGER'";
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
    }
}