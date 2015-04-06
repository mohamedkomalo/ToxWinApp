using SharpTox.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.UserProfile;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ToxWinApp
{
    public sealed partial class MainPage : Page
    {   
        public MainPage()
        {
            this.InitializeComponent();

            App app = (App.Current as App);
            //myIDtxt.Text = app.MyAccount.Id;

            conversationsListView.ItemsSource = app.Conversations;
        }

        private async void tox_OnFriendRequest(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Request recieved {0}", e.Id);
            ////automatically accept every friend request we receive
            
            //await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            //{
            //    myRequests.Add(new ToxAccount() { Id = e.Id, Name = e.Id.ToString() });
            //});
        }

        private void SelectedConversationChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Select changed" + e.ToString());
            if (e.AddedItems.Count > 0)
            {
                Conversation selectedConversation = (Conversation)e.AddedItems[0];
                Frame.Navigate(typeof(ConversationPage), selectedConversation);
            }
        }

        private void requestsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RequestsPage));
        }

        private void friendsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RequestsPage), (App.Current as App).Friends);
        }
        
    }
}
