using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToxWinApp
{
    public class Message
    {
        public ToxAccount Sender { get; set; }
        public string Content { get; set; }
    }
}
