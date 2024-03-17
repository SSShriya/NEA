using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NEA;
using System.IO;
using Android.Media;
using AndroidX.Core.Content;
using Android.Graphics;
using Path = System.IO.Path;
using System.Runtime.Remoting.Contexts;
using Context = Android.Content.Context;
using Java.Util;

namespace NEA
{
    [BroadcastReceiver(Label = "Received Image", Exported = false)]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    [MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
    public class Widget : AppWidgetProvider
    {
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            base.OnUpdate(context, appWidgetManager, appWidgetIds);
            //Create an intent 
            for (int i = 0; i < appWidgetIds.Length; i++)
            {
                int appWidgetId = appWidgetIds[i];
                Intent intent = new Intent(context, typeof(Widget));
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);
                intent.SetAction("Clicked");
                PendingIntent imageBG = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent);

                // Register click event for the widget
                var widgetView = new RemoteViews(context.PackageName, Resource.Layout.widget);
                widgetView.SetOnClickPendingIntent(Resource.Id.widgetBackground, imageBG);
                UpdateWidget(context, widgetView);
                appWidgetManager.UpdateAppWidget(appWidgetId, widgetView);
            }
        }

        public override void OnEnabled(Context context)
        {
            base.OnEnabled(context);
            
        }

        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);
            if (intent.Action == "Clicked")
            {
                var widgetView = new RemoteViews(context.PackageName, Resource.Layout.widget);
                UpdateWidget(context, widgetView);
            }
        }

        private void UpdateWidget(Context context, RemoteViews widgetView)
        {
            string path = GetImage();
            if (path != "")
            {
                Android.Net.Uri uri = FileProvider.GetUriForFile(context, context.ApplicationContext.PackageName + ".fileprovider", new Java.IO.File(path));
                Console.WriteLine(path);
                Bitmap bitmap = null;
                try
                {
                    ContentResolver contentResolver = context.ContentResolver;
                    System.IO.Stream inputStream = contentResolver.OpenInputStream(uri);
                    bitmap = BitmapFactory.DecodeStream(inputStream);
                    inputStream.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }

                widgetView.SetImageViewBitmap(Resource.Id.widgetImage, bitmap);

                AppWidgetManager appWidgetManager = AppWidgetManager.GetInstance(context);
                ComponentName thisWidget = new ComponentName(context, Java.Lang.Class.FromType(typeof(Widget)).Name);
                appWidgetManager.UpdateAppWidget(thisWidget, widgetView);
            }
        }

        private int GetUID()
        {
            string path = Application.Context.GetExternalFilesDir(null).ToString();
            string folderPath = Path.Combine(path, "userdetails");
            if (!Directory.Exists(folderPath))
            {
                return 0;
            }
            string filePath = Path.Combine(folderPath, "user.txt");
            if (File.Exists(filePath))
            {
                StreamReader reader = new StreamReader(filePath);
                int id = Convert.ToInt32(reader.ReadLine());
                return id;
            }
            else
            {
                return 0;
            }
        }

        private string GetImage()
        {
            //Gets the most recent file from directory
            string path = Application.Context.GetExternalFilesDir(null).ToString();
            string folderPath = Path.Combine(path, "images");
            if (!Directory.Exists(folderPath))
            {
                return "";
            }
            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            files.OrderBy(f => new FileInfo(f).LastWriteTime);
            if (files.Length == 0)
            {
                return "";
            }
            int id = GetUID();

            //Gets the most recent file that was received by someone else
            if (id != 0)
            {

                int count = files.Length-1;
                var sender = "";
                do
                {
                    ExifInterface exif = new ExifInterface(files[count]);
                    sender = exif.GetAttribute(ExifInterface.TagArtist);
                    count--;
                }
                while (sender == id.ToString() && count > 0);
                if (count ==-1)
                {
                    return "";
                }
                return files[count+1];
            }
            return "";
        }
    }
}