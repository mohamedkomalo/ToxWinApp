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
        }

        

        private async void tox_OnFriendMessage(object sender, ToxEventArgs.FriendMessageEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.FriendNumber);

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Friend senderFriend = myFriends.First(f => f.FriendNumber == e.FriendNumber);

                senderFriend.Conversation.Add(new Message() { Sender = senderFriend, Content = e.Message });

                System.Diagnostics.Debug.WriteLine("<{0}> {1}", senderFriend.Id, e.Message);
            });

        }

        private async void tox_OnFriendRequest(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            //automatically accept every friend request we receive
            int friendNumber = ToxController.Tox.AddFriendNoRequest(new ToxKey(ToxKeyType.Public, e.Id));

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Friend f = new Friend();
                f.Id = e.Id;
                f.FriendNumber = friendNumber;
                f.Name = ToxController.Tox.GetName(friendNumber);

                while (String.IsNullOrEmpty(f.Name))
                {
                    Task.Delay(30);
                    f.Name = ToxController.Tox.GetName(friendNumber);
                }

                f.Status = ToxController.Tox.GetStatusMessage(friendNumber);

                myFriends.Add(f);
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
