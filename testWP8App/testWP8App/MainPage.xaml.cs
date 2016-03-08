using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Band;
using Microsoft.Band.Notifications;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace testWP8App
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
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
                        var samplesReceived = 0; // the number of SkinTemperature samples received
                        var readings = new List<double>();

                        // Subscribe to SkinTemperature data.
                        bandClient.SensorManager.SkinTemperature.ReadingChanged += (s, args) =>
                        {
                            samplesReceived++;
                            // viewModel.StatusMessage = args.SensorReading.SkinTemperature.ToString();
                            readings.Add(args.SensorReading.Temperature);
                        };
                        await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();

                        // Receive SkinTemperature data for a while, then stop the subscription.
                        while (readings.Count == 0)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                        await bandClient.SensorManager.SkinTemperature.StopReadingsAsync();

                        var average = (decimal)readings.Sum() / readings.Count;
                        var message = average + " C";

                        /*var message = readings.Aggregate("", (current, i) => current + $"{i} \n");
                        message += $"Done. {samplesReceived} SkinTemperature samples were received.";*/

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
                        var samplesReceived = 0; // the number of HeartRate samples received
                        var readings = new List<int>();

                        // Subscribe to HeartRate data.
                        bandClient.SensorManager.HeartRate.ReadingChanged += (s, args) =>
                        {
                            samplesReceived++;
                            // viewModel.StatusMessage = args.SensorReading.HeartRate.ToString();
                            readings.Add(args.SensorReading.HeartRate);
                        };
                        await bandClient.SensorManager.HeartRate.StartReadingsAsync();

                        // Receive HeartRate data for a while, then stop the subscription.
                        while (readings.Count == 0)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1));
                        }
                        await bandClient.SensorManager.HeartRate.StopReadingsAsync();

                        var average = (decimal)readings.Sum() / readings.Count;
                        var message = average + " BPM";

                        /*var message = readings.Aggregate("", (current, i) => current + $"{i} \n");
                        message += $"Done. {samplesReceived} HeartRate samples were received.";*/

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

                    // Remove the Tile from the Band, if present. An application won't need to do this everytime it runs. 
                    // But in case you modify this sample code and run it again, let's make sure to start fresh.
                    // await bandClient.TileManager.RemoveTileAsync(myTileId);

                    // Create the Tile on the Band.
                    await bandClient.TileManager.AddTileAsync(myTile);

                    // Send a notification.
                    await bandClient.NotificationManager.SendMessageAsync(myTileId, titleInput.Text, bodyInput.Text, DateTimeOffset.Now, MessageFlags.ShowDialog);

                    bandOutput.Text = "Done. Check the Tile on your Band (it's the last Tile).";
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

        private async void CreatePanicBand(object sender, RoutedEventArgs e)
        {
            //TODO fix this
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
                    // Create a Tile with a TextButton on it.
                    Guid myTileId = new Guid("12408A60-13EB-46C2-9D24-F14BF6A033C6");
                    BandTile myTile = new BandTile(myTileId)
                    {
                        Name = "My Tile",
                        TileIcon = await LoadIcon("ms-appx:///Assets/SampleTileIconLarge.png"),
                        SmallIcon = await LoadIcon("ms-appx:///Assets/SampleTileIconSmall.png")
                    };
                    TextButton button = new TextButton() { ElementId = 1, Rect = new PageRect(10, 10, 200, 90) };
                    FilledPanel panel = new FilledPanel(button) { Rect = new PageRect(0, 0, 220, 150) };
                    myTile.PageLayouts.Add(new PageLayout(panel));

                    // Remove the Tile from the Band, if present. An application won't need to do this everytime it runs. 
                    // But in case you modify this sample code and run it again, let's make sure to start fresh.
                    await bandClient.TileManager.RemoveTileAsync(myTileId);

                    // Create the Tile on the Band.
                    await bandClient.TileManager.AddTileAsync(myTile);
                    await bandClient.TileManager.SetPagesAsync(myTileId, new PageData(new Guid("5F5FD06E-BD37-4B71-B36C-3ED9D721F200"), 0, new TextButtonData(1, "Click here")));

                    // Subscribe to Tile events.
                    int buttonPressedCount = 0;
                    TaskCompletionSource<bool> closePressed = new TaskCompletionSource<bool>();

                    bandClient.TileManager.TileButtonPressed += (s, args) =>
                    {
                        var a = Dispatcher.RunAsync(
                            CoreDispatcherPriority.Normal,
                            () =>
                            {
                                buttonPressedCount++;
                                bandOutput.Text = string.Format("TileButtonPressed = {0}", buttonPressedCount);
                            }
                        );
                    };
                    bandClient.TileManager.TileClosed += (s, args) =>
                    {
                        closePressed.TrySetResult(true);
                    };

                    await bandClient.TileManager.StartReadingsAsync();

                    // Receive events until the Tile is closed.
                    bandOutput.Text = "Check the Tile on your Band (it's the last Tile). Waiting for events ...";

                    await closePressed.Task;

                    // Stop listening for Tile events.
                    await bandClient.TileManager.StopReadingsAsync();

                    bandOutput.Text = "Done.";
                }
            }
            catch (Exception ex)
            {
                bandOutput.Text = ex.ToString();
            }
        }

        private void ToggleClick(object sender, RoutedEventArgs e)
        {
            //TODO
        }
    }
}
