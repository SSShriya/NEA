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
using Firebase.Storage;
using Google.Apis.Util;
using System.Net.Http;
using Org.Json;
using System.Net;
using Firebase.Messaging;
using System.Runtime.CompilerServices;
using Android.Gms.Tasks;
using Firebase;
using Plugin.FirebasePushNotification;
using Java.Lang;
using Android.Gms.Stats;
using Exception = System.Exception;
using NEA;
using Resource = NEA.Resource;
using System.Linq;



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
            Manifest.Permission.ManageExternalStorage,
            Manifest.Permission.Camera,
            Manifest.Permission.PostNotifications

        };
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            FirebaseApp.InitializeApp(this);

            //get file + camera + notification permissions
            RequestPermissions(permissionGroup, 0);

            Init(this, savedInstanceState);
            FirebasePushNotificationManager.DefaultNotificationChannelId = "FirebasePushNotificationChannel";
            FirebasePushNotificationManager.DefaultNotificationChannelName = "ImageAvailable";
            FirebasePushNotificationManager.Initialize(this, true);
            FirebasePushNotificationManager.ProcessIntent(this, Intent);

            // Token event
            CrossFirebasePushNotification.Current.OnTokenRefresh += (s, p) =>
            {
                System.Diagnostics.Debug.WriteLine($"TOKEN : {p.Token}");
            };

            // Push message received event
            CrossFirebasePushNotification.Current.OnNotificationReceived += (s, p) =>
            {

                System.Diagnostics.Debug.WriteLine("Received");
            };

            //if first run, creates new user
            if (Settings.FirstRun)
            {
                FirstRun();
                Settings.FirstRun = false;
            }
            else
            {
                currentUser = currentUser = CreateObject();                
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
                    string jsonUserFriends = JsonConvert.SerializeObject(currentUser.userFriends);
                    Canvas.PutExtra("jsonUserFriends", jsonUserFriends);
                    Canvas.PutExtra("id", currentUser.currentID);
                    StartActivity(Canvas);
                };


            };          
            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();
            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);


            //Get friends
            if (currentUser != null)
            {
                currentUser.GetFriends();
                //Get any new pairings
                GetNewPairings();
                ShowFriends();

                //Download images
                DownloadImage();

            }

        }

        public async void GetNewPairings()
        {
            APIRequest request = new APIRequest();
            int pairingID = 0;
            if (currentUser.userFriends.Count != 0)
            {
                pairingID = currentUser.userFriends.Last().pairingID;
            }

            List<APIRequest.PairingData> newPairings = await request.GetPairings(currentUser.currentID, pairingID);

            if (newPairings.Count != 0)
            {
                foreach (APIRequest.PairingData pairing in newPairings)
                {
                    string username = await request.GetUser(pairing.user1);
                    Friend friend = new Friend(pairing.user1, username, pairing.pairingID);
                    currentUser.userFriends.Add(friend);
                    friend.AddPairing();
                }
            }
        }

        //Download images from firebase
        private async void DownloadImage()
        {
            if (currentUser.userFriends != null)
            {
                //iterates through friends list to go to their folders in firebase using the pairing and userID
                foreach (Friend friend in currentUser.userFriends)
                {
                    FirebaseStorage storage = new FirebaseStorage("nea-files-a5ba8.appspot.com", new FirebaseStorageOptions
                    {
                        ThrowOnCancel = true
                    });
                    
                    try
                    {
                        //gets an array of all the files in the folder of particular user
                        string[] paths = await GetFilesInFolder($"{friend.pairingID}/{friend.userID}/");
                        
                        if(paths != null)
                        {
                            //iterates through this array and downloads each image by getting the download url for each
                            for(int i = 0;i<paths.Length;i++)
                            {
                                int index = paths[i].IndexOf('/', paths[i].IndexOf('/') + 1);
                                string name = paths[i].Substring(index + 1);
                                FirebaseStorageReference reference = storage.Child($"{paths[i]}");
                                var downloadurl = await reference.GetDownloadUrlAsync();

                                //reads downloaded data into byte array
                                byte[] imageArray = await DownloadWithURL(downloadurl);
                                if (imageArray != null)
                                {
                                    //saves image in appropriate location
                                    FileSave save = new FileSave(name, friend.pairingID, imageArray, friend.userID);
                                    save.WriteFile();
                                }

                                //deletes file from firebase
                                await reference.DeleteAsync();
                            }   
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex.Message.ToString());
                    }
                }
            }
        }
    

        //Get list of files by sending GET request
        private async Task<string[]> GetFilesInFolder(string path)
        {
            const string baseUrl = "https://firebasestorage.googleapis.com/v0/b/nea-files-a5ba8.appspot.com/";
            string url = $"{baseUrl}o?prefix={path}";
            
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            var jsonResponse = "";
            
            try
            {
                //does request
                HttpResponseMessage response = await client.GetAsync(url) ;
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    //prints response
                    jsonResponse = await response.Content.ReadAsStringAsync();
                }

                //Gets 'items' array.
                JSONObject jsonobj = new JSONObject(jsonResponse);
                JSONArray items = jsonobj.GetJSONArray("items");

                //Iterates through items array, then prints the name of each file.
                string[] filenames = new string[items.Length()];
                for(int i =0;i<items.Length();i++ ) 
                {
                    JSONObject name = items.GetJSONObject(i);
                    filenames[i] = name.GetString("name");
                }
                return filenames;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            return null;        
        }

        //Download image from url
        private async Task<byte[]> DownloadWithURL(string url)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                if (response.IsSuccessStatusCode)
                {
                    byte[] imageArray = await response.Content.ReadAsByteArrayAsync();
                    return imageArray;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            return null;
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
                        btn.Click += (s, e) =>
                            {
                                var Friend = new Intent(this, typeof(FriendActivity));
                                Friend.PutExtra("friendName", friend.name);
                                Friend.PutExtra("pairingID", friend.pairingID);
                                Friend.PutExtra("currentID", currentUser.currentID);
                                StartActivity(Friend);
                            };
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
            var Confirm = new Intent(this, typeof(ConfirmActivity));
            string jsonUserFriends = JsonConvert.SerializeObject(currentUser.userFriends);
            Confirm.PutExtra("jsonUserFriends", jsonUserFriends);
            Confirm.PutExtra("image", imageArray);
            Confirm.PutExtra("id", currentUser.currentID);
            StartActivity(Confirm);
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
            string jsonUserFriends = JsonConvert.SerializeObject(currentUser.userFriends);
            Confirm.PutExtra("jsonUserFriends", jsonUserFriends);
            Confirm.PutExtra("image", imageArray);
            Confirm.PutExtra("id", currentUser.currentID);
            StartActivity(Confirm);
        }

    }
}


