﻿using System;

namespace QuizBowlDiscordScoreTracker
{
    public static class Verify
    {
        public static void IsNotNull(object parameter, string parameterName)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
