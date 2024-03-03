using System;
using System.IO;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Android.Widget;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Android;
using Plugin.Media;
using Android.Graphics;
using Bitmap = Android.Graphics.Bitmap;
using Plugin.Media.Abstractions;
using static Xamarin.Essentials.Platform;
using Intent = Android.Content.Intent;
using System.Threading.Tasks;
using AlertDialog = Android.App.AlertDialog;
using Android.Content;
using Path = System.IO.Path;
using Environment = System.Environment;
using static Android.Provider.ContactsContract;
using Directory = System.IO.Directory;
using AndroidX.Core.Content;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace NEA
{
    [Activity(Label = "@string/home", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
    {
        public User currentUser;

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
            Init(this, savedInstanceState);

            //if first run, creates new user
            if (Settings.FirstRun)
            {
                FirstRun();
                Settings.FirstRun = false;
            }
            else
            {
                currentUser = CreateObject();
            }

            //items on page
            SetContentView(Resource.Layout.activity_main);
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += (s, arg) =>
            {
                View view = LayoutInflater.Inflate(Resource.Layout.choice_menu, null, true);
                PopupWindow popupWindow = new PopupWindow(view, LinearLayout.LayoutParams.WrapContent, LinearLayout.LayoutParams.WrapContent, true);
                popupWindow.ShowAtLocation(view, GravityFlags.Center, 0, 725);

                //Camera click
                ImageButton menu_cam = view.FindViewById<ImageButton>(Resource.Id.menu_cam);
                
                menu_cam.Click += LaunchCamera;
                menu_cam.Click += (sender,e) => popupWindow.Dismiss();

                //Gallery click
                ImageButton menu_gallery = view.FindViewById<ImageButton>(Resource.Id.menu_gallery);
                menu_gallery.Click += LaunchGallery;
                menu_gallery.Click += (sender, e) => popupWindow.Dismiss();

                //Drawing click
                ImageButton menu_draw = view.FindViewById<ImageButton>(Resource.Id.menu_draw);
                menu_draw.Click += (sender, e) =>
                {
                    popupWindow.Dismiss();
                    var Canvas = new Intent(this, typeof(CanvasActivity));
                    StartActivityForResult(Canvas, 0);
                };


            };          
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();
            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);

            //get file + camera permissions
            RequestPermissions(permissionGroup, 0);

            //Get friends
            currentUser.GetFriends();
            ShowFriends();
        }

        //Create friend buttons
        private void ShowFriends()
        {
            LinearLayout btnLayout = FindViewById<LinearLayout>(Resource.Id.homepageLinear);
            btnLayout.RemoveAllViews();
            if (currentUser.userFriends != null)
            {
                if (currentUser.userFriends.Count == 0)
                {
                    TextView textView = new TextView(this);
                    textView.Text = "You have no pairings yet. Go to the Pairings page to add a pairing!";
                    textView.Gravity = Android.Views.GravityFlags.Center;
                    textView.TextSize = 24;
                    btnLayout.AddView(textView);
                }
                else
                {
                    TextView textView = new TextView(this);
                    textView.Text = "Friends";
                    textView.Gravity = Android.Views.GravityFlags.CenterHorizontal;
                    textView.TextSize = 24;
                    textView.SetTextColor(Color.Black);
                    LinearLayout.LayoutParams paramtext = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent,
                        LinearLayout.LayoutParams.WrapContent);
                    paramtext.SetMargins(0, 0, 0, 30);
                    btnLayout.AddView(textView, paramtext);
                    foreach (Friend friend in currentUser.userFriends)
                    {
                        LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent,
                        LinearLayout.LayoutParams.WrapContent);
                        param.SetMargins(50, 20, 50, 50);
                        Button btn = new Button(this);
                        btn.SetBackgroundResource(Resource.Drawable.text_border);
                        btn.SetBackgroundColor(new Color(ContextCompat.GetColor(this, Resource.Color.colorAccent)));
                        btn.SetAllCaps(false);
                        btn.Id = friend.userID;
                        btn.Text = friend.name;
                        btnLayout.AddView(btn, param);
                    }
                }
            }
            else
            {
                TextView textView = new TextView(this);
                textView.Text = "You have no pairings yet. Go to the Pairings page to add a pairing!";
                textView.Gravity = Android.Views.GravityFlags.CenterHorizontal;
                textView.TextSize = 24;
                btnLayout.AddView(textView);
            }
        }

        //First run
        protected void FirstRun()
        {
            //first dialog with welcome message
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Welcome!");
            builder.SetCancelable(false);
            builder.SetPositiveButton("Get Started", (sender, args) =>
            {
                UsernameAlert();
            });
            builder.Show();
        }
        protected void UsernameAlert()
        {
            //second dialog with input username
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
                if(editText.Text == "")
                {
                    Toast.MakeText(this, "Username cannot be empty", ToastLength.Short).Show();
                    UsernameAlert();
                }
                else if(editText.Text.Length > 15)
                {
                    Toast.MakeText(this, "Username has to be less than 15 characters", ToastLength.Short).Show();
                    UsernameAlert();
                }
                else
                {
                    Toast.MakeText(this, "Creating your user!", ToastLength.Short).Show();
                    var inputUsername = editText.Text.ToString();
                    CreateUser(inputUsername);
                }
            });
            dialog = builder.Create();
            dialog.Show();
        }

        //Creating a User by requesting API
        protected async void CreateUser(string username)
        {
            APIRequest request = new APIRequest();
            string userID = await request.AddUser(username);
            if(userID == "")
            {
                Toast.MakeText(this, "User creation unsuccessful.", ToastLength.Short).Show();
                UsernameAlert();
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
                writer.WriteLine(userID);
                writer.WriteLine(username);
                writer.Flush();
                writer.Close();

                currentUser = CreateObject();
            }
        }

        //Creates instance of class User
        public User CreateObject()
        {
            //Read user from file and create object of class
            string path = Application.Context.GetExternalFilesDir(null).ToString();
            string filePath = Path.Combine(path, "userdetails", "user.txt");
            StreamReader reader = new StreamReader(filePath);
            int id = Convert.ToInt32(reader.ReadLine());
            string username = reader.ReadLine();
            User currentUser = new User(id, username);
            reader.Close();
            return currentUser;
        }

        [Obsolete]
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
        public bool OnNavigationItemSelected(IMenuItem item)
        {
            int id = item.ItemId;

            if (id == Resource.Id.nav_profile)
            {
                var Profile = new Intent(this, typeof(ProfileActivity));
                Profile.PutExtra("username", currentUser.currentUsername);
                Profile.PutExtra("id", currentUser.currentID);
                StartActivityForResult(Profile, 1);
            }
            else if (id == Resource.Id.nav_pairings)
            {
                string jsonUserFriends = JsonConvert.SerializeObject(currentUser.userFriends);
                var Pairings = new Intent(this, typeof(PairingActivity));
                Pairings.PutExtra("id", currentUser.currentID);
                Pairings.PutExtra("jsonUserFriends", jsonUserFriends);
                StartActivityForResult(Pairings, 2);
            }
            

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            drawer.CloseDrawer(GravityCompat.Start);
            return true;
        }

        //For receiving data from canvas
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            //Result from canvas activity
            if (resultCode == Result.Ok && requestCode == 0)
            {
                byte[] byteArray = data.GetByteArrayExtra("image");
                ImageSelected(byteArray);
            }

            //Result from profile activity
            if (resultCode == Result.Ok && requestCode == 1)
            {
                currentUser.currentUsername = data.GetStringExtra("currentUsername");
            }

            //Result from pairings activity
            if (resultCode == Result.Ok && requestCode == 2)
            {
                string jsonUserFriends = data.GetStringExtra("jsonUserFriends");
                currentUser.userFriends = JsonConvert.DeserializeObject<List<Friend>>(jsonUserFriends);
                ShowFriends();
            }
        }

        //App permissions
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        
        //Camera Capture
        async void LaunchCamera(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if(CrossMedia.Current.IsCameraAvailable == false || CrossMedia.Current.IsTakePhotoSupported == false)
            {
                return;
            }

            //To capture image and set attributes of the image (size, compression..)
            var capturedImage = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                CompressionQuality = 90,
            }
            ) ;

            if (capturedImage == null)
            {
                return;
            }

            //Convert file to byte array
            byte[] imageArray = System.IO.File.ReadAllBytes(capturedImage.Path);
            ImageSelected(imageArray);
        }

        //Gallery 
        async void LaunchGallery(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (CrossMedia.Current.IsPickPhotoSupported == false)
            {
                return;
            }

            var chosenImage = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
            {
                PhotoSize = PhotoSize.Medium,
                CompressionQuality = 90
                
            });

            if (chosenImage == null)
            {
                return;
            }

            //Convert file to byte array
            byte[] imageArray = System.IO.File.ReadAllBytes(chosenImage.Path);
            var Confirm = new Intent(this, typeof(ConfirmActivity));
            Confirm.PutExtra("image", imageArray);
            StartActivity(Confirm);
            ImageSelected(imageArray);
        }

        //Handle image

        public void ImageSelected(byte[] imageArray)
        {
            //Create bitmap
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);

            //Create temporary imageview
            ImageView imageView = new ImageView(this);
            imageView.SetImageBitmap(bitmap);

            //Add it to layout
            DrawerLayout layout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            layout.AddView(imageView);
        }

    }
}


