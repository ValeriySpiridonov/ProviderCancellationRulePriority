using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ProviderCancellationRule.Entities;
using ProviderCancellationRule.Entities.Enums;

namespace ProviderCancellationRule
{
    class CancellationRulePenaltyCalculator
    {
        private readonly ILogger _logger;
        private readonly string _connectionString;

        public CancellationRulePenaltyCalculator(
            ILogger logger, 
            string connectionString )
        {
            _logger = logger;
            _connectionString = connectionString;
        }

        public Tuple<int, int> Calculate( CancellationRule cancellationRule )
        {
            Tuple<int, int, int> cancellationRulePower = null;
            
            List<CancellationRuleCondition> cancellationRuleConditions = GetActiveCancellationRuleConditions( cancellationRule.Id, DateTime.Now );
            CancellationRuleConditionPenaltyCalculator cancellationRuleConditionPenaltyCalculator = new CancellationRuleConditionPenaltyCalculator(_logger);

            foreach ( CancellationRuleCondition cancellationRuleCondition in cancellationRuleConditions )
            {
                if ( cancellationRuleCondition.PenaltyCalcMode != CancellationPenaltyCalcMode.NoPenalty )
                {
                    var currentConditionPenalty = cancellationRuleConditionPenaltyCalculator.Calculate( cancellationRuleCondition );
                    cancellationRulePower = cancellationRulePower == null
                        ? GetRulePower( currentConditionPenalty )
                        : GetRulePower( cancellationRulePower, currentConditionPenalty );
                }
            }

            int totalAffectedDays = cancellationRulePower?.Item2 - cancellationRulePower?.Item1 ?? 0;

            return Tuple.Create( totalAffectedDays, cancellationRulePower?.Item3 ?? 0 );
        }

        private Tuple<int, int, int> GetRulePower( Tuple<int, int, int> cancellationRulePower, Tuple<int, int, int> conditionPower )
        {
            int totalHours = 0;
            if ( conditionPower.Item1 < cancellationRulePower.Item1 )
            {
                totalHours += cancellationRulePower.Item1 - conditionPower.Item1;
            }

            if ( conditionPower.Item2 > cancellationRulePower.Item2 )
            {
                totalHours += conditionPower.Item2 - cancellationRulePower.Item2;
            }

            int conditionWeight = totalHours * conditionPower.Item3;
            int totalRuleWeight = conditionWeight + cancellationRulePower.Item3;

            return Tuple.Create(
                Math.Min( conditionPower.Item1, cancellationRulePower.Item1 ),
                Math.Max( conditionPower.Item2, cancellationRulePower.Item2 ),
                totalRuleWeight );
        }

        private Tuple<int, int, int> GetRulePower( Tuple<int, int, int> conditionPower )
        {
            int totalHours = conditionPower.Item2 - conditionPower.Item1;

            return Tuple.Create(
                conditionPower.Item1,
                conditionPower.Item2,
                conditionPower.Item3 * totalHours );
        }

        private List<CancellationRuleCondition> GetActiveCancellationRuleConditions( int cancellationRuleId, DateTime date )
        {
            List<CancellationRuleCondition> result = new List<CancellationRuleCondition>();
            string queryString = @"SELECT * FROM [cancellation_rule_condition] condition
            WHERE condition.[id_cancellation_rule] = @id_cancellation_rule
            AND EXISTS
            (
                SELECT 1 FROM [cancellation_rule_condition_period]
            WHERE [id_cancellation_rule_condition] = condition.[id_cancellation_rule_condition]

            AND (@date BETWEEN [start_date] AND [end_date]
            OR @date >= [start_date] AND[is_endless] = 1 )
                ) ORDER BY [id_cancellation_rule_condition] ASC";
            using ( SqlConnection connection = new SqlConnection( _connectionString ) )
            {
                SqlCommand command = new SqlCommand( queryString, connection );
                command.Parameters.Add( "@id_cancellation_rule", SqlDbType.Int ).Value = cancellationRuleId;
                command.Parameters.Add( "@date", SqlDbType.DateTime ).Value = date;
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    while ( reader.Read() )
                    {
                        result.Add( new CancellationRuleCondition()
                        {
                            Id = ( int )reader[ "id_cancellation_rule_condition" ],
                            CancellationRuleId = ( int )reader[ "id_cancellation_rule" ],

                            CancellationBeforeArrivalMatching = ( CancellationBeforeArrivalMatching )reader[ "cancellation_before_arrival_matching" ],
                            CancellationBeforeArrivalUnit = ( TimeUnit )reader[ "cancellation_before_arrival_unit" ],
                            CancellationBeforeArrivalValue = ( int )reader[ "cancellation_before_arrival_value" ],
                            CancellationBeforeArrivalValueMax = ( int )reader[ "cancellation_before_arrival_value_max" ],

                            RoomTypeQuantityMatching = ( CancellationRoomTypeQuantityMatching )reader[ "room_type_quantity_matching" ],
                            RoomTypeQuantityValue = ( int )reader[ "room_type_quantity_value" ],
                            RoomTypeQuantityValueMax = ( int )reader[ "room_type_quantity_value_max" ],

                            GuestQuantityMatching = ( CancellationGuestQuantityMatching )reader[ "guest_quantity_matching" ],
                            GuestQuantityValue = ( int )reader[ "guest_quantity_value" ],
                            GuestQuantityValueMax = ( int )reader[ "guest_quantity_value_max" ],

                            PenaltyCalcMode = ( CancellationPenaltyCalcMode )reader[ "penalty_calc_mode" ],
                            PenaltyValue = Convert.ToDecimal( reader[ "penalty_value" ] ),
                            PenaltyValueCurrency = Convert.ToString( reader[ "penalty_value_currency" ] ),
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
