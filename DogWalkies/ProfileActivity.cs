using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Content;
using Uri = Android.Net.Uri;
using Android.Provider;
using System.IO;
using Android.Graphics.Drawables;

namespace DogWalkies
{
    [Activity(Label = "ProfileActivity", Theme = "@android:style/Theme.Material.Light.NoActionBar")]
    public class ProfileActivity : Activity
    {
        private TextView TextViewDogFirstName;
        private RelativeLayout RelativeLayoutDogProfile;
        private ImageButton ImageButtonAddDogProfileImage;
        private Button ButtonViewAlbum;
        private Button ButtonMetrics;

        private DogAccessLayer dataDogAccess = DogAccessLayer.getInstance();
        private Dog dog;
        private byte[] dogProfileImage;

        private int REQUEST_PICK_IMAGE = 1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Profile);

            //Don't automatically popup the android keyboard when activity starts
            Window.SetSoftInputMode(SoftInput.StateHidden);

            loadViews();
            initializeFontStyle();
            initializeDogProfileImage();
            initializeClickEvents();
        }

        private void loadViews()
        {
            TextViewDogFirstName = FindViewById<TextView>(Resource.Id.TextViewDogFirstName);
            RelativeLayoutDogProfile = FindViewById<RelativeLayout>(Resource.Id.RelativeLayoutDogImage);
            ImageButtonAddDogProfileImage = FindViewById<ImageButton>(Resource.Id.ImageButtonAddDogProfileImage);
            ButtonViewAlbum = FindViewById<Button>(Resource.Id.ButtonViewAlbum);
            ButtonMetrics = FindViewById<Button>(Resource.Id.ButtonMetrics);
        }

        private void initializeFontStyle()
        {
            Typeface centuryGothic = Typeface.CreateFromAsset(Assets, "centuryGothic.ttf");

            TextViewDogFirstName.SetTypeface(centuryGothic, TypefaceStyle.Normal);
        }

        private void initializeDogProfileImage()
        {
            dog = dataDogAccess.getDogByID(0);

            //Set the ImageView for the dog profile image
            var bitmapDrawable = new BitmapDrawable(BitmapFactory.DecodeByteArray(dog.ProfileImage, 0, dog.ProfileImage.Length));
            RelativeLayoutDogProfile.SetBackgroundDrawable(bitmapDrawable);
        }

        private void initializeClickEvents()
        {

            ButtonMetrics.Click += ButtonMetrics_Click;
            ImageButtonAddDogProfileImage.Click += GrabAPictureFromGallery;
        }

        private void ButtonMetrics_Click(object sender, EventArgs e)
        {
            StartActivity(typeof(MetricsActivity));
        }

        private void GrabAPictureFromGallery(object sender, EventArgs e)
        {
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), REQUEST_PICK_IMAGE);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == REQUEST_PICK_IMAGE)
            {
                if (data == null)
                {
                    Toast.MakeText(this, GetText(Resource.String.GeneralError), ToastLength.Short).Show();
                }
                else
                {
                    if (resultCode == Result.Ok)
                    {
                        UpdateDogProfileImageFromIntentData(data);
                    }
                }
            }
        }

        private void UpdateDogProfileImageFromIntentData(Intent data)
        {
            //Uri --> Bitmap
            Uri newProfileImgUri = data.Data;
            Bitmap newProfileImgBitmap = MediaStore.Images.Media.GetBitmap(ContentResolver, newProfileImgUri);

            //Bitmap --> byte[] array
            MemoryStream stream = new MemoryStream();
            newProfileImgBitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
            dogProfileImage = stream.ToArray();

            //Update database
            dog.ProfileImage = dogProfileImage;
            dataDogAccess.updateDog(dog);

            //Re-set the dog profile ImageView
            var bitmapDrawable = new BitmapDrawable(BitmapFactory.DecodeByteArray(dog.ProfileImage, 0, dog.ProfileImage.Length));
            RelativeLayoutDogProfile.SetBackgroundDrawable(bitmapDrawable);
        }
    }
}