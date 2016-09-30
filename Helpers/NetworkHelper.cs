using System;

using Android.Net;
using Android.Content;

namespace NewsGet_Android
{
	public static class NetworkHelper
	{
		public static bool IsOnline (Context context)
		{
			ConnectivityManager connectivityManager = (ConnectivityManager) context.GetSystemService(Context.ConnectivityService);
			NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
			bool isOnline = (activeConnection != null) && activeConnection.IsConnected;

			return isOnline;
		}
	}
}

