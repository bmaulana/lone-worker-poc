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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace LoneWorkerPoC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NotificationsPage : Page
    {
        public NotificationsPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void NavigateToDashboard(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void NavigateToProfile(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(ProfilePage));
        }

        private void ComboBox_SelectionChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            if (SourceBox2?.SelectedItem == null) return;
            if (SourceBox2.SelectedItem.ToString() == "Profile")
            {
                Frame.Navigate(typeof(ProfilePage));
            }
            else if (SourceBox2.SelectedItem.ToString() == "Dashboard)")
            {
                Frame.Navigate(typeof(MainPage));
            }
            else if (SourceBox2.SelectedItem.ToString() == "Notifications")
            {
                Frame.Navigate(typeof(NotificationsPage));

            }
        }

        private void BandNotifClick(object sender, RoutedEventArgs e)
        {
            //TODO
        }

        private void HqNotifClick(object sender, RoutedEventArgs e)
        {
            //TODO
        }
    }
}
