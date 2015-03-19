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

        //check https://wiki.tox.im/Nodes for an up-to-date list of nodes
        private ToxNode[] Nodes = new ToxNode[]
        {
            new ToxNode("178.62.250.138", 33445, new ToxKey(ToxKeyType.Public, "788236D34978D1D5BD822F0A5BEBD2C53C64CC31CD3149350EE27D4D9A2F9B6B"))
        };

        private Tox tox;

        private ObservableCollection<Friend> myFriends = new ObservableCollection<Friend>();
        private ToxAccount myAccount = new ToxAccount();

        public MainPage()
        {
            this.InitializeComponent();
            
            initTox();

            //tox.Dispose();
        }

        public async void initTox()
        {
            ToxOptions options = new ToxOptions(true, false);

            tox = new Tox(options);
            
            friendsListView.ItemsSource = myFriends;
            messagesListView.ItemsSource = null;

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

            tox.OnFriendRequest += tox_OnFriendRequest;
            tox.OnFriendMessage += tox_OnFriendMessage;

            foreach (ToxNode node in Nodes)
                tox.BootstrapFromNode(node);

            string displayName = await UserInformation.GetDisplayNameAsync();

            tox.Name = String.IsNullOrEmpty(displayName) ? "New User" : displayName;
            
            tox.StatusMessage = "Hello World!";

            tox.Start();

            myAccount.Id = tox.Id.ToString();
            myAccount.Name = tox.Name;
            myAccount.Status = tox.StatusMessage;

            myIDtxt.Text = myAccount.Id;
        }

        private void tox_OnFriendMessage(object sender, ToxEventArgs.FriendMessageEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(e.FriendNumber);

            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Friend senderFriend = myFriends.First(f => f.FriendNumber == e.FriendNumber);

                senderFriend.Conversation.Add(new Message() { Sender = senderFriend, Content = e.Message });
                
                System.Diagnostics.Debug.WriteLine("<{0}> {1}", senderFriend.Id, e.Message);
            });

        }

        private void tox_OnFriendRequest(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            //automatically accept every friend request we receive
            int friendNumber = tox.AddFriendNoRequest(new ToxKey(ToxKeyType.Public, e.Id));
            
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Friend f = new Friend();
                f.Id = e.Id;
                f.FriendNumber = friendNumber;
                f.Name = tox.GetName(friendNumber);

                while (String.IsNullOrEmpty(f.Name))
                {
                    Task.Delay(30);
                    f.Name = tox.GetName(friendNumber);
                }

                f.Status = tox.GetStatusMessage(friendNumber);

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
                int isSent = tox.SendMessage(reciptFriend.FriendNumber, messageBody);

                if (isSent > 0)
                {
                    reciptFriend.Conversation.Add(new Message() { Sender = myAccount, Content = messageBody });
                    sendMessageText.Text = String.Empty;
                }
            }
        }
    }
}
