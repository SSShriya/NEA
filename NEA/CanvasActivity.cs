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
using Android.Database.Sqlite;

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

            //Toolbar
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            //Canvas
            CanvasView canvas = FindViewById<CanvasView>(Resource.Id.canvasView);

            //Radio buttons
            RadioButton pen = FindViewById<RadioButton>(Resource.Id.tools_pen);
            RadioButton eraser = FindViewById<RadioButton>(Resource.Id.tools_eraser);
            RadioButton fill = FindViewById<RadioButton>(Resource.Id.tools_fill);

            fill.Click += (sender,e)=>
                {
                    canvas.setFillMode();
                };

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
            private Color colour = Color.Black;
            private bool fillMode = false;
            private bool canvasFill = false;
            private bool penMode = false;
            private bool eraserMode = false;


            //Constructor
            public CanvasView(Context context) : base(context)
            {
                Initialise();
            }
            public CanvasView(Context context, IAttributeSet attrs) : base(context, attrs)
            {
                Initialise();
            }

            private void Initialise()          
                {
                    paint = new Paint();
                    paint.Color = colour;
                    paint.StrokeWidth = 5;
                }
            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
                SetMeasuredDimension(MeasuredWidth, MeasuredWidth);
            }

            public void setFillMode()
            {
                fillMode = true;
                penMode = false;
                eraserMode = false;
            }

            public void setPenMode()
            {
                penMode = true;
                fillMode = false;
                eraserMode = false;
            }

            public void setEraserMode()
            {
                eraserMode = true;
                fillMode = false;
                penMode = false;
            }

            public override bool OnTouchEvent(MotionEvent e)
            {
                if (fillMode && e.Action == MotionEventActions.Down)
                {
                    CanvasView canvas = FindViewById<CanvasView>(Resource.Id.canvasView);
                    canvasFill = true;
                    canvas.Invalidate();
                    return true;
                }
                return base.OnTouchEvent(e);
            }

            //Method to draw something
            protected override void OnDraw(Canvas canvas)
            {
                base.OnDraw(canvas);

                if(canvasFill)
                {
                    canvas.DrawRect(0, 0, canvas.Width, canvas.Height, paint);
                    canvasFill = false;
                }

            }

        }
    }
}