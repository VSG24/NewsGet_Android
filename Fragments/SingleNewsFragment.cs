using System;
using System.Collections.Generic;

using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.Design.Widget;
using Android.Webkit;
using NewsGet_Android.Models;
using NewsGet_Android.Helpers;
using Android.Runtime;

namespace NewsGet_Android.Fragments
{
	public class SingleNewsFragment : Android.Support.V4.App.Fragment
	{
		private Context globalContext = null;
		private View view;
		Article article = null;
		private RestAccess rest;
		private DatabaseAccess db = new DatabaseAccess ();

		private string source;
		private string url;
        private string originalUrl;
        WebView webview;
        public static ProgressBar WebviewProgressBar = null;
        //public static int Progress;
		private NewsGetApplication app;
        private IMenuItem share;
        private IMenuItem faved;
		private IMenuItem fav;
        private IMenuItem browser;
        private List<Article> allfavedarticles = null;
		private bool offline_file_exists = false;
		private bool dummy = false;
//		private SingleArticleShareActionProvider share_provider;
		private string offlinearticle;
        private bool shouldLoadOriginalUrl = false;

        private bool shareReady = false;
		public static int TextSize = 100;
		public static string images;

		public SingleNewsFragment()
		{
			this.RetainInstance = true;
			HasOptionsMenu = true;
        }

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			view = inflater.Inflate(Resource.Layout.fragment_singlenews, null);

            WebviewProgressBar = view.FindViewById<ProgressBar>(Resource.Id.progressbar_webview);
            WebviewProgressBar.LayoutDirection = LayoutDirection.Ltr;

            return view;
		}

