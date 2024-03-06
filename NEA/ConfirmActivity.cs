using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Media;
using Newtonsoft.Json;
using Firebase.Storage;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using System.IO;
using System.Drawing;
using Path = System.IO.Path;
using Bitmap = Android.Graphics.Bitmap;
using System.Linq;
using Google.Cloud.Storage.V1;

namespace NEA
{
    [Activity(Label = "Confirm Image", Theme = "@style/AppTheme.NoActionBar")]
    public class ConfirmActivity : AppCompatActivity
    {
        private List<Friend> userFriends;
        private Bitmap bitmap;
        private byte[] imageArray;
        private int currentID;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_confirm);

            currentID = Intent.GetIntExtra("id", 0);
            string jsonUserFriends = Intent.GetStringExtra("jsonUserFriends");
            userFriends = JsonConvert.DeserializeObject<List<Friend>>(jsonUserFriends);
            imageArray = Intent.GetByteArrayExtra("image");
            ImageSelected(imageArray);

            //Toolbar
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            //Button
            Button btn_confirm = FindViewById<Button>(Resource.Id.btn_confirm);
            btn_confirm.Click += ShowRecipient;
        }

        private void ImageSelected(byte[] imageArray)
        {
            //Create bitmap
            bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);

            //Create temporary imageview
            ImageView imageView = FindViewById<ImageView>(Resource.Id.selectedImage);
            imageView.SetImageBitmap(bitmap);
        }

        private void ShowRecipient(object sender, EventArgs e)
        {
            //alert dialog with friend list
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            LayoutInflater layoutInflater = LayoutInflater.FromContext(this);
            View view = layoutInflater.Inflate(Resource.Layout.menu_recipients, null);
            LinearLayout layout = view.FindViewById<LinearLayout>(Resource.Id.recipientLayout);

            List<CheckBox> recipientList = new List<CheckBox>();

            //Add radio buttons with friend usernames to radio group:
            foreach (Friend friend in userFriends)
            {
                CheckBox cbx = new CheckBox(this);
                cbx.Id = friend.pairingID;
                cbx.Text = friend.name;
                cbx.TextSize = 16;
                cbx.SetPadding(7, 7, 7, 7);
                layout.AddView(cbx);
                recipientList.Add(cbx);
            }

            builder.SetView(view);
            builder.SetTitle("Choose your recipient:");
            AlertDialog dialog = null;
            builder.SetPositiveButton("Confirm", (sender, args) =>
            {
                //Finds which boxes are checked
                List<int> recipients = new List<int>();
                foreach(CheckBox cbx in  recipientList)
                {
                    if(cbx.Checked)
                    {
                        recipients.Add(cbx.Id);
                    }
                }    

                //If no recipients checked
                if (recipients.Count == 0)
                {
                    Toast.MakeText(this, "Select a recipient.", ToastLength.Short).Show();
                    ShowRecipient(sender, e);
                }
                //Uploads image to firebase
                else
                {
                    Toast.MakeText(this, "Sending...", ToastLength.Short).Show();
                    foreach(int recipient in recipients)
                    {
                        SendImage(recipient);
                    }
                }
            });
            builder.SetNegativeButton("Cancel", (sender, args) =>
            {
                return;
            });
            dialog = builder.Create();
            dialog.Show();
        }

        private async void SendImage(int pairingID)
        {

            MemoryStream stream = new MemoryStream(imageArray);
            FirebaseStorage storage = new FirebaseStorage("nea-files-a5ba8.appspot.com", new FirebaseStorageOptions
            {
                ThrowOnCancel = true
            });
            string filename = $"{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}.png";
            FirebaseStorageReference reference = storage.Child($"{pairingID}/{currentID}/{filename}");

            try
            {
                var task = reference.PutAsync(stream);
                await task;
                Toast.MakeText(this, "Upload Succeeded", ToastLength.Short).Show();
                var Homepage = new Intent(this, typeof(MainActivity));
                StartActivity(Homepage);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Toast.MakeText(this, "Upload Failed", ToastLength.Short).Show();
                return;
            }
            FileSave save = new FileSave(filename, pairingID, imageArray, currentID);
            save.WriteFile();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var Homepage = new Intent(this, typeof(MainActivity));
            StartActivity(Homepage);
            Finish();
            return true;
        }
    }
}