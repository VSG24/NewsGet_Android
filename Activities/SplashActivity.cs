using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using NewsGet_Android.Helpers;

namespace NewsGet_Android.Activities
{
	[Activity (Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/ic_launcher", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
	public class SplashActivity : AppCompatActivity
	{
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			SetContentView (Resource.Layout.activity_splash);

			NewsGetApplication app = (NewsGetApplication) this.Application;
			app.SetLocale (app.GetLanguage ());

			this.Window.AddFlags(WindowManagerFlags.Fullscreen);

			Task startupWork = new Task(() => {
//				Task.Delay(5000);  // Simulate a bit of startup work.
				Thread.Sleep (1500);
			});

			startupWork.ContinueWith(t => {
				StartActivity(new Intent(Application.Context, typeof(HomeActivity)));
				Finish ();
			}, TaskScheduler.FromCurrentSynchronizationContext());

			startupWork.Start();
		}

//		protected override void OnResume ()
//		{
//			base.OnResume ();
//
//			Task startupWork = new Task(() => {
//				Task.Delay(5000);  // Simulate a bit of startup work.
//			});
//
//			startupWork.ContinueWith(t => {
//				StartActivity(new Intent(Application.Context, typeof(HomeActivity)));
//				Finish ();
//			}, TaskScheduler.FromCurrentSynchronizationContext());
//
//			startupWork.Start();
//		}
	}
}