		public override async void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);
			globalContext = this.Context;

			app = (NewsGetApplication) Activity.Application;
			SingleNewsFragment.TextSize = app.GetTextSize ();

			source = Arguments.GetString ("Source");
			url = Arguments.GetString ("Url");
            originalUrl = Arguments.GetString("OriginalUrl");

            try
            {
                this.offlinearticle = OfflineHelper.GenerateOfflineFileName(this.url, OfflineHelper.OfflineArticlesPrefix);

                // Check whether device is connected to any types of network
                if (!NetworkHelper.IsOnline(globalContext))
                {
                    if (app.GetSaveArticlesOfflineState() && OfflineHelper.OfflineArticleExist(offlinearticle))
                    {
                        this.offline_file_exists = true;
                        goto AFTER_NETWORK_CHECK;
                    }
                    Snackbar
                        .Make(view, Resource.String.no_connection, Snackbar.LengthIndefinite)
                        .SetAction(Resource.String.retryU, delegate { this.OnActivityCreated(savedInstanceState); })
                        .Show(); // Don’t forget to show!
                    return;
                }
                this.dummy = true;
            }
            catch (Exception)
            {
                this.offline_file_exists = false;
            }

            AFTER_NETWORK_CHECK:

            // Show an error layout when retrieving article fails
            var retry = view.FindViewById<Button> (Resource.Id.alert_retry);
			retry.Click += delegate {
				FragmentTransaction tr = FragmentManager.BeginTransaction ();
				tr.Detach (this).Attach (this).Commit ();
			};

			//

			var prog = new ProgressB (globalContext, view.FindViewById<ProgressBar> (Resource.Id.progressBar));
			prog.Toggle ();

			rest = new RestAccess (app.GetServer ());
			article = null;

			var stillflag = false;
			var servercatchran = 0;
			SingleNewsFragment.images = (app.GetImagesState ()) ? "1" : "0";
			try
			{
				if(app.GetSaveArticlesOfflineState ())
				{
					// Not connected to any network
					if(this.offline_file_exists)
					{
						// An saved version exists on the disk
						try
						{
							article = OfflineHelper.ReadOfflineArticle (offlinearticle);
						}
						catch (Exception)
						{
							Snackbar
								.Make (view, Resource.String.something_wrong_reading_article, Snackbar.LengthLong)
								.Show (); // Don’t forget to show!
						}
					}
					else if (this.dummy) // Connected
					{
						if (OfflineHelper.OfflineArticleExist (offlinearticle))
						{
							article = OfflineHelper.ReadOfflineArticle (offlinearticle);
						}
						else
						{
							article = await rest.GetArticleAsync (this.source, this.url, images + "h");
							OfflineHelper.SaveOfflineArticle (article);
						}
					}
				}
				else
				{
					article = await rest.GetArticleAsync (this.source, this.url, images + "h");
				}
			}
			catch (Exception)
			{
				stillflag = true;
				if(!ServerChecker.Checked)
				{
					servercatchran = 1;
				}
			}

			if(servercatchran == 1)
			{
                //var pr = new ProgressD (globalContext, GetString (Resource.String.pls_wait));
                Snackbar.Make(view, Resource.String.investigating_the_issue, Snackbar.LengthLong).Show();
                int res = await ServerChecker.CheckAndAction (globalContext, app);
				servercatchran = 2;
			}


			if (stillflag)
			{
				var stilltrying = view.FindViewById<TextView> (Resource.Id.stilltrying);
				try
				{
					stilltrying.Visibility = ViewStates.Visible;
					article = await rest.GetArticleAsync (this.source, this.url, images + "h");
				}
				catch
				{
                    if(!app.GetLoadOriginalUrlWebView())
                    {
                        var alert = view.FindViewById<RelativeLayout>(Resource.Id.alert);
                        alert.Visibility = ViewStates.Visible;
                        return;
                    }
                    else
                    {
                        shouldLoadOriginalUrl = true;
                    }
				}
				stilltrying.Visibility = ViewStates.Gone;
			}

			prog.Toggle (); // Hide progress dialog

			allfavedarticles = db.GetAllGeneric<Article> (db.FavoritesDb);

            TextView textViewTitle = view.FindViewById<TextView>(Resource.Id.title_singlenews);
            var cardview = view.FindViewById<Android.Support.V7.Widget.CardView>(Resource.Id.cardView);
            var provider_name = view.FindViewById<TextView>(Resource.Id.provider_name);


            if (!shouldLoadOriginalUrl)
            {
                textViewTitle.Text = article.Title;
                cardview.Visibility = ViewStates.Visible;
                provider_name.Text = article.Source_DisplayName;
                provider_name.Visibility = ViewStates.Visible;
            }

			webview = view.FindViewById<WebView>(Resource.Id.webview_singlenews);
			webview.SetBackgroundColor (Android.Graphics.Color.Transparent);
			webview.Settings.JavaScriptEnabled = true;
			webview.Settings.DomStorageEnabled = true;
			webview.Settings.AllowContentAccess = true;
			webview.Settings.AllowFileAccess = true;
			webview.Settings.AllowFileAccessFromFileURLs = true;
			webview.Settings.AllowUniversalAccessFromFileURLs = true;
			webview.Settings.SetSupportZoom(true);
			webview.Settings.BuiltInZoomControls = true;
			webview.Settings.DisplayZoomControls = false;

			LinearLayout.LayoutParams webview_params = new LinearLayout.LayoutParams (ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent, 1.0f);
			webview.LayoutParameters = webview_params;

			webview.SetWebChromeClient (new SingleArticleWebChromeClient());
			webview.SetWebViewClient (new SingleArticleWebViewClinet());

            if(!shouldLoadOriginalUrl)
            {
                browser.SetShowAsAction(ShowAsAction.Never);
                webview.LoadData(article.Content, "text/html; charset=UTF-8", null);
            }
            else
            {
                fav.SetVisible(false);
                faved.SetVisible(false);
                share.SetVisible(false);
                browser.SetShowAsAction(ShowAsAction.Always);

                WebviewProgressBar.Visibility = ViewStates.Visible;
                webview.LoadUrl(this.originalUrl);
                WebviewProgressBar.Progress = 0;
            }
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate(Resource.Menu.single, menu);
            share = menu.FindItem(Resource.Id.action_share);
			faved = menu.FindItem(Resource.Id.action_faved);
			fav = menu.FindItem(Resource.Id.action_fav);
            browser = menu.FindItem(Resource.Id.action_open_in_browser);

			faved.SetVisible (false);

//			var share_article = menu.FindItem (Resource.Id.action_share);
//			share_provider = new SingleArticleShareActionProvider (globalContext);
//			Android.Support.V4.View.MenuItemCompat.SetActionProvider (share_article, share_provider);
			shareReady = true;
