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
        private Tox tox;

        //check https://wiki.tox.im/Nodes for an up-to-date list of nodes
        private ToxNode[] Nodes = new ToxNode[]
        {
            new ToxNode("178.62.250.138", 33445, new ToxKey(ToxKeyType.Public, "788236D34978D1D5BD822F0A5BEBD2C53C64CC31CD3149350EE27D4D9A2F9B6B"))
        };

        private ObservableCollection<ToxAccount> myFriends = new ObservableCollection<ToxAccount>();


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

            foreach (int friendNumber in tox.FriendList)
            {
                ToxAccount f = new ToxAccount();
                f.Name = tox.GetName(friendNumber);
                f.Status = tox.GetStatusMessage(friendNumber);
                myFriends.Add(f);
            }
            myFriends.Add(new ToxAccount() { Name = "komalo", Status = "offline" });

            tox.OnFriendRequest += tox_OnFriendRequest;
            tox.OnFriendMessage += tox_OnFriendMessage;

            foreach (ToxNode node in Nodes)
                tox.BootstrapFromNode(node);

            string displayName = await UserInformation.GetDisplayNameAsync();

            tox.Name = String.IsNullOrEmpty(displayName) ? "New User" : displayName;

            tox.StatusMessage = "Hello World!";

            tox.Start();

            string id = tox.Id.ToString();
            myIDtxt.Text = id;
        }

        private void tox_OnFriendMessage(object sender, ToxEventArgs.FriendMessageEventArgs e)
        {
            //get the name associated with the friendnumber
            string name = tox.GetName(e.FriendNumber);

            //print the message to the console
            System.Diagnostics.Debug.WriteLine("<{0}> {1}", name, e.Message);
        }

        private void tox_OnFriendRequest(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            //automatically accept every friend request we receive
            int friendNumber = tox.AddFriendNoRequest(new ToxKey(ToxKeyType.Public, e.Id));
            
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                ToxAccount f = new ToxAccount();
                f.Id = e.Id;
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
    }
}
