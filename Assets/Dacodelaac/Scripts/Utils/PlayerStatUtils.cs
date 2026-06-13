namespace Dacodelaac.Utils
{
    public static class PlayerStatUtils
    {
        public const float MAX_DAMAGE_REDUCTION = 0.8f;
        public const float MAX_MOVE_SPEED_BONUS = 0.35f;
        public const float MAX_CD_REDUCTION = -0.5f;

        public static string ConvertCurrencyTxt(int currency)
        {
            if (currency >= 1000000000)
            {
                var billionValue = 1f * currency / 1000000000;
                return $"{billionValue:0.##}B";
            }
            else if (currency >= 1000000)
            {
                var millionValue = 1f * currency / 1000000;
                return $"{millionValue:0.##}M";
            }
            else if (currency >= 100000)
            {
                var millionValue = 1f * currency / 1000;
                return $"{millionValue:0.##}K";
            }
            else
            {
                return currency.ToString();
            }
        }
    }
}