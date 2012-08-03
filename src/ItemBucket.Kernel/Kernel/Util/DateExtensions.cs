// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateExtensions.cs" company="Sitecore">
//   Sitecore
// </copyright>
// <summary>
//   Defines the DateExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------


namespace Sitecore.ItemBucket.Kernel.Kernel.Util
{
    using System;

    /// <summary>
    /// DateTime Extensions
    /// </summary>
    public static class DateExtensions
    {
        /// <summary>
        /// Will give you the DateTime object given the number of days you go back.
        /// </summary>
        /// <param name="days">
        /// The days.
        /// </param>
        /// <returns>
        /// DateTime object
        /// </returns>
        public static DateTime DaysAgo(this int days)
        {
            var t = new TimeSpan(days, 0, 0, 0);
            return DateTime.Now.Subtract(t);
        }
        
        /// <summary>
        /// Will give you the DateTime object given the number of days you go forward.
        /// </summary>
        /// <param name="days">
        /// The days.
        /// </param>
        /// <returns>
        /// DateTime object
        /// </returns>
        public static DateTime DaysFromNow(this int days)
        {
            var t = new TimeSpan(days, 0, 0, 0);
            return DateTime.Now.Add(t);
        }

        /// <summary>
        /// Will give you the DateTime object given the number of hours you go back.
        /// </summary>
        /// <param name="hours">
        /// The days.
        /// </param>
        /// <returns>
        /// DateTime object
        /// </returns>
        public static DateTime HoursAgo(this int hours)
        {
            var t = new TimeSpan(hours, 0, 0);
            return DateTime.Now.Subtract(t);
        }

        /// <summary>
        /// Will give you the DateTime object given the number of hours you go forward.
        /// </summary>
        /// <param name="hours">
        /// The days.
        /// </param>
        /// <returns>
        /// DateTime object
        /// </returns>
        public static DateTime HoursFromNow(this int hours)
        {
            var t = new TimeSpan(hours, 0, 0);
            return DateTime.Now.Add(t);
        }

        /// <summary>
        /// Will give you the DateTime object given the number of minutes you go back.
        /// </summary>
        /// <param name="minutes">
        /// The days.
        /// </param>
        /// <returns>
        /// DateTime object
        /// </returns>
        public static DateTime MinutesAgo(this int minutes)
        {
            var t = new TimeSpan(0, minutes, 0);
            return DateTime.Now.Subtract(t);
        }

        /// <summary>
        /// Will give you the DateTime object given the number of minutes you go forward.
        /// </summary>
        /// <param name="minutes">
        /// The days.
        /// </param>
        /// <returns>
        /// DateTime object
        /// </returns>
        public static DateTime MinutesFromNow(this int minutes)
        {
            var t = new TimeSpan(0, minutes, 0);
            return DateTime.Now.Add(t);
        }

        /// <summary>
        /// Will give you the DateTime object given the number of seconds you go back.
        /// </summary>
        /// <param name="seconds">
        /// The days.
        /// </param>
        /// <returns>
        /// DateTime object
        /// </returns>
        public static DateTime SecondsAgo(this int seconds)
        {
            var t = new TimeSpan(0, 0, seconds);
            return DateTime.Now.Subtract(t);
        }

        /// <summary>
        /// Will give you the DateTime object given the number of seconds you go forward.
        /// </summary>
        /// <param name="seconds">
        /// The days.
        /// </param>
        /// <returns>
        /// DateTime object
        /// </returns>
        public static DateTime SecondsFromNow(this int seconds)
        {
            var t = new TimeSpan(0, 0, seconds);
            return DateTime.Now.Add(t);
        }

        /// <summary>
        /// Will give you the difference between two dates
        /// </summary>
        /// <param name="dateOne">
        /// The Start Date.
        /// </param>
        /// <param name="dateTwo">
        /// The End Date
        /// </param>
        /// <returns>
        /// TimeSpan object
        /// </returns>
        public static TimeSpan Diff(this DateTime dateOne, DateTime dateTwo)
        {
            var t = dateOne.Subtract(dateTwo);
            return t;
        }

