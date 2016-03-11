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

namespace LoneWorkerPoC
{
    /// <summary>
    /// The main page of the LoneWorkerPoC app.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private BandManager _bandManager;
        private bool _started;
        private long _initSteps;
        private long _initDistance;
        private Stopwatch _initTime;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            _bandManager = new BandManager();
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
            await _bandManager.DisplayHeartRate(heartRateOutput);
            await _bandManager.DisplaySkinTemperature(tempOutput);
        }

        private async void NotifClick(object sender, RoutedEventArgs e)
        {
            await _bandManager.SendNotification(bandOutput, titleInput.Text, bodyInput.Text);
        }

        private async void ToggleClick(object sender, RoutedEventArgs e)
        {
            _started = !_started;
            if (_started)
            {
                _initSteps = await _bandManager.GetPedometer(stepsOutput);
                _initDistance = await _bandManager.GetDistance(distanceOutput);
                if (_initTime == null) { _initTime = new Stopwatch(); }
                _initTime.Start();

                toggleButton.Content = "End work";
                stepsOutput.Text = "0";
                distanceOutput.Text = "0 m";
                timeOutput.Text = "0h 0min 0sec";
            }
            else
            {
                _initTime.Stop();
                _initTime.Reset();
                toggleButton.Content = "Start work";
                stepsOutput.Text = "Not started";
                distanceOutput.Text = "Not started";
                timeOutput.Text = "Not started";
            }
        }

        private async void RefreshClick(object sender, RoutedEventArgs e)
        {
            if (!_started) return;

            var steps = await _bandManager.GetPedometer(stepsOutput) - _initSteps;
            stepsOutput.Text = steps.ToString();

            var distance = await _bandManager.GetDistance(distanceOutput) - _initDistance;
            distanceOutput.Text = Convert.ToDecimal(distance) / 100 + " m";

            timeOutput.Text = _initTime.Elapsed.Hours + "h " + _initTime.Elapsed.Minutes + "min " + _initTime.Elapsed.Seconds + "sec";
        }
    }
}
