using SharpTox.Core;
using System;
using System.Collections.Generic;
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
            f.Name = ToxController.Instance.tox.GetName(friendNumber);
            f.Status = ToxController.Instance.tox.GetStatusMessage(friendNumber);

            ToxController.Instance.Requests.Remove(this);
            ToxController.Instance.Friends.Add(f);
        }
    }
}
