using System;
using ProviderCancellationRule.Entities;
using ProviderCancellationRule.Entities.Enums;

namespace ProviderCancellationRule
{
    class CancellationRuleConditionPenaltyCalculator
    {
        private readonly int _maxCancellationBeforeArrivalValue;
        private readonly ILogger _logger;

        public CancellationRuleConditionPenaltyCalculator(int maxCancellationBeforeArrivalValue, ILogger logger)
        {
            _maxCancellationBeforeArrivalValue = maxCancellationBeforeArrivalValue;
            _logger = logger;
        }

        public decimal Calculate(CancellationRuleCondition cancellationRuleCondition, Booking booking)
        {
            decimal penalty = 0;

            switch ( cancellationRuleCondition.PenaltyCalcMode )
            {
                case CancellationPenaltyCalcMode.Percent:
                    penalty = booking.AmountBeforeTax * cancellationRuleCondition.PenaltyValue / 100;
                    break;
                case CancellationPenaltyCalcMode.Fixed:
                    penalty = Math.Min( booking.AmountBeforeTax, cancellationRuleCondition.PenaltyValue );
                    break;
                case CancellationPenaltyCalcMode.FirstNightPercent:
                    penalty = ( booking.AmountBeforeTax / booking.RoomTypeCount ) * cancellationRuleCondition.PenaltyValue / 100;
                    break;
                case CancellationPenaltyCalcMode.PrepaymentPercent:
                    penalty = booking.PrepaySum * cancellationRuleCondition.PenaltyValue / 100;
                    break;
                case CancellationPenaltyCalcMode.FirstNights:
                    penalty = ( booking.AmountBeforeTax / booking.RoomTypeCount ) * cancellationRuleCondition.PenaltyValue;
                    break;
            }

            int k = 1;
            if (cancellationRuleCondition.CancellationBeforeArrivalUnit != TimeUnit.None)
            {
                switch (cancellationRuleCondition.CancellationBeforeArrivalMatching)
                {
                    case CancellationBeforeArrivalMatching.AtLeast:
                        k = cancellationRuleCondition.CancellationBeforeArrivalUnit == TimeUnit.Day ? cancellationRuleCondition.CancellationBeforeArrivalValue * 24 : cancellationRuleCondition.CancellationBeforeArrivalValue;
                        break;
                    case CancellationBeforeArrivalMatching.Between:
                        int betweenValue = cancellationRuleCondition.CancellationBeforeArrivalValueMax - cancellationRuleCondition.CancellationBeforeArrivalValue;
                        k = cancellationRuleCondition.CancellationBeforeArrivalUnit == TimeUnit.Day ? betweenValue * 24 : betweenValue;
                        break;
                    case CancellationBeforeArrivalMatching.NoMatter:
                        k = _maxCancellationBeforeArrivalValue + 1;
                        break;
                }
            }

//            _logger.Info( $"\t\tcondition: {cancellationRuleCondition}, penalty: {penalty} * k:{k} = {penalty * k}" );
            return penalty * k;
        }
    }
}
