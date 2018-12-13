using System;
using ProviderCancellationRule.Entities;
using ProviderCancellationRule.Entities.Enums;

namespace ProviderCancellationRule
{
    class CancellationRuleConditionPenaltyCalculator
    {
        private readonly ILogger _logger;

        public CancellationRuleConditionPenaltyCalculator(ILogger logger)
        {
            _logger = logger;
        }

        public Tuple<int, int, int> Calculate(CancellationRuleCondition cancellationRuleCondition)
        {
            int from = 0;
            int to = 0;
            if (cancellationRuleCondition.CancellationBeforeArrivalUnit != TimeUnit.None)
            {
                int maxTotalHoursBeforeArrival = 365 * 2 * 24; // Как бы два года
                switch (cancellationRuleCondition.CancellationBeforeArrivalMatching)
                {
                    case CancellationBeforeArrivalMatching.AtLeast:
                        from = cancellationRuleCondition.CancellationBeforeArrivalValue;
                        to = maxTotalHoursBeforeArrival;
                        break;
                    case CancellationBeforeArrivalMatching.Between:
                        from = cancellationRuleCondition.CancellationBeforeArrivalValue;
                        to = cancellationRuleCondition.CancellationBeforeArrivalValueMax;
                        break;
                    case CancellationBeforeArrivalMatching.NoMoreThan:
                        from = 0;
                        to = cancellationRuleCondition.CancellationBeforeArrivalValue;
                        break;
                    case CancellationBeforeArrivalMatching.NoMatter:
                        from = 0;
                        to = maxTotalHoursBeforeArrival;
                        break;
                }
            }

            from = cancellationRuleCondition.CancellationBeforeArrivalUnit == TimeUnit.Day
                ? from * 24
                : from;
            to = cancellationRuleCondition.CancellationBeforeArrivalUnit == TimeUnit.Day
                ? to * 24
                : to;

            int conditionWeight = 0;
            switch ( cancellationRuleCondition.PenaltyCalcMode )
            {
                case CancellationPenaltyCalcMode.Percent:
                    conditionWeight = 2;
                    break;
                case CancellationPenaltyCalcMode.Fixed:
                    conditionWeight = 0; // таких нет на проде
                    break;
                case CancellationPenaltyCalcMode.FirstNightPercent:
                    conditionWeight = 1;
                    break;
                case CancellationPenaltyCalcMode.PrepaymentPercent:
                    conditionWeight = cancellationRuleCondition.PenaltyValue == 100m ? 5 : 3; // При проценте предоплаты равном 100 - считаем правило более приоритетным
                    break;
                case CancellationPenaltyCalcMode.FirstNights:
                    conditionWeight = 4;
                    break;
            }

            return Tuple.Create( from, to, conditionWeight );

            // _logger.Info( $"\t\tcondition: {cancellationRuleCondition}, totalHoursBeforeArrival: {totalHoursBeforeArrival} * conditionWeight:{conditionWeight} = {conditionWeight * totalHoursBeforeArrival}" );
            // return totalHoursBeforeArrival * conditionWeight;
        }
    }
}
