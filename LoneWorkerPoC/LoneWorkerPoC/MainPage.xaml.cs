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
        private DispatcherTimer _timer;
        private DispatcherTimer _clearTimer;
        private bool _started;
        private bool _connected;
        private long _initSteps;
        private long _initDistance;
        private Stopwatch _initTime;
        private long _steps;
        private long _distance;
        private decimal _heartRateLow;
        private decimal _heartRateHigh;
        private decimal _heartRate;
        private decimal _temperature;
        private double _latitude;
        private double _longitude;
        private DateTime _lastStarted;
        private DateTime _lastRefreshed;

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
            // Prepare page for display here.

            // If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private async void NotifClick(object sender, RoutedEventArgs e)
        {
            // TODO: Automate sending notifs to Band when message from web DB is received.
            // TODO: Move notifications to separate page
            await BandConnect(); //or if(!_started) return;
            await _bandManager.SendNotification(BandOutput, TitleInput.Text, BodyInput.Text);
            InitClearTimer();
        }

        private async void ToggleClick(object sender, RoutedEventArgs e)
        {
            await BandConnect();

            _started = !_started;
            if (_started)
            {
                if (!_connected) return;
                await StartWork();
                ToggleButton.Content = "End work";
            }
            else
            {
                EndWork();
                ToggleButton.Content = "Start work";
            }
        }

        private async Task BandConnect()
        {
            if (!_connected)
            {
                BandOutput.Text = "Connecting to Band...";
                if (await _bandManager.ConnectTask())
                {
                    BandOutput.Text = "Connected to Band.";
                    _connected = true;
                    InitClearTimer();
                }
                else
                {
                    BandOutput.Text = "We cannot detect a paired Microsoft Band. " +
                                      "Make sure that you have the latest firmware installed on your Band, as provided by the latest Microsoft Health app.";
                }
            }
        }

        private async Task StartWork()
        {
            _lastStarted = DateTime.Now;

            if (_initTime == null) { _initTime = new Stopwatch(); }
            _initTime.Start();
            TimeOutput.Text = "0h 0min 0sec";

            _initSteps = await _bandManager.GetPedometer(StepsOutput);
            _steps = 0;
            StepsOutput.Text = "0";

            _initDistance = await _bandManager.GetDistance(DistanceOutput);
            _distance = 0;
            DistanceOutput.Text = "0 m";

            _heartRate = await _bandManager.DisplayHeartRate(HeartRateOutput);
            _heartRateLow = _heartRate;
            HeartRateLow.Text = _heartRate.ToString();
            _heartRateHigh = _heartRate;
            HeartRateHigh.Text = _heartRate.ToString();

            _temperature = await _bandManager.DisplaySkinTemperature(TempOutput);

            await OneShotLocation();

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 1, 0) }; // refresh every 1 min
            _timer.Tick += TimerOnTick;
            _timer.Start();

            _lastRefreshed = DateTime.Now;

            BandOutput.Text = "Startup time: " + _initTime.Elapsed.Seconds + "." + _initTime.Elapsed.Milliseconds + " s";
            InitClearTimer();
        }

        private async void TimerOnTick(object sender, object o)
        {
            await PullSensors();
        }

        private void EndWork()
        {
            // TODO save end of work data (and send it to DB/web dashboard?)

            _timer.Stop();
            _timer = null;

            _initTime.Stop();
            _initTime.Reset();
            TimeOutput.Text = "Not started";

            _initSteps = 0;
            _steps = 0;
            StepsOutput.Text = "Not started";

            _initDistance = 0;
            _distance = 0;
            DistanceOutput.Text = "Not started";

            _heartRate = 0;
            _heartRateHigh = 0;
            _heartRateLow = 0;
            HeartRateOutput.Text = "Not started";
            HeartRateLow.Text = "Not started";
            HeartRateHigh.Text = "Not started";


            TempOutput.Text = "Not started";
            LatOutput.Text = "Not started";
            LongOutput.Text = "Not started";

            BandOutput.Text = "Task ended";
            InitClearTimer();
        }

        private async void RefreshClick(object sender, RoutedEventArgs e)
        {
            await PullSensors();
        }

        private async Task PullSensors()
        {
            // TODO check whether band is still connected and handle band not connected

            if (!_started) return;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            BandOutput.Text = "Refreshing...";

            UpdateTime();

            var steps = await _bandManager.GetPedometer(StepsOutput) - _initSteps;
            StepsOutput.Text = steps.ToString();

            var distance = await _bandManager.GetDistance(DistanceOutput) - _initDistance;
            DistanceOutput.Text = Convert.ToDecimal(distance) / 100 + " m";

            _heartRate = await _bandManager.DisplayHeartRate(HeartRateOutput);
            if (_heartRate < _heartRateLow)
            {
                HeartRateLow.Text = _heartRate.ToString();
                _heartRateLow = _heartRate;
            }
            if (_heartRate > _heartRateHigh)
            {
                HeartRateHigh.Text = _heartRate.ToString();
                _heartRateHigh = _heartRate;
            }

            _temperature = await _bandManager.DisplaySkinTemperature(TempOutput);

            await OneShotLocation();

            UpdateTime();

            BandOutput.Text = "Refresh time: " + stopwatch.Elapsed.Seconds + "." + stopwatch.Elapsed.Milliseconds + " s";
            InitClearTimer();
        }

        private void InitClearTimer()
        {
            _clearTimer?.Stop();
            _clearTimer = null;
            _clearTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 15) }; // wait 15 sec
            _clearTimer.Tick += ClearTimerOnTick;
            _clearTimer.Start();
        }

        private void ClearTimerOnTick(object sender, object e)
        {
            BandOutput.Text = "";
            _clearTimer?.Stop();
            _clearTimer = null;
        }

        private void CheckInClick(object sender, RoutedEventArgs e)
        {
            if(!_started) return;
            var panic = new PanicString(_lastRefreshed, _lastStarted, _initTime.Elapsed, _steps - _initSteps, _distance - _initDistance, _heartRate, _heartRateLow, _heartRateHigh,
                _temperature, _latitude, _longitude);
            var json = panic.ToJsonString(false);
            Debug.WriteLine(json); // test code
            // TODO implement sending JSON string to HQ
        }

        private void PanicClick(object sender, RoutedEventArgs e)
        {
            if(!_started) return;
            var panic = new PanicString(_lastRefreshed, _lastStarted, _initTime.Elapsed, _steps - _initSteps, _distance - _initDistance, _heartRate, _heartRateLow, _heartRateHigh, 
                _temperature, _latitude, _longitude);
            var json = panic.ToJsonString(true);
            Debug.WriteLine(json); // test code
            // TODO implement sending JSON string to HQ
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

                _latitude = geoposition.Coordinate.Latitude;
                _longitude = geoposition.Coordinate.Longitude;

                if (_latitude >= 0)
                {
                    LatOutput.Text = _latitude + " N";
                }
                else
                {
                    LatOutput.Text = -_latitude + " S";
                }

                if (_longitude >= 0)
                {
                    LongOutput.Text = _longitude + " E";
                }
                else
                {
                    LongOutput.Text = -_longitude + " W";
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

        private void ComboBox_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            
           if (SourceBox.SelectedItem.ToString() == "Profile")
              {
                 Frame.Navigate(typeof(Profile));
              }
           else if (SourceBox.SelectedItem.ToString() == "Dashboard)")
           {
                 Frame.Navigate(typeof(MainPage));
           }
           else if (SourceBox.SelectedItem.ToString() == "Notifications")
           {
                 Frame.Navigate(typeof(NotificationsPage));
                
           }
    }
}
