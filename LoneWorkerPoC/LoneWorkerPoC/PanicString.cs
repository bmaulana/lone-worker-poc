using System;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.System.Profile;
using Newtonsoft.Json;

namespace LoneWorkerPoC
{
    public class PanicString
    {
        public bool Panic;
        public string DeviceId;
        public string Sender;
        public DateTime LastRefreshed;
        public DateTime WorkStarted;
        public TimeSpan TimeElasped;
        public readonly long Steps;
        public readonly long Distance;
        public readonly decimal HeartRate;
        public readonly decimal HeartRateLow;
        public readonly decimal HeartRateHigh;
        public readonly decimal SkinTemperature;
        public readonly double Latitude;
        public readonly double Longitude;

        public PanicString(DateTime lastRefreshed, DateTime workStarted, TimeSpan timeElasped, long steps, long distance,
            decimal heartRate, decimal heartRateLow, decimal heartRateHigh, decimal temperature, double latitude, double longitude)
        {
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
            DeviceId = GetDeviceId();
            Sender = GetName();
        }

        public string ToJsonString(bool panic)
        {
            Panic = panic;
            return JsonConvert.SerializeObject(this);
        }

        private string GetName()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            return localSettings.Values.ContainsKey("Name") ? (string) localSettings.Values["Name"] : "N/A";
        }

        private string GetDeviceId()
        {
            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var hardwareId = token.Id;

            var hasher = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var hashed = hasher.HashData(hardwareId);

            return CryptographicBuffer.EncodeToHexString(hashed);
        }
    }
}
