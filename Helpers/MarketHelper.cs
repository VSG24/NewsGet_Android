using System;
using Android.Content;
using Uri = Android.Net.Uri;

namespace NewsGet_Android.Helpers
{
	public static class MarketHelper
	{
        public static string MarketName = "cafebazaar";
        //public static string MarketName = "myket";

        public static Intent GetOpenMarketIntent ()
		{
			try
			{
				NewsGetApplication app = (NewsGetApplication)NewsGetApplication.context.ApplicationContext;
				Intent intent = null;
				if(MarketHelper.MarketName == "cafebazaar")
				{
					intent = new Intent(Intent.ActionView, Uri.Parse("bazaar://details?id=" + app.PackageName));
				}
				else if(MarketHelper.MarketName == "myket")
				{
					intent = new Intent(Intent.ActionView, Uri.Parse("myket://application/#Intent;scheme=application;package=" + app.PackageName + ";end"));
				}

				return intent;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static Intent GetRateIntent ()
		{
			try
			{
				NewsGetApplication app = (NewsGetApplication)NewsGetApplication.context.ApplicationContext;
				Intent intent = null;
				if(MarketHelper.MarketName == "cafebazaar")
				{
					intent = new Intent(Intent.ActionEdit, Uri.Parse("bazaar://details?id=" + app.PackageName));
				}
				else if(MarketHelper.MarketName == "myket")
				{
					intent = new Intent(Intent.ActionView, Uri.Parse ("myket://application/#Intent;scheme=application;package=" + app.PackageName + ";end"));
				}

				return intent;
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}

