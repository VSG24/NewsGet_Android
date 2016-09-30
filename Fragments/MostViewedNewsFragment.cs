using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Support.Design.Widget;
using NewsGet_Android.Activities;
using NewsGet_Android.Models;
using NewsGet_Android.Helpers;
using Square.Picasso;

namespace NewsGet_Android.Fragments
{
	public class MostViewedNewsFragment : Android.Support.V4.App.Fragment
	{
		private Context globalContext = null;
		RecyclerView recyclerview;
		MostViewedNewsAdapter adapter;
		private SwipeRefreshLayout swiperefresh;
		private RestAccess rest;
		private List<Article> listofarticles;
		private LinearLayoutManager layoutmanager;
		private View view;
		public static bool imagesState;
//		private static int page = 1;

		// Wait for some time before user can call the function again
//		private static readonly TimeSpan MinInterval = TimeSpan.FromSeconds(5);
//		private readonly Stopwatch stopwatch = new Stopwatch(); // Stopped initially
//		private bool ShoulLoad = true;

		public MostViewedNewsFragment()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			view = inflater.Inflate(Resource.Layout.fragment_latestnews, null);

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

			// Check whether device is connected to any types of network
//			if(!NetworkHelper.IsOnline (globalContext)) { return; }

			// Show an error layout when retrieving article fails

			var retry = view.FindViewById<Button> (Resource.Id.alert_retry);
			retry.Click += delegate {
				FragmentTransaction tr = FragmentManager.BeginTransaction ();
				tr.Detach (this).Attach (this).Commit ();
			};

			NewsGetApplication app = (NewsGetApplication) Activity.Application;
			imagesState = app.GetImagesState ();

			//

			swiperefresh.SetColorSchemeResources (Resource.Color.accent, Resource.Color.primary_dark);

			// Create a new rest object
			rest = new RestAccess(app.GetServer ());

			var prog = new ProgressB(globalContext, view.FindViewById<ProgressBar> (Resource.Id.progressBar));
			prog.Toggle ();

			// Download the data from GetData method
			listofarticles = await GetData ();

			prog.Toggle();

			//............................................................
			// Adapter Setup:

			// Create an adapter for the RecyclerView, and pass it the
			// data set to manage:
			adapter = new MostViewedNewsAdapter (listofarticles, globalContext);

			// Register the item click handler (below) with the adapter:
			adapter.ItemClick += OnItemClick;

			// Plug the adapter into the RecyclerView:
			recyclerview.SetAdapter (adapter);

			swiperefresh.Refresh += async delegate {
				imagesState = app.GetImagesState ();
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

			try
			{
				result = await rest.ListMostViewedArticlesAsync ();
			}
			catch(Exception)
			{
				stillflag = true;
			}

			if (stillflag)
			{
				var stilltrying = view.FindViewById<TextView> (Resource.Id.stilltrying);
				try
				{
					stilltrying.Visibility = ViewStates.Visible;
					result = await rest.ListMostViewedArticlesAsync ();
				}
				catch
				{
					var alert = view.FindViewById<RelativeLayout> (Resource.Id.alert);
					alert.Visibility = ViewStates.Visible;
				}
				stilltrying.Visibility = ViewStates.Gone;
			}

			return result;
		}

		// Handler for the item click event:
		void OnItemClick (object sender, int position)
		{
			// use intent to pass the single articles data to the MainActivity
			Article art = listofarticles[position];

			var intent = new Intent(Activity, typeof(SwitcherActivity));
			intent.PutExtra("Source", art.Source);
			intent.PutExtra("Url", art.Url);
			StartActivity(intent);
		}
	}

	// ViewHolder
	public class MostViewedNewsViewHolder : RecyclerView.ViewHolder
	{
		public ImageView Thumbnail { get; private set; }
		public TextView Title { get; private set; }

		// Get references to the views defined in the CardView layout.
		public MostViewedNewsViewHolder (View itemView, Action<int> listener) : base (itemView)
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
	public class MostViewedNewsAdapter : RecyclerView.Adapter
	{
		// Event handler for item clicks:
		public event EventHandler<int> ItemClick;
		private readonly Context _globalContext = null;

		// Underlying data set:
		public List<Article> _listofarticles;

		// Load the adapter with the data set (articles) at construction time:
		public MostViewedNewsAdapter (List<Article> listofarticle, Context context)
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

			// We use Picasso to load images efficiently

			if(MostViewedNewsFragment.imagesState)
			{
				if (!string.IsNullOrEmpty(_listofarticles[position].Thumbnail))
				{
                    Picasso.With(_globalContext).Load(_listofarticles[position].Thumbnail)
                        .NetworkPolicy(NetworkPolicy.Offline)
                        .Into(vh.Thumbnail, () =>
                        {
                            // on success
                            Console.WriteLine("Picture is cached");
                        }, () => {
                            // on error
                            Picasso.With(_globalContext).Load(_listofarticles[position].Thumbnail)
                            .Into(vh.Thumbnail);
                            Console.WriteLine("Picture is not cached. Will be loaded.");
                        });
                }
			}

			vh.Title.Text = _listofarticles[position].Title;
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
//	public class  RecyclerViewOnScrollListener : RecyclerView.OnScrollListener
//	{
//		public delegate void LoadMoreEventHandler(object sender, EventArgs e);
//		public event LoadMoreEventHandler LoadMoreEvent;
//
//		private LinearLayoutManager LayoutManager;
//
//		public RecyclerViewOnScrollListener (LinearLayoutManager layoutManager)
//		{
//			LayoutManager = layoutManager;
//		}
//
//		public override void OnScrolled (RecyclerView recyclerView, int dx, int dy)
//		{
//			base.OnScrolled (recyclerView, dx, dy);
//
//			var visibleItemCount = recyclerView.ChildCount;
//			var totalItemCount = recyclerView.GetAdapter().ItemCount;
//			var pastVisiblesItems = LayoutManager.FindFirstVisibleItemPosition();
//
//			if ((visibleItemCount + pastVisiblesItems) >= totalItemCount) {
//				LoadMoreEvent (this, null);
//			}
//		}
//	}
}

