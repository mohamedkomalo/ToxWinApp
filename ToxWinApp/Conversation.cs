using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToxWinApp
{
    public class ConversationsList : ObservableCollection<Conversation> { }

    public class FriendssList : ObservableCollection<Friend> { }

    public class Conversation : ObservableCollection<Message>
    {
        public ObservableCollection<Friend> Members { get; set; }
        public String Title
        {
            get
            {
                StringBuilder title = new StringBuilder();

                for (int i = 0; i < Members.Count; i++)
                {
                    title.Append(Members[i].Name);

                    if (i != Members.Count - 1)
                        title.Append(", ");
                }
                return title.ToString();
            }
        }

        public bool SendMessage(String messageBody){
            bool isSent = Tox.SendMessage(Members[0].FriendNumber, messageBody) > 0;

            if (isSent)
            {
                this.Add(new Message() { Sender = ToxController.Instance.MyAccount, Content = messageBody });
            }

            return isSent;
        }

        private SharpTox.Core.Tox Tox
        {
            get { return ToxController.Instance.tox; }
        }
    }
}
