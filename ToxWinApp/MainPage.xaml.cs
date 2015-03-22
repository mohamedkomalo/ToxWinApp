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
        private ObservableCollection<Friend> myFriends = new ObservableCollection<Friend>();
        private ObservableCollection<ToxAccount> myRequests = new ObservableCollection<ToxAccount>();
        private ToxAccount myAccount = new ToxAccount();
        
        public MainPage()
        {
            this.InitializeComponent();

            friendsListView.ItemsSource = myFriends;
            messagesListView.ItemsSource = null;

            init();
        }

        public async void init()
        {
            await ToxController.LoadTox();

            Tox tox = ToxController.Tox;

            tox.OnFriendRequest += tox_OnFriendRequest;
            tox.OnFriendMessage += tox_OnFriendMessage;

            foreach (int friendNumber in tox.FriendList)
            {
                Friend f = new Friend();
                f.Name = tox.GetName(friendNumber);
                f.Status = tox.GetStatusMessage(friendNumber);
                myFriends.Add(f);
            }

            Friend dummyFriend = new Friend() { FriendNumber = -1, Name = "komalo", Status = "offline" };
            dummyFriend.Conversation.Add(new Message() { Sender = new Friend(), Content = "AAAAAAAAAA" });

            myFriends.Add(dummyFriend);

            myAccount.Id = tox.Id.ToString();
            myAccount.Name = tox.Name;
            myAccount.Status = tox.StatusMessage;

            myIDtxt.Text = myAccount.Id;

            requestsListView.ItemsSource = myRequests;
            tox.OnNameChange += tox_OnNameChange;
            tox.OnUserStatus += tox_OnUserStatus;
        }

        async void tox_OnUserStatus(object sender, ToxEventArgs.UserStatusEventArgs e)
        {
            //throw new NotImplementedException();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Friend senderFriend = myFriends.First(f => f.FriendNumber == e.FriendNumber);
                senderFriend.Status = e.UserStatus.ToString();
            });
        }

        private async void tox_OnNameChange(object sender, ToxEventArgs.NameChangeEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Friend senderFriend = myFriends.First(f => f.FriendNumber == e.FriendNumber);
                senderFriend.Name = e.Name;
            });
        }

        private async void tox_OnFriendMessage(object sender, ToxEventArgs.FriendMessageEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Friend {0} sent: {1}", e.FriendNumber, e.Message);

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Friend senderFriend = myFriends.First(f => f.FriendNumber == e.FriendNumber);

                senderFriend.Conversation.Add(new Message() { Sender = senderFriend, Content = e.Message });

                System.Diagnostics.Debug.WriteLine("<{0}> {1}", senderFriend.Id, e.Message);
            });

        }

        private async void tox_OnFriendRequest(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Request recieved {0}", e.Id);
            //automatically accept every friend request we receive
            
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                myRequests.Add(new ToxAccount() { Id = e.Id, Name = e.Id.ToString() });
            });

        }

        private async void SelectedRequestChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            ToxAccount account = (ToxAccount) e.AddedItems[0];

            MessageDialog confirmRequest = new MessageDialog("Accept this friend request?");
            
            bool confirm = false;
            
            confirmRequest.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler((cmd) => {confirm = true;})));
            confirmRequest.Commands.Add(new UICommand("No", new UICommandInvokedHandler((cmd) => { confirm = false; })));

            await confirmRequest.ShowAsync();

            if(!confirm) return;

            int friendNumber = ToxController.Tox.AddFriendNoRequest(new ToxKey(ToxKeyType.Public, account.Id));

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Friend f = new Friend();
                f.Id = account.Id;
                f.FriendNumber = friendNumber;
                f.Name = ToxController.Tox.GetName(friendNumber);

                while (String.IsNullOrEmpty(f.Name))
                {
                    Task.Delay(30);
                    f.Name = ToxController.Tox.GetName(friendNumber);
                }

                f.Status = ToxController.Tox.GetStatusMessage(friendNumber);

                myFriends.Add(f);
                myRequests.Remove(account);
            });
        }

        private void SelectedFriendChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Select changed" + e.ToString());
            if (e.AddedItems.Count > 0)
            {
                Friend found = (Friend)e.AddedItems[0];
                messagesListView.ItemsSource = found.Conversation;
            }
        }
        private async void SendRequestClick(object sender, RoutedEventArgs e)
        {
            int friendNumber = ToxController.Tox.AddFriend(new ToxId(requestIDText.Text), "Hi hi");

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Friend f = new Friend();
                f.Id = requestIDText.Text;
                f.FriendNumber = friendNumber;
                
                f.Name = requestIDText.Text;
                
                myFriends.Add(f);
            });

        }
        
        private void SendClick(object sender, RoutedEventArgs e)
        {
            if (friendsListView.SelectedItems.Count > 0)
            {
                string messageBody = sendMessageText.Text;
                Friend reciptFriend = (Friend)friendsListView.SelectedItems[0];
                int isSent = ToxController.Tox.SendMessage(reciptFriend.FriendNumber, messageBody);

                if (isSent > 0)
                {
                    reciptFriend.Conversation.Add(new Message() { Sender = myAccount, Content = messageBody });
                    sendMessageText.Text = String.Empty;
                }
            }
        }
    }
}
