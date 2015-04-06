using SharpTox.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
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

            this.DataContext = (App.Current as App);
            conversationsListView.ItemsSource = (App.Current as App).Conversations;
        }

        private void SelectedConversationChanged(object sender, SelectionChangedEventArgs e)
        {
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
            Frame.Navigate(typeof(FriendsPage), (App.Current as App).Friends);
        }

        private void CopyID(object sender, RoutedEventArgs e)
        {
            DataPackage myIdPackage = new DataPackage();
            myIdPackage.SetText((App.Current as App).MyAccount.Id);

            Clipboard.SetContent(myIdPackage);

            showIdButton.Flyout.Hide();
        }
        
    }
}
