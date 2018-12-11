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

        public ProviderCancellationRulePriorityUpdater(Provider provider, string connectionString, ILogger logger)
        {
            _provider = provider;
            _connectionString = connectionString;
            _logger = logger;
        }


        public void Execute()
        {
            _logger.Warning( $"{_provider.Id}" );
            List<CancellationRule> cancellationRules = GetCancellationRules();

            List<CancellationRulePenalty> result = new List<CancellationRulePenalty>();
//            _logger.Info( $"MaxCancellationBeforeArrivalValue={maxCancellationBeforeArrivalValue}" );
            CancellationRulePenaltyCalculator cancellationRulePenaltyCalculator = new CancellationRulePenaltyCalculator(
                _logger, 
                _connectionString );
            foreach ( CancellationRule cancellationRule in cancellationRules)
            {
                //_logger.Info( $"cancellation_rule: {cancellationRule}" );
                decimal penalty = cancellationRulePenaltyCalculator.Calculate( cancellationRule);
                result.Add(new CancellationRulePenalty {CancellationRuleId = cancellationRule.Id, Penalty = penalty});
            }

            List<int> sortedCancellationRuleIds = result.OrderByDescending(penalty => penalty.Penalty).Select(penalty => penalty.CancellationRuleId).ToList();
            string rules= String.Empty;
            for (int index = 0; index < sortedCancellationRuleIds.Count; index++)
            {
                _logger.Info($"cancellationRule: {cancellationRules.Find(rule => rule.Id == sortedCancellationRuleIds[ index ] )}, penalty={result.Find(cancellationRulePenalty => cancellationRulePenalty.CancellationRuleId == sortedCancellationRuleIds[ index ] ).Penalty}, priority: {index}");
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