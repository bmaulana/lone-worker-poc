﻿using System;
using Newtonsoft.Json;

namespace LoneWorkerPoC
{
    public class PanicString
    {
        public bool Panic;
        public DateTime LastRefreshed;
        public DateTime WorkStarted;
        public TimeSpan TimeElasped;
        // public readonly int Hours;
        // public readonly int Minutes;
        // public readonly int Seconds;
        public readonly long Steps;
        public readonly long Distance;
        public readonly decimal HeartRate;
        public readonly decimal HeartRateLow;
        public readonly decimal HeartRateHigh;
        public readonly decimal SkinTemperature;
        public readonly double Latitude;
        public readonly double Longitude;

        public PanicString(DateTime lastRefreshed, DateTime workStarted, TimeSpan timeElasped, long steps, long distance,
            decimal heartRate, decimal heartRateLow, decimal heartRateHigh,
            decimal temperature, double latitude, double longitude)
        {
            // Hours = timeElasped.Hours;
            // Minutes = timeElasped.Minutes;
            // Seconds = timeElasped.Seconds;
            LastRefreshed = lastRefreshed;
            WorkStarted = workStarted;
            TimeElasped = timeElasped;
            Steps = steps;
            Distance = distance;
            HeartRate = heartRate;
            HeartRateLow = heartRateLow;
            HeartRateHigh = heartRateHigh;
            SkinTemperature = temperature;
            Latitude = latitude;
            Longitude = longitude;
            Longitude = longitude;
        }

        public string ToJsonString(bool panic)
        {
            Panic = panic;
            return JsonConvert.SerializeObject(this);
        }
    }
}