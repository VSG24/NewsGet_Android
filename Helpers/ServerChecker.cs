using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using UptimeSharp;
using UptimeSharp.Models;

using Android.App;
using Android.Content;
using Android.Widget;
using NewsGet_Android.Helpers;

namespace NewsGet_Android.Helpers
{
	public class ServerChecker
	{
		public static bool Checked = false;

		public async static Task<int> CheckAndAction (Context context, NewsGetApplication app)
		{
			try
			{
				Checked = true;
				UptimeClient _client = new UptimeClient("u302761-bb8514dea72fb68e8ca2b09d");

				List<Monitor> monitors = null;

				int timeout = 8000;
				var task = _client.GetMonitors();
				if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
				{
					monitors = task.Result;
				}
				else
				{ 
					throw new Exception ();
				}

//				List<Monitor> monitors = await _client.GetMonitors();

				bool serverDown = false;
				List<string> downServers = new List<string> ();

				foreach (var item in monitors)
				{
					if(item.Status != Status.Up)
					{
						serverDown = true;
						downServers.Add (item.Target);
					}
				}

				var defServer = app.GetServer ();
				var allServers = app.GetAllServers ();

				if(serverDown && downServers.Count < 2)
				{
					foreach (var server in downServers)
					{
						if(defServer == server)
						{
							//Toast.MakeText (context, "Server changed", ToastLength.Long).Show ();
							if(defServer == allServers[0])
							{
								app.SetServer (allServers[1]);
								app.SetDefServer ("(sv2)");
							}
							else if(defServer == allServers[1])
							{
								app.SetServer (allServers[0]);
								app.SetDefServer ("(sv1)");
							}
							break;
						}
					}
				}

				return 0;
			}
			catch (Exception)
			{
				return -1;
			}
		}
	}
}

