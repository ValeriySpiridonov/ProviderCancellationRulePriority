using System;
using ProviderCancellationRule.Entities.Enums;

namespace ProviderCancellationRule.Entities
{
    public class CancellationRuleCondition
    {
        public int Id { get; set; }
        public int CancellationRuleId { get; set; }

        public CancellationBeforeArrivalMatching CancellationBeforeArrivalMatching { get; set; }
        public TimeUnit CancellationBeforeArrivalUnit { get; set; }
        public int CancellationBeforeArrivalValue { get; set; }
        public int CancellationBeforeArrivalValueMax { get; set; }

        public CancellationRoomTypeQuantityMatching RoomTypeQuantityMatching { get; set; }
        public int RoomTypeQuantityValue { get; set; }
        public int RoomTypeQuantityValueMax { get; set; }

        public CancellationGuestQuantityMatching GuestQuantityMatching { get; set; }
        public int GuestQuantityValue { get; set; }
        public int GuestQuantityValueMax { get; set; }

        public CancellationPenaltyCalcMode PenaltyCalcMode { get; set; }
        public decimal PenaltyValue { get; set; }
        public string PenaltyValueCurrency { get; set; }

        public override string ToString()
        {
            if ( PenaltyCalcMode == CancellationPenaltyCalcMode.NoPenalty )
            {
                // Без штрафа
                return "Без штрафа";
            }
            else if ( PenaltyCalcMode == CancellationPenaltyCalcMode.Percent )
            {
                // От стоимости брони, %
                return PenaltyValue + "% От стоимости всей брони";
            }
            else if ( PenaltyCalcMode == CancellationPenaltyCalcMode.FirstNightPercent )
            {
                // От первых суток, %
                return PenaltyValue + "% От стоимости первых суток";
            }
            else if ( PenaltyCalcMode == CancellationPenaltyCalcMode.PrepaymentPercent )
            {
                // От размера предоплаты, %
                return PenaltyValue + "% От размера предоплаты";
            }
            else if ( PenaltyCalcMode == CancellationPenaltyCalcMode.FirstNights )
            {
                // Первых суток
                return "Стоимость первых суток";
            }
            return String.Empty;
        }
    }
}
