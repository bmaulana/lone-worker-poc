using System;
using Newtonsoft.Json;

namespace LoneWorkerPoC
{
    public class NotifString
    {
        public DateTime TimeStamp;
        public string Title;
        public string Message;

        public NotifString(string title, string message)
        {
            TimeStamp = DateTime.Now;
            Title = title;
            Message = message;
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}