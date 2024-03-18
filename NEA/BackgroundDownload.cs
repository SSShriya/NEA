using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.Work;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NEA
{
    public class BackgroundDownload : Worker
    {
        public BackgroundDownload(Context context, WorkerParameters workerParameters) : base(context,workerParameters)
        { 
            
        }

        
        public override Result DoWork()
        {
            //Gets user
            User user = GetUser();
            try
            {
                if (user != null)
                {
                    //gets friends list
                    user.GetFriends();
                    //downloads images
                    FirebaseDownload download = new FirebaseDownload(user);
                    download.DownloadImage();
                    Log.WriteLine(LogPriority.Info, "BACKGROUND SUCCESS", "success");
                }
                return Result.InvokeSuccess();
            }
            //if unsuccessful
            catch
            {
                return Result.InvokeFailure();
            }
        }

        //reads user details
        private User GetUser()
        {
            string path = Application.Context.GetExternalFilesDir(null).ToString();
            string folderPath = Path.Combine(path, "userdetails");
            if (!Directory.Exists(folderPath))
            {
                return null;
            }
            string filePath = Path.Combine(folderPath, "user.txt");
            if (File.Exists(filePath))
            {
                StreamReader reader = new StreamReader(filePath);
                int id = Convert.ToInt32(reader.ReadLine());
                string username = reader.ReadLine();
                reader.Close();
                User user = new User(id, username);
                return user;
            }
            else
            {
                return null;
            }
        }
    }
}