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

namespace NEA
{
    [Activity(Label = "Confirm Image", Theme = "@style/AppTheme.NoActionBar")]
    public class ConfirmActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_confirm);

            //Toolbar
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);
        }
    }
}