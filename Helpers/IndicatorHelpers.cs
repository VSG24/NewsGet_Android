using Android.App;
using Android.Views;
using Android.Content;
using Android.Widget;
using Android.Graphics;
using Android.Support.V4.Content;

namespace NewsGet_Android.Helpers
{
	public sealed class ProgressD
	{
		private Context context;
		private ProgressDialog prog;
		private string msg;
		public bool Cancelable = false;

		public ProgressD(Context context, string msg = "")
		{
			this.context = context;
			this.prog = new ProgressDialog (this.context);
			this.prog.SetCancelable (Cancelable);

			if(msg == "")
				this.msg = (this.context.Resources.GetString (Resource.String.loading));
			else
				this.msg = msg;
			
			this.prog.SetMessage (this.msg);
		}

		public void Toggle()
		{
			if(prog.IsShowing)
			{
				prog.Hide ();
			}
			else
			{
				prog.Show ();
			}
		}
	}

	public sealed class ProgressB
	{
		private Context context;
		private ProgressBar prog;

		public ProgressB(Context context, ProgressBar prog)
		{
			this.context = context;
			this.prog = prog;
			// Setting the color for the progress bar
			this.prog.IndeterminateDrawable.SetColorFilter (new Color (ContextCompat.GetColor(this.context, Resource.Color.accent)),
				 Android.Graphics.PorterDuff.Mode.Multiply);
		}

		public void Toggle()
		{
			if(prog.Visibility == ViewStates.Visible)
			{
				prog.Visibility = ViewStates.Gone;
			}
			else
			{
				prog.Visibility = ViewStates.Visible;
			}
		}
	}
}

