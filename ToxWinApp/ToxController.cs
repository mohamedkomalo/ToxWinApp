using SharpTox.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
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

        const string TOX_DATA_FILE_NAME = "tox_data.tox";

        private static async Task SaveData()
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
        private static async Task LoadData(){
            byte[] toxData = null;
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(TOX_DATA_FILE_NAME);
                using (IRandomAccessStream stream = await file.OpenReadAsync())
                {
                    toxData = new byte[stream.Size];
                    using (DataReader reader = new DataReader(stream))
                    {
                        await reader.LoadAsync((uint) stream.Size);
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

        public static async Task LoadTox()
        {
            ToxOptions options = new ToxOptions(true, false);
            
            tox = new Tox(options);

            foreach (ToxNode node in Nodes)
                tox.BootstrapFromNode(node);

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
        }

        static void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("suspend");
            UnloadTox();
        }

        static void UnloadTox()
        {
            tox.Dispose();
        }
    }
}
