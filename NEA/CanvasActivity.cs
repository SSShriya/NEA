using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using AndroidX.AppCompat.App;
using System;
using System.Collections.Generic;
using System.Linq;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using System.Text;
using static NEA.CanvasActivity;
using Android.Util;
using AndroidX.CoordinatorLayout.Widget;

namespace NEA
{
    [Activity(Label = "@string/canvas", Theme = "@style/AppTheme.NoActionBar")]
    public class CanvasActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_canvas);
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            CanvasView canvas = FindViewById<CanvasView>(Resource.Id.canvasView);
            int desiredHeight = canvas.Width;

            /* ViewGroup.LayoutParams layoutParams = canvas.LayoutParameters;
            layoutParams.Height = desiredHeight;
            canvas.LayoutParameters = layoutParams;*/

        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var Homepage = new Intent(this, typeof(MainActivity));
            StartActivity(Homepage);
            return true;
        }

        //Canvas Class
        public class CanvasView : View 
        {
            private Paint paint;

            //Constructor

            public CanvasView(Context context) : base(context)
            {
                Initialise();
            }

            public CanvasView(Context context, IAttributeSet attrs) : base(context, attrs)
            {
                Initialise();
            }

            public CanvasView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
            {
                Initialise();
            }

            public CanvasView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
            {
                Initialise();
            }

            private void Initialise()          
                {
                    paint = new Paint();
                    paint.Color = Color.Red;
                    paint.StrokeWidth = 5;
                }
            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
                SetMeasuredDimension(MeasuredWidth, MeasuredWidth);
            }

            //Method to draw something
            protected override void OnDraw(Canvas canvas)
                {
                    base.OnDraw(canvas);
                    canvas.DrawCircle(100, 500, 50, paint);
                }

        }
    }
}