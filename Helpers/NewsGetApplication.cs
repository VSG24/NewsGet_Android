using System;

using Android.Runtime;
using Android.App;
using Android.Support.V4.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using NewsGet_Android.Models.Db;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using Com.OneSignal;
using Android.Gms.Common;
using NewsGet_Android.Activities;
using System.Collections.Generic;

namespace NewsGet_Android.Helpers
{
	[Application]
	public class NewsGetApplication : Application
	{
		private DatabaseAccess db;

		public static Context context;

		private int AppVerNum = 0;
        private List<string> AllServers = new List<string>();
        private string Server = "http://newsget.in";
		// DefServer is the short word used in nav drawer
		private string DefServer;
		private string Language = "fa";
		private bool Images = true;
		private int TextSize;
		private bool SaveArticlesOffline = true;
		private bool DisplayDateStringOnNav = true;
		private bool PreventFromSleep = true;
        private bool LoadOriginalUrlWebView = true;

		public bool shouldhardkill = false;
		public static bool shouldfinishhome = false;


		private static readonly int NewVersionNotificationId = 1000;

		public NewsGetApplication (IntPtr handle, JniHandleOwnership transfer) : base (handle, transfer)
		{
			// do any initialisation you want here (for example initialising properties)

			context = this;
		}

		public override void OnCreate ()
		{
			base.OnCreate();

			try
			{
				this.AppVerNum = this.PackageManager.GetPackageInfo(this.PackageName, 0).VersionCode;
			}
			catch (Exception)
			{
				this.AppVerNum = -1;
			}

			var gpsqueryres = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable (this);
			// Check if Google Play Services is available
			if (gpsqueryres == ConnectionResult.Success)
			{
				// Initialize OneSignal
				try
				{
//					OneSignal.StartInit (this).SetNotificationOpenedHandler (new PushNotificationHandler ()).Init ();
					OneSignal.Init (PushNotificationHandler.NotificationOpenedDelegate);
					OneSignal.EnableInAppAlertNotification (true);
				}
				catch (Exception)
				{
					HomeActivity.PushNotifFailInit = true;
				}
			}

			//

//			this.db = new DatabaseAccess (GetAppVerNum ());
			this.db = new DatabaseAccess (GetAppVerNum (), this);
			this.db.CreateDatabase ();
			this.RefreshSettings ();

			this.SetLocale (this.Language);
		}

        public bool SetOption(string option_name, object option_value)
        {
            var preferences = context.GetSharedPreferences("newsget_settings", FileCreationMode.Private);
            var editor = preferences.Edit();

            if(option_value is int)
            {
                editor.PutInt(option_name, (int) option_value);
            }
            else if(option_value is bool)
            {
                editor.PutBoolean(option_name, (bool) option_value);
            }
            else if(option_value is string)
            {
                editor.PutString(option_name, (string) option_value);
            }
            editor.Apply();
            this.RefreshSettings();
            return true;
        }

        //public bool SetOptionDelayed(string option_name, string option_value)
        //{
        //	var optionsdb = this.db.GetAllGeneric<Setting> (this.db.SettingsDb);
        //	var index = optionsdb.FindIndex (o => o.Name == option_name);
        //	if(index == -1)
        //	{
        //		return false;
        //	}

        //	optionsdb [index].Value = option_value;
        //	this.db.InsertAllGeneric (optionsdb, this.db.SettingsDb);
        //	return true;
        //}

