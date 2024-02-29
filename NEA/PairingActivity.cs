using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.Snackbar;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using System.IO;


namespace NEA
{
    [Activity(Label = "@string/pairings", Theme = "@style/AppTheme.NoActionBar")]
    public class PairingActivity : AppCompatActivity
    {
        private int currentID;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            currentID = Intent.GetIntExtra("id", 0);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_pairings);
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            TextView showId = FindViewById<TextView>(Resource.Id.userID);
            showId.Text = Convert.ToString(currentID);

            Button btn_submit = FindViewById<Button>(Resource.Id.btn_pairing);
            btn_submit.Click += ValidateUserID;

        }

        public void ValidateUserID(object sender, EventArgs e)
        {
            EditText enterUID = FindViewById<EditText>(Resource.Id.enterUserID);
            string UID = enterUID.Text;
            int UIDint;
            if (UID == "")
            {
                Toast.MakeText(this, "Code cannot be empty", ToastLength.Short).Show();
                return;
            }
            else if (int.TryParse(UID, out UIDint) == false)
            {
                Toast.MakeText(this, "Invalid code", ToastLength.Short).Show();
                return;
            }
            UIDint = Convert.ToInt32(UID);
            if (UIDint <= 0)
            {
                Toast.MakeText(this, "Invalid code", ToastLength.Short).Show();
                return;
            }
            else
            {
                Toast.MakeText(this, "Pairing...", ToastLength.Short).Show();
                ShowUsername(UIDint);
            }
        }

        protected async void ShowUsername(int UID)
        {
            APIRequest request = new APIRequest();
            string username = await request.GetUser(UID);
            if (username == "")
            {
                Toast.MakeText(this, "Pairing unsuccessful.", ToastLength.Short).Show();
            }
            else
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Confirm Pairing");
                builder.SetMessage($"Do you want to pair with '{username}'");
                builder.SetPositiveButton("Confirm", (sender, args) =>
                {
                    CreatePairing(UID, username);
                });
                builder.SetNegativeButton("Cancel", (sender, args) =>
                {
                    return;
                });
                builder.Show();
            }
        }

        protected async void CreatePairing(int UID, string username)
        {
            APIRequest request = new APIRequest();
            string pairingID = await request.AddPairing(currentID, UID);
            if (pairingID == "")
            {
                Toast.MakeText(this, "Pairing unsuccessful.", ToastLength.Short).Show();
            }
            else
            {
                //Writes friend details to file pairings.txt
                string path = Application.Context.GetExternalFilesDir(null).ToString();
                string folderPath = Path.Combine(path, "userdetails");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string filePath = Path.Combine(folderPath, "pairings.txt");
                StreamWriter writer = new StreamWriter(filePath, append:true);
                writer.WriteLine(UID);
                writer.WriteLine(username);
                writer.WriteLine(pairingID);
                writer.Flush();
                writer.Close();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var Homepage = new Intent(this, typeof(MainActivity));
            StartActivity(Homepage);
            return true;
        }
    }

}