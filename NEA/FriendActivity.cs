using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using Android.Util;
using AndroidX.ExifInterface.Media;
using Android.Graphics;
using Path = System.IO.Path;
using Xamarin.Essentials;

namespace NEA
{
    [Activity(Label = "FriendActivity", Theme = "@style/AppTheme.NoActionBar")]
    public class FriendActivity : AppCompatActivity
    {
        private int pairingID;
        private int currentID;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_friend);
            string friendName = Intent.GetStringExtra("friendName");
            pairingID = Intent.GetIntExtra("pairingID", 0);
            currentID = Intent.GetIntExtra("currentID", 0);
            

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = friendName;
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            ScrollView scrollView = FindViewById<ScrollView>(Resource.Id.scrollView);

            scrollView.Post(() =>
            {
                scrollView.FullScroll(FocusSearchDirection.Down);
            });

            ShowImages();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var Homepage = new Intent(this, typeof(MainActivity));
            StartActivity(Homepage);
            Finish();
            return true;
        }

        private void ShowImages()
        {
            LinearLayout imageLayout = FindViewById<LinearLayout>(Resource.Id.imageLinear);

            //half screen size
            var info = DeviceDisplay.MainDisplayInfo;
            LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(Convert.ToInt32(info.Width/2), Convert.ToInt32(info.Width/2));

            string path = Application.Context.GetExternalFilesDir(null).ToString();
            string folderPath = Path.Combine(path, "images", $"{pairingID}" );
            if (!Directory.Exists(folderPath))
            {
                TextView textView = new TextView(this);
                textView.Text = "You have no images with this person yet. Go to the homepage to send an image!";
                textView.Gravity = GravityFlags.Center;
                textView.TextSize = 24;
                imageLayout.AddView(textView); 
            }
            else
            {
                string[] files = Directory.GetFiles(folderPath);
                files.OrderBy(f => new FileInfo(f).LastWriteTime);
                if(files.Length == 0)
                {
                    TextView textView = new TextView(this);
                    textView.Text = "You have no images with this person yet. Go to the homepage to send an image!";
                    textView.Gravity = GravityFlags.Center;
                    LinearLayout.LayoutParams textParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
                    textParams.SetMargins(20, 20, 20, 20);
                    textView.LayoutParameters = textParams;
                    textView.TextSize = 22;
                    imageLayout.AddView(textView);                   
                }

                for(int i=0;i<files.Length;i++)
                {
                    ImageView image = new ImageView(this);
                    image.Id = i;
                    
                    ExifInterface exif = new ExifInterface(files[i]);
                    var sender = exif.GetAttribute(ExifInterface.TagArtist);
                    if (sender == $"{currentID}")
                    {
                        layoutParams.Gravity = GravityFlags.Right;
                    }
                    layoutParams.BottomMargin = 25;
                    layoutParams.TopMargin = 25;
                    image.SetBackgroundResource(Resource.Drawable.canvas_border);
                    image.LayoutParameters = layoutParams;
                    
                    image.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(files[i])));
                    imageLayout.AddView(image);

                }
            }
        }
    }
}