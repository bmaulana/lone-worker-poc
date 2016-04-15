﻿using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace LoneWorkerPoC
{
    /// <summary>
    /// The main page of the LoneWorkerPoC app.
    /// </summary>
    public sealed partial class MainPage
    {
        public static readonly BandManager BandManager = new BandManager();
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
                if (BandManager.IsConnected()) // band already connected from another page
                {
                    BandOutput.Text = "Connected to Band.";
                    _connected = true;
                    InitClearTimer();
                    return;
                }
                BandOutput.Text = "Connecting to Band...";
                if (await BandManager.ConnectTask())
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
            TimeOutput.Text = "0h 0m 0s";

            _initSteps = await BandManager.GetPedometer(StepsOutput);
            _steps = 0;
            StepsOutput.Text = "0";

            _initDistance = await BandManager.GetDistance(DistanceOutput);
            _distance = 0;
            DistanceOutput.Text = "0 m";

            _heartRate = await BandManager.DisplayHeartRate(HeartRateOutput);
            _heartRateLow = _heartRate;
            HeartRateLow.Text = _heartRate.ToString();
            _heartRateHigh = _heartRate;
            HeartRateHigh.Text = _heartRate.ToString();

            _temperature = await BandManager.DisplaySkinTemperature(TempOutput);

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
            TimeOutput.Text = "N/A";

            _initSteps = 0;
            _steps = 0;
            StepsOutput.Text = "N/A";

            _initDistance = 0;
            _distance = 0;
            DistanceOutput.Text = "N/A";

            _heartRate = 0;
            _heartRateHigh = 0;
            _heartRateLow = 0;
            HeartRateOutput.Text = "N/A";
            HeartRateLow.Text = "N/A";
            HeartRateHigh.Text = "N/A";

            TempOutput.Text = "N/A";
            LatOutput.Text = "N/A";
            LongOutput.Text = "N/A";

            BandOutput.Text = "Work ended";
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

            var steps = await BandManager.GetPedometer(StepsOutput) - _initSteps;
            StepsOutput.Text = steps.ToString();

            var distance = await BandManager.GetDistance(DistanceOutput) - _initDistance;
            DistanceOutput.Text = Convert.ToDecimal(distance) / 100 + " m";

            _heartRate = await BandManager.DisplayHeartRate(HeartRateOutput);
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

            _temperature = await BandManager.DisplaySkinTemperature(TempOutput);

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

        private async void CheckInClick(object sender, RoutedEventArgs e)
        {
            if (!_started) return;
            var panic = new PanicString(_lastRefreshed, _lastStarted, _initTime.Elapsed, _steps, _distance, _heartRate, _heartRateLow, _heartRateHigh,
                _temperature, _latitude, _longitude);
            var json = panic.ToJsonString(false);
            Debug.WriteLine(json); // test code
            // TODO implement sending JSON string to DB
            await SendPostRequest();
        }

        private async void PanicClick(object sender, RoutedEventArgs e)
        {
            if (!_started) return;
            var panic = new PanicString(_lastRefreshed, _lastStarted, _initTime.Elapsed, _steps, _distance, _heartRate, _heartRateLow, _heartRateHigh,
                _temperature, _latitude, _longitude);
            var json = panic.ToJsonString(true);
            Debug.WriteLine(json); // test code
            // TODO implement sending JSON string to DB
            await SendPostRequest();
        }


        private async Task SendPostRequest()
        {
            var values = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("api_key", "12345"),
                        new KeyValuePair<string, string>("game_id", "123456")
                    };
            var url = "https://polar-thicket-50764.herokuapp.com/test";

            var handler = new HttpClientHandler();
            var httpClient = new HttpClient(handler);
            var request = new HttpRequestMessage(HttpMethod.Post, url) {Content = new FormUrlEncodedContent(values)};
            if (handler.SupportsTransferEncodingChunked())
            {
                request.Headers.TransferEncodingChunked = true;
            }
            var response = await httpClient.SendAsync(request);
            Debug.WriteLine(response);
        }

        private void UpdateTime()
        {
            TimeOutput.Text = _initTime.Elapsed.Hours + "h " + _initTime.Elapsed.Minutes + "m " + _initTime.Elapsed.Seconds + "s";
        }

        private async Task OneShotLocation()
        {
            //TODO improve time, use newer non deprecated API, ask for consent

            var geolocator = new Geolocator { DesiredAccuracyInMeters = 50 };

            LatOutput.Text = "Refreshing";
            LongOutput.Text = "Refreshing";

            try
            {
                var geoposition = await geolocator.GetGeopositionAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(10));

                _latitude = geoposition.Coordinate.Latitude;
                _longitude = geoposition.Coordinate.Longitude;

                if (_latitude >= 0)
                {
                    LatOutput.Text = Math.Round(_latitude, 5) + " N";
                }
                else
                {
                    LatOutput.Text = Math.Round(-_latitude, 5) + " S";
                }

                if (_longitude >= 0)
                {
                    LongOutput.Text = Math.Round(_longitude, 5) + " E";
                }
                else
                {
                    LongOutput.Text = Math.Round(-_longitude, 5) + " W";
                }
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    LatOutput.Text = "Location disabled";
                    LongOutput.Text = "Location disabled";
                }
                else
                {
                    LatOutput.Text = "Error";
                    LongOutput.Text = "Error";
                }
            }
        }

        private void NavigateToProfile(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProfilePage));
        }

        private void NavigateToNotifications(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(NotificationsPage));
        }
    }

}