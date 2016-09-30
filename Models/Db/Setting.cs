using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace NewsGet_Android.Models.Db
{
	public class Setting
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}
}