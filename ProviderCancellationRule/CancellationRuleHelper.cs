using System;
using ProviderCancellationRule.Entities;
using ProviderCancellationRule.Entities.Enums;

namespace ProviderCancellationRule
{
    public static class CancellationRuleHelper
    {
        /// <summary>
        /// Возвращает точку отсчёта (дату и время) для правила отмены
        /// </summary>
        /// <param name="cancellationRule">Правило отмены</param>
        /// <param name="provider">Провайдер</param>
        /// <param name="guestArrivalDateTime">Дата и время заезда гостя</param>
        /// <param name="bookingCreationTimeLocalForProvider">Локальное для провайдера время бронирования</param>
        /// <returns>Точка отсчёта для правила отмены</returns>
        public static DateTime GetReferencePointDateTimeForCancellationRule(
            CancellationRule cancellationRule,
            Provider provider,
            DateTime guestArrivalDateTime,
            TimeSpan bookingCreationTimeLocalForProvider )
        {
            Time referencePointTime = GetReferencePointTimeForCancellationRule(
                cancellationRule,
                provider,
                guestArrivalDateTime.TimeOfDay.ToTime(),
                bookingCreationTimeLocalForProvider.ToTime() );

            return guestArrivalDateTime.Date.AddTime( referencePointTime );
        }

        /// <summary>
        /// Возвращает точку отсчёта (дату и время) для правила отмены из брони
        /// </summary>
        /// <param name="bookingCancellationRule">Правило отмены</param>
        /// <param name="guestArrivalDateTime">Дата и время заезда гостя</param>
        /// <param name="bookingCreationTimeLocalForProvider">Локальное для провайдера время бронирования гостем</param>
        /// <returns>Точка отсчёта для правила отмены из брони</returns>
        public static DateTime GetReferencePointDateTimeForBookingCancellationRule(
            CancellationRule bookingCancellationRule,
            DateTime guestArrivalDateTime,
            TimeSpan bookingCreationTimeLocalForProvider )
        {
            Time guestArrivalTime = guestArrivalDateTime.TimeOfDay.ToTime();
            Time referencePointTime = GetReferencePointTimeForBookingCancellationRule(
                bookingCancellationRule.ReferencePointKind,
                bookingCancellationRule.ReferencePointTime,
                guestArrivalTime,
                bookingCreationTimeLocalForProvider.ToTime() );

            return guestArrivalDateTime.Date.AddTime( referencePointTime );
        }

        /// <summary>
        /// Возвращает точку отсчёта (время) для правила отмены
        /// </summary>
        /// <param name="cancellationRule">Правило отмены</param>
        /// <param name="provider">Провайдер</param>
        /// <param name="guestArrivalTime">Время заезда гостя</param>
        /// <param name="bookingCreationTimeLocalForProvider">Локальное для провайдера время бронирования</param>
        /// <returns>Точка отсчёта для правила отмены</returns>
        public static Time GetReferencePointTimeForCancellationRule(
            CancellationRule cancellationRule,
            Provider provider,
            Time guestArrivalTime,
            Time bookingCreationTimeLocalForProvider )
        {
            if ( cancellationRule == null )
            {
                throw new ArgumentNullException( "cancellationRule" );
            }

            if ( provider == null )
            {
                throw new ArgumentNullException( "provider" );
            }

            switch ( cancellationRule.ReferencePointKind )
            {
                case CancellationReferencePointKind.ProviderArrivalTime:
                    return provider.ArrivalTime;
                case CancellationReferencePointKind.ProviderDepartureTime:
                    return provider.DepartureTime;
                case CancellationReferencePointKind.GuestArrivalTime:
                    return guestArrivalTime;
                case CancellationReferencePointKind.CustomArrivalTime:
                    return cancellationRule.ReferencePointTime;
                case CancellationReferencePointKind.BookingCreationTime:
                    return bookingCreationTimeLocalForProvider;
            }

            throw new ArgumentOutOfRangeException( "cancellationRule.ReferencePointKind", "Invalid ReferencePointKind value" );
        }

        static Time GetReferencePointTimeForBookingCancellationRule(
            CancellationReferencePointKind referencePointKind,
            Time bookingCancellationRuleReferencePointTime,
            Time guestArrivalTime,
            Time bookingCreationTimeLocalForProvider )
        {
            switch ( referencePointKind )
            {
                case CancellationReferencePointKind.ProviderArrivalTime:
                case CancellationReferencePointKind.ProviderDepartureTime:
                case CancellationReferencePointKind.CustomArrivalTime:
                    return bookingCancellationRuleReferencePointTime;
                case CancellationReferencePointKind.GuestArrivalTime:
                    return guestArrivalTime;
                case CancellationReferencePointKind.BookingCreationTime:
                    return bookingCreationTimeLocalForProvider;
            }

            throw new ArgumentOutOfRangeException( "referencePointKind", "Invalid ReferencePointKind value" );
        }

        /// <summary>
        /// Возвращает период действия условия правила отмены
        /// </summary>
        /// <param name="condition">Условие правила отмены</param>
        /// <param name="referencePointDateTime">Локальная для провайдера точка отсчёта правила отмены</param>
        /// <returns>Кортеж: StartDate; EndDate</returns>
        public static Tuple<DateTime, DateTime> GetActivityPeriod( CancellationRuleCondition condition, DateTime referencePointDateTime )
        {
            if ( condition == null )
            {
                throw new ArgumentNullException( "condition" );
            }

            return GetActivityPeriod( condition.CancellationBeforeArrivalMatching,
                condition.CancellationBeforeArrivalUnit, condition.CancellationBeforeArrivalValue,
                condition.CancellationBeforeArrivalValueMax, referencePointDateTime );
        }

