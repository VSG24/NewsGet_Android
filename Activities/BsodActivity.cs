using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.Res;

namespace NewsGet_Android.Activities
{
    [Activity(Label = "BsodActivity")]
    public class BsodActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_bsod);
            
            RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;

            var originalOrientation = Resources.Configuration.Orientation;

            var bsod = FindViewById(Resource.Id.bsod_activity);
            bsod.Click += (object sender, EventArgs e) =>
            {
                if (originalOrientation == Android.Content.Res.Orientation.Landscape)
                {
                    RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
                }
                else if (originalOrientation == Android.Content.Res.Orientation.Portrait)
                {
                    RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
                }
                else
                {
                    RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
                }
                this.OnBackPressed();
                //Finish();
            };

            View decorView = Window.DecorView;
            // Hide the status bar.
            decorView.SystemUiVisibility = StatusBarVisibility.Hidden;
        }
    }
}