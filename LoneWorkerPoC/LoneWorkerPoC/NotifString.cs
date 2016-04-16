using System;
using System.Collections.Generic;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.System.Profile;
using Newtonsoft.Json;

namespace LoneWorkerPoC
{
    public class NotifString
    {
        public string DeviceId;
        public string Sender;
        public DateTime TimeStamp;
        public string Title;
        public string Message;

        public NotifString(string title, string message)
        {
            TimeStamp = DateTime.Now;
            Title = title;
            Message = message;
            DeviceId = GetDeviceId();
            Sender = GetName();
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
        
        private string GetName()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            return localSettings.Values.ContainsKey("Name") ? (string)localSettings.Values["Name"] : "N/A";
        }

        private string GetDeviceId()
        {
            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var hardwareId = token.Id;

            var hasher = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var hashed = hasher.HashData(hardwareId);

            return CryptographicBuffer.EncodeToHexString(hashed);
        }

        public List<KeyValuePair<string, string>> ToKeyValuePairs()
        {
            var values = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("DeviceId", DeviceId),
                        new KeyValuePair<string, string>("Sender", Sender),
                        new KeyValuePair<string, string>("TimeStamp", TimeStamp.ToString()),
                        new KeyValuePair<string, string>("Title", Title),
                        new KeyValuePair<string, string>("Message", Message)
                    };
            return values;
        }
    }
}