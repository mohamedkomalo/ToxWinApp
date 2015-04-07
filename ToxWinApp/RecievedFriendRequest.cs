using SharpTox.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace ToxWinApp
{
    public class RecievedFriendRequest
    {
        public String Id { get; set; }
        public String Message { get; set; }

        public void Accept()
        {
            int friendNumber = ToxController.Instance.tox.AddFriendNoRequest(new ToxKey(ToxKeyType.Public, Id));

            Friend f = new Friend();
            f.Id = Id;
            f.FriendNumber = friendNumber;
            f.Name = f.Id;
            f.Status = ToxController.Instance.tox.GetStatusMessage(friendNumber);

            ToxController.Instance.Requests.Remove(this);
            ToxController.Instance.Friends.Add(f);

            Conversation friendConversation = new Conversation();
            friendConversation.Members = new ObservableCollection<Friend>();
            friendConversation.Members.Add(f);

            ToxController.Instance.Conversations.Add(friendConversation);
        }
    }
}
