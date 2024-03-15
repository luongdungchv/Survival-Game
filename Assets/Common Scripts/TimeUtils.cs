using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using DG.Tweening;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace DL.Utils
{
    public static class TimeUtils
    {
        public static string SecondsToMS(int second)
        {
            string Normalize(int input)
            {
                if (input >= 10) return input.ToString();
                else return $"0{input}";
            }
            string res = "";
            res += (second / 60).ToString();
            res += ":";
            res += Normalize(second % 60);
            return res;
        }
        public static string SecondsToHMS(int totalSecs)
        {
            int hours = totalSecs / 3600;
            int minutes = (totalSecs % 3600) / 60;
            int seconds = (totalSecs % 3600) % 60;
            return $"{hours}:{minutes}:{seconds}";
        }
        public static string SecondsToMinutes(int totalSecs)
        {
            return (totalSecs / 60).ToString() + "m";
        }
        public static long GetCurrentTimeSeconds()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            var seconds = timeSpan.TotalSeconds;
            return (long)seconds;
        }
        public static int GetCurrentDayOfWeek()
        {
            DateTime now = DateTime.Now;
            int today = (int)now.DayOfWeek;
            today--;
            if (today == -1) today = 6;
            return today;
        }
        public static int GetCurrentWeek()
        {
            DateTime now = DateTime.Now;
            TimeSpan duration = now - new DateTime(1970, 1, 5, 0, 0, 0, DateTimeKind.Utc);
            int weeksSince1970 = (int)(duration.TotalDays / 7);
            //return 2793;
            return weeksSince1970;
        }
    }
}