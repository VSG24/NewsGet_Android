using System;

namespace NewsGet_Android.Helpers
{
	public static class DateHelpers
	{
		public static string ToPeStringRep (this int dayofweek)
		{
			switch (dayofweek)
			{
				case 0:
				return "یکشنبه";
				case 1:
				return "دوشنبه";
				case 2:
				return "سه شنبه";
				case 3:
				return "چهارشنبه";
				case 4:
				return "پنجشنبه";
				case 5:
				return "جمعه";
				case 6:
				return "شنبه";
			default:
				return null;
			}
		}

		public static string AddLeadingZeros (this int num)
		{
			if (num < 10)
				return "0" + num;
			else
				return num.ToString ();
		}

		public static string GetTimestamp (DateTime datetime)
		{
			return datetime.ToString ("yyyyMMddHHmmssffff");
		}
	}
}

