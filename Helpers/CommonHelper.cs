using System;
using System.Threading.Tasks;

using Android;
using Android.Support.V4.Content;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Content;
using Android.App;

namespace NewsGet_Android.Helpers
{
	public class CommonHelper
	{
		public const int StoragePermissionId = 0;

		public static string ToLowerFirstLetter (string s)
		{
			if (String.IsNullOrEmpty(s))
				return s;
			if (s.Length == 1)
				return s.ToLower ();
			return s.Remove(1).ToLower () + s.Substring(1);
		}

		public static string ToUpperFirstLetter (string s)
		{
			if (String.IsNullOrEmpty(s))
				return s;
			if (s.Length == 1)
				return s.ToUpper();
			return s.Remove(1).ToUpper() + s.Substring(1);
		}

		public async static Task<string[]> CheckForUpdate ()
		{
			var rest = new RestAccess ();
			var result = await rest.GetLatestVersionNumber ();

			var ret = new string[] { result.Status, result.Description };
			return ret;
		}
	}
}

