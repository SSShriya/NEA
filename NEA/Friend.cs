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

        public void AddPairing()
        {
            //Writes friend details to file pairings.txt
            string path = Application.Context.GetExternalFilesDir(null).ToString();
            string folderPath = Path.Combine(path, "userdetails");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            string filePath = Path.Combine(folderPath, "pairings.txt");
            StreamWriter writer = new StreamWriter(filePath, append: true);
            writer.WriteLine(userID);
            writer.WriteLine(name);
            writer.WriteLine(pairingID);
            writer.Flush();
            writer.Close();
        }
    }
}