using System;
using System.Globalization;

using Android.App;
using Android.Views;
using Android.OS;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using NewsGet_Android.Fragments;
using NewsGet_Android.Helpers;

namespace NewsGet_Android.Activities
{
    [Activity (Label = "@string/app_name", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize, ParentActivity = typeof(HomeActivity))]			
	public class SwitcherActivity : BaseActivity
	{
		private NavigationView navigationView;
		private DrawerLayout drawerLayout;

		public NewsGetApplication app;

		private bool isSingle;
		public static bool isLatestProvider = false;

		protected override int LayoutResource
		{
			get { return Resource.Layout.activity_switcher; }
		}

		protected override int ActionBarTitleResource
		{
			get { return Resource.String.app_name; }
		}

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			app = (NewsGetApplication) this.Application;

			string source = Intent.GetStringExtra ("Source") ?? null;
			string url = Intent.GetStringExtra ("Url") ?? null;
            string originalUrl = Intent.GetStringExtra("OriginalUrl") ?? null;
            string provider = Intent.GetStringExtra ("Provider") ?? null;
			string displayname = Intent.GetStringExtra ("DisplayName") ?? null;
			string favorites = Intent.GetStringExtra ("Favorites") ?? null;
			string settings = Intent.GetStringExtra ("Settings") ?? null;
			string report = Intent.GetStringExtra ("Report") ?? null;
			string about = Intent.GetStringExtra ("About") ?? null;

			navigationView = FindViewById<NavigationView> (Resource.Id.nav_view);

			if(app.GetPreventFromSleepState ())
			{
				Window.AddFlags (WindowManagerFlags.KeepScreenOn);
			}

			View header = navigationView.GetHeaderView (0);
			var count = navigationView.HeaderCount;
			Android.Widget.TextView today_persian = (Android.Widget.TextView) header.FindViewById(Resource.Id.today_persian);
			Android.Widget.TextView today_gregorian = (Android.Widget.TextView) header.FindViewById(Resource.Id.today_gregorian);

			Android.Widget.ImageView app_logo = (Android.Widget.ImageView)header.FindViewById (Resource.Id.app_logo);
			app_logo.Click += (object sender, EventArgs e) => {
				Android.Widget.Toast.MakeText (this, "Client: " + GetString (Resource.String.app_name_eng) + " " + HomeActivity.app_ver + "\nServer: " + app.GetDefServer () + " " + app.GetServer (), Android.Widget.ToastLength.Long).Show ();
			};

			var pc = new PersianCalendar ();
			string persian_date = null;
			string gregorian_date = null;

			if (app.GetDisplayDateStringOnNavState ())
			{
				var dayofweekint = (int) DateTime.Now.DayOfWeek;
				var dayofweek_pestring = dayofweekint.ToPeStringRep ();

				persian_date = "     " + dayofweek_pestring + "   " + pc.GetYear (DateTime.Now) + "/" + pc.GetMonth (DateTime.Now).AddLeadingZeros () + "/" + pc.GetDayOfMonth (DateTime.Now).AddLeadingZeros ();
				gregorian_date = DateTime.Now.Year + "/" + DateTime.Now.Month.AddLeadingZeros () + "/" + DateTime.Now.Day.AddLeadingZeros () + "   " + DateTime.Now.DayOfWeek;
			}
			else
			{
				persian_date = pc.GetYear (DateTime.Now) + "/" + pc.GetMonth (DateTime.Now).AddLeadingZeros () + "/" + pc.GetDayOfMonth (DateTime.Now).AddLeadingZeros ();
				gregorian_date = DateTime.Now.Year + "/" + DateTime.Now.Month.AddLeadingZeros () + "/" + DateTime.Now.Day.AddLeadingZeros ();
			}

			today_persian.Text = persian_date;
			today_gregorian.Text = gregorian_date;

			// Enable support action bar to display hamburger button
			SupportActionBar.SetDisplayHomeAsUpEnabled (true);

			drawerLayout = FindViewById<DrawerLayout> (Resource.Id.drawer);

