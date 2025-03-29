using System;

namespace DLCQuestipelago.MoveLink
{
    public class FoolManager
    {
        private static bool _shouldPrankOnFishDay = true;
        private static bool _shouldPrankOnOtherDays = false;

        public static bool IsPrankMonth()
        {
            return DateTime.Now.Month == 4;
        }

        public static bool IsPrankDay()
        {
            return IsPrankMonth() && DateTime.Now.Day == 1;
        }

        internal static bool ShouldPrank()
        {
            return true;
            return IsPrankDay() ? _shouldPrankOnFishDay : _shouldPrankOnOtherDays;
        }

        internal static void TogglePrank(bool silent)
        {
            if (IsPrankDay())
            {
                _shouldPrankOnFishDay = !_shouldPrankOnFishDay;
            }
            else
            {
                _shouldPrankOnOtherDays = !_shouldPrankOnOtherDays;
            }
        }
    }
}
