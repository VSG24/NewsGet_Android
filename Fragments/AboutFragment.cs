using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Views;
using Android.Text;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Support.Design.Widget;
using NewsGet_Android.Models;
using NewsGet_Android.Activities;
using NewsGet_Android.Helpers;
using Square.Picasso;

using Uri = Android.Net.Uri;
using Android.App;

namespace NewsGet_Android.Fragments
{
	public class AboutFragment : Android.Support.V4.App.Fragment
	{
		private View view;

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			view = inflater.Inflate(Resource.Layout.fragment_about, null);

			return view;
		}

		public override void OnActivityCreated (Bundle savedInstanceState)
		{
			base.OnActivityCreated (savedInstanceState);

			var designed_developed = view.FindViewById<TextView> (Resource.Id.designed_developed);
			var disclaimer = view.FindViewById<TextView> (Resource.Id.disclaimer);
			var email = view.FindViewById<TextView> (Resource.Id.email);
			var app_website = view.FindViewById<TextView> (Resource.Id.app_website);
            var dev_website = view.FindViewById<TextView>(Resource.Id.dev_website);
			var app_ver = view.FindViewById<TextView> (Resource.Id.app_ver);
			var app_market = view.FindViewById<TextView> (Resource.Id.app_market);

			designed_developed.TextFormatted = Html.FromHtml (GetString (Resource.String.designed_developed) + "<br><strong><font color='"  + HomeActivity.color_primary + "'>" + GetString (Resource.String.app_author) + "</font></strong>");
			disclaimer.Text = GetString (Resource.String.disclaimer);
			email.Text = GetString (Resource.String.email) + " " + GetString (Resource.String.author_email);
			app_website.Text = GetString (Resource.String.app_website) + " " + GetString (Resource.String.newsget_home);
            dev_website.Text = GetString(Resource.String.dev_website) + " " + GetString(Resource.String.dev_home);
            app_ver.Text = GetString (Resource.String.version) + " " + HomeActivity.app_ver;
			app_market.TextFormatted = Html.FromHtml (GetString (Resource.String.market) + " <strong><font color='" + HomeActivity.color_primary + "'>" + MarketHelper.MarketName + "</font></strong>");

            var bsod_counter = 0;
            designed_developed.Click += (object sender, EventArgs e) =>
            {
                if(++bsod_counter == 3)
                {
                    bsod_counter = 0;
                    StartActivity(new Intent(Application.Context, typeof(BsodActivity)));
                }
            };

			app_market.Click += (object sender, EventArgs e) => {
				Intent newsget_on_market = null;
                // Get the corresponding intent
                newsget_on_market = MarketHelper.GetOpenMarketIntent();

				try
				{
					StartActivity (newsget_on_market);
				}
				catch(Exception)
				{
					Android.Support.V7.App.AlertDialog errorAlertDialog = new Android.Support.V7.App.AlertDialog.Builder(Activity).Create();
					errorAlertDialog.SetTitle (Resource.String.error);
					errorAlertDialog.SetMessage (GetString (Resource.String.problem_starting_market));
					errorAlertDialog.SetButton ((int) Android.Content.DialogButtonType.Neutral, GetString (Resource.String.close), delegate {
						errorAlertDialog.Dismiss ();
					});
					errorAlertDialog.Show ();
				}
			};
		}
	}
}

