using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Payroll
{
    public class MonthlyPay
    {
        public class Configuration
        {

            public enum eWorkingDay
            {
                AllDays,
                Fix30Days,
                Config
            }
            public bool WorkingDay_RemoveSunday { get; set; }
            public bool WorkingDay_RemoveHoliday { get; set; }
            public bool WorkingDay_Shift { get; set; }

            public enum eEncashmentMode
            {
                AutoSelection,
                FixedDaysBy7Days,
                FixedDaysEOM,
                Config
            }

            public enum eEncashmentValue
            {
                Gross,
                Basic
            }
            public bool Encashment_AllSunday { get; set; }
            public bool Encashment_AllHoliday { get; set; }
            public bool HolidaySalary_OneDayOnLeave { get; set; }
            public bool HolidaySalary_TwoDayOnPresent { get; set; }
            public bool HolidaySalary_OneDayOnPresent { get; set; }
            public bool HolidaySalary_NoSalary { get; set; }
        }
    }
}