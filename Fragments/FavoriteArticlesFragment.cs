using System;
using System.Collections.ObjectModel;
using System.Linq;

using Android.Content;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using Android.Support.Design.Widget;
using NewsGet_Android.Models;
using NewsGet_Android.Activities;
using NewsGet_Android.Helpers;
using Square.Picasso;
using XamarinItemTouchHelper;

namespace NewsGet_Android.Fragments
{
	public class FavoriteArticlesFragment : Android.Support.V4.App.Fragment
	{
		private Context globalContext = null;
		private RecyclerView recyclerview;
		private FavNewsAdapter adapter;
		private ObservableCollection<Article> listofarticles;
		private ItemTouchHelper mItemTouchHelper;
		private LinearLayoutManager layoutmanager;
		public static View view;

		public static DatabaseAccess db = new DatabaseAccess ();

		public static bool images = true;

		public FavoriteArticlesFragment()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			view = inflater.Inflate(Resource.Layout.fragment_favoritearticles, null);

			var title = view.FindViewById<TextView> (Resource.Id.page_title);
			title.TextFormatted = Html.FromHtml ("<strong><font color='" + HomeActivity.color_black + "'>" + GetString (Resource.String.all_favorite_articles) + "</font></strong>");

			// Get our RecyclerView layout:
			recyclerview = view.FindViewById<RecyclerView> (Resource.Id.recyclerView_latestnews);

			recyclerview.HasFixedSize = true;

			//............................................................
			// Layout Manager Setup:

			// Use the built-in linear layout manager:
			layoutmanager = new LinearLayoutManager (Activity);

			// Plug the layout manager into the RecyclerView:
			recyclerview.SetLayoutManager (layoutmanager);

			return view;
		}

		public override void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);
			globalContext = Activity;

			// Check whether device is connected to any types of network
			//if(!NetworkHelper.IsOnline (globalContext)) { return; }

			// Show an error layout when retrieving article fails

			var retry = view.FindViewById<TextView> (Resource.Id.alert_retry);
			retry.Click += delegate {
				FragmentTransaction tr = FragmentManager.BeginTransaction ();
				tr.Detach (this).Attach (this).Commit ();
			};

			//

			var prog = new ProgressB(globalContext, view.FindViewById<ProgressBar> (Resource.Id.progressBar));
			prog.Toggle ();

			// Download the data from GetData method
			listofarticles = GetData ();

			prog.Toggle();

			//............................................................
			// Adapter Setup:

			// Create an adapter for the RecyclerView, and pass it the
			// data set to manage:
			adapter = new FavNewsAdapter (listofarticles, globalContext);

			// Register the item click handler (below) with the adapter:
			adapter.ItemClick += OnItemClick;

			// Plug the adapter into the RecyclerView:
			recyclerview.SetAdapter (adapter);

			ItemTouchHelper.Callback callback = new SimpleItemTouchHelperCallback(adapter);
			mItemTouchHelper = new ItemTouchHelper(callback);
			mItemTouchHelper.AttachToRecyclerView(recyclerview);
		}

		public override void OnDestroy ()
		{
			if (FavNewsAdapter.shouldWrite)
			{
				// Write to disk
				db.InsertAllGeneric<Article> (adapter._listofarticles, db.FavoritesDb);
			}

			base.OnDestroy ();
		}

		ObservableCollection<Article> GetData ()
		{
			ObservableCollection<Article> result = null;

			try
			{
				NewsGetApplication app = (NewsGetApplication) Activity.Application;
				var state = app.GetImagesState ();
				if(state)
					images = true;
				else
					images = false;
				
				result = new ObservableCollection<Article> (db.GetAllGeneric<Article> (db.FavoritesDb));
			}
			catch(Exception e)
			{
				if (e is ArgumentNullException)
				{
					// nothing to show
				}
				else
				{
					var alert = view.FindViewById<RelativeLayout> (Resource.Id.alert);
					alert.Visibility = ViewStates.Visible;
				}
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

	// Adapter
	public class FavNewsAdapter : RecyclerView.Adapter, IItemTouchHelperAdapter
	{
		// Event handler for item clicks:
		public event EventHandler<int> ItemClick;
		private Context globalContext = null;

		private int lastItemPos = 0;
		public static bool shouldWrite = false;

		// Underlying data set:
		public ObservableCollection<Article> _listofarticles;

		// Load the adapter with the data set (articles) at construction time:
		public FavNewsAdapter (ObservableCollection<Article> listofarticle, Context context)
		{
			this._listofarticles = listofarticle;
			globalContext = context;
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
			// from this position in the collection

			if(FavoriteArticlesFragment.images)
			{
                if(!string.IsNullOrEmpty(_listofarticles[position].Thumbnail))
                {
                    Picasso.With(globalContext).Load(_listofarticles[position].Thumbnail)
                        .NetworkPolicy(NetworkPolicy.Offline)
                        .Into(vh.Thumbnail, () =>
                        {
                            // on success
                            Console.WriteLine("Picture is cached");
                        }, () => {
                            // on error
                            Picasso.With(globalContext).Load(_listofarticles[position].Thumbnail)
                            .Into(vh.Thumbnail);
                            Console.WriteLine("Picture is not cached. Will be loaded.");
                        });
                }
			}

			vh.Title.Text = _listofarticles[position].Title;
		}

		public void OnItemDismiss (int position)
		{
			if (!shouldWrite)
				shouldWrite = true;

			UpdateLastItemPos ();

			var article = _listofarticles.ElementAt (position);
			_listofarticles.Remove(article);
			NotifyItemRemoved(position);

			Snackbar
				.Make (FavoriteArticlesFragment.view, Resource.String.removed_from_favorites, Snackbar.LengthLong)
				.SetAction (Resource.String.undo, delegate { this.AddToList (article, position); })
				.Show (); // Don’t forget to show!
		}

		public void UpdateLastItemPos()
		{
			lastItemPos = ItemCount - 1;
		}

		public bool OnItemMove (int fromPosition, int toPosition)
		{
			_listofarticles.Move(fromPosition, toPosition);
			NotifyItemMoved(fromPosition, toPosition);
			return true;
		}

		// This should be called when user hits undo on the snackbar
		public void AddToList(Article newitem, int position)
		{
			_listofarticles.Add (newitem);
			NotifyDataSetChanged ();
			OnItemMove (lastItemPos, position);
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
}

