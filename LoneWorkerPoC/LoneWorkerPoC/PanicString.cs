using System;
using Newtonsoft.Json;

namespace LoneWorkerPoC
{
    public class PanicString
    {
        private bool _panic;
        private int _hours;
        private int _minutes;
        private int _seconds;
        private long _steps;
        private long _distance;
        private decimal _heartRate;
        private decimal _heartRateLow;
        private decimal _heartRateHigh;
        private decimal _temperature;
        private double _latitude;
        private double _longitude;

        public PanicString(TimeSpan timeElasped, long steps, long distance, decimal heartRate, decimal heartRateLow, decimal heartRateHigh, 
            decimal temperature, double latitude, double longitude)
        {
            _hours = timeElasped.Hours;
            _minutes = timeElasped.Minutes;
            _seconds = timeElasped.Seconds;
            _steps = steps;
            _distance = distance;
            _heartRate = heartRate;
            _heartRateLow = heartRateLow;
            _heartRateHigh = heartRateHigh;
            _temperature = temperature;
            _latitude = latitude;
            _longitude = longitude;
        }

        public string ToJsonString(bool panic)
        {
            _panic = panic;
            return JsonConvert.SerializeObject(this);
        }
    }
}
