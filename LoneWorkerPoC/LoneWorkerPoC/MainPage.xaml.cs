using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace LoneWorkerPoC
{
    /// <summary>
    /// The main page of the LoneWorkerPoC app.
    /// </summary>
    public sealed partial class MainPage
    {
        private readonly BandManager _bandManager;
        private bool _started;
        private bool _connected;
        private long _initSteps;
        private long _initDistance;
        private decimal _prevHeartRateLow;
        private decimal _prevHeartRateHigh;
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

        private async void NotifClick(object sender, RoutedEventArgs e)
        {
            await _bandManager.SendNotification(NotifOutput, TitleInput.Text, BodyInput.Text);
        }

        private async void ToggleClick(object sender, RoutedEventArgs e)
        {
            if (!_connected)
            {
                if (await _bandManager.ConnectTask())
                {
                    BandOutput.Text = "Connected.";
                    _connected = true;
                }
                else
                {
                    BandOutput.Text = "We cannot detect a paired Microsoft Band. Make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                }
            }

            _started = !_started;
            if (_started)
            {
                ToggleButton.Content = "End work";

                if (_initTime == null) { _initTime = new Stopwatch(); }
                _initTime.Start();
                TimeOutput.Text = "0h 0min 0sec";

                _initSteps = await _bandManager.GetPedometer(StepsOutput);
                StepsOutput.Text = "0";

                _initDistance = await _bandManager.GetDistance(DistanceOutput);
                DistanceOutput.Text = "0 m";

                var heartRate = await _bandManager.DisplayHeartRate(HeartRateOutput);
                _prevHeartRateLow = heartRate;
                HeartRateLow.Text = heartRate.ToString();
                _prevHeartRateHigh = heartRate;
                HeartRateHigh.Text = heartRate.ToString();

                await _bandManager.DisplaySkinTemperature(TempOutput);

                await OneShotLocation();

                BandOutput.Text = "Startup time: " + _initTime.Elapsed.Seconds + "." + _initTime.Elapsed.Milliseconds + " s"; //TODO remove after 1 min or so
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
                HeartRateLow.Text = "Not started";
                HeartRateHigh.Text = "Not started";
                TempOutput.Text = "Not started";
                LatOutput.Text = "Not started";
                LongOutput.Text = "Not started";
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
            if (heartRate < _prevHeartRateLow)
            {
                HeartRateLow.Text = heartRate.ToString();
                _prevHeartRateLow = heartRate;
            }
            if (heartRate > _prevHeartRateHigh)
            {
                HeartRateHigh.Text = heartRate.ToString();
                _prevHeartRateHigh = heartRate;
            }

            //UpdateTime();

            await _bandManager.DisplaySkinTemperature(TempOutput);

            await OneShotLocation();

            UpdateTime();

            BandOutput.Text = "Refresh time: " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds + " s"; //TODO remove after 1 min or so
        }
        
        private void PanicClick(object sender, RoutedEventArgs e)
        {
            //TODO
        }

        private void UpdateTime()
        {
            TimeOutput.Text = _initTime.Elapsed.Hours + "h " + _initTime.Elapsed.Minutes + "min " + _initTime.Elapsed.Seconds + "sec";
        }

        private async Task OneShotLocation()
        {
            //TODO improve time, use newer non deprecated API, ask for consent

            var geolocator = new Geolocator {DesiredAccuracyInMeters = 50};

            LatOutput.Text = "Refreshing ...";
            LongOutput.Text = "Refreshing ...";

            try
            {
                var geoposition = await geolocator.GetGeopositionAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(10));

                var latitude = geoposition.Coordinate.Latitude;
                var longitude = geoposition.Coordinate.Longitude;

                if (latitude > 0)
                {
                    LatOutput.Text = latitude + " N";
                }
                else
                {
                    LatOutput.Text = -latitude + " S";
                }

                if (longitude > 0)
                {
                    LongOutput.Text = longitude + " E";
                }
                else
                {
                    LongOutput.Text = -longitude + " W";
                }
            }
            catch (Exception ex)
            {
                if ((uint) ex.HResult == 0x80004004)
                {
                    LatOutput.Text = "location is disabled in phone settings.";
                    LongOutput.Text = "location is disabled in phone settings.";
                }
                else
                {
                    LatOutput.Text = "Something went wrong.";
                    LongOutput.Text = "Something went wrong.";
                }
            }
        }
    }
}
