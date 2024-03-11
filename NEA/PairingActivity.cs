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
using Newtonsoft.Json;


namespace NEA
{
    [Activity(Label = "@string/pairings", Theme = "@style/AppTheme.NoActionBar")]
    public class PairingActivity : AppCompatActivity
    {
        private int currentID;
        private List<Friend> userFriends;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            currentID = Intent.GetIntExtra("id", 0);
            string jsonUserFriends = Intent.GetStringExtra("jsonUserFriends");
            userFriends = JsonConvert.DeserializeObject<List<Friend>>(jsonUserFriends);

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

            Button btn_delete = FindViewById<Button>(Resource.Id.btn_delete);
            btn_delete.Click += ShowFriends;

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
            else if(UIDint==currentID)
            {
                Toast.MakeText(this, "Enter someone else's code", ToastLength.Short).Show();
                return;
            }
            else if(userFriends.Any(obj => obj.userID == UIDint))
                {
                Toast.MakeText(this, "You are already paired with this person.", ToastLength.Short).Show();
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
                Toast.MakeText(this, "Pairing successful.", ToastLength.Short).Show();
                //Adds friend details to user friends list:
                Friend newFriend = new Friend(UID, username, Convert.ToInt32(pairingID));
                userFriends.Add(newFriend);
                newFriend.AddPairing();
            }
        }

        //TAKE THIS OUT WHEN COPYING AND PASTING CODE
        protected async void DeletePairing(int pairingID)
        {
            APIRequest request = new APIRequest();
            var response = await request.DelPairing(pairingID);
            if (response == false)
            {
                Toast.MakeText(this, "Deleting unsuccessful.", ToastLength.Short).Show();
            }
            else
            {
                Toast.MakeText(this, "Deleting successful.", ToastLength.Short).Show();
                userFriends.RemoveAt(userFriends.FindIndex(obj => obj.pairingID == pairingID));

                //Writes friend details to file pairings.txt
                string path = Application.Context.GetExternalFilesDir(null).ToString();
                string folderPath = Path.Combine(path, "userdetails");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string filePath = Path.Combine(folderPath, "pairings.txt");
                StreamWriter writer = new StreamWriter(filePath);
                foreach (Friend friend in userFriends)
                {
                    writer.WriteLine(friend.userID);
                    writer.WriteLine(friend.name);
                    writer.WriteLine(friend.pairingID);
                }
                writer.Flush();
                writer.Close();
            }
        }

        public void ShowFriends(object sender, EventArgs e)
        {            

            //alert dialog with friend list
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            LayoutInflater layoutInflater = LayoutInflater.FromContext(this);
            View view = layoutInflater.Inflate(Resource.Layout.delete_menu, null);

            //Add radio buttons with friend usernames to radio group:
            RadioGroup friendList = view.FindViewById<RadioGroup>(Resource.Id.friendList);
            foreach (Friend friend in userFriends)
            {
                RadioButton rbtn = new RadioButton(this);
                rbtn.Id = friend.pairingID;
                rbtn.Text = friend.name;
                rbtn.TextSize = 16;
                rbtn.SetPadding(7, 7, 7, 7);
                friendList.AddView(rbtn);
            }

            builder.SetView(view);
            builder.SetTitle("Choose a pairing to delete");
            AlertDialog dialog = null;
            builder.SetPositiveButton("Confirm", (sender, args) =>
            {
                if(friendList.CheckedRadioButtonId == -1)
                {
                    Toast.MakeText(this, "Select a pairing.", ToastLength.Short).Show();
                }
                else
                {
                    //TAKE THIS OUT WHEN COPYING AND PASTING CODE
                    Toast.MakeText(this, "Deleting...", ToastLength.Short).Show();
                    DeletePairing(friendList.CheckedRadioButtonId);
                }
            });
            builder.SetNegativeButton("Cancel", (sender, args) =>
            {
                return;
            });
            dialog = builder.Create();
            dialog.Show();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var Homepage = new Intent(this, typeof(MainActivity));
            string jsonUserFriends = JsonConvert.SerializeObject(userFriends);
            Homepage.PutExtra("jsonUserFriends", jsonUserFriends);
            SetResult(Result.Ok, Homepage);
            Finish();
            return true;
        }
    }

}