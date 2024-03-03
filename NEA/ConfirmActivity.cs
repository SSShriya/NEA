using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Graphics;
using AndroidX.DrawerLayout.Widget;

namespace NEA
{
    [Activity(Label = "Confirm Image", Theme = "@style/AppTheme.NoActionBar")]
    public class ConfirmActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_confirm);

            byte[] imageArray = Intent.GetByteArrayExtra("image");
            ImageSelected(imageArray);

            //Toolbar
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }

        public void ImageSelected(byte[] imageArray)
        {
            //Create bitmap
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);

            //Create temporary imageview
            ImageView imageView = FindViewById<ImageView>(Resource.Id.selectedImage);
            imageView.SetImageBitmap(bitmap);
        }
    }
}