using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
using NewsGet_Android.Models;
using NewsGet_Android.Activities;
using NewsGet_Android.Helpers;
using Square.Picasso;

namespace NewsGet_Android.Fragments
{
	public class ProvidersFragment : Android.Support.V4.App.Fragment
	{
//		public override void OnCreate (Bundle savedInstanceState)
//		{
//			base.OnCreate (savedInstanceState);
//
//			// Create your fragment here
//		}

		private Context globalContext = null;
		RecyclerView recyclerview;
		ProvidersAdapter adapter;
		private RestAccess rest;
		private SwipeRefreshLayout swiperefresh;
		private List<Provider> listofproviders;
		private View view;
		private NewsGetApplication app;

		public ProvidersFragment()
		{
			this.RetainInstance = true;
		}

		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			view = inflater.Inflate(Resource.Layout.fragment_providers, null);

			// Get our RecyclerView layout:
			recyclerview = view.FindViewById<RecyclerView> (Resource.Id.recyclerView_providers);

			//............................................................
			// Layout Manager Setup:

			// Use the built-in linear layout manager:
			var layoutmanager = new StaggeredGridLayoutManager (2, StaggeredGridLayoutManager.Vertical);

			// Plug the layout manager into the RecyclerView:
			recyclerview.SetLayoutManager (layoutmanager);

			swiperefresh = view.FindViewById<SwipeRefreshLayout> (Resource.Id.providers_swipe_refresh_layout);

			return view;
		}

		public override async void OnActivityCreated(Bundle savedInstanceState)
		{
			base.OnActivityCreated(savedInstanceState);
			globalContext = Activity;

			app = (NewsGetApplication) Activity.Application;

			var retry = view.FindViewById<Button> (Resource.Id.alert_retry);
			retry.Click += delegate {
				FragmentTransaction tr = FragmentManager.BeginTransaction ();
				tr.Detach (this).Attach (this).Commit ();
			};

			// Check whether device is connected to any types of network
//			if(!NetworkHelper.IsOnline (globalContext)) { return; }

			swiperefresh.SetColorSchemeResources (Resource.Color.accent, Resource.Color.primary_dark);

			// Create a new rest object
			rest = new RestAccess(app.GetServer ());

			var prog = new ProgressB(globalContext, view.FindViewById<ProgressBar> (Resource.Id.progressBar));
			prog.Toggle ();

			// Download the data from GetData method
			listofproviders = await GetData ();

			prog.Toggle ();

			//............................................................
			// Adapter Setup:

			// Create an adapter for the RecyclerView, and pass it the
			// data set to manage:
			adapter = new ProvidersAdapter (listofproviders, globalContext);

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
					adapter._listofproviders = await GetData ();
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

		async Task<List<Provider>> GetData()
		{
			List<Provider> result = null;

			try
			{
				result = await rest.ListProvidersAsync ("allmodulesdetailed");
			}
			catch (Exception)
			{
				var alert = view.FindViewById<RelativeLayout> (Resource.Id.alert);
				alert.Visibility = ViewStates.Visible;
			}

			return result;
		}

		// Handler for the item click event:
		void OnItemClick (object sender, int position)
		{
			Provider pro = listofproviders[position];

			var intent = new Intent(Activity, typeof(SwitcherActivity));
			intent.PutExtra("Provider", pro.Name);
			intent.PutExtra ("DisplayName", pro.DisplayName);
			intent.PutExtra ("SourceWebsite", pro.Url);
			StartActivity(intent);
		}

		private void SupportFragmentSwtich(Android.Support.V4.App.Fragment fragment, int containerView, Bundle bundle)
		{
			if(bundle != null)
			{
				fragment.Arguments = bundle;
			}
			Activity.SupportFragmentManager.BeginTransaction ()
				.Replace (containerView, fragment)
				.Commit ();
		}
	}

	// ViewHolder
	public class ProviderViewHolder : RecyclerView.ViewHolder
	{
		public ImageView Logo { get; private set; }
		public TextView Name { get; private set; }

		// Get references to the views defined in the CardView layout.
		public ProviderViewHolder (View itemView, Action<int> listener) : base (itemView)
		{
			// Locate and cache view references:
			Logo = itemView.FindViewById<ImageView> (Resource.Id.imageView_providers);
			Name = itemView.FindViewById<TextView> (Resource.Id.textView_providers);

			// Detect user clicks on the item view and report which item
			// was clicked (by position) to the listener:
			itemView.Click += (sender, e) => listener (base.LayoutPosition);
		}
	}

	// Adapter
	public class ProvidersAdapter : RecyclerView.Adapter
	{
		// Event handler for item clicks:
		public event EventHandler<int> ItemClick;
		private Context globalContext = null;

		// Underlying data set (a photo album):
		public List<Provider> _listofproviders;

		// Load the adapter with the data set (photo album) at construction time:
		public ProvidersAdapter (List<Provider> listofproviders, Context context)
		{
			this._listofproviders = listofproviders;
			globalContext = context;
		}

		// Create a new provider CardView (invoked by the layout manager): 
		public override RecyclerView.ViewHolder OnCreateViewHolder (ViewGroup parent, int viewType)
		{
			// Inflate the CardView for the provider:
			View itemView = LayoutInflater.From (parent.Context).Inflate (Resource.Layout.providers_recycler_provider, parent, false);

			// Create a ViewHolder to find and hold these view references, and 
			// register OnClick with the view holder:
			ProviderViewHolder vh = new ProviderViewHolder (itemView, OnClick); 
			return vh;
		}

		// Fill in the contents of the photo card (invoked by the layout manager):
		public override void OnBindViewHolder (RecyclerView.ViewHolder holder, int position)
		{
			ProviderViewHolder vh = holder as ProviderViewHolder;

			// Set the ImageView and TextView in this ViewHolder's CardView 
			// from this position in the photo album:

			//vh.Thumbnail.SetImageResource (_listofarticles[position].Thumbnail);

			// We use Picasso to load images efficiently
            if (!string.IsNullOrEmpty(_listofproviders[position].Logo))
            {
                Picasso.With(globalContext).Load(_listofproviders[position].Logo)
                .NetworkPolicy(NetworkPolicy.Offline)
                .Into(vh.Logo, () =>
                {
                    // on success
                    Console.WriteLine("Picture is cached");
                }, () => {
                    // on error
                    Picasso.With(globalContext).Load(_listofproviders[position].Logo)
                    .Into(vh.Logo);
                    Console.WriteLine("Picture is not cached. Will be loaded.");
                });
            }
            vh.Name.Text = _listofproviders[position].DisplayName;
		}

		// Return the number of photos available in the photo album:
		public override int ItemCount
		{
			get
			{
				try
				{
					return this._listofproviders.Count;
				}
				catch (Exception)
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
