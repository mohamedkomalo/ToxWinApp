using SharpTox.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.UserProfile;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ToxWinApp
{

    class ToxController
    {
        static ToxController _Instance;
        public static ToxController Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new ToxController();
                }

                return _Instance;
            }
        }

        const string TOX_DATA_FILE_NAME = "tox_data.tox";

        public readonly Tox tox;

        private ToxController()
        {
            ToxOptions options = new ToxOptions(true, false);

            tox = new Tox(options);

            foreach (ToxNode node in ToxNodes.Nodes)
                tox.BootstrapFromNode(node);

            init();
        }

        private async void init()
        {
            string displayName = await UserInformation.GetDisplayNameAsync();

            if (String.IsNullOrEmpty(tox.Name))
            {
                tox.Name = String.IsNullOrEmpty(displayName) ? "New User" : displayName;
                tox.StatusMessage = "Hello World!";
            }

            await LoadData();

            tox.Name = "New User";
            await SaveData();

            tox.Start();

            foreach (int friendNumber in tox.FriendList)
            {
                Friend f = new Friend();
                f.Name = tox.GetName(friendNumber);
                f.Status = tox.GetStatusMessage(friendNumber);
                _Friends.Add(f);
            }

            Friend dummyFriend = new Friend() { FriendNumber = -1, Name = "Dummy Friend", Status = "Hey everybody" };
            _Friends.Add(dummyFriend);

            foreach (Friend f in Friends)
            {
                Conversation friendConversation = new Conversation();
                friendConversation.Members = new ObservableCollection<Friend>();
                friendConversation.Members.Add(f);

                if (f.FriendNumber == -1)
                {
                    friendConversation.Add(new Message() { Sender = dummyFriend, Content = "Hello world" });
                }

                Conversations.Add(friendConversation);
            }

            _MyAccount.Id = tox.Id.ToString();
            _MyAccount.Name = tox.Name;
            _MyAccount.Status = tox.StatusMessage;

            tox.OnNameChange += tox_OnNameChange;
            tox.OnUserStatus += tox_OnUserStatus;
            tox.OnFriendRequest += tox_OnFriendRequest;

            tox.OnFriendMessage += tox_OnFriendMessage;

            Requests.Add(new RecievedFriendRequest() { Id = "75BC509C2AE5375C28F1EF9CAB53BD75DF0049F25FAE48AA3363A78E05CAB16D09DCE60DA7BF", Message = "Dummy request" });
        }

        public void UnLoad()
        {
            tox.Dispose();
        }

        private async void tox_OnUserStatus(object sender, ToxEventArgs.UserStatusEventArgs e)
        {
            await UIDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Friend senderFriend = _Friends.First(f => f.FriendNumber == e.FriendNumber);
                senderFriend.Status = e.UserStatus.ToString();
            });
        }

        private async void tox_OnNameChange(object sender, ToxEventArgs.NameChangeEventArgs e)
        {
            await UIDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Friend friend = _Friends.First(f => f.FriendNumber == e.FriendNumber);
                friend.Name = e.Name;
            });
        }

        private async void tox_OnFriendMessage(object sender, ToxEventArgs.FriendMessageEventArgs e)
        {
            await UIDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                Friend senderFriend = null;

                // Need someway to differntiate between normal and group conversation
                Conversation conversation = Conversations.First(c =>
                {
                    senderFriend = c.Members.FirstOrDefault(f => f.FriendNumber == e.FriendNumber);
                    return senderFriend != null;
                });

                conversation.Add(new Message() { Sender = senderFriend, Content = e.Message });

                //System.Diagnostics.Debug.WriteLine("Friend {0} sent: {1}", e.FriendNumber, e.Message);

                System.Diagnostics.Debug.WriteLine("<{0}> {1}", senderFriend.Name, e.Message);
            });
        }

        private async void tox_OnFriendRequest(object sender, ToxEventArgs.FriendRequestEventArgs e)
        {
            await UIDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Requests.Add(new RecievedFriendRequest() { Id = e.Id, Message = e.Message });
            });
        }

        private CoreDispatcher UIDispatcher
        {
            get { return CoreApplication.MainView.CoreWindow.Dispatcher; }
        }

        private async Task SaveData()
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(TOX_DATA_FILE_NAME, CreationCollisionOption.ReplaceExisting);
            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (DataWriter writer = new DataWriter(stream))
                {
                    writer.WriteBytes(tox.GetData().Bytes);
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                }
            }

        }

        private async Task LoadData()
        {
            byte[] toxData = null;
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(TOX_DATA_FILE_NAME);
                using (IRandomAccessStream stream = await file.OpenReadAsync())
                {
                    toxData = new byte[stream.Size];
                    using (DataReader reader = new DataReader(stream))
                    {
                        await reader.LoadAsync((uint)stream.Size);
                        reader.ReadBytes(toxData);
                    }
                }
            }
            catch (Exception) { }

            if (toxData != null)
            {
                tox.Load(ToxData.FromBytes((byte[])toxData));
                System.Diagnostics.Debug.WriteLine(tox.Name);
            }
            else
            {
                // Save the data for the first time
                //await SaveData();
            }
        }

        private ObservableCollection<Friend> _Friends = new ObservableCollection<Friend>();
        public ObservableCollection<Friend> Friends
        {
            get { return _Friends; }
        }

        private ObservableCollection<Conversation> _Conversation = new ObservableCollection<Conversation>();
        public ObservableCollection<Conversation> Conversations
        {
            get { return _Conversation; }
        }

        private RequestsList _Requests = new RequestsList();
        public RequestsList Requests
        {
            get { return _Requests; }
        }

        private ToxAccount _MyAccount = new ToxAccount();
        public ToxAccount MyAccount
        {
            get { return _MyAccount; }
        }
    }
}
