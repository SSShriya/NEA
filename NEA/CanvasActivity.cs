using Android.App;
using Android.Content;
using Android.OS;
using Android;
using Java.Lang;
using AndroidX.Core.View;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using AndroidX.AppCompat.App;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Android.Util;
using System;
using Android.Test.Suitebuilder.Annotation;
using System.Collections;
using Android.Gestures;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AndroidX.Core.Graphics;
using Android.Runtime;
using System.Drawing;
using Color = Android.Graphics.Color;
using Bitmap = Android.Graphics.Bitmap;
using Google.Android.Material.Slider;
using Javax.Security.Auth;

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
            RadioButton thickness = FindViewById<RadioButton>(Resource.Id.tools_thickness);
            RadioButton colour = FindViewById<RadioButton>(Resource.Id.tools_colour);

            //Rangeslider
            RangeSlider rangeSlider;

            fill.Click += (sender,e)=>
                {
                    canvas.SetFillMode();
                };

            pen.Click += (sender, e) =>
            {
                canvas.SetPenMode();
            };

            eraser.Click += (sender, e) =>
            {
                canvas.SetEraserMode();
            };

            thickness.Click += (sender, e) =>
            {
                rangeSlider = FindViewById<RangeSlider>(Resource.Id.thickness_slider);
                if (rangeSlider.Visibility == ViewStates.Gone)
                {
                    rangeSlider.Visibility = ViewStates.Visible;
                }
                else
                {
                    rangeSlider.Visibility = ViewStates.Gone;
                }
            };
    
}

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var Homepage = new Intent(this, typeof(MainActivity));
            StartActivity(Homepage);
            return true;
        }

        //Stroke Class
        public class Stroke
        {
            public Path strokepath;
            public Paint strokepaint;
            public Stroke(Color newColour, float newStrokeWidth, Path newPath, Paint.Style style)
            {
                strokepath = newPath;
                strokepaint = new Paint();
                strokepaint.Color = newColour;
                strokepaint.StrokeWidth= newStrokeWidth;
                strokepaint.SetStyle(style);
            }
        }

        //Canvas Class
        public class CanvasView : View 
        {
            private Paint paint;
            private Color colour;
            private Path path;
            private Canvas canvas;
            private Bitmap bitmap;
            private bool fillMode = false;
            private bool touched = false;
            private bool penMode = false;
            private bool eraserMode = false;
            private List<Stroke> paths = new List<Stroke>();


            //Constructors
            public CanvasView(Context context) : base(context)
            {
                Initialise();
            }
            public CanvasView(Context context, IAttributeSet attrs) : base(context, attrs)
            {
                Initialise();
            }

            public CanvasView(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
                Initialise();
            }

            private void Initialise()          
            {
                paint = new Paint();
                paint.StrokeWidth=5;
                paint.AntiAlias = true;
                path = new Path();


            }

            //Canvas dimens
            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
                SetMeasuredDimension(MeasuredWidth, MeasuredWidth);
                bitmap = Bitmap.CreateBitmap(MeasuredWidth, MeasuredHeight, Bitmap.Config.Argb8888);
                canvas = new Canvas(bitmap);

            }

            //Methods for which drawing tool is selected
            public void SetFillMode()
            {
                paint.Color = colour;
                paint.SetStyle(Paint.Style.Fill);
                fillMode = true;
                penMode = false;
                eraserMode = false;
            }

            public void SetPenMode()
            {
                colour = Color.Black;
                paint.Color = colour;
                paint.SetStyle(Paint.Style.Stroke);
                penMode = true;
                fillMode = false;
                eraserMode = false;
            }

            public void SetEraserMode()
            {
                colour = Color.White;
                paint.Color = colour;
                paint.SetStyle(Paint.Style.Stroke);
                eraserMode = true;
                fillMode = false;
                penMode = false;
            }

            public override bool OnTouchEvent(MotionEvent e)
            {
                touched = true;
                switch (e.Action)
                {
                    case MotionEventActions.Down:
                        path = new Path();
                        Stroke stroke = new Stroke(paint.Color, paint.StrokeWidth, path, paint.GetStyle());
                        paths.Add(stroke);

                        if (fillMode)
                        {
                            path.AddRect(0, 0, Width, Height, Path.Direction.Cw);
                            Invalidate();
                        }
                        if(penMode || eraserMode)
                        {
                            path.MoveTo(e.GetX(), e.GetY());
                            Invalidate();
                        }
                        return true;

                    case MotionEventActions.Move:
                        if (penMode || eraserMode)
                        {
                            path.LineTo(e.GetX(), e.GetY());
                            Invalidate();
                        }
                        return true;

                    case MotionEventActions.Up:
                        if (penMode || eraserMode)
                        {
                            path.LineTo(e.GetX(), e.GetY());
                            Invalidate();
                        }
                        return true;
                }
                
                return base.OnTouchEvent(e);
            }


            //Method to draw something
            protected override void OnDraw(Canvas canvas)
            {
                base.OnDraw(canvas);

                canvas.Save();
                foreach (Stroke stroke in paths)
                {
                    if (stroke.strokepaint != null && stroke.strokepath != null)
                    {
                        canvas.DrawPath(stroke.strokepath, stroke.strokepaint);
                    }
                }
                canvas.DrawBitmap(bitmap, 0, 0, paint);
                canvas.Restore();
               

                if (touched)
                {
                    if (fillMode)
                    {
                        canvas.DrawRect(0, 0, canvas.Width, canvas.Height, paint);
                    }
                    if (penMode || eraserMode)
                    {
                        if (path != null)
                        {
                            canvas.DrawPath(path, paint);
                        }
                    }
                    touched = false;
                }

            }

        }
    }
}