using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

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

namespace NewsGet_Android.Fragments
{
	public class LatestNewsFragment : Android.Support.V4.App.Fragment
	{
		private Context globalContext = null;
		private RecyclerView recyclerview;
		private NewsAdapter adapter;
		private SwipeRefreshLayout swiperefresh;
		private RestAccess rest;
		private List<Article> listofarticles;
		private LinearLayoutManager layoutmanager;
		private string provider;
		public string displayname;
		public string provider_website;
		private View view;
		NewsGetApplication app;
		private TextView retry = null;
		private static int failcount = 0;
		//private static int page = 1;

		// Wait for some time before user can call the function again
		private static readonly TimeSpan MinInterval = TimeSpan.FromSeconds(2);
		private readonly Stopwatch stopwatch = new Stopwatch(); // Stopped initially
		private bool ShoulLoad = true;
		private static bool ScrollLoad = false;

		public LatestNewsFragment()
		{
			this.RetainInstance = true;
			if(SwitcherActivity.isLatestProvider)
			{
				HasOptionsMenu = true;
				SwitcherActivity.isLatestProvider = false;
			}
			else
			{
				HasOptionsMenu = false;
			}
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			try
			{
				this.provider = this.Arguments.GetString ("Provider");
				this.displayname = this.Arguments.GetString ("DisplayName");
				this.provider_website = this.Arguments.GetString ("SourceWebsite");
				view = inflater.Inflate(Resource.Layout.fragment_latestnews_provider, null);

				var title = view.FindViewById<TextView> (Resource.Id.provider_info_title);
				title.TextFormatted = Html.FromHtml ("<strong><font color='" + HomeActivity.color_black + "'>" + GetString (Resource.String.latest_articles_from) + "</font>   " + "<font color='" + HomeActivity.color_primary + "'>" + this.displayname + "</font></strong>");
			}
			catch (NullReferenceException)
			{
				this.provider = "getlatestcombine";
				this.displayname = "Persian";
				view = inflater.Inflate(Resource.Layout.fragment_latestnews, null);
			}

			// Get our RecyclerView layout:
			recyclerview = view.FindViewById<RecyclerView> (Resource.Id.recyclerView_latestnews);

			recyclerview.HasFixedSize = true;

			//............................................................
			// Layout Manager Setup:

			// Use the built-in linear layout manager:
			layoutmanager = new LinearLayoutManager (Activity);

			// Or use the built-in grid layout manager (two horizontal rows):
			// layoutmanager = new GridLayoutManager
			//        (this, 2, GridLayoutManager.Horizontal, false);

			// Plug the layout manager into the RecyclerView:
			recyclerview.SetLayoutManager (layoutmanager);

			swiperefresh = view.FindViewById<SwipeRefreshLayout> (Resource.Id.latestnews_swipe_refresh_layout);

			return view;
		}

		public override async void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);
			globalContext = Activity;

			// Show an error layout when retrieving article fails

			retry = view.FindViewById<Button> (Resource.Id.alert_retry);
			retry.Click += delegate {
				try
				{
					FragmentTransaction tr = FragmentManager.BeginTransaction ();
					tr.Detach (this).Attach (this).Commit ();
				}
				catch (Exception e)
				{
					Console.WriteLine (e.Message);
				}
			};

			var page = 1;

			//
			
			swiperefresh.SetColorSchemeResources (Resource.Color.accent, Resource.Color.primary_dark);

			app = (NewsGetApplication) Activity.Application;

			// Create a new rest object
			rest = new RestAccess(app.GetServer ());

			var prog = new ProgressB(globalContext, view.FindViewById<ProgressBar> (Resource.Id.progressBar));
			prog.Toggle ();

			// Download the data from GetData method
			listofarticles = await GetData ();
			if(ServerChecker.Checked)
			{
				ServerChecker.Checked = false;
				retry.PerformClick ();
			}

			prog.Toggle();

			var onScrollListener = new RecyclerViewOnScrollListener (layoutmanager);
			onScrollListener.LoadMoreEvent += async (object sender, EventArgs e) => {
				// Load more stuff here
				if (stopwatch.IsRunning && stopwatch.Elapsed < MinInterval)
				{
					return;
				}
				try
				{
					// Don't load several times for one scroll
					if(ShoulLoad)
					{
						Snackbar snack = Snackbar.Make(view, Resource.String.loading, Snackbar.LengthLong);
						snack.Show();

						ShoulLoad = false;
						ScrollLoad = true;
						var newarticles = await GetData ((++page).ToString ());
						var pageafter = page;
						listofarticles = listofarticles.Concat (newarticles).ToList ();
						ScrollLoad = false;
						adapter.AddToList (newarticles);

						// Hide the snackbar
						snack.Dismiss ();

						ShoulLoad = true;
					}
				}
				catch (Exception)
				{
					Snackbar
						.Make (view, Resource.String.loading_failed, Snackbar.LengthShort)
						.Show (); // Don’t forget to show!
					page--;
					ShoulLoad = true;
					return;
				}
				finally
				{
					stopwatch.Restart();
				}
			};

