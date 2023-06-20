
using Android;
using Android.App;
using Android.Content.PM;
using Android.Gms.Vision;
using Android.Gms.Vision.Texts;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using Javax.Security.Auth;
using Plugin.Media;
using System;
using System.Reflection;
using System.Text;
using static Android.Gms.Vision.Detector;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.IO;
using Plugin.Media.Abstractions;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Xamarin.Forms;

namespace task2
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ISurfaceHolderCallback, IProcessor
    {
        Android.Widget.Button button1;
        ImageView imageView1;
        readonly string[] permissionGroup =
        {
            Manifest.Permission.ReadExternalStorage,
            Manifest.Permission.WriteExternalStorage,
            Manifest.Permission.Camera
        };

        private SurfaceView cameraview;
        private TextView textview;
        private CameraSource cameraSource;
        private const int RequestCameraPermissionID = 1001;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            AppCenter.Start("{Your app secret here}",
                   typeof(Analytics), typeof(Crashes));
            SetContentView(Resource.Layout.activity_main);
            Xamarin.Essentials.Platform.Init(this, bundle);
            button1 = (Android.Widget.Button)FindViewById(Resource.Id.button1);
            imageView1 = (ImageView)FindViewById(Resource.Id.imageView1);
            button1.Click += button1_Click;
            RequestPermissions(permissionGroup, 0);
            cameraview = FindViewById<SurfaceView>(Resource.Id.surface_view);
            textview = FindViewById<TextView>(Resource.Id.text_view);
            TextRecognizer textRecognizer = new TextRecognizer.Builder(Application.Context).Build();
            if (!textRecognizer.IsOperational)
            {
                Log.Error("MainActivity", "Detector Dependencies are not available");
            }

            else
            {
                cameraSource = new CameraSource.Builder(ApplicationContext, textRecognizer)
                    .SetFacing(CameraFacing.Back)
                    .SetRequestedPreviewSize(1280, 1024)
                    .SetRequestedFps(2.0f)
                    .SetAutoFocusEnabled(true)
                    .Build();
                cameraview.Holder.AddCallback(this);
                textRecognizer.SetProcessor(this);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            TakePhoto();
        }
        async void TakePhoto()
        {
            await CrossMedia.Current.Initialize();
            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium,
                CompressionQuality = 40,
                Name = "image.jpg",
                Directory = "sample"
            });
            if (file == null) { return; }
            byte[] imageArray = System.IO.File.ReadAllBytes(file.Path);
            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageArray, 0, imageArray.Length);
            imageView1.SetImageBitmap(bitmap);


        }



        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestCameraPermissionID:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            cameraSource.Start(cameraview.Holder);
                        }

                    }
                    break;
            }
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {

        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            if (ActivityCompat.CheckSelfPermission(ApplicationContext, Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted)
            {

                ActivityCompat.RequestPermissions(this, new string[]
                     {
                    Android.Manifest.Permission.Camera
                 }, RequestCameraPermissionID);
                return;

            }
            cameraSource.Start(cameraview.Holder);
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            cameraSource.Stop();
        }

        public void ReceiveDetections(Detections detections)
        {
            SparseArray items = detections.DetectedItems;
            if (items.Size() != 0)
            {
                textview.Post(() =>
                {
                    StringBuilder strBuilder = new StringBuilder();
                    for (int i = 0; i < items.Size(); ++i)
                    {
                        strBuilder.Append(((TextBlock)items.ValueAt(i)).Value);
                        strBuilder.Append("\n");
                    }
                    textview.Text = strBuilder.ToString();
                });
            }
        }

        public void Release()
        {

        }





    }
}