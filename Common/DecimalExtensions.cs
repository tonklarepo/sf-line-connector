using System;

namespace Common
{
    public static class DecimalExtensions
    {
        public static decimal RoundMoney(this decimal input)
        {
            return Math.Round(input, 2, MidpointRounding.AwayFromZero);
        }
    }
}