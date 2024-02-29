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
    public class Friend
    {
        public int userID { get; private set; }
        public string name { get;private set; }

        public int pairingID { get; private set; }

        public Friend(int userID, string name, int pairingID)
        {
            this.userID = userID;
            this.name = name;
            this.pairingID = pairingID;
        }
    }
}