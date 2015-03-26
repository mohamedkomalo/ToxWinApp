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
            new ToxNode("178.62.250.138", 33445, new ToxKey(ToxKeyType.Public, "788236D34978D1D5BD822F0A5BEBD2C53C64CC31CD3149350EE27D4D9A2F9B6B")),
            new ToxNode("23.226.230.47", 33445, new ToxKey(ToxKeyType.Public, "A09162D68618E742FFBCA1C2C70385E6679604B2D80EA6E84AD0996A1AC8A074")),
            new ToxNode("195.154.119.113", 33445, new ToxKey(ToxKeyType.Public, "E398A69646B8CEACA9F0B84F553726C1C49270558C57DF5F3C368F05A7D71354")),
            new ToxNode(  "178.62.250.138",33445,new ToxKey(ToxKeyType.Public,  "788236D34978D1D5BD822F0A5BEBD2C53C64CC31CD3149350EE27D4D9A2F9B6B" )),
            new ToxNode(  "144.76.60.215",33445,new ToxKey(ToxKeyType.Public,  "04119E835DF3E78BACF0F84235B300546AF8B936F035185E2A8E9E0A67C8924F" )),
            new ToxNode(  "23.226.230.47",33445,new ToxKey(ToxKeyType.Public,  "A09162D68618E742FFBCA1C2C70385E6679604B2D80EA6E84AD0996A1AC8A074" )),
            new ToxNode(  "178.62.125.224",33445,new ToxKey(ToxKeyType.Public,  "10B20C49ACBD968D7C80F2E8438F92EA51F189F4E70CFBBB2C2C8C799E97F03E" )),
            new ToxNode(  "37.187.46.132",33445,new ToxKey(ToxKeyType.Public,  "5EB67C51D3FF5A9D528D242B669036ED2A30F8A60E674C45E7D43010CB2E1331" )),
            new ToxNode(  "178.21.112.187",33445,new ToxKey(ToxKeyType.Public,  "4B2C19E924972CB9B57732FB172F8A8604DE13EEDA2A6234E348983344B23057" )),
            new ToxNode(  "195.154.119.113",33445,new ToxKey(ToxKeyType.Public,  "E398A69646B8CEACA9F0B84F553726C1C49270558C57DF5F3C368F05A7D71354" )),
            new ToxNode(  "192.210.149.121",33445,new ToxKey(ToxKeyType.Public,  "F404ABAA1C99A9D37D61AB54898F56793E1DEF8BD46B1038B9D822E8460FAB67" )),
            new ToxNode(  "54.199.139.199",33445,new ToxKey(ToxKeyType.Public,  "7F9C31FE850E97CEFD4C4591DF93FC757C7C12549DDD55F8EEAECC34FE76C029" )),
            new ToxNode(  "76.191.23.96",33445,new ToxKey(ToxKeyType.Public,  "93574A3FAB7D612FEA29FD8D67D3DD10DFD07A075A5D62E8AF3DD9F5D0932E11" )),
            new ToxNode(  "46.38.239.179",33445,new ToxKey(ToxKeyType.Public,  "F5A1A38EFB6BD3C2C8AF8B10D85F0F89E931704D349F1D0720C3C4059AF2440A" )),
            new ToxNode(  "144.76.93.230",33445,new ToxKey(ToxKeyType.Public,  "2C308B4518862740AD9A121598BCA7713AFB25858B747313A4D073E2F6AC506C"))
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
