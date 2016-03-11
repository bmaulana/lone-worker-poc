using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
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
        private IBandClient bandClient;

        public async Task<bool> ConnectTask(TextBlock output)
        {
            var pairedBands = await BandClientManager.Instance.GetBandsAsync();
            if (pairedBands.Length < 1)
            {
                output.Text = "We cannot detect a paired Microsoft Band. Make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                return false;
            }
            bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]);
            return true;
        }

        public async Task<decimal> DisplaySkinTemperature(TextBlock tempOutput)
        {
            //tempOutput.Text = "Running ...";

            try
            {
                // Get the list of Microsoft Bands paired to the phone.
                var pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    tempOutput.Text = "We cannot detect a paired Microsoft Band. Make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                    return -1;
                }

                // Connect to Microsoft Band.
                using (IBandClient bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    bool tempConsentGranted;

                    // Check whether the user has granted access to the HeartRate sensor.
                    if (bandClient.SensorManager.SkinTemperature.GetCurrentUserConsent() == UserConsent.Granted)
                    {
                        tempConsentGranted = true;
                    }
                    else
                    {
                        tempConsentGranted = await bandClient.SensorManager.SkinTemperature.RequestUserConsentAsync();
                    }

                    if (!tempConsentGranted)
                    {
                        tempOutput.Text = "Access to the skin temperature sensor is denied.";
                    }
                    else
                    {
                        var readings = new List<double>();

                        // Subscribe to SkinTemperature data.
                        bandClient.SensorManager.SkinTemperature.ReadingChanged += (s, args) =>
                        {
                            readings.Add(args.SensorReading.Temperature);
                        };
                        await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();

                        // Receive SkinTemperature data for a while, then stop the subscription.
                        while (readings.Count == 0)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(BandDelay));
                        }
                        await bandClient.SensorManager.SkinTemperature.StopReadingsAsync();

                        var average = (decimal)readings.Sum() / readings.Count;
                        var message = average + " C";
                        tempOutput.Text = message;
                        return average;
                    }
                }
            }
            catch (Exception ex)
            {
                tempOutput.Text = ex.ToString();
            }
            return -1;
        }

        public async Task<decimal> DisplayHeartRate(TextBlock heartRateOutput)
        {
            //heartRateOutput.Text = "Running ...";

            try
            {
                // Get the list of Microsoft Bands paired to the phone.
                var pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    heartRateOutput.Text = "We cannot detect a paired Microsoft Band. Make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                    return -1;
                }

                // Connect to Microsoft Band.
                using (IBandClient bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    bool heartRateConsentGranted;

                    // Check whether the user has granted access to the HeartRate sensor.
                    if (bandClient.SensorManager.HeartRate.GetCurrentUserConsent() == UserConsent.Granted)
                    {
                        heartRateConsentGranted = true;
                    }
                    else
                    {
                        heartRateConsentGranted = await bandClient.SensorManager.HeartRate.RequestUserConsentAsync();
                    }

                    if (!heartRateConsentGranted)
                    {
                        heartRateOutput.Text = "Access to the heart rate sensor is denied.";
                    }
                    else
                    {
                        var readings = new List<int>();

                        // Subscribe to HeartRate data.
                        bandClient.SensorManager.HeartRate.ReadingChanged += (s, args) =>
                        {
                            readings.Add(args.SensorReading.HeartRate);
                        };
                        await bandClient.SensorManager.HeartRate.StartReadingsAsync();

                        // Receive HeartRate data for a while, then stop the subscription.
                        while (readings.Count == 0)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(BandDelay));
                        }
                        await bandClient.SensorManager.HeartRate.StopReadingsAsync();

                        var average = (decimal)readings.Sum() / readings.Count;
                        var message = average + " BPM";

                        heartRateOutput.Text = message;
                        return average;
                    }
                }
            }
            catch (Exception ex)
            {
                heartRateOutput.Text = ex.ToString();
            }
            return -1;
        }

        public async Task<long> GetPedometer(TextBlock stepsOutput)
        {
            try
            {
                // Get the list of Microsoft Bands paired to the phone.
                var pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    stepsOutput.Text = "We cannot detect a paired Microsoft Band. Make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                    return -1;
                }

                // Connect to Microsoft Band.
                using (IBandClient bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    bool consentGranted;

                    // Check whether the user has granted access to the HeartRate sensor.
                    if (bandClient.SensorManager.Pedometer.GetCurrentUserConsent() == UserConsent.Granted)
                    {
                        consentGranted = true;
                    }
                    else
                    {
                        consentGranted = await bandClient.SensorManager.Pedometer.RequestUserConsentAsync();
                    }

                    if (!consentGranted)
                    {
                        stepsOutput.Text = "Access to the pedometer is denied.";
                    }
                    else
                    {
                        var readings = new List<long>();

                        bandClient.SensorManager.Pedometer.ReadingChanged += (s, args) =>
                        {
                            readings.Add(args.SensorReading.TotalSteps);
                        };
                        await bandClient.SensorManager.Pedometer.StartReadingsAsync();

                        while (readings.Count == 0)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(BandDelay));
                        }
                        await bandClient.SensorManager.Distance.StopReadingsAsync();

                        return readings[readings.Count - 1];
                    }
                }
            }
            catch (Exception ex)
            {
                stepsOutput.Text = ex.ToString();
            }
            return -1;
        }

        public async Task<long> GetDistance(TextBlock distanceOutput)
        {
            try
            {
                // Get the list of Microsoft Bands paired to the phone.
                var pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    distanceOutput.Text = "We cannot detect a paired Microsoft Band. Make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                    return -1;
                }

                // Connect to Microsoft Band.
                using (IBandClient bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    bool consentGranted;

                    // Check whether the user has granted access to the HeartRate sensor.
                    if (bandClient.SensorManager.Distance.GetCurrentUserConsent() == UserConsent.Granted)
                    {
                        consentGranted = true;
                    }
                    else
                    {
                        consentGranted = await bandClient.SensorManager.Distance.RequestUserConsentAsync();
                    }

                    if (!consentGranted)
                    {
                        distanceOutput.Text = "Access to the distance sensor is denied.";
                    }
                    else
                    {
                        var readings = new List<long>();

                        bandClient.SensorManager.Distance.ReadingChanged += (s, args) =>
                        {
                            readings.Add(args.SensorReading.TotalDistance);
                        };
                        await bandClient.SensorManager.Distance.StartReadingsAsync();

                        while (readings.Count == 0)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(BandDelay));
                        }
                        await bandClient.SensorManager.Distance.StopReadingsAsync();

                        return readings[readings.Count - 1];
                    }
                }
            }
            catch (Exception ex)
            {
                distanceOutput.Text = ex.ToString();
            }
            return -1;
        }

        public async Task SendNotification(TextBlock bandOutput, string title, string message)
        {
            bandOutput.Text = "Running ...";

            try
            {
                // Get the list of Microsoft Bands paired to the phone.
                IBandInfo[] pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    bandOutput.Text = "This sample app requires a Microsoft Band paired to your device. Also make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                    return;
                }

                // Connect to Microsoft Band.
                using (IBandClient bandClient = await BandClientManager.Instance.ConnectAsync(pairedBands[0]))
                {
                    // Create a Tile.
                    Guid myTileId = new Guid("D0BAB7A8-FFDC-43C3-B995-87AFB2A43387");
                    BandTile myTile = new BandTile(myTileId)
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

                    // Create the Tile on the Band.
                    // await bandClient.TileManager.AddTileAsync(myTile);

                    var installedApps = await bandClient.TileManager.GetTilesAsync();
                    bool[] exists = { false };
                    foreach (var tile in installedApps.Where(tile => !exists[0] && tile.TileId == myTileId))
                    {
                        exists[0] = true;
                    }


                    if (!exists[0])
                    {
                        await bandClient.TileManager.AddTileAsync(myTile);
                    }

                    // Send a notification.
                    await bandClient.NotificationManager.SendMessageAsync(myTileId, title, message, DateTimeOffset.Now, MessageFlags.ShowDialog);

                    bandOutput.Text = "Message sent."; //TODO create task (don't await it?) to remove text after ~5 sec
                }
            }
            catch (Exception ex)
            {
                bandOutput.Text = ex.ToString();
            }
        }

        private async Task<BandIcon> LoadIcon(string uri)
        {
            StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                WriteableBitmap bitmap = new WriteableBitmap(1, 1);
                await bitmap.SetSourceAsync(fileStream);
                return bitmap.ToBandIcon();
            }
        }
    }
}