        public void RefreshSettings ()
		{
            var preferences = context.GetSharedPreferences("newsget_settings", FileCreationMode.Private);

            this.Language = preferences.GetString("lang", "fa");
            this.Images = preferences.GetBoolean("load_images", true);
            this.SaveArticlesOffline = preferences.GetBoolean("save_articles_for_offline", true);
            this.DisplayDateStringOnNav = preferences.GetBoolean("display_datestring_nav", true);
            this.PreventFromSleep = preferences.GetBoolean("prevent_from_sleep", true);
            this.TextSize = preferences.GetInt("textsize_single", 100);
            this.LoadOriginalUrlWebView = preferences.GetBoolean("loadOriginalUrlOnWebView", true);

            var server1Url = preferences.GetString("server1", "http://newsget.in");
            var server2Url = preferences.GetString("server2", "http://newsget.vsgcdn.com");

            if(preferences.GetString("default_server", "") == "server1")
            {
                this.Server = server1Url;
                this.DefServer = "(sv1)";
            }
            else if(preferences.GetString("default_server", "") == "server2")
            {
                this.Server = server2Url;
                this.DefServer = "(sv2)";
            }

            this.AllServers.Add(server1Url);
            this.AllServers.Add(server2Url);

   //         var optionsdb = this.db.GetAllGeneric<Setting> (this.db.SettingsDb);

			//var server1 = optionsdb.Find (s => s.Name == "server1");
			//var server2 = optionsdb.Find (s => s.Name == "server2");
			//var server = optionsdb.Find (s => s.Name == "default_server");
			//var imgstate = optionsdb.Find (i => i.Name == "images");
			//var txtsize = optionsdb.Find (t => t.Name == "textsize_single");
			//var savearticlesoffline = optionsdb.Find (o => o.Name == "save_articles_offline");
			//var displatdatestring_nav = optionsdb.Find (o => o.Name == "display_datestring_nav");
			//var preventfromsleep = optionsdb.Find (p => p.Name == "prevent_from_sleep");

			//this.AllServers.Add (server1.Value);
			//this.AllServers.Add (server2.Value);

			//var svval = server.Value;
			//// Possible crash because of the lack of else block
			//if(svval == server1.Name)
			//{
			//	this.Server = server1.Value;
			//	this.DefServer = "(sv1)";
			//}
			//else if(svval == server2.Name)
			//{
			//	this.Server = server2.Value;
			//	this.DefServer = "(sv2)";
			//}

			//if (imgstate.Value == "true")
			//	this.Images = true;
			//else if (imgstate.Value == "false")
			//	this.Images = false;
			//else
			//	this.Images = true;

			//if (savearticlesoffline.Value == "true")
			//	this.SaveArticlesOffline = true;
			//else if (savearticlesoffline.Value == "false")
			//	this.SaveArticlesOffline = false;
			//else
			//	this.SaveArticlesOffline = true;

			//if (displatdatestring_nav.Value == "true")
			//	this.DisplayDateStringOnNav = true;
			//else if (displatdatestring_nav.Value == "false")
			//	this.DisplayDateStringOnNav = false;
			//else
			//	this.DisplayDateStringOnNav = true;

			//if (preventfromsleep.Value == "true")
			//	this.PreventFromSleep = true;
			//else if (preventfromsleep.Value == "false")
			//	this.PreventFromSleep = false;
			//else
			//	this.PreventFromSleep = true;

			// Language

			//var preferences = context.GetSharedPreferences ("newsget_settings", FileCreationMode.Private);
			//if(preferences.Contains ("lang"))
			//{
			//	var lang = preferences.GetString("lang", "fa");
			//	this.Language = lang;
			//}

			//

			//this.TextSize = int.Parse (txtsize.Value);
		}

		//public void SetLanguage (string language)
		//{
		//	var preferences = context.GetSharedPreferences ("newsget_settings", FileCreationMode.Private);
		//	var editor = preferences.Edit ();

		//	if(preferences.Contains ("lang"))
		//	{
		//		editor.PutString ("lang", language);
		//		editor.Commit ();
		//	}
		//}

		public int GetAppVerNum ()
		{
			return this.AppVerNum;
		}

        public List<string> GetAllServers()
        {
            return this.AllServers;
        }

        public string GetServer ()
		{
			return this.Server;
		}

		public void SetServer (string server)
		{
			this.Server = server;
		}

		public string GetDefServer ()
		{
			return this.DefServer;
		}

		public void SetDefServer (string defserver)
		{
			this.DefServer = defserver;
		}

		public string GetLanguage ()
		{
			return this.Language;
		}

		public bool GetImagesState ()
		{
			return this.Images;
		}

		public int GetTextSize ()
		{
			return this.TextSize;
		}

		public bool GetSaveArticlesOfflineState ()
		{
			return this.SaveArticlesOffline;
		}

		public bool GetDisplayDateStringOnNavState ()
		{
			return this.DisplayDateStringOnNav;
		}

		public bool GetPreventFromSleepState ()
		{
			return this.PreventFromSleep;
		}

        public bool GetLoadOriginalUrlWebView ()
        {
            return this.LoadOriginalUrlWebView;
        }
			
