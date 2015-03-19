using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToxWinApp
{
    class Friend : ToxAccount
    {
        public int FriendNumber { get; set; }

        private Conversation _conversation = new Conversation();
        public Conversation Conversation
        {
            get { return _conversation; }
            set { _conversation = value; }
        }
    }
}
