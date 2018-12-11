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

        public decimal Calculate(CancellationRuleCondition cancellationRuleCondition)
        {
            int totalHoursBeforeArrival = 0;
            if (cancellationRuleCondition.CancellationBeforeArrivalUnit != TimeUnit.None)
            {
                int maxTotalHoursBeforeArrival = 365 * 2 * 24; // Как бы два года
                switch (cancellationRuleCondition.CancellationBeforeArrivalMatching)
                {
                    case CancellationBeforeArrivalMatching.AtLeast:
                        var atLeastValue = cancellationRuleCondition.CancellationBeforeArrivalUnit == TimeUnit.Day
                            ? cancellationRuleCondition.CancellationBeforeArrivalValue * 24
                            : cancellationRuleCondition.CancellationBeforeArrivalValue;

                        totalHoursBeforeArrival = maxTotalHoursBeforeArrival - atLeastValue;
                        break;
                    case CancellationBeforeArrivalMatching.Between:
                        int betweenValue = cancellationRuleCondition.CancellationBeforeArrivalValueMax - cancellationRuleCondition.CancellationBeforeArrivalValue;
                        totalHoursBeforeArrival = cancellationRuleCondition.CancellationBeforeArrivalUnit == TimeUnit.Day ? betweenValue * 24 : betweenValue;
                        break;
                    case CancellationBeforeArrivalMatching.NoMoreThan:
                        var noMoreValue = cancellationRuleCondition.CancellationBeforeArrivalUnit == TimeUnit.Day
                            ? cancellationRuleCondition.CancellationBeforeArrivalValue * 24
                            : cancellationRuleCondition.CancellationBeforeArrivalValue;
                        totalHoursBeforeArrival = noMoreValue;
                        break;
                    case CancellationBeforeArrivalMatching.NoMatter:
                        totalHoursBeforeArrival = maxTotalHoursBeforeArrival;
                        break;
                }
            }

            decimal conditionWeight = 0;
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

            // _logger.Info( $"\t\tcondition: {cancellationRuleCondition}, totalHoursBeforeArrival: {totalHoursBeforeArrival} * conditionWeight:{conditionWeight} = {conditionWeight * totalHoursBeforeArrival}" );
            return totalHoursBeforeArrival * conditionWeight;
        }
    }
}
