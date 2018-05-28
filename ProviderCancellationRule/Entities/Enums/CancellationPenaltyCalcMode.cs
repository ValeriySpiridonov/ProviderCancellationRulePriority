namespace ProviderCancellationRule.Entities.Enums
{
    public enum CancellationPenaltyCalcMode
    {
        /// <summary>
        /// Без штрафа
        /// </summary>
        NoPenalty = 0,
        /// <summary>
        /// Штраф равен указанным процентам от стоимости брони
        /// </summary>
        Percent = 1,
        /// <summary>
        /// Штраф равен фиксированной величине
        /// </summary>
        Fixed = 2,
        /// <summary>
        /// Штраф равен указанным процентам от стоимости первой ночи
        /// </summary>
        FirstNightPercent = 3,
        /// <summary>
        /// Штраф равен указанным процентам от внесённой предоплаты
        /// </summary>
        PrepaymentPercent = 4,
        /// <summary>
        /// Штраф равен сумме оплаты за первые N-ночей
        /// </summary>
        FirstNights = 5
    }
}