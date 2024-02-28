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
    [Activity(Label = "@string/profile", Theme = "@style/AppTheme.NoActionBar")]
    public class ProfileActivity : AppCompatActivity
    {
        private string currentUsername;
        private int currentID;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            currentUsername = Intent.GetStringExtra("username");
            currentID = Intent.GetIntExtra("id", 0);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_profile);
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            TextView tbx_username = FindViewById<TextView>(Resource.Id.editUsername);
            tbx_username.Text = currentUsername;

            Button changeUsername = FindViewById<Button>(Resource.Id.btn_username);
            changeUsername.Click += ChangeUsername;
        }

        public void ChangeUsername(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            LayoutInflater layoutInflater = LayoutInflater.FromContext(this);
            View view = layoutInflater.Inflate(Resource.Layout.firstrun, null);
            builder.SetView(view);
            builder.SetCancelable(false);
            builder.SetTitle("Enter a username");
            AlertDialog dialog = null;
            builder.SetPositiveButton("Confirm", (sender, args) =>
            {
                //username validation
                EditText editText = view.FindViewById<EditText>(Resource.Id.username);
                if (editText.Text == "")
                {
                    Toast.MakeText(this, "Username cannot be empty", ToastLength.Short).Show();
                    ChangeUsername(sender, e);
                }
                else if (editText.Text.Length > 15)
                {
                    Toast.MakeText(this, "Username has to be less than 15 characters", ToastLength.Short).Show();
                    ChangeUsername(sender, e);
                }
                else
                {
                    Toast.MakeText(this, "Changing your username!", ToastLength.Short).Show();
                    var inputUsername = editText.Text.ToString();
                    CreateRequest(inputUsername);
                }
            });
            dialog = builder.Create();
            dialog.Show();
        }

        protected async void CreateRequest(string username)
        {
            APIRequest request = new APIRequest();
            string newUsername = await request.EditUsername(currentID, username);
            if (newUsername == "")
            {
                Toast.MakeText(this, "Username change unsuccessful.", ToastLength.Short).Show();
            }
            else
            {
                //Writes username and userID to file user.txt
                string path = Application.Context.GetExternalFilesDir(null).ToString();
                string folderPath = Path.Combine(path, "userdetails");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string filePath = Path.Combine(folderPath, "user.txt");
                StreamWriter writer = new StreamWriter(filePath);
                writer.WriteLine(currentID);
                writer.WriteLine(newUsername);
                writer.Flush();
                writer.Close();
                currentUsername = newUsername;
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