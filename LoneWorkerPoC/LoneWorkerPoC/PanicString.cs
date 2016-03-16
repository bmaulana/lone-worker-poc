using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoneWorkerPoC
{
    public class PanicString
    {
        private TimeSpan _timeElasped;
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
            _timeElasped = timeElasped;
            _steps = steps;
            _distance = distance;
            _heartRate = heartRate;
            _heartRateLow = heartRateLow;
            _heartRateHigh = heartRateHigh;
            _temperature = temperature;
            _latitude = latitude;
            _longitude = longitude;
        }

        public string GetJsonString(bool panic)
        {
            //TODO
            return null;
        }
    }
}
