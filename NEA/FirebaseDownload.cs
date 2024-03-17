using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Storage;
using Org.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    public class FirebaseDownload
    {
        private User currentUser;
        public FirebaseDownload(User user) 
        {
            currentUser = user;
        }

        public async void DownloadImage()
        {
            if (currentUser.userFriends.ToList() != null)
            {
                FirebaseStorage storage = new FirebaseStorage("nea-files-a5ba8.appspot.com", new FirebaseStorageOptions
                {
                    ThrowOnCancel = true
                });

                //iterates through friends list to go to their folders in firebase using the pairing and userID
                foreach (Friend friend in currentUser.userFriends.ToList())
                {
                   

                    try
                    {
                        //gets an array of all the files in the folder of particular user
                        string[] paths = await GetFilesInFolder($"{friend.pairingID}/{currentUser.currentID}/");

                        if (paths != null || paths.Length!=0)
                        {
                            //iterates through this array and downloads each image by getting the download url for each
                            for (int i = 0; i < paths.Length; i++)
                            {
                                int index = paths[i].IndexOf('/', paths[i].IndexOf('/') + 1);
                                string name = paths[i].Substring(index + 1);
                                FirebaseStorageReference reference = storage.Child($"{paths[i]}");
                                var downloadurl = await reference.GetDownloadUrlAsync();

                                //reads downloaded data into byte array
                                byte[] imageArray = await DownloadWithURL(downloadurl);
                                if (imageArray != null)
                                {
                                    //saves image in appropriate location
                                    FileSave save = new FileSave(name, friend.pairingID, imageArray, friend.userID);
                                    save.WriteFile();
                                }

                                //deletes file from firebase
                                await reference.DeleteAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                    }
                }
            }
        }

        //Get list of files by sending GET request
        private async Task<string[]> GetFilesInFolder(string path)
        {
            const string baseUrl = "https://firebasestorage.googleapis.com/v0/b/nea-files-a5ba8.appspot.com/";
            string url = $"{baseUrl}o?prefix={path}";

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            var jsonResponse = "";

            try
            {
                //does request
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    //prints response
                    jsonResponse = await response.Content.ReadAsStringAsync();
                }

                //Gets 'items' array.
                JSONObject jsonobj = new JSONObject(jsonResponse);
                JSONArray items = jsonobj.GetJSONArray("items");

                //Iterates through items array, then prints the name of each file.
                string[] filenames = new string[items.Length()];
                for (int i = 0; i < items.Length(); i++)
                {
                    JSONObject name = items.GetJSONObject(i);
                    filenames[i] = name.GetString("name");
                }
                return filenames;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            return null;
        }

        //Download image from url
        private async Task<byte[]> DownloadWithURL(string url)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    byte[] imageArray = await response.Content.ReadAsByteArrayAsync();
                    return imageArray;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            return null;
        }
    }
}