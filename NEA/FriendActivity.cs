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
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using AndroidX.CardView.Widget;
using System.Runtime.Remoting.Contexts;
using Google.Android.Material.Card;

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
            ShapeDrawable shapeDrawable = new ShapeDrawable(new RoundRectShape(new float[] { 10, 10, 10, 10, 10, 10, 10, 10 }, null, null));
            shapeDrawable.Paint.Color = Color.ParseColor("#FFFFFF"); // Background color
            shapeDrawable.Paint.SetStyle(Paint.Style.Stroke);
            shapeDrawable.Paint.StrokeWidth = 5; // Border width
            shapeDrawable.Paint.SetPathEffect(new CornerPathEffect(10)); // Rounded corners
            shapeDrawable.Paint.Color = Color.ParseColor("#FF0000"); // Border color

            //half screen size
            var info = DeviceDisplay.MainDisplayInfo;

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
                    //textview with time+date
                    DateTime filetime = new FileInfo(files[i]).LastWriteTime;
                    TextView textView = new TextView(this);
                    textView.Text = filetime.ToString("dd/MM/yyyy HH:mm");
                    LinearLayout.LayoutParams textparams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent) { LeftMargin =20, BottomMargin = 35, RightMargin = 20, TopMargin = 0 };


                    //imageview
                    ImageView image = new ImageView(this);
                    image.Id = i;
                    LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(Convert.ToInt32(info.Width * 0.6), Convert.ToInt32(info.Width * 0.6));

                    //gets sender of image to put image on left or right accordingly
                    ExifInterface exif = new ExifInterface(files[i]);
                    var sender = exif.GetAttribute(ExifInterface.TagArtist);
                    if (sender == currentID.ToString())
                    {
                        layoutParams.Gravity = GravityFlags.Right;
                        textparams.Gravity = GravityFlags.Right;

                    }
                    else
                    {
                        layoutParams.Gravity = GravityFlags.Left;
                        textparams.Gravity = GravityFlags.Left;

                    }
                    layoutParams.SetMargins(10, 25, 10, 10);
                    textView.LayoutParameters = textparams;

                    //create cardview to hold image
                    MaterialCardView cardView = new MaterialCardView(this);
                    cardView.LayoutParameters = layoutParams;
                    cardView.Radius = 25;
                    cardView.StrokeColor = Color.ParseColor("#cf8abe");
                    cardView.StrokeWidth = 7;
                    cardView.SetContentPadding(6, 6, 6, 6);

                    image.SetScaleType(ImageView.ScaleType.CenterCrop);
                    image.SetImageURI(Android.Net.Uri.FromFile(new Java.IO.File(files[i])));
                    cardView.AddView(image);
                    imageLayout.AddView(cardView);
                    imageLayout.AddView(textView);

                }
            }
        }
    }
}