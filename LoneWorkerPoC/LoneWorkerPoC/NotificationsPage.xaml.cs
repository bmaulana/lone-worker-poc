using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace LoneWorkerPoC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NotificationsPage
    {
        private DispatcherTimer _clearTimer;

        public NotificationsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            RefreshNotifications();
        }

        private void RefreshNotifications()
        {
            // Displays the last five notifications
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var notifs = localSettings.Values.ContainsKey("NotifList") ? (string)localSettings.Values["NotifList"] : null;
            if (notifs == null) return;

            var notifsArray = JsonConvert.DeserializeObject<string[]>(notifs);
            var notifsString = "";
            notifsString = notifsArray.Where(notif => notif != "").Aggregate(notifsString, (current, notif) => current + notif + "\n");
            NotifOutput.Text = notifsString;
        }

        private void NavigateToDashboard(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void NavigateToProfile(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProfilePage));
        }

        private async void BandNotifClick(object sender, RoutedEventArgs e)
        {
            var bandManager = MainPage.BandManager;
            if (!bandManager.IsConnected())
            {
                if (!await bandManager.ConnectTask())
                {
                    BandOutput.Text = "Band not connected.";
                    return;
                }
            }
            await bandManager.SendNotification(BandOutput, TitleInput.Text, BodyInput.Text);
            InitClearTimer();
        }

        private async void HqNotifClick(object sender, RoutedEventArgs e)
        {
            var notifString = new NotifString(TitleInput2.Text, BodyInput2.Text);
            await HttpManager.SendPostRequest(notifString.ToKeyValuePairs());
            BandOutput.Text = "Message sent";
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

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            RefreshNotifications();
        }

        private void ClearClick(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["NotifList"] = JsonConvert.SerializeObject(new string[5]);
            RefreshNotifications();
        }
    }
}
