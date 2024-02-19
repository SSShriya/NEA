using Android.App;
using Android.Content;
using Android.OS;
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
            public Path path;
            public Color colour;
            public float strokeWidth;
            public Stroke(Color newColour, float newStrokeWidth, Path newPath)
            {
                colour = newColour;
                strokeWidth = newStrokeWidth;
                path = newPath;
            }
        }

        //Canvas Class
        public class CanvasView : View 
        {
            private Paint paint;
            private Color colour = Color.Black;
            private Path path;
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

            private void Initialise()          
            {
                paint = new Paint();
                paint.StrokeWidth=5;
                path = new Path();
            }

            //Canvas dimens
            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
                SetMeasuredDimension(MeasuredWidth, MeasuredWidth);
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
                paint.Color = colour;
                paint.SetStyle(Paint.Style.Stroke);
                penMode = true;
                fillMode = false;
                eraserMode = false;
            }

            public void SetEraserMode()
            {
                paint.Color = Color.Transparent;
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
                        if(fillMode)
                        {
                            Invalidate();
                        }
                        if(penMode || eraserMode)
                        {
                            path.MoveTo(e.GetX(), e.GetY());
                            Stroke stroke = new Stroke(colour, paint.StrokeWidth, path);
                            paths.Add(stroke);
                            Invalidate();
                        }
                        return true;

                    case MotionEventActions.Move:
                        if (penMode || eraserMode)
                        {
                            path.LineTo(e.GetX(), e.GetY());
                            Stroke stroke = new Stroke(colour, paint.StrokeWidth, path);
                            paths.Add(stroke);
                            Invalidate();
                        }
                        return true;

                    case MotionEventActions.Up:
                        if (penMode || eraserMode)
                        {
                            path.LineTo(e.GetX(), e.GetY());
                            Stroke stroke = new Stroke(colour, paint.StrokeWidth, path);
                            paths.Add(stroke);
                            Invalidate();
                        }
                        return true;
                }
                
                return base.OnTouchEvent(e);
            }


            //Method to draw something
            protected override void OnDraw(Canvas canvas)
            {
                canvas.Save();
                foreach(Stroke stroke in paths)
                {
                    paint.Color = stroke.colour;
                    paint.StrokeWidth = stroke.strokeWidth;
                    canvas.DrawPath(stroke.path, paint);
                }
                //canvas.DrawBitmap(bitmap, 0, 0, paint);
                canvas.Restore();
                base.OnDraw(canvas);

                if (touched)
                {
                    if (fillMode)
                    {
                        canvas.DrawRect(0, 0, canvas.Width, canvas.Height, paint);
                    }
                    if (penMode)
                    {
                        if (path != null)
                        {
                            canvas.DrawPath(path, paint);
                        }
                    }
                    if (eraserMode)
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