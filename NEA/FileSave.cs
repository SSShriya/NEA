using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.ExifInterface.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NEA
{
    public class FileSave
    {
        private string filename;
        private int pairingID;
        private byte[] imageArray;
        private int senderID;
        public FileSave(string name, int id, byte[] array, int userID)
        {
            filename = name;
            pairingID = id;
            imageArray = array;
            senderID = userID;
        }

        public void WriteFile()
        {
            //write file to app files            
            string path = Application.Context.GetExternalFilesDir(null).ToString();
            string folderPath = Path.Combine(path, "images", $"{pairingID}");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, $"{filename}");
            File.WriteAllBytes(filePath, imageArray);

            ExifInterface exif = new ExifInterface(filePath);
            exif.SetAttribute(ExifInterface.TagArtist, $"{senderID}");
            exif.SaveAttributes();
        }
    }
}