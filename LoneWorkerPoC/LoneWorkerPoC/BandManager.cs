using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Band;
using Microsoft.Band.Notifications;
using Microsoft.Band.Tiles;

namespace LoneWorkerPoC
{
    public class BandManager
    {
        private const int BandDelay = 100; //delay in communicating with the band for sensor retreival in milliseconds
        private const string RunningMessage = "Refreshing";

        private IBandClient _bandClient;
        private bool _started;

        public async Task<bool> ConnectTask()
        {
            var pairedBands = await BandClientManager.Instance.GetBandsAsync();
            if (pairedBands.Length < 1) return false;
            _bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]);

            //TODO add consent stuff

            _started = true;
            return true;
        }

        public bool IsConnected()
        {
            return _started;
        }

        public async Task<decimal> DisplaySkinTemperature(TextBlock output)
        {
            if (!_started) return -1;

            output.Text = RunningMessage;

            try
            {
                    bool tempConsentGranted;

                    // Check whether the user has granted access to the SkinTemperature sensor.
                    if (_bandClient.SensorManager.SkinTemperature.GetCurrentUserConsent() == UserConsent.Granted)
                    {
                        tempConsentGranted = true;
                    }
                    else
                    {
                        tempConsentGranted = await _bandClient.SensorManager.SkinTemperature.RequestUserConsentAsync();
                    }

                    if (!tempConsentGranted)
                    {
                        output.Text = "Access to the skin temperature sensor is denied.";
                    }
                    else
                    {
                        var readings = new List<double>();

                        // Subscribe to SkinTemperature data.
                        _bandClient.SensorManager.SkinTemperature.ReadingChanged += (s, args) =>
                        {
                            readings.Add(args.SensorReading.Temperature);
                        };
                        await _bandClient.SensorManager.SkinTemperature.StartReadingsAsync();

                        // Receive SkinTemperature data for a while, then stop the subscription.
                        while (readings.Count == 0)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(BandDelay));
                        }
                        await _bandClient.SensorManager.SkinTemperature.StopReadingsAsync();

                        var average = (decimal)readings.Sum() / readings.Count;
                        var message = average + " C";
                        output.Text = message;
                        return average;
                    }
            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
            }
            return -1;
        }

        public async Task<decimal> DisplayHeartRate(TextBlock output)
        {
            if (!_started) return -1;

            output.Text = RunningMessage;

            try
            {
                    bool heartRateConsentGranted;

                    // Check whether the user has granted access to the HeartRate sensor.
                    if (_bandClient.SensorManager.HeartRate.GetCurrentUserConsent() == UserConsent.Granted)
                    {
                        heartRateConsentGranted = true;
                    }
                    else
                    {
                        heartRateConsentGranted = await _bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
                    }

                    if (!heartRateConsentGranted)
                    {
                        output.Text = "Access to the heart rate sensor is denied.";
                    }
                    else
                    {
                        var readings = new List<int>();

                        // Subscribe to HeartRate data.
                        _bandClient.SensorManager.HeartRate.ReadingChanged += (s, args) =>
                        {
                            readings.Add(args.SensorReading.HeartRate);
                        };
                        await _bandClient.SensorManager.HeartRate.StartReadingsAsync();

                        // Receive HeartRate data for a while, then stop the subscription.
                        while (readings.Count == 0)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(BandDelay));
                        }
                        await _bandClient.SensorManager.HeartRate.StopReadingsAsync();

                        var average = (decimal)readings.Sum() / readings.Count;
                        var message = average + " BPM";

                        output.Text = message;
                        return average;
                    }
            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
            }
            return -1;
        }

        public async Task<long> GetPedometer(TextBlock output)
        {
            if (!_started) return -1;

            output.Text = RunningMessage;

            try
            {
                    bool consentGranted;

                    // Check whether the user has granted access to the Pedometer sensor.
                    if (_bandClient.SensorManager.Pedometer.GetCurrentUserConsent() == UserConsent.Granted)
                    {
                        consentGranted = true;
                    }
                    else
                    {
                        consentGranted = await _bandClient.SensorManager.Pedometer.RequestUserConsentAsync();
                    }

                    if (!consentGranted)
                    {
                        output.Text = "Access to the pedometer is denied.";
                    }
                    else
                    {
                        var readings = new List<long>();

                        _bandClient.SensorManager.Pedometer.ReadingChanged += (s, args) =>
                        {
                            readings.Add(args.SensorReading.TotalSteps);
                        };
                        await _bandClient.SensorManager.Pedometer.StartReadingsAsync();

                        while (readings.Count == 0)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(BandDelay));
                        }
                        await _bandClient.SensorManager.Distance.StopReadingsAsync();

                        return readings[readings.Count - 1];
                    }
            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
            }
            return -1;
        }

        public async Task<long> GetDistance(TextBlock output)
        {
            if (!_started) return -1;

            output.Text = RunningMessage;

            try
            {
                    bool consentGranted;

                    // Check whether the user has granted access to the Distance sensor.
                    if (_bandClient.SensorManager.Distance.GetCurrentUserConsent() == UserConsent.Granted)
                    {
                        consentGranted = true;
                    }
                    else
                    {
                        consentGranted = await _bandClient.SensorManager.Distance.RequestUserConsentAsync();
                    }

                    if (!consentGranted)
                    {
                        output.Text = "Access to the distance sensor is denied.";
                    }
                    else
                    {
                        var readings = new List<long>();

                        _bandClient.SensorManager.Distance.ReadingChanged += (s, args) =>
                        {
                            readings.Add(args.SensorReading.TotalDistance);
                        };
                        await _bandClient.SensorManager.Distance.StartReadingsAsync();

                        while (readings.Count == 0)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(BandDelay));
                        }
                        await _bandClient.SensorManager.Distance.StopReadingsAsync();

                        return readings[readings.Count - 1];
                    }
            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
            }
            return -1;
        }

        public async Task SendNotification(TextBlock output, string title, string message)
        {
            if (!_started) return;
            output.Text = "Sending...";

            try
            {
                    // Create a Tile.
                    var myTileId = new Guid("D0BAB7A8-FFDC-43C3-B995-87AFB2A43387");
                    var myTile = new BandTile(myTileId)
                    {
                        Name = "Notifications Tile",
                        TileIcon = await LoadIcon("ms-appx:///Assets/SampleTileIconLarge.png"),
                        SmallIcon = await LoadIcon("ms-appx:///Assets/SampleTileIconSmall.png")
                    };
                    //
                    // Remove the Tile from the Band, if present. An application won't need to do this everytime it runs. 
                    // But in case you modify this sample code and run it again, let's make sure to start fresh.
                    // TODO add version check - if version is same skip this step, if changed remove old tile first
                    // await bandClient.TileManager.RemoveTileAsync(myTileId);

                    // Check if tile exists in the band
                    var installedApps = await _bandClient.TileManager.GetTilesAsync();
                    bool[] exists = { false };
                    foreach (var tile in installedApps.Where(tile => !exists[0] && tile.TileId == myTileId))
                    {
                        exists[0] = true;
                    }

                    // Create new tile if not
                    if (!exists[0])
                    {
                        await _bandClient.TileManager.AddTileAsync(myTile);
                    }

                    // Send a notification.
                    await _bandClient.NotificationManager.SendMessageAsync(myTileId, title, message, DateTimeOffset.Now, MessageFlags.ShowDialog);

                    output.Text = "Message sent.";
            }
            catch (Exception ex)
            {
                output.Text = ex.ToString();
            }
        }

        private static async Task<BandIcon> LoadIcon(string uri)
        {
            var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (var fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                var bitmap = new WriteableBitmap(1, 1);
                await bitmap.SetSourceAsync(fileStream);
                return bitmap.ToBandIcon();
            }
        }
    }
}