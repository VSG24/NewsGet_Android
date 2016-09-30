using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;

using NewsGet_Android.Models.Db;
using Newtonsoft.Json;
using Android.Content;

namespace NewsGet_Android.Helpers
{
	public sealed class DatabaseAccess
	{
		public readonly string SettingsDb = System.IO.Path.Combine( System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ngdb.json");
		public readonly string VerHelper = System.IO.Path.Combine( System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "verhelper.json");
		public readonly string FavoritesDb = System.IO.Path.Combine( System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "ng_favarticles_db.json");
		public readonly string SavedArticlesFolder = System.IO.Path.Combine( System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "saved_articles");
		public readonly string ErrorLogFolder = System.IO.Path.Combine( System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "errorlog");

		private int appVerNum = 0;
		private Context context;

		public DatabaseAccess ()
		{
			//
		}

		public DatabaseAccess (int appvernum, Context context)
		{
			this.appVerNum = appvernum;
			this.context = context;
		}

		public DatabaseAccess (int appvernum)
		{
			this.appVerNum = appvernum;
		}

		public void CreateDatabase()
		{
			if(!Directory.Exists (SavedArticlesFolder))
			{
				Directory.CreateDirectory (SavedArticlesFolder);
			}
			if(!Directory.Exists (ErrorLogFolder))
			{
				Directory.CreateDirectory (ErrorLogFolder);
			}
			if(!File.Exists (SettingsDb))
			{
				this.WriteSettings ();
			}
			if(!File.Exists (FavoritesDb))
			{
				File.WriteAllText (FavoritesDb, "");
			}
			if(File.Exists (VerHelper) && File.ReadAllText (VerHelper) != this.appVerNum.ToString ())
			{
				File.WriteAllText (VerHelper, this.appVerNum.ToString ());
			}
		}

		private void WriteSettings ()
		{
			var preferences = context.GetSharedPreferences("newsget_settings", FileCreationMode.Private);
			var editor = preferences.Edit();

			editor.PutString("lang", "fa");
			editor.PutString("default_server", "server1");
			editor.PutString("server1", "http://newsget.in");
			editor.PutString("server2", "http://newsget.vsgcdn.com");
			editor.PutBoolean("load_images", true);
			editor.PutBoolean("save_articles_for_offline", true);
			editor.PutInt("textsize_single", 100);
			editor.PutBoolean("display_datestring_nav", true);
			editor.PutBoolean("prevent_from_sleep", true);
            editor.PutBoolean("loadOriginalUrlOnWebView", true);

            editor.Apply();

			File.WriteAllText (SettingsDb, "App is initialized successfully.");
		}

        public void InsertAllGeneric<T>(List<T> list, string jsonfile)
        {
            string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(jsonfile, json);
        }

        public void InsertAllGeneric<T>(ObservableCollection<T> list, string jsonfile)
        {
            string json = JsonConvert.SerializeObject(list, Formatting.Indented);
            File.WriteAllText(jsonfile, json);
        }

        public List<T> GetAllGeneric<T>(string jsonfile)
        {
            /*
        	"/data/data/com.atvsg.android.newsget/files/ngdb.json"
        	*/
            var json = File.ReadAllText(jsonfile);
            return JsonConvert.DeserializeObject<List<T>>(json);
        }
    }
}