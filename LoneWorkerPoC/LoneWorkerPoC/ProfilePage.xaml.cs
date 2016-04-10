using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace LoneWorkerPoC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfilePage
    {
        private DispatcherTimer _clearTimer;

        public ProfilePage()
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
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            NameInput.Text = localSettings.Values.ContainsKey("Name") ? (string) localSettings.Values["Name"] : "N/A";
            EmailInput.Text = localSettings.Values.ContainsKey("Email") ? (string)localSettings.Values["Email"] : "N/A";
            PasswordInput.Text = localSettings.Values.ContainsKey("Password") ? (string)localSettings.Values["Password"] : "N/A";
            HeightInput.Text = localSettings.Values.ContainsKey("Height") ? (string)localSettings.Values["Height"] : "N/A";
            WeightInput.Text = localSettings.Values.ContainsKey("Weight") ? (string)localSettings.Values["Weight"] : "N/A";
            DoBInput.Text = localSettings.Values.ContainsKey("DoB") ? (string)localSettings.Values["DoB"] : "N/A";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            localSettings.Values["Name"] = NameInput.Text;
            localSettings.Values["Email"] = EmailInput.Text;
            localSettings.Values["Password"] = PasswordInput.Text;
            localSettings.Values["Height"] = HeightInput.Text;
            localSettings.Values["Weight"] = WeightInput.Text;
            localSettings.Values["DoB"] = DoBInput.Text;
            SaveOutput.Text = "Profile saved.";
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
            SaveOutput.Text = "";
            _clearTimer?.Stop();
            _clearTimer = null;
        }

        private void NavigateToDashboard(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void NavigateToNotifications(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(NotificationsPage));
        }
    }
}
