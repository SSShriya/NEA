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
using System.Linq;
using Yuku.AmbilWarna;
using AndroidX.Core.Content;
using Java.Security;
using Math = Java.Lang.Math;
using static AndroidX.Activity.Result.Contract.ActivityResultContracts;
using static Xamarin.Essentials.Platform;
using Intent = Android.Content.Intent;
using System.IO;
using Path = Android.Graphics.Path;
using AndroidX.DrawerLayout.Widget;
using AndroidX.Browser.Trusted;
using Point = Android.Graphics.Point;

namespace NEA
{
    [Activity(Label = "@string/canvas", Theme = "@style/AppTheme.NoActionBar")]
    public class CanvasActivity : AppCompatActivity, IBaseOnChangeListener
    {
        private readonly int color = unchecked((int)0xffffff00);
        private RadioButton current;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_canvas);

            //Toolbar
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            //Canvas
            CanvasView canvas = FindViewById<CanvasView>(Resource.Id.canvasView);
            canvas.OutlineProvider = ViewOutlineProvider.Background;
            canvas.ClipToOutline = true;

            //Radio buttons
            RadioButton pen = FindViewById<RadioButton>(Resource.Id.tools_pen);
            RadioButton eraser = FindViewById<RadioButton>(Resource.Id.tools_eraser);
            RadioButton fill = FindViewById<RadioButton>(Resource.Id.tools_fill);
            RadioButton thickness = FindViewById<RadioButton>(Resource.Id.tools_thickness);
            RadioButton colour = FindViewById<RadioButton>(Resource.Id.tools_colour);
            RadioButton undo = FindViewById<RadioButton>(Resource.Id.tools_undo);

            //Slider            
            Slider slider = FindViewById<Slider>(Resource.Id.thickness_slider);
            slider.ValueFrom= 0;
            slider.ValueTo = 100;
            slider.StepSize = 2;

            //Completed button
            Button complete = FindViewById<Button>(Resource.Id.btn_complete);

            fill.Click += (sender,e)=>
                {
                    current = fill;
                    canvas.SetFillMode();
                };

            pen.Click += (sender, e) =>
            {
                current = pen;
                canvas.SetPenMode();
            };

            eraser.Click += (sender, e) =>
            {
                current = eraser;
                canvas.SetEraserMode();
            };

            thickness.Click += (sender, e) =>
            {
                if (slider.Visibility == ViewStates.Gone)
                {
                    slider.Visibility = ViewStates.Visible;
                    var method = Class.ForName("com.google.android.material.slider.BaseSlider").GetDeclaredMethods().FirstOrDefault(x => x.Name == "addOnChangeListener");
                    method?.Invoke(slider, this);
                }
                else
                {
                    slider.Visibility = ViewStates.Gone;
                    if (current != null)
                    {
                        current.PerformClick();
                    }
                }
            };
            
            colour.Click += (sender, e) =>
            {
                OpenDialog(true);
            };

            undo.Click += (sender, e) =>
            {
                canvas.Undo();
            };

