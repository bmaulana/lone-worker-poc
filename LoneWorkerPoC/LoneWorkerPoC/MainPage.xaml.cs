using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Band;
using Microsoft.Band.Notifications;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace LoneWorkerPoC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private const int BandDelay = 100;

        private bool started = false;
        private long initSteps;
        private long initDistance;
        private Stopwatch initTime;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private async void BandClick(object sender, RoutedEventArgs e)
        {
            await DisplayHeartRate();
            await DisplaySkinTemperature();
        }
        private async Task DisplaySkinTemperature()
        {
            tempOutput.Text = "Running ...";

            try
            {
                // Get the list of Microsoft Bands paired to the phone.
                var pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    tempOutput.Text = "We cannot detect a paired Microsoft Band. Make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                    return;
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
                    }
                }
            }
            catch (Exception ex)
            {
                tempOutput.Text = ex.ToString();
            }
        }

        private async Task DisplayHeartRate()
        {
            heartRateOutput.Text = "Running ...";

            try
            {
                // Get the list of Microsoft Bands paired to the phone.
                var pairedBands = await BandClientManager.Instance.GetBandsAsync();
                if (pairedBands.Length < 1)
                {
                    heartRateOutput.Text = "We cannot detect a paired Microsoft Band. Make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                    return;
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
                    }
                }
            }
            catch (Exception ex)
            {
                heartRateOutput.Text = ex.ToString();
            }
        }

        private async void NotifClick(object sender, RoutedEventArgs e)
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
                    await bandClient.NotificationManager.SendMessageAsync(myTileId, titleInput.Text, bodyInput.Text, DateTimeOffset.Now, MessageFlags.ShowDialog);

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

        private async void ToggleClick(object sender, RoutedEventArgs e)
        {
            started = !started;
            if (started)
            {
                initSteps = await GetPedometer();
                initDistance = await GetDistance();
                if (initTime == null) { initTime = new Stopwatch(); }
                initTime.Start();

                toggleButton.Content = "End work";
                stepsOutput.Text = "0";
                distanceOutput.Text = "0 m";
                timeOutput.Text = "0h 0min 0sec";
            }
            else
            {
                initTime.Stop();
                initTime.Reset();
                toggleButton.Content = "Start work";
                stepsOutput.Text = "Not started";
                distanceOutput.Text = "Not started";
                timeOutput.Text = "Not started";
            }
        }

        private async void RefreshClick(object sender, RoutedEventArgs e)
        {
            if (!started) return;

            var steps = await GetPedometer() - initSteps;
            stepsOutput.Text = steps.ToString();

            var distance = await GetDistance() - initDistance;
            distanceOutput.Text = Convert.ToDecimal(distance) / 100 + " m";

            timeOutput.Text = initTime.Elapsed.Hours + "h " + initTime.Elapsed.Minutes + "min " + initTime.Elapsed.Seconds + "sec";
        }

        private async Task<long> GetPedometer()
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

        private async Task<long> GetDistance()
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
    }
}
