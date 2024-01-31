using NEA;
using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Snackbar;
using Android.Content;
using Android.Widget;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Android.Hardware;
using Android;
using Plugin.Media;
using System.Drawing;
using Android.Graphics;
using Bitmap = Android.Graphics.Bitmap;

namespace NEA
{
    [Activity(Label = "@string/home", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        readonly string[] permissionGroup =
        {
            //Needed to request permission to camera and files
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += (s, arg) =>
            {
                View view = LayoutInflater.Inflate(Resource.Layout.choice_menu, null);
                PopupWindow popupWindow = new PopupWindow(view, LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent, true);
                popupWindow.ShowAtLocation(view, GravityFlags.Center, 0, 725);

                ImageButton menu_cam = FindViewById<ImageButton>(Resource.Id.menu_cam);
                if (menu_cam != null)
                {
                    menu_cam.Click += menu_camClick;
                }
            };

            
            ImageButton menu_gallery = FindViewById<ImageButton>(Resource.Id.menu_gallery);
            ImageButton menu_draw = FindViewById<ImageButton>(Resource.Id.menu_draw);

            
            RequestPermissions(permissionGroup, 0);



            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
        }

        public override void OnBackPressed()
        {
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            if(drawer.IsDrawerOpen(GravityCompat.Start))
            {
                drawer.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        //Options menus
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }
        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_profile)
            {
                var Profile = new Intent(this, typeof(ProfileActivity));
                StartActivity(Profile);
            }
            else if (id == Resource.Id.nav_pairings)
            {
                var Pairings = new Intent(this, typeof(PairingActivity));
                StartActivity(Pairings);
            }
            

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        //App permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        //Camera Capture
        public void menu_camClick(object sender, EventArgs e)
        {
            TakePhoto();
        }
    
        async void TakePhoto()
        {
            await CrossMedia.Current.Initialize();

            //To capture image and set attributes of the image (size, compression..)
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 40
            }
            ) ;

            //if null

            if(file == null)
            {
                return;
            }

            //Convert file to byte array
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);

            //Create temporary imageview
            ImageView imageView = new ImageView(this);
            imageView.SetImageBitmap(bitmap);
        }
    }
}