		public void ShowUpdateNotif (int newVersion, string description)
		{
			// When the user clicks the notification, SecondActivity will start up.
//			Intent resultIntent = new Intent(this, typeof (NewsGet_Android.Activities.HomeActivity));

			// Use ActionEdit for rating and comment

			Intent newsget_on_market = MarketHelper.GetOpenMarketIntent ();

//			newsget_on_market.AddFlags(ActivityFlags.NewTask);
//			StartActivity(newsget_on_market);

			// Construct a back stack for cross-task navigation:
			TaskStackBuilder stackBuilder = TaskStackBuilder.Create (this);
			stackBuilder.AddParentStack (Java.Lang.Class.FromType(typeof(NewsGet_Android.Activities.HomeActivity)));
			stackBuilder.AddNextIntent (newsget_on_market);

			// Create the PendingIntent with the back stack:            
			PendingIntent resultPendingIntent = 
				stackBuilder.GetPendingIntent (0, (int) PendingIntentFlags.UpdateCurrent);

			string content = null;
			if (description != null)
			{
				content = description;
			}
			else
			{
				content = GetString (Resource.String.new_version_available_desc);
			}

			Bitmap largeicon_bitmap = BitmapFactory.DecodeResource (this.Resources, Resource.Mipmap.ic_launcher);

			// Build the notification:
			NotificationCompat.Builder builder = new NotificationCompat.Builder (this)
				.SetAutoCancel (true)                    // Dismiss from the notif. area when clicked
				.SetContentIntent (resultPendingIntent)  // Start 2nd activity when the intent is clicked.
				.SetContentTitle (GetString (Resource.String.new_version_available))      // Set its title
				.SetSound (RingtoneManager.GetDefaultUri(RingtoneType.Notification))    // Play the default notifications sound
				//.SetNumber (count)                       // Display the count in the Content Info
				.SetSmallIcon(Resource.Drawable.ic_stat_onesignal_default)  // Display this icon
				.SetLargeIcon (largeicon_bitmap)
				.SetContentText (content); // The message to display.

			// Finally, publish the notification:
			NotificationManager notificationManager = 
				(NotificationManager) GetSystemService (Context.NotificationService);
			notificationManager.Notify (NewsGetApplication.NewVersionNotificationId, builder.Build());
		}

		public void SetLocale (string lang)
		{
			Java.Util.Locale locale = new Java.Util.Locale (lang);
			Java.Util.Locale.Default = locale;
			Android.Content.Res.Configuration config = new Android.Content.Res.Configuration ();
			config.SetLocale (locale);
			BaseContext.Resources.UpdateConfiguration (config, BaseContext.Resources.DisplayMetrics);
//			this.Resources.UpdateConfiguration (config, null);
//			return;
		}

		public bool ShouldAskForRate ()
		{
			var preferences = this.GetSharedPreferences ("newsget_settings", FileCreationMode.Private);
			var editor = preferences.Edit ();

			// Check if the setting for rate exist
			if(!preferences.Contains ("rated"))
			{
				// It doesn't exist, create it
				editor.PutBoolean ("rated", false);
				editor.PutBoolean ("shouldask", true);
				editor.Apply();
				return true;
			}
			else
			{
				// It exists
				if(preferences.GetBoolean ("shouldask", true) == false)
				{
					// User doesn't want to be bothered again
					return false;
				}
				else if(preferences.Contains ("remindday") && preferences.GetBoolean ("remindactive", false) == true && long.Parse (preferences.GetString ("remindday", DateHelpers.GetTimestamp (DateTime.Now))) <= long.Parse (DateHelpers.GetTimestamp (DateTime.Now)))
				{
					editor.PutBoolean ("remindactive", false);
					editor.Apply();
					return true;
				}
				else if(preferences.GetBoolean ("rated", false) != true && preferences.GetBoolean ("remindactive", false) != true)
				{
					// Not rated yet
					return true;
				}
				else
				{
					// Already rated
					return false;
				}
			}
		}

		public void SetRateOptions (string key, object value)
		{
			var preferences = this.GetSharedPreferences ("newsget_settings", FileCreationMode.Private);
			var editor = preferences.Edit ();

			if(value.GetType () == typeof(bool))
			{
				editor.PutBoolean (key, (bool) value);
				editor.Commit ();
			}
			else if(value.GetType () == typeof(string))
			{
				editor.PutString (key, (string) value);
				editor.Commit ();
			}
		}
	}
}

