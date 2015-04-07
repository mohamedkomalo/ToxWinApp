using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToxWinApp
{
    public class RequestsList : ObservableCollection<RecievedFriendRequest>
    {
        public bool SendRequest(String toxId, String requestMessage)
        {
            return ToxController.Instance.tox.AddFriend(new SharpTox.Core.ToxId(toxId), requestMessage) > 0;
        }
    }
}
