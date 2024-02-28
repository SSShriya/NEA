using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NEA
{
    public class User 
    {
        public int currentID; 
        public string currentUsername;
        public User(int id, string username)
        {
            currentID = id;
            currentUsername = username;
        }
    }
}