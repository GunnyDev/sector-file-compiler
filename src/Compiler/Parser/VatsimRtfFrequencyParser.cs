﻿namespace Compiler.Parser
{
    public class VatsimRtfFrequencyParser : IFrequencyParser
    {
        // This has to be much lower because some are defined on the VORs
        const int firstMinValue = 108;
        const int firstMaxValue = 136;
        const int secondDividend = 25;

        private const string prePositionsFrequency = "199.998";
        private const string notValidFrequency = "199.900";

        public string ParseFrequency(string frequency)
        {
            // No frequency, accept this
            if (frequency == prePositionsFrequency || frequency == notValidFrequency)
            {
                return frequency;
            }

            string[] split = frequency.Split('.');
            if (split.Length != 2)
            {
                return null;
            }

            if (!int.TryParse(split[0], out int first) || first < firstMinValue || first > firstMaxValue)
            {
                return null;
            }

            if (!int.TryParse(split[1], out int second))
            {
                return null;
            }

            if ((second % secondDividend) != 0 && ((second + 5) % secondDividend) != 0)
            {
                return null;
            }

            return frequency;
        }
    }
}
