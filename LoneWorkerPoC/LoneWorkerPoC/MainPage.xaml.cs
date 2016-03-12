using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;

namespace LoneWorkerPoC
{
    /// <summary>
    /// The main page of the LoneWorkerPoC app.
    /// </summary>
    public sealed partial class MainPage
    {
        private readonly BandManager _bandManager;
        private bool _started;
        private long _initSteps;
        private long _initDistance;
        private Stopwatch _initTime;

        public MainPage()
        {
            InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Required;

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

        //TODO: heart rate low/high, GPS location, panic button

        private async void NotifClick(object sender, RoutedEventArgs e)
        {
            await _bandManager.SendNotification(NotifOutput, TitleInput.Text, BodyInput.Text);
        }

        private async void ToggleClick(object sender, RoutedEventArgs e)
        {
            _started = !_started;
            if (_started)
            {
                _initSteps = await _bandManager.GetPedometer(StepsOutput);
                _initDistance = await _bandManager.GetDistance(DistanceOutput);
                if (_initTime == null) { _initTime = new Stopwatch(); }
                _initTime.Start();

                ToggleButton.Content = "End work";

                TimeOutput.Text = "0h 0min 0sec";
                StepsOutput.Text = "0";
                DistanceOutput.Text = "0 m";

                var heartRate = await _bandManager.DisplayHeartRate(HeartRateOutput);
                HeartRateLow.Text = heartRate.ToString();
                HeartRateHigh.Text = heartRate.ToString();

                await _bandManager.DisplaySkinTemperature(TempOutput);

                BandOutput.Text = "Startup time: " + _initTime.Elapsed.Seconds + "." + _initTime.Elapsed.Milliseconds; //TODO remove after 1 min or so
            }
            else
            {
                //TODO save end of work data (and send it to DB/web dashboard?)

                _initTime.Stop();
                _initTime.Reset();

                ToggleButton.Content = "Start work";

                StepsOutput.Text = "Not started";
                DistanceOutput.Text = "Not started";
                TimeOutput.Text = "Not started";
                HeartRateOutput.Text = "Not started";
                TempOutput.Text = "Not started";
            }
        }

        private async void RefreshClick(object sender, RoutedEventArgs e)
        {
            if (!_started) return;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            UpdateTime();

            var steps = await _bandManager.GetPedometer(StepsOutput) - _initSteps;
            StepsOutput.Text = steps.ToString();

            //UpdateTime();

            var distance = await _bandManager.GetDistance(DistanceOutput) - _initDistance;
            DistanceOutput.Text = Convert.ToDecimal(distance) / 100 + " m";

            //UpdateTime();

            var heartRate = await _bandManager.DisplayHeartRate(HeartRateOutput);
            HeartRateLow.Text = heartRate.ToString();
            HeartRateHigh.Text = heartRate.ToString();

            //UpdateTime();

            await _bandManager.DisplaySkinTemperature(TempOutput);

            UpdateTime();

            BandOutput.Text = "Refresh time: " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds; //TODO remove after 1 min or so
        }

        private void UpdateTime()
        {
            TimeOutput.Text = _initTime.Elapsed.Hours + "h " + _initTime.Elapsed.Minutes + "min " + _initTime.Elapsed.Seconds + "sec";
        }

        private async void ConnectClick(object sender, RoutedEventArgs e)
        {
            if (await _bandManager.ConnectTask())
            {
                BandOutput.Text = "Connected."; //TODO remove after 1 min or so
            }
            else
            {
                BandOutput.Text = "We cannot detect a paired Microsoft Band. Make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
            }
        }

        private void PanicClick(object sender, RoutedEventArgs e)
        {
            //TODO
        }
    }
}