        private static Tuple<DateTime, DateTime> GetActivityPeriod(
            CancellationBeforeArrivalMatching cancellationBeforeArrivalMatching,
            TimeUnit cancellationBeforeArrivalUnit,
            int cancellationBeforeArrivalValue,
            int cancellationBeforeArrivalValueMax,
            DateTime referencePointDateTime )
        {
            DateTime startDate = cancellationBeforeArrivalUnit == TimeUnit.Day
                ? referencePointDateTime.AddDays( -cancellationBeforeArrivalValue )
                : referencePointDateTime.AddHours( -cancellationBeforeArrivalValue );

            DateTime endDate = cancellationBeforeArrivalUnit == TimeUnit.Day
                ? referencePointDateTime.AddDays( -cancellationBeforeArrivalValueMax )
                : referencePointDateTime.AddHours( -cancellationBeforeArrivalValueMax );

            switch ( cancellationBeforeArrivalMatching )
            {
                case CancellationBeforeArrivalMatching.NoMatter:
                    startDate = DateTime.MinValue;
                    endDate = DateTime.MaxValue;
                    break;
                case CancellationBeforeArrivalMatching.AtLeast:
                    endDate = startDate;
                    startDate = DateTime.MinValue;
                    break;
                case CancellationBeforeArrivalMatching.NoMoreThan:
                    endDate = DateTime.MaxValue;
                    break;
                case CancellationBeforeArrivalMatching.Between:
                    DateTime tmp = startDate;
                    startDate = endDate;
                    endDate = tmp < referencePointDateTime ? tmp : DateTime.MaxValue;
                    break;
            }
            return new Tuple<DateTime, DateTime>( startDate, endDate );
        }

        /// <summary>
        /// Проверяет истекло ли условие правила отмены из брони
        /// </summary>
        /// <param name="cancellationCondition">Условие правила отмены</param>
        /// <param name="paymentDateTimeLocalForProvider">Локальная для провайдера дата и время бронирования</param>
        /// <param name="referencePointDateTime">Локальная для провайдера точка отсчёта правила отмены</param>
        /// <returns>true, если условие правило отмены истекло; иначе false</returns>
        public static bool IsConditionExpired( CancellationRuleCondition cancellationCondition, DateTime paymentDateTimeLocalForProvider, DateTime referencePointDateTime )
        {
            Tuple<DateTime, DateTime> activityPeriod = GetActivityPeriod( cancellationCondition, referencePointDateTime );
            return paymentDateTimeLocalForProvider > activityPeriod.Item2;
        }

        /// <summary>
        /// Проверяет действует ли условие правила отмены на указанную дату
        /// </summary>
        /// <param name="condition">Условие правила отмены</param>
        /// <param name="cancellationDateTimeLocalForProvider">Локальная для провайдера дата отмены</param>
        /// <param name="referencePointDateTime">Локальная для провайдера точка отсчёта правила отмены</param>
        /// <returns>true, если условие правила отмены действует на указанную дату; иначе false</returns>
        public static bool IsConditionActualForDate( CancellationRuleCondition condition, DateTime cancellationDateTimeLocalForProvider, DateTime referencePointDateTime )
        {
            if ( condition == null )
                throw new ArgumentNullException( "condition" );

            bool isActual = false;
            DateTime dateMargin = condition.CancellationBeforeArrivalUnit == TimeUnit.Day
                ? referencePointDateTime.AddDays( -condition.CancellationBeforeArrivalValue )
                : referencePointDateTime.AddHours( -condition.CancellationBeforeArrivalValue );

            switch ( condition.CancellationBeforeArrivalMatching )
            {
                case CancellationBeforeArrivalMatching.NoMatter:
                    isActual = true;
                    break;
                case CancellationBeforeArrivalMatching.AtLeast:
                    isActual = cancellationDateTimeLocalForProvider <= dateMargin;
                    break;
                case CancellationBeforeArrivalMatching.NoMoreThan:
                    isActual = cancellationDateTimeLocalForProvider >= dateMargin;
                    break;
                case CancellationBeforeArrivalMatching.Between:
                    DateTime dateMarginMax = dateMargin;
                    dateMargin = condition.CancellationBeforeArrivalUnit == TimeUnit.Day ?
                        referencePointDateTime.AddDays( -condition.CancellationBeforeArrivalValueMax ) :
                        referencePointDateTime.AddHours( -condition.CancellationBeforeArrivalValueMax );

                    isActual = cancellationDateTimeLocalForProvider >= dateMargin && cancellationDateTimeLocalForProvider <= dateMarginMax;
                    break;
            }

            return isActual;
        }

        /// <summary>
        /// Возвращает смещение по времени от даты заезда срабатывания условия правила отмены
        /// </summary>
        /// <param name="referencePointKind">Тип смещения</param>
        /// <param name="referencePointTime">Время смещения</param>
        /// <param name="provider">Провайдер</param>
        /// <returns>смещение по времени от даты заезда срабатывания условия правила отмены</returns>
        public static TimeSpan GetCancellationDeadlineOffsetOfArrivalTime( CancellationReferencePointKind referencePointKind, Time referencePointTime, Provider provider )
        {
            Time arrivalTime = provider.ArrivalTime;
            Time departureTime = provider.DepartureTime;

            switch ( referencePointKind )
            {
                case CancellationReferencePointKind.ProviderDepartureTime:
                    return new TimeSpan( 0, departureTime.Hours - arrivalTime.Hours, departureTime.Minutes - arrivalTime.Minutes, 0 );
                case CancellationReferencePointKind.CustomArrivalTime:
                    return new TimeSpan( 0, referencePointTime.Hours - arrivalTime.Hours, referencePointTime.Minutes - arrivalTime.Minutes, 0 );
            }
            return new TimeSpan();
        }
    }
}