//			share_article.SetIcon (Resource.Drawable.ic_share_white_24dp);

			if(allfavedarticles != null)
			{
				var isfaved = allfavedarticles.FindIndex (a => a.Url == this.article.Url);
				if(isfaved != -1)
				{
					fav.SetVisible(false);
					faved.SetVisible(true);
				}
				else
				{
					faved.SetVisible(false);
					fav.SetVisible(true);
				}
			}

			base.OnPrepareOptionsMenu (menu);
		}

		private Intent CreateShareIntent ()
		{   
			try
			{
				var sendArticleIntent = new Intent (Intent.ActionSend);
				sendArticleIntent.SetType ("text/plain");
				// Article's excerpt is actually just the original articles url
				sendArticleIntent.PutExtra (Intent.ExtraText, this.article.Title + "\n\n" + this.article.Excerpt + "\n --- \n" + globalContext.GetString (Resource.String.sharedby) + "\n" + globalContext.GetString (Resource.String.newsget_home));

				return sendArticleIntent;
			}
			catch (Exception)
			{}
			return null;
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId)
			{
			    case Resource.Id.action_refresh:
				    try
				    {
					    RefreshWebView ();
				    }
				    catch (Exception)
				    {
					    Snackbar
						    .Make (view, Resource.String.loading_failed, Snackbar.LengthLong)
						    .Show (); // Don’t forget to show!
				    }
				    return true;
			    case Resource.Id.action_copyurl:
				    CopyUrl ();
				    return true;
			    case Resource.Id.action_text_size:
				    TextSizePopUpMenu (item);
				    return true;
			    case Resource.Id.action_share:
				    try
				    {
					    if (this.article != null && shareReady)
					    {
						    StartActivity (CreateShareIntent ());
					    }
				    }
				    catch(Exception)
				    {
					    //
				    }
				    return true;
			    case Resource.Id.action_fav:
				    var result = FavArticle ();
				    if(result)
				    {
					    item.SetVisible (false);
					    faved.SetVisible (true);
				    }
				    return true;
			    case Resource.Id.action_faved:
				    var result2 = UnFavArticle ();
				    if (result2)
				    {
					    item.SetVisible (false);
					    fav.SetVisible (true);
				    }
				    return true;
                case Resource.Id.action_open_in_browser:
                    if (originalUrl == null)
                        return true;
                    Intent browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(originalUrl));
                    StartActivity(browserIntent);
                    return true;
            }

			return base.OnOptionsItemSelected (item);
		}

		private void CopyUrl ()
		{
			if (this.article != null)
			{
				try
				{
					ClipboardManager clipboard = (ClipboardManager)Activity.GetSystemService (Context.ClipboardService); 
					ClipData clip = ClipData.NewPlainText ("URL", this.article.Excerpt);
					clipboard.PrimaryClip = clip;

					Toast.MakeText (Activity, Resource.String.copyurl_toast, ToastLength.Short).Show ();
				}
				catch (Exception)
				{
					//
				}
			}
            if (shouldLoadOriginalUrl && originalUrl != null)
            {
                try
                {
                    ClipboardManager clipboard = (ClipboardManager)Activity.GetSystemService(Context.ClipboardService);
                    ClipData clip = ClipData.NewPlainText("URL", this.originalUrl);
                    clipboard.PrimaryClip = clip;

                    Toast.MakeText(Activity, Resource.String.copyurl_toast, ToastLength.Short).Show();
                }
                catch (Exception)
                {
                    //
                }
            }
        }

		private async void RefreshWebView ()
		{
			if(!NetworkHelper.IsOnline (globalContext))
			{
				Snackbar
					.Make (view, Resource.String.no_connection, Snackbar.LengthIndefinite)
					.SetAction (Resource.String.retryU, delegate { this.RefreshWebView (); })
					.Show (); // Don’t forget to show!
				return;
			}
			else
			{
				var shouldsave = false;
				if (app.GetSaveArticlesOfflineState () && OfflineHelper.OfflineArticleExist (this.offlinearticle))
				{
					OfflineHelper.DeleteSavedArticleNoCheck (this.offlinearticle);
					shouldsave = true;
				}
				Snackbar snack = Snackbar.Make(view, Resource.String.loading, Snackbar.LengthLong);
				snack.Show();
				try
				{
                    if(shouldLoadOriginalUrl && originalUrl != null)
                    {
                        WebviewProgressBar.Visibility = ViewStates.Visible;
                        webview.LoadUrl(this.originalUrl);
                        WebviewProgressBar.Progress = 0;
                        goto END;
                    }
					var new_article = await rest.GetArticleAsync (this.source, this.url, SingleNewsFragment.images + "h");
					webview.LoadData (new_article.Content, "text/html; charset=UTF-8", null);

					if(shouldsave || app.GetSaveArticlesOfflineState ())
					{
						OfflineHelper.SaveOfflineArticle (new_article);
					}
				}
				catch (Exception)
				{
					Snackbar
						.Make (view, Resource.String.loading_failed, Snackbar.LengthLong)
						.Show (); // Don’t forget to show!
				}
                END:
				snack.Dismiss ();
			}
		}

		private void TextSizePopUpMenu (IMenuItem item)
		{
			PopupMenu popup;

			View menuitemView = Activity.FindViewById (item.ItemId);

			popup = new PopupMenu (Activity, menuitemView);
			popup.Inflate (Resource.Menu.text_size);
			popup.Show ();

			popup.MenuItemClick += (s1, arg1) => {

				int size = 0;

                var currentTextSize = app.GetTextSize();
                if (arg1.Item.ItemId == Resource.Id.action_textsize_larger)
				{
					size = app.GetTextSize () + 15;
                }
				else if(arg1.Item.ItemId == Resource.Id.action_textsize_smaller)
				{
					size = app.GetTextSize () - 15;
                }
                app.SetOption("textsize_single", size);

                if ((int)Build.VERSION.SdkInt >= 19)
				{
					webview.EvaluateJavascript ("document.getElementsByTagName(\"body\")[0].style.fontSize = \"" + size + "%\";", null);
				}
				else
				{
					webview.LoadUrl ("javascript:" + "document.getElementsByTagName(\"body\")[0].style.fontSize = \"" + size + "%\";");
				}
			};
		}

		private bool FavArticle()
		{
			if(this.article != null)
			{
				var allrecarticles = db.GetAllGeneric<Article> (db.FavoritesDb);
				if (allrecarticles == null)
					allrecarticles = new List<Article> ();
				allrecarticles.Add (new Article () { Thumbnail = this.article.Thumbnail, Title = this.article.Title, Source = this.article.Source, Url = this.article.Url });

				db.InsertAllGeneric (allrecarticles, db.FavoritesDb);

				Snackbar
					.Make (view, Resource.String.added_to_favorites, Snackbar.LengthShort)
					.Show (); // Don’t forget to show!

				return true;
			}
			else
			{
				Snackbar
					.Make (view, Resource.String.action_failed, Snackbar.LengthShort)
					.Show (); // Don’t forget to show!
				return false;
			}
		}

		private bool UnFavArticle()
		{
			if(this.article != null)
			{
				var allrecarticles = db.GetAllGeneric<Article> (db.FavoritesDb);
				allrecarticles.RemoveAll (a => a.Url == this.article.Url);

				db.InsertAllGeneric (allrecarticles, db.FavoritesDb);
				return true;
			}
			else
			{
				Snackbar
					.Make (view, Resource.String.action_failed, Snackbar.LengthShort)
					.Show (); // Don’t forget to show!
				return false;
			}
		}
	}

	public class SingleArticleWebViewClinet : WebViewClient
	{
		public override void OnPageFinished (WebView view, string url)
		{
			base.OnPageFinished (view, url);
			if ((int)Build.VERSION.SdkInt >= 19)
			{
				view.EvaluateJavascript ("document.getElementsByTagName(\"body\")[0].style.fontSize = \"" + SingleNewsFragment.TextSize + "%\";", null);
			}
			else
			{
				view.LoadUrl ("javascript:" + "document.getElementsByTagName(\"body\")[0].style.fontSize = \"" + SingleNewsFragment.TextSize + "%\";");
			}

            SingleNewsFragment.WebviewProgressBar.Visibility = ViewStates.Gone;
		}
	}

    public class SingleArticleWebChromeClient : WebChromeClient
    {
        public override void OnProgressChanged(WebView view, int newProgress)
        {
            SingleNewsFragment.WebviewProgressBar.Progress = newProgress;
            base.OnProgressChanged(view, newProgress);
        }
    }

//	public class SingleArticleShareActionProvider : Android.Support.V7.Widget.ShareActionProvider
//	{
//		public SingleArticleShareActionProvider (Context context) : base (context)
//		{
//			
//		}
//
//		public override View OnCreateActionView ()
//		{
//			return null;
//		}
//	}
}

