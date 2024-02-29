using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NEA
{
    public class User 
    {
        public int currentID; 
        public string currentUsername;
        public List<Friend> userFriends; 
        public User(int id, string username)
        {
            currentID = id;
            currentUsername = username;
        }

        public void GetFriends()
        {
            string path = Application.Context.GetExternalFilesDir(null).ToString();
            string filePath = Path.Combine(path, "userdetails", "pairings.txt");
            if (File.Exists(filePath))
            {
                StreamReader reader = new StreamReader(filePath);
                while (!reader.EndOfStream)
                {
                    int friendId = Convert.ToInt32(reader.ReadLine());
                    string friendName = reader.ReadLine();
                    int friendPair = Convert.ToInt32(reader.ReadLine());
                    Friend newFriend = new Friend(friendId, friendName, friendPair);
                    userFriends.Add(newFriend);
                }
            }
        }
    }
}