			recyclerview.AddOnScrollListener (onScrollListener);

			//............................................................
			// Adapter Setup:

			// Create an adapter for the RecyclerView, and pass it the
			// data set to manage:
			adapter = new NewsAdapter (listofarticles, globalContext);

			// Register the item click handler (below) with the adapter:
			adapter.ItemClick += OnItemClick;

			// Plug the adapter into the RecyclerView:
			recyclerview.SetAdapter (adapter);

			swiperefresh.Refresh += async delegate {
				// Check whether device is connected to any types of network
				if(!NetworkHelper.IsOnline (globalContext))
				{
					Snackbar
						.Make (view, Resource.String.no_connection, Snackbar.LengthLong)
						.Show (); // Don’t forget to show!
					swiperefresh.Refreshing = false;
					return;
				}
				try
				{
					adapter._listofarticles = await GetData ();
					adapter.NotifyDataSetChanged ();
					swiperefresh.Refreshing = false;
				}
				catch(Exception)
				{
					Snackbar
						.Make (view, Resource.String.no_connection, Snackbar.LengthLong)
						.Show (); // Don’t forget to show!
					swiperefresh.Refreshing = false;
					return;
				}
			};
		}

		async Task<List<Article>> GetData(string page = null)
		{
			List<Article> result = null;
			var stillflag = false;
			string images = null;
			RelativeLayout alert = null;

			var servercatchran = 0;

			try
			{
				var state = app.GetImagesState ();
				if(state)
					images = "1";
				else
					images = "0";

				result = await rest.ListArticlesAsync(this.provider, images, page);
			}
			catch (Exception)
			{
				failcount++;
				if(!ScrollLoad)
				{
					stillflag = true;
					if(!ServerChecker.Checked && NetworkHelper.IsOnline (globalContext) && failcount >= 2)
					{
						failcount = 0;
						servercatchran = 1;
					}
				}
			}

			if(servercatchran == 1)
			{
                Snackbar.Make(view, Resource.String.investigating_the_issue, Snackbar.LengthLong).Show();
                int res = await ServerChecker.CheckAndAction (globalContext, app);
				if(res != 0)
				{
					stillflag = false;
					alert = view.FindViewById<RelativeLayout> (Resource.Id.alert);
					alert.Visibility = ViewStates.Visible;
					servercatchran = 0;
					ServerChecker.Checked = false;
				}
				else
				{
					servercatchran = 0;
				}
			}

			if (stillflag)
			{
				var stilltrying = view.FindViewById<TextView> (Resource.Id.stilltrying);
				try
				{
					stilltrying.Visibility = ViewStates.Visible;
					result = await rest.ListArticlesAsync(this.provider, images, page);
				}
				catch
				{
					if(servercatchran == 0)
					{
						alert = view.FindViewById<RelativeLayout> (Resource.Id.alert);
						alert.Visibility = ViewStates.Visible;
					}
				}

				stilltrying.Visibility = ViewStates.Gone;
			}

			return result;
		}

		// Handler for the item click event:
		void OnItemClick (object sender, int position)
		{
			// Use intent to pass the single articles data to the MainActivity
			// Note that you MUST use the list of the adapter not the original one in the activity!
			// Because it's the adapter one that gets updated
			try
			{
				Article art = adapter._listofarticles[position];

				var intent = new Intent(Activity, typeof(SwitcherActivity));
				intent.PutExtra("Source", art.Source);
				intent.PutExtra("Url", art.Url);
                intent.PutExtra("OriginalUrl", art.Content);
				StartActivity(intent);
			}
			catch (Exception)
			{
				//
			}
		}

		public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
		{
			inflater.Inflate (Resource.Menu.latest_provider, menu);

			base.OnPrepareOptionsMenu (menu);
		}

		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId)
			{
			    case Resource.Id.action_provider_website:
				    Intent browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(this.provider_website));
				    StartActivity(browserIntent);
				    return true;
			}
			return base.OnOptionsItemSelected (item);
		}
	}

	// ViewHolder
	public class NewsViewHolder : RecyclerView.ViewHolder
	{
		public ImageView Thumbnail { get; private set; }
		public TextView Title { get; private set; }

		// Get references to the views defined in the CardView layout.
		public NewsViewHolder (View itemView, Action<int> listener) : base (itemView)
		{
			// Locate and cache view references:
			Thumbnail = itemView.FindViewById<ImageView> (Resource.Id.imageView_latestnews);
			Title = itemView.FindViewById<TextView> (Resource.Id.textView_latestnews);

			// Detect user clicks on the item view and report which item
			// was clicked (by position) to the listener:
			itemView.Click += (sender, e) => listener (base.LayoutPosition);
		}
	}

	// Adapter
	public class NewsAdapter : RecyclerView.Adapter
	{
		// Event handler for item clicks:
		public event EventHandler<int> ItemClick;
		private readonly Context _globalContext = null;

		// Underlying data set:
		public List<Article> _listofarticles;

		// Load the adapter with the data set (articles) at construction time:
		public NewsAdapter (List<Article> listofarticle, Context context)
		{
			this._listofarticles = listofarticle;
			_globalContext = context;
		}

		// Create a new article CardView (invoked by the layout manager): 
		public override RecyclerView.ViewHolder OnCreateViewHolder (ViewGroup parent, int viewType)
		{
			// Inflate the CardView for the photo:
			View itemView = LayoutInflater.From (parent.Context).Inflate (Resource.Layout.latestnews_recycler_article, parent, false);

			// Create a ViewHolder to find and hold these view references, and 
			// register OnClick with the view holder:
			NewsViewHolder vh = new NewsViewHolder (itemView, OnClick); 
			return vh;
		}

		// Fill in the contents of the article card (invoked by the layout manager):
		public override void OnBindViewHolder (RecyclerView.ViewHolder holder, int position)
		{
			NewsViewHolder vh = holder as NewsViewHolder;

			// Set the ImageView and TextView in this ViewHolder's CardView 
			// from this position in the photo album:

			//vh.Thumbnail.SetImageResource (_listofarticles[position].Thumbnail);

			var article = _listofarticles [position];
			if (!string.IsNullOrEmpty(article.Thumbnail))
			{
                //Picasso.With(_globalContext).Load(article.Thumbnail)
                //.Placeholder(R.drawable.ic_placeholder) // optional
                //.Error(R.drawable.ic_error_fallback) // optional
                //.Resize(250, 200)                        // optional
                //.Rotate(90)    // optional
                //.Into(vh.Thumbnail);
                Picasso.With(_globalContext).Load(article.Thumbnail)
                    .NetworkPolicy(NetworkPolicy.Offline)
                    .Into(vh.Thumbnail, () =>
                    {
                        // on success
                        Console.WriteLine("Picture is cached");
                    }, () => {
                        // on error
                        Picasso.With(_globalContext).Load(article.Thumbnail)
                        .Into(vh.Thumbnail);
                        Console.WriteLine("Picture is not cached. Will be loaded.");
                    });
			}
			vh.Title.Text = article.Title;
		}

		public void AddToList(List<Article> newitemslist)
		{
			_listofarticles = _listofarticles.Concat (newitemslist).ToList ();
			this.NotifyDataSetChanged ();
		}

		// Return the number of articles available in the list:
		public override int ItemCount
		{
			get
			{
				try
				{
					return this._listofarticles.Count;
				}
				catch(Exception)
				{
					return 0;
				}
			}
		}

		// Raise an event when the item-click takes place:
		void OnClick (int position)
		{
			if (ItemClick != null)
				ItemClick (this, position);
		}
	}
		
	// Detect scrolling to the bottom so that we can use this to load more articles in the list
	public class  RecyclerViewOnScrollListener : RecyclerView.OnScrollListener
	{
		public delegate void LoadMoreEventHandler(object sender, EventArgs e);
		public event LoadMoreEventHandler LoadMoreEvent;

		private LinearLayoutManager LayoutManager;

		public RecyclerViewOnScrollListener (LinearLayoutManager layoutManager)
		{
			LayoutManager = layoutManager;
		}

		public override void OnScrolled (RecyclerView recyclerView, int dx, int dy)
		{
			base.OnScrolled (recyclerView, dx, dy);

			var visibleItemCount = recyclerView.ChildCount;
			var totalItemCount = recyclerView.GetAdapter().ItemCount;
			var pastVisiblesItems = LayoutManager.FindFirstVisibleItemPosition();

			if ((visibleItemCount + pastVisiblesItems) >= totalItemCount) {
				LoadMoreEvent (this, null);
			}
		}
	}
}

