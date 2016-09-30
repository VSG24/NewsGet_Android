using System;
using System.IO;

using Android;
using Android.Content;

namespace NewsGet_Android.Helpers
{
	public class CacheHelper
	{
		public static void ClearAppData(Context applicationContext)
		{
			string cache_dir = applicationContext.CacheDir.ToString ();
			if (cache_dir != null && Directory.Exists (cache_dir))
			{
				Directory.Delete (cache_dir, true);
			}
		}
	}
}