            complete.Click += (sender, e) =>
            {
                Bitmap image = canvas.Save();
                MemoryStream stream = new MemoryStream();
                image.Compress(Bitmap.CompressFormat.Png, 100, stream);
                byte[] byteArray = stream.ToArray();
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra("image", byteArray);
                SetResult(Result.Ok, intent);
                Finish();
            };

        }

        //For colour wheel
        private void OpenDialog(bool supportAlpha)
        {
            CanvasView canvas = FindViewById<CanvasView>(Resource.Id.canvasView);        
            var dialog = new AmbilWarnaDialog(this, color, supportAlpha);
            dialog.Ok += (sender, e) =>
            {
                canvas.SetColour(e.Color.ToString("x8"));
            };
            dialog.Show();
            if (current != null)
            {
                current.PerformClick();
            }
        }

        //For thickness slider
        public void OnValueChange(Java.Lang.Object p0, float p1, bool p2)
        {
            CanvasView canvas = FindViewById<CanvasView>(Resource.Id.canvasView);
            canvas.SetThickness(p1);
            if (current != null)
            {
                current.PerformClick();
            }
        }

        //For toolbar
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
                strokepaint.StrokeCap = Paint.Cap.Round;
                strokepaint.StrokeJoin = Paint.Join.Round;
                strokepaint.Dither = true;
                strokepaint.SetStyle(style);
            }
        }

        //Canvas Class
        public class CanvasView : View 
        {
            private Paint paint;
            private Path path;
            private Color color;
            private Canvas newCanvas;
            private Bitmap bitmap;
            private bool fillMode = false;
            private bool touched = false;
            private bool penMode = false;
            private bool eraserMode = false;
            private readonly List<Stroke> paths = new List<Stroke>();
            private readonly Paint mBitmapPaint = new Paint();

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
                paint.StrokeWidth = 5;
                paint.AntiAlias = true;
                paint.Color = Color.Black;
                color = paint.Color;
                path = new Path();
                paint.Dither = true;
                paint.StrokeCap = Paint.Cap.Round;
                paint.StrokeJoin = Paint.Join.Round;
            }

            //Canvas dimens
            protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
            {
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
                SetMeasuredDimension(MeasuredWidth, MeasuredWidth);
                bitmap = Bitmap.CreateBitmap(MeasuredWidth,MeasuredHeight, Bitmap.Config.Argb8888);
                newCanvas = new Canvas(bitmap);
            }

            //Methods for which drawing tool is selected
            public void SetFillMode()
            {
                paint.Color = color;
                paint.SetStyle(Paint.Style.Fill);
                fillMode = true;
                penMode = false;
                eraserMode = false;
            }

            public void SetPenMode()
            {
                paint.Color = color;
                paint.SetStyle(Paint.Style.Stroke);
                penMode = true;
                fillMode = false;
                eraserMode = false;
            }

            public void SetEraserMode()
            {
                paint.Color = Color.White;
                paint.SetStyle(Paint.Style.Stroke);
                eraserMode = true;
                fillMode = false;
                penMode = false;
            }

            //Style methods
            public void SetThickness(float thickness)
            {
                paint.StrokeWidth = thickness;
            }

            public void SetColour(string newColour)
            {
                var a = newColour.Substring(0, 2);
                var r = newColour.Substring(2, 2);
                var g = newColour.Substring(4, 2);
                var b = newColour.Substring(6, 2);
                int a_int = int.Parse(a, System.Globalization.NumberStyles.HexNumber);
                int r_int = int.Parse(r, System.Globalization.NumberStyles.HexNumber);
                int g_int = int.Parse(g, System.Globalization.NumberStyles.HexNumber);
                int b_int = int.Parse(b, System.Globalization.NumberStyles.HexNumber);

                paint.SetARGB(a_int, r_int, g_int, b_int);
                color = paint.Color;
            }

            //Flood fill
            private int GetPixelColour(int x, int y)
            {
                int selectedColour = Color.White;
                if(bitmap != null)
                {
                    if(x>=0 && x<bitmap.Width && y>=0 && y<bitmap.Height)
                    {
                        selectedColour = bitmap.GetPixel(x, y);
                        
                    }
                }
                return selectedColour;
            }

            private void SetPixelColour(int x, int y, int colour)
            {
                Color color = new Color(colour);
                bitmap.SetPixel(x, y, color);
            }

            private void FloodFill(int x, int y, int oldColour, int newColour)
            {
                if (x >= 0 && x < bitmap.Width && y >= 0 && y < bitmap.Height)
                {
                    Stack<Point> toVisit = new Stack<Point>();
                    toVisit.Push(new Point(x, y));
                    while (toVisit.Count > 0)
                    {
                        Point newPoint = toVisit.Pop();
                        x = newPoint.X;
                        y = newPoint.Y;
                        if (GetPixelColour(x, y) == oldColour)
                        {
                            SetPixelColour(x, y, newColour);
                            if (x < bitmap.Width - 1)
                            {

                                toVisit.Push(new Point(x + 1, y));

                            }
                            if (x > 0)
                            {

                                toVisit.Push(new Point(x - 1, y));

                            }
                            if (y < bitmap.Height - 1)
                            {

                                toVisit.Push(new Point(x, y + 1));

                            }
                            if (y > 0)
                            {

                                toVisit.Push(new Point(x, y - 1));

                            }
                        }
                    }
                }
            }

            //Undo
            public void Undo()
            {
                if (paths.Count > 0)
                {
                    paths.RemoveAt(paths.Count - 1);
                    Invalidate();
                }
            }

            //Save
            public Bitmap Save()
            {
                newCanvas.Save();
                newCanvas.DrawColor(Color.White);
                foreach (Stroke stroke in paths)
                {
                    if (stroke.strokepaint != null && stroke.strokepath != null)
                    {
                        newCanvas.DrawPath(stroke.strokepath, stroke.strokepaint);
                    }
                }
                newCanvas.DrawBitmap(bitmap, 0, 0, mBitmapPaint);
                newCanvas.Restore();
                return bitmap;
            }

            //Recognising path
            public override bool OnTouchEvent(MotionEvent e)
            {
                touched = true;
                switch (e.Action)
                {
                    case MotionEventActions.Down:
                        path = new Path();
                        if (eraserMode) { paint.Color = Color.White; }
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
                            if (eraserMode) { paint.Color = Color.White; }
                            Invalidate();
                        }
                        return true;

                    case MotionEventActions.Up:
                        if (penMode || eraserMode)
                        {
                            path.LineTo(e.GetX(), e.GetY());
                            if (eraserMode) { paint.Color = Color.White; }
                        }
                        Invalidate();
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
                canvas.DrawBitmap(bitmap, 0, 0, mBitmapPaint);
                canvas.Restore();

                if (touched)
                {
                    if (path != null)
                        {
                            canvas.DrawPath(path, paint);
                        }
                    
                    touched = false;
                }

            }

        }
    }
}