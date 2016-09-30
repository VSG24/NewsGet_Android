using System;
using System.Globalization;

using Android.App;
using Android.Views;
using Android.OS;
using Android.Content;
using Android.Content.Res;
using Android.Support.V7.Widget;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V4.View;
using NewsGet_Android.Fragments;
using NewsGet_Android.Helpers;

using JavaString = Java.Lang.String;
using Android.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Webkit;
using System.IO;

namespace NewsGet_Android.Activities
{
	[Activity (Label = "@string/app_name", ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
	public class HomeActivity : BaseActivity, Android.Support.V4.View.ViewPager.IOnPageChangeListener
	{
		private NavigationView navigationView;
		private DrawerLayout drawerLayout;
		public static Resources resources;
		public ViewPager viewPager;
		public static string app_ver;

		public int backpressed = 0;

		public NewsGetApplication app;

		public static string color_black;
		public static string color_primary;

		public static bool PushNotifFailInit = false;

		protected override int LayoutResource
		{
			get { return Resource.Layout.activity_home; }
		}

		protected override int ActionBarTitleResource
		{
			get { return Resource.String.app_name; }
		}

		public override void OnBackPressed ()
		{
			backpressed++;
			if(app.ShouldAskForRate ())
			{
				if(backpressed >= 2)
				{
					base.OnBackPressed ();
					return;
				}
				Android.Support.V7.App.AlertDialog alertDialog = new Android.Support.V7.App.AlertDialog.Builder(this).Create();
				alertDialog.SetTitle(Resource.String.rate_newsget);
				alertDialog.SetMessage (GetString (Resource.String.rate_sum));
				alertDialog.SetIcon (Resource.Mipmap.ic_launcher);

				alertDialog.SetButton ((int) Android.Content.DialogButtonType.Positive, GetString (Resource.String.rate_now), (asender, args) => {
					Intent openrate = MarketHelper.GetRateIntent ();
					try
					{
						StartActivity (openrate);
					}
					catch(Exception)
					{
						Android.Support.V7.App.AlertDialog errorAlertDialog = new Android.Support.V7.App.AlertDialog.Builder(this).Create();
						errorAlertDialog.SetTitle (Resource.String.error);
						errorAlertDialog.SetMessage (GetString (Resource.String.problem_starting_market));
						errorAlertDialog.SetButton ((int) Android.Content.DialogButtonType.Neutral, GetString (Resource.String.close), delegate {
							errorAlertDialog.Dismiss ();
						});
						alertDialog.Dismiss ();
						errorAlertDialog.Show ();
					}
					app.SetRateOptions ("rated", true);
					alertDialog.Dismiss ();
				});

				alertDialog.SetButton ((int) Android.Content.DialogButtonType.Neutral, GetString (Resource.String.no_thanks), (asender, args) => {
					app.SetRateOptions ("shouldask", false);
					alertDialog.Dismiss ();
				});

				alertDialog.SetButton ((int) Android.Content.DialogButtonType.Negative, GetString (Resource.String.remind_later), (asender, args) => {
					app.SetRateOptions ("remindday", DateHelpers.GetTimestamp (DateTime.Now.AddDays (7)));
					app.SetRateOptions ("remindactive", true);
					alertDialog.Dismiss ();
				});

				alertDialog.Show ();
			}
			else
			{
				base.OnBackPressed ();
			}
		}

		protected override void OnDestroy ()
		{
			base.OnDestroy ();

			// Force close the app
			if (app.shouldhardkill)
			{
				Android.OS.Process.KillProcess (Android.OS.Process.MyPid ());
			}

			//CacheHelper.ClearAppData (app);
		}

		protected async override void OnCreate (Bundle savedInstanceState)
		{
			if(NewsGetApplication.shouldfinishhome)
			{
				NewsGetApplication.shouldfinishhome = false;
				Finish ();
			}

			base.OnCreate (savedInstanceState);
			resources = Resources;

			app_ver = this.PackageManager.GetPackageInfo(this.PackageName, 0).VersionName;
			app = (NewsGetApplication) this.Application;

			navigationView = FindViewById<NavigationView> (Resource.Id.nav_view);

			View header = navigationView.GetHeaderView (0);
			var count = navigationView.HeaderCount;
			Android.Widget.TextView today_persian = (Android.Widget.TextView) header.FindViewById(Resource.Id.today_persian);
			Android.Widget.TextView today_gregorian = (Android.Widget.TextView) header.FindViewById(Resource.Id.today_gregorian);

			Android.Widget.ImageView app_logo = (Android.Widget.ImageView)header.FindViewById (Resource.Id.app_logo);
			app_logo.Click += (object sender, EventArgs e) => {
				Toast.MakeText (this, "Client: " + GetString (Resource.String.app_name_eng) + " " + HomeActivity.app_ver + "\nServer: " + app.GetDefServer () + " " + app.GetServer (), ToastLength.Long).Show ();
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
			SupportActionBar.SetHomeAsUpIndicator (Resource.Drawable.ic_menu_white_24dp);
			SupportActionBar.SetDisplayHomeAsUpEnabled (true);

			drawerLayout = FindViewById<DrawerLayout> (Resource.Id.drawer);

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

			// Get the ViewPager and set it's PagerAdapter so that it can display items
			viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);
			viewPager.Adapter = new HomeFragmentPagerAdapter (SupportFragmentManager, this);

			TabLayout tabLayout = FindViewById<TabLayout> (Resource.Id.sliding_tabs);
			// Select the default tab
			viewPager.CurrentItem = 1;
			viewPager.AddOnPageChangeListener (this);
			tabLayout.SetupWithViewPager (viewPager);

			this.ConnSnackNotif ();

			var mcolor_black = this.Resources.GetString (Resource.Color.black).Replace ("f", "");
			var mcolor_primary = this.Resources.GetString (Resource.Color.primary).Remove (1, 3);

			color_black = mcolor_black;
			color_primary = mcolor_primary;

			navigationView.NavigationItemSelected += (sender, e) => {
				e.MenuItem.SetChecked (true);
				// React to click here and swap fragments or navigate

				//this.drawerListView.SetItemChecked (position, true);
				//SupportActionBar.Title = this.title = Sections [position];
				//this.drawerLayout.CloseDrawers();

				if (e.MenuItem.ItemId == Resource.Id.nav_favorites)
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

            // Show the app's changelog

            var preferences = this.GetSharedPreferences("newsget_settings", FileCreationMode.Private);
            var editor = preferences.Edit();

            if(preferences.Contains($"v{app.GetAppVerNum()}-changelog-shown"))
            {
                goto SKIP_CHANGELOG;
            }

            AlertDialog.Builder changelogDialog = new AlertDialog.Builder(this);
            LayoutInflater inflater = LayoutInflater;
            View convertView = (View) inflater.Inflate(Resource.Layout.dialog_changelog, null);

            changelogDialog.SetView(convertView);

            string changelogString;
            AssetManager assets = this.Assets;
            using (StreamReader sr = new StreamReader(assets.Open($"newsget50changelog_{app.GetLanguage()}.txt")))
            {
                changelogString = sr.ReadToEnd();
            }

            var changelogWebview = convertView.FindViewById<WebView>(Resource.Id.webview_changelog_dialog);
            changelogWebview.LoadData(changelogString, "text/html; charset=UTF-8", null);

            changelogDialog.SetCancelable(true);
            changelogDialog.SetTitle(Resource.String.newsget_changelog);
            changelogDialog.Show();

            editor.PutBoolean($"v{app.GetAppVerNum()}-changelog-shown", true);
            editor.Apply();

            OfflineHelper.DeleteAllSavedArticles ();

            SKIP_CHANGELOG:

            //

			// Check for update and show a notification if there is one available
			try
			{
				var appVersionNumber = app.GetAppVerNum ();

				var latestappver = await CommonHelper.CheckForUpdate ();

				if (int.Parse (latestappver[0]) > appVersionNumber)
				{
					app.ShowUpdateNotif (int.Parse (latestappver[0]), latestappver[1]);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine (e.Message);
			}

			if(PushNotifFailInit)
			{
				if(!System.IO.File.Exists (System.IO.Path.Combine( System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "pushnotif_fail")))
				{
					Android.Support.V7.App.AlertDialog alertDialog = new Android.Support.V7.App.AlertDialog.Builder(this).Create();
					alertDialog.SetTitle(Resource.String.warning);
					alertDialog.SetMessage (GetString (Resource.String.push_notif_failed));

					alertDialog.SetButton ((int) Android.Content.DialogButtonType.Neutral, GetString (Resource.String.close), (asender, args) => {
						alertDialog.Dismiss ();
					});

					alertDialog.Show();
				}
				System.IO.File.Create (System.IO.Path.Combine( System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "pushnotif_fail"));
			}
		}

		public void OnPageScrollStateChanged (int state)
		{
		}
		public void OnPageScrolled (int position, float positionOffset, int positionOffsetPixels){
		}

		public void OnPageSelected (int position)
		{
			this.ConnSnackNotif ();
		}

		private void ConnSnackNotif()
		{
			if(!NetworkHelper.IsOnline (this))
			{
				Snackbar
					.Make (FindViewById<ViewPager> (Resource.Id.viewpager), Resource.String.no_connection, Snackbar.LengthLong)
					.SetAction (Resource.String.retryU, delegate {
						if(NetworkHelper.IsOnline (this))
						{
							Recreate ();
						}
						else
							this.ConnSnackNotif ();
					})
					.Show (); // Don’t forget to show!
			}
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate (Resource.Menu.main, menu);
            var beta_text = menu.FindItem(Resource.Id.beta_text);
            beta_text.SetVisible(false);
            //SpannableString ss = new SpannableString(Resources.GetString(Resource.String.beta_version));
            //ss.SetSpan(new ForegroundColorSpan(Android.Graphics.Color.Red), 0, ss.Length(), SpanTypes.User);
            //beta_text.SetTitle(ss);
            //beta_text.SetCheckable(false);

            return base.OnPrepareOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId) 
			{
			    case Android.Resource.Id.Home:
				    drawerLayout.OpenDrawer (Android.Support.V4.View.GravityCompat.Start);
				    return true;
			    case Resource.Id.action_manual:
				    Intent browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(GetString (Resource.String.newsget_manual)));
				    StartActivity(browserIntent);
				    return true;
			}
			return base.OnOptionsItemSelected (item);
		}
	}

	public class HomeFragmentPagerAdapter : Android.Support.V4.App.FragmentPagerAdapter
	{
		readonly int page_count = 3;
		public static JavaString[] tabTitles = new[]
		{
			new JavaString(HomeActivity.resources.GetString (Resource.String.sites)),
			new JavaString(HomeActivity.resources.GetString (Resource.String.latest)),
			new JavaString(HomeActivity.resources.GetString (Resource.String.hot))
		};
		private Android.Content.Context context;

		public HomeFragmentPagerAdapter(Android.Support.V4.App.FragmentManager fm, Android.Content.Context context) : base(fm)
		{
			this.context = context;
		}

		public override int Count
		{
			get { return page_count; }
		}

		public override Android.Support.V4.App.Fragment GetItem (int position)
		{
			switch (position)
			{
			case 0:
				ProvidersFragment tab1 = new ProvidersFragment ();
				return tab1;
			case 1:
				LatestNewsFragment tab2 = new LatestNewsFragment ();
				return tab2;
			case 2:
				MostViewedNewsFragment tab3 = new MostViewedNewsFragment ();
				return tab3;
			default:
				return null;
			}
		}

		public override Java.Lang.ICharSequence GetPageTitleFormatted (int position)
		{
			return tabTitles [position];
		}
	}
}


