using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.System.UserProfile;

namespace AroundMe
{
    class LockscreenHelpers
    {
        private const string BackgroundRoot = "Images/";
        private const string IconRoot = "Shared/ShellContent/";
        private const string LockScreenData = "LockScreenData.json";
        public static void CleanStorage()
        {
            using (IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                TryToDeleteAllFiles(storageFolder, BackgroundRoot);
                TryToDeleteAllFiles(storageFolder, IconRoot);
            }
        }
        public static void TryToDeleteAllFiles(IsolatedStorageFile storageFolder, string directory)
        {
            if (storageFolder.DirectoryExists(directory))
            {
                try
                {
                    string[] files = storageFolder.GetFileNames(directory);
                    foreach (string File in files)
                    {
                        storageFolder.DeleteFile(directory + files);

                    }
                }
                catch (Exception)
                {
                    //ignoring the exception
                }
             }
        }
        public static void SaveSelectedBackgroundScreens(List<FlickrImage> data)
        {
            string stringData = JsonConvert.SerializeObject(data);
            using (var storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var stream = storageFolder.CreateFile(LockScreenData))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(stringData);
                    }
                } 
            }
            
        }
        public static void SetRandomImageFromLocalStorage()
        {
            string fileData;
            using (IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storageFolder.FileExists(LockScreenData))
                    return;
                using (IsolatedStorageFileStream stream = storageFolder.OpenFile(LockScreenData, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        fileData = reader.ReadToEnd();
                    }
                }

            
            }
            List<FlickrImage> images = JsonConvert.DeserializeObject<List<FlickrImage>>(fileData);
            if (images != null)
            {
                Random rndNumber = new Random();
                int index = rndNumber.Next(images.Count);
                //set img as lockscreen
                SetImage(images[index].Image1024);
            } 
        }
        public static async void SetImage(Uri uri)
        {
            string fileName = uri.Segments[uri.Segments.Length - 1];
            string imageName = BackgroundRoot + fileName;
            string iconName = IconRoot + fileName;
            using (IsolatedStorageFile storageFolder = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!storageFolder.DirectoryExists(BackgroundRoot))
                    storageFolder.CreateDirectory(BackgroundRoot);

                if (!storageFolder.FileExists(imageName))
                {


                    using (IsolatedStorageFileStream stream = storageFolder.CreateFile(imageName))
                    {
                        HttpClient client = new HttpClient();
                        byte[] flickrResult = await client.GetByteArrayAsync(uri);
                        await stream.WriteAsync(flickrResult, 0, flickrResult.Length);
                    }
                    storageFolder.CopyFile(imageName, iconName);
                }

            }
            //set the lockscreen
            SetLockscreen(fileName);
        }
        private static async void SetLockscreen(string fileName)
        {
            bool hasAccessForLockscreen = LockScreenManager.IsProvidedByCurrentApplication;
            if (!hasAccessForLockscreen)
            {
                var accessRequested = await LockScreenManager.RequestAccessAsync();
                hasAccessForLockscreen = (accessRequested == LockScreenRequestResult.Granted);
            }

            if (hasAccessForLockscreen)
            {
                Uri imgUri = new Uri("ms-appdata:///Local/" + BackgroundRoot + fileName, UriKind.Absolute);
                LockScreen.SetImageUri(imgUri);
            }

            var mainTile = ShellTile.ActiveTiles.FirstOrDefault();

            if (mainTile != null)
            {
                Uri iconUri = new Uri("isostore:/" + IconRoot + fileName, UriKind.Absolute);
                var imgs = new List<Uri>();
                imgs.Add(iconUri);
                CycleTileData tileData = new CycleTileData();
                tileData.CycleImages = imgs;
                mainTile.Update(tileData);
            }
        }

    }
}
