using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Preferences;
using NewsGet_Android.Helpers;

namespace NewsGet_Android.Fragments
{
    public class SettingsFragment : PreferenceFragment
	{
		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			AddPreferencesFromResource (Resource.Layout.fragment_settings);

			Preference button = (Preference) FindPreference ("deleteSavedArticles");
			button.PreferenceClick += (object sender, Preference.PreferenceClickEventArgs e) => {
				Android.Support.V7.App.AlertDialog alertDialog = new Android.Support.V7.App.AlertDialog.Builder(Activity).Create();
				alertDialog.SetTitle(Resource.String.delete_saved_articles_q);

				alertDialog.SetButton ((int) Android.Content.DialogButtonType.Positive, GetString (Resource.String.delete), (asender, args) => {
					OfflineHelper.DeleteAllSavedArticles ();
				});

				alertDialog.SetButton ((int) Android.Content.DialogButtonType.Negative, GetString (Resource.String.cancel), (asender, args) => {
					alertDialog.Dismiss ();
				});

				alertDialog.Show();
			};
		}

		public override void OnDestroy ()
		{
			base.OnDestroy ();
			NewsGetApplication app = (NewsGetApplication) Activity.Application;

            var oldLang = app.GetLanguage();

			ISharedPreferences sp = PreferenceManager.GetDefaultSharedPreferences(Activity);
			bool showDayOfTheWeek = sp.GetBoolean("dayOfTheWeek", true);
			string language = sp.GetString ("appLanguage", "fa");
			string server = sp.GetString ("appServer", "server1");
			bool keepScreenOn = sp.GetBoolean ("keepScreenOn", true);
            bool loadOriginalUrlOnWebView = sp.GetBoolean("loadOriginalUrlOnWebView", true);


            var preferences = Activity.GetSharedPreferences("newsget_settings", FileCreationMode.Private);
			var editor = preferences.Edit();

			editor.PutString("lang", language);
            editor.PutString("default_server", server);
            editor.PutBoolean("display_datestring_nav", showDayOfTheWeek);
            editor.PutBoolean("prevent_from_sleep", keepScreenOn);
            editor.PutBoolean("loadOriginalUrlOnWebView", loadOriginalUrlOnWebView);

            editor.Apply();

			app.RefreshSettings ();

            if(oldLang != language)
            {
                app.shouldhardkill = true;
                Toast.MakeText(Activity, Resource.String.changes_rerun, ToastLength.Short).Show();
            }

        }
	}
}

