using System;
using Android.OS;
using Android.Views;
using Android.Content.Res;
using Android.Support.V7.App;
using NewsGet_Android.Helpers;

namespace NewsGet_Android.Activities
{
	public abstract class BaseActivity : AppCompatActivity
	{
		public Android.Support.V7.Widget.Toolbar Toolbar { get; set; }
		private NewsGetApplication app;

		public override void OnConfigurationChanged (Configuration newConfig)
		{
			base.OnConfigurationChanged (newConfig);
			// This will fix the locale change when switching from portrait to landscape and otherwise
			app.SetLocale (app.GetLanguage ());
		}

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			try
			{
				SetContentView (LayoutResource);
			}
			catch (Exception)
			{
				SetContentView (LayoutResource);
			}

			app = (NewsGetApplication) this.Application;

			Toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);

			// Set the status bar color based on styles for API 21+
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
			{
				Window window = this.Window;
				window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
			}

			if (Toolbar != null)
			{
				SetSupportActionBar(Toolbar);
				SupportActionBar.SetTitle (ActionBarTitleResource);
//				SupportActionBar.SetDisplayHomeAsUpEnabled(true);
//				SupportActionBar.SetHomeButtonEnabled (true);
			}
		}

		protected abstract int LayoutResource { get; }
		protected abstract int ActionBarTitleResource { get; }
	}
}