        /// <summary>
        /// Will give you the difference between two days
        /// </summary>
        /// <param name="dateOne">
        /// The Start Day.
        /// </param>
        /// <param name="dateTwo">
        /// The End Day
        /// </param>
        /// <returns>
        /// Double value
        /// </returns>
        public static double DiffDays(this string dateOne, string dateTwo)
        {
            DateTime dtOne;
            DateTime dtTwo;
            if (DateTime.TryParse(dateOne, out dtOne) && DateTime.TryParse(dateTwo, out dtTwo))
            {
                return Diff(dtOne, dtTwo).TotalDays;
            }

            return 0;
        }

        /// <summary>
        /// Will give you the difference in days between two dates
        /// </summary>
        /// <param name="dateOne">
        /// The Start Date.
        /// </param>
        /// <param name="dateTwo">
        /// The End Date
        /// </param>
        /// <returns>
        /// Double Value
        /// </returns>
        public static double DiffDays(this DateTime dateOne, DateTime dateTwo)
        {
            return Diff(dateOne, dateTwo).TotalDays;
        }

        /// <summary>
        /// Will give you the difference between two hour strings
        /// </summary>
        /// <param name="dateOne">
        /// The Start Hour.
        /// </param>
        /// <param name="dateTwo">
        /// The End Hour
        /// </param>
        /// <returns>
        /// Double Value
        /// </returns>
        public static double DiffHours(this string dateOne, string dateTwo)
        {
            DateTime dtOne;
            DateTime dtTwo;
            if (DateTime.TryParse(dateOne, out dtOne) && DateTime.TryParse(dateTwo, out dtTwo))
            {
                return Diff(dtOne, dtTwo).TotalHours;
            }

            return 0;
        }

        /// <summary>
        /// Will give you the difference between two Hours
        /// </summary>
        /// <param name="dateOne">
        /// The Start Date.
        /// </param>
        /// <param name="dateTwo">
        /// The End Date
        /// </param>
        /// <returns>
        /// Double Value
        /// </returns>
        public static double DiffHours(this DateTime dateOne, DateTime dateTwo)
        {
            return Diff(dateOne, dateTwo).TotalHours;
        }

        /// <summary>
        /// Will give you the difference between two minute strings
        /// </summary>
        /// <param name="dateOne">
        /// The Start minute.
        /// </param>
        /// <param name="dateTwo">
        /// The End minute
        /// </param>
        /// <returns>
        /// Double Value
        /// </returns>
        public static double DiffMinutes(this string dateOne, string dateTwo)
        {
            DateTime dtOne;
            DateTime dtTwo;
            if (DateTime.TryParse(dateOne, out dtOne) && DateTime.TryParse(dateTwo, out dtTwo))
            {
                return Diff(dtOne, dtTwo).TotalMinutes;
            }

            return 0;
        }

        /// <summary>
        /// Will give you the difference between two minute DateTimes
        /// </summary>
        /// <param name="dateOne">
        /// The Start Date.
        /// </param>
        /// <param name="dateTwo">
        /// The End Date
        /// </param>
        /// <returns>
        /// Double Value
        /// </returns>
        public static double DiffMinutes(this DateTime dateOne, DateTime dateTwo)
        {
            return Diff(dateOne, dateTwo).TotalMinutes;
        }

        /// <summary>
        /// Will give you the difference between two dates
        /// </summary>
        /// <param name="dateTime">
        /// The Date to change.
        /// </param>
        /// <param name="hours">
        /// The Hours
        /// </param>
        /// <param name="minutes">
        /// The Minutes
        /// </param>
        /// <param name="seconds">
        /// The Seconds
        /// </param>
        /// <param name="milliseconds">
        /// The Milliseconds
        /// </param>
        /// <returns>
        /// TimeSpan object
        /// </returns>
        public static DateTime ChangeTime(this DateTime dateTime, int hours, int minutes, int seconds, int milliseconds)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                hours,
                minutes,
                seconds,
                milliseconds,
                dateTime.Kind);
        }
    }
}