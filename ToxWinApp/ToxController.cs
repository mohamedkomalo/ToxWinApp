using SharpTox.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Xaml;

namespace ToxWinApp
{
    class ToxController
    {
        //check https://wiki.tox.im/Nodes for an up-to-date list of nodes
        private static ToxNode[] Nodes = new ToxNode[]
        {
            new ToxNode("178.62.250.138", 33445, new ToxKey(ToxKeyType.Public, "788236D34978D1D5BD822F0A5BEBD2C53C64CC31CD3149350EE27D4D9A2F9B6B"))
        };

        private static Tox tox;

        public static Tox Tox
        {
            get { return tox; }
        }

        const string TOX_DATA_SETTING = "TOX_DATA";

        private static ToxData ToxSavedData{
            set
            {
                ApplicationData.Current.LocalSettings.Values[TOX_DATA_SETTING] = value;
            }
            get
            {
                Object toxData = ApplicationData.Current.LocalSettings.Values[TOX_DATA_SETTING];
                
                return toxData == null ? null : ToxData.FromBytes((byte[]) toxData);
            }
        }

        public static async Task LoadTox()
        {
            ToxOptions options = new ToxOptions(true, false);
            
            tox = new Tox(options);

            ToxData savedToxData = ToxSavedData;
            if (savedToxData != null)
            {
                tox.Load(savedToxData);
            }

            foreach (ToxNode node in Nodes)
                tox.BootstrapFromNode(node);

            string displayName = await UserInformation.GetDisplayNameAsync();


            if (String.IsNullOrEmpty(tox.Name))
            {
                tox.Name = String.IsNullOrEmpty(displayName) ? "New User" : displayName;
                tox.StatusMessage = "Hello World!";
            }

            tox.Start();
        }

        static void UnloadTox()
        {
            ToxSavedData = tox.GetData();

            tox.Dispose();
        }
    }
}