			if (source != null && url != null)
			{
				Bundle bundle = new Bundle();
				bundle.PutString("Source", source);
				bundle.PutString("Url", url);
                bundle.PutString("OriginalUrl", originalUrl);

				this.isSingle = true;
				SupportActionBar.SetHomeAsUpIndicator (Resource.Drawable.ic_arrow_back_white_24dp);
				drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
				SupportFragmentSwtich (new SingleNewsFragment(), Resource.Id.content_frame, bundle);
			}
			else if (provider != null && displayname != null)
			{
				Bundle bundle = new Bundle();
				bundle.PutString("Provider", provider);
				bundle.PutString ("DisplayName", displayname);

				// This should definitely exist, just saying
				var source_url = Intent.GetStringExtra ("SourceWebsite") ?? null;
				bundle.PutString ("SourceWebsite", source_url);

				SupportActionBar.SetHomeAsUpIndicator (Resource.Drawable.ic_menu_white_24dp);
				isLatestProvider = true;
				SupportFragmentSwtich (new LatestNewsFragment(), Resource.Id.content_frame, bundle);
			}
			else if (favorites != null)
			{
				// This is just used to fix the behaviour of the back icon that is the same as the one in single fragment
				this.isSingle = true;
				SupportActionBar.SetHomeAsUpIndicator (Resource.Drawable.ic_menu_white_24dp);
				SupportActionBar.SetHomeAsUpIndicator (Resource.Drawable.ic_arrow_back_white_24dp);
				drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
				SupportFragmentSwtich (new FavoriteArticlesFragment(), Resource.Id.content_frame, null);
			}
			else if (settings != null)
			{
				this.isSingle = true;
				SupportActionBar.SetHomeAsUpIndicator (Resource.Drawable.ic_menu_white_24dp);
				SupportActionBar.SetHomeAsUpIndicator (Resource.Drawable.ic_arrow_back_white_24dp);
				drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
				SupportFragmentSwtich (new SettingsFragment (), Resource.Id.content_frame, null);
			}
			else if (about != null)
			{
				this.isSingle = true;
				SupportActionBar.SetHomeAsUpIndicator (Resource.Drawable.ic_menu_white_24dp);
				SupportActionBar.SetHomeAsUpIndicator (Resource.Drawable.ic_arrow_back_white_24dp);
				drawerLayout.SetDrawerLockMode(DrawerLayout.LockModeLockedClosed);
				SupportFragmentSwtich (new AboutFragment (), Resource.Id.content_frame, null);
			}
			else
			{
				var intent = new Intent(this, typeof(HomeActivity));
				StartActivity (intent);
			}

            var menu = navigationView.Menu;
			var imageswitch_parent = menu.FindItem (Resource.Id.image_switch_parent);
			SwitchCompat imageswitch = imageswitch_parent.ActionView.FindViewById<SwitchCompat> (Resource.Id.nav_image_switch);

			var offlinesave_parent = menu.FindItem (Resource.Id.offlinesave_switch_parent);
			SwitchCompat offlineswitch = offlinesave_parent.ActionView.FindViewById<SwitchCompat> (Resource.Id.nav_offlinearticles_switch);

			if (app.GetImagesState ())
			{
				imageswitch.Checked = true;
			}
			else
			{
				imageswitch.Checked = false;
			}

			imageswitch.CheckedChange += (sender, e) => {
                if (e.IsChecked)
                {
                    app.SetOption("load_images", true);
                }
                else
                {
                    app.SetOption("load_images", false);
                }
            };

			if (app.GetSaveArticlesOfflineState ())
			{
				offlineswitch.Checked = true;
			}
			else
			{
				offlineswitch.Checked = false;
			}

			offlineswitch.CheckedChange += (sender, e) => {
                if (e.IsChecked)
                {
                    app.SetOption("save_articles_for_offline", true);
                }
                else
                {
                    app.SetOption("save_articles_for_offline", false);
                }
            };

			navigationView.NavigationItemSelected += (sender, e) => {
				e.MenuItem.SetChecked (true);

//				this.drawerLayout.SetItemChecked (position, true);
//				SupportActionBar.Title = this.title = Sections [position];
//				this.drawerLayout.CloseDrawers();

				if (e.MenuItem.ItemId == Resource.Id.nav_home)
				{
					Finish ();
				}
				else if (e.MenuItem.ItemId == Resource.Id.nav_favorites)
				{
					var intent = new Intent(this, typeof(SwitcherActivity));
					intent.PutExtra("Favorites", "true");
					StartActivity(intent);
				}
				else if (e.MenuItem.ItemId == Resource.Id.nav_settings)
				{
					var intent = new Intent (this, typeof(SwitcherActivity));
					intent.PutExtra ("Settings", "true");
					StartActivity (intent);
				}
				else if (e.MenuItem.ItemId == Resource.Id.nav_about)
				{
					var intent = new Intent (this, typeof(SwitcherActivity));
					intent.PutExtra ("About", "true");
					StartActivity (intent);
				}

				drawerLayout.CloseDrawers ();
			};
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
//			MenuInflater.Inflate (Resource.Menu.main, menu);
//			menu.FindItem (Resource.Id.action_refresh).SetVisible (false);

			return base.OnPrepareOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			if(this.isSingle)
			{
				switch (item.ItemId) 
				{
					case Android.Resource.Id.Home:
						Finish ();
						return true;
				}
			}
			else
			{
				switch (item.ItemId) 
				{
					case Android.Resource.Id.Home:
						drawerLayout.OpenDrawer (Android.Support.V4.View.GravityCompat.Start);
						return true;
				}
			}
			return base.OnOptionsItemSelected (item);
		}

		private void SupportFragmentSwtich(Android.Support.V4.App.Fragment fragment, int containerView, Bundle bundle)
		{
			if(bundle != null)
			{
				fragment.Arguments = bundle;
			}
			SupportFragmentManager.BeginTransaction ()
				.Replace (containerView, fragment)
				.Commit ();
		}

		private void SupportFragmentSwtich(Android.Preferences.PreferenceFragment fragment, int containerView, Bundle bundle)
		{
			if(bundle != null)
			{
				fragment.Arguments = bundle;
			}
			FragmentManager.BeginTransaction ()
				.Replace (containerView, fragment)
				.Commit ();
		}
	}
}

