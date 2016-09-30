using System;
using System.Collections.Generic;
using Org.Json;
using Com.OneSignal;
using NewsGet_Android.Models.Db;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;

namespace NewsGet_Android.Helpers
{
	public class PushNotificationHandler
	{
//		public void NotificationOpened (string message, JSONObject additionalData, bool isActive)
//		{
//			try
//			{
//				if (additionalData != null)
//				{
//					PushNotificationHandler.ServerValuesHandler (additionalData);
//					if (additionalData.Has ("actionSelected"))
//					{
//						if (additionalData.GetString ("actionSelected") == "update")
//						{
//							var intent = MarketHelper.GetOpenMarketIntent ();
//							intent.AddFlags (ActivityFlags.NewTask);
//							NewsGetApplication.shouldfinishhome = true;
//							NewsGetApplication.context.StartActivity (intent);
//							Intent close_statusbar = new Intent(Intent.ActionCloseSystemDialogs);
//							NewsGetApplication.context.SendBroadcast (close_statusbar);
//						}
//					}
//				}
//			}
//			catch (Exception e)
//			{
//				Console.WriteLine (e.StackTrace);
//			}
//		}

		public static OneSignal.NotificationOpened NotificationOpenedDelegate = delegate (string message, Dictionary<string, object> additionalData, bool isActive)
		{
			try
			{
				if (additionalData != null)
				{
					PushNotificationHandler.ServerValuesHandler (additionalData);
					if (additionalData.ContainsKey ("actionSelected"))
					{
						if ((string) additionalData["actionSelected"] == "update")
						{
							var intent = MarketHelper.GetOpenMarketIntent ();
							intent.AddFlags (ActivityFlags.NewTask);
							NewsGetApplication.shouldfinishhome = true;
							NewsGetApplication.context.StartActivity (intent);
							Intent close_statusbar = new Intent(Intent.ActionCloseSystemDialogs);
							NewsGetApplication.context.SendBroadcast (close_statusbar);
						}
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine (e.StackTrace);
			}
		};

		public static void ServerValuesHandler (Dictionary<string, object> additionalData)
		{
			var db = new DatabaseAccess();
			var alloptions = db.GetAllGeneric<Setting> (db.SettingsDb);

			NewsGetApplication app = (NewsGetApplication) NewsGetApplication.context.ApplicationContext;

			bool shouldRefresh = false;

			if (additionalData.ContainsKey("server1"))
			{
				var server1_broadcast = (string) additionalData["server1"];

				var index = alloptions.FindIndex (o => o.Name == "server1");
				if(index == -1)
				{
					return;
				}
				alloptions [index].Value = server1_broadcast.ToString ();

				shouldRefresh = true;
			}
			if (additionalData.ContainsKey ("server2"))
			{
				var server2_broadcast = (string) additionalData["server2"];

				var index = alloptions.FindIndex (o => o.Name == "server2");
				if(index == -1)
				{
					return;
				}
				alloptions [index].Value = server2_broadcast.ToString ();

				shouldRefresh = true;
			}
			if (additionalData.ContainsKey ("default_server"))
			{
				var server2_broadcast = (string) additionalData["default_server"];

				var index = alloptions.FindIndex (o => o.Name == "default_server");
				if(index == -1)
				{
					return;
				}
				alloptions [index].Value = server2_broadcast.ToString ();

				shouldRefresh = true;
			}

			// Write back to storage
			db.InsertAllGeneric (alloptions, db.SettingsDb);

			if(shouldRefresh)
			{
				app.RefreshSettings ();
			}
		}

		public static void ServerValuesHandler (JSONObject additionalData)
		{
			var db = new DatabaseAccess();
			var alloptions = db.GetAllGeneric<Setting> (db.SettingsDb);

			NewsGetApplication app = (NewsGetApplication) NewsGetApplication.context.ApplicationContext;

			bool shouldRefresh = false;

			if (additionalData.Has("server1"))
			{
				var server1_broadcast = additionalData.GetString ("server1");

				var index = alloptions.FindIndex (o => o.Name == "server1");
				if(index == -1)
				{
					return;
				}
				alloptions [index].Value = server1_broadcast;

				shouldRefresh = true;
			}
			if (additionalData.Has ("server2"))
			{
				var server2_broadcast = additionalData.GetString ("server2");

				var index = alloptions.FindIndex (o => o.Name == "server2");
				if(index == -1)
				{
					return;
				}
				alloptions [index].Value = server2_broadcast;

				shouldRefresh = true;
			}
			if (additionalData.Has ("default_server"))
			{
				var server2_broadcast = additionalData.GetString ("default_server");

				var index = alloptions.FindIndex (o => o.Name == "default_server");
				if(index == -1)
				{
					return;
				}
				alloptions [index].Value = server2_broadcast;

				shouldRefresh = true;
			}

			// Write back to storage
			db.InsertAllGeneric (alloptions, db.SettingsDb);

			if(shouldRefresh)
			{
				app.RefreshSettings ();
			}
		}
	}

	[BroadcastReceiver(Enabled = true)]
	[IntentFilter(new[] { "com.onesignal.BackgroundBroadcast.RECEIVE" })]
	public class NotificationDataHandler : WakefulBroadcastReceiver
	{
		public override void OnReceive (Context context, Intent intent)
		{
			Bundle dataBundle = intent.GetBundleExtra ("data");

			try
			{
				JSONObject customJSON = new JSONObject (dataBundle.GetString ("custom"));
				bool isappfocused = dataBundle.GetBoolean ("isActive");
				if(isappfocused)
				{
					// Just skip this method
					return;
				}

				if(customJSON.Has ("a"))
				{
					JSONObject additionalData = customJSON.GetJSONObject("a");
					PushNotificationHandler.ServerValuesHandler (additionalData);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine (e.StackTrace);
			}
		}
	}
}

