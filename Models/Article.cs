using System;
using System.Collections.Generic;
using System.Linq;

namespace NewsGet_Android.Models
{
	public class Article
	{
		public string Source { get; set; }
		public string Source_DisplayName { get; set; }
		public string Thumbnail { get; set; }
		public string Title { get; set; }
		public string Excerpt { get; set; }
		public string Content { get; set; }
		public string DateTime { get; set; }
		public string Url { get; set; }

		public override string ToString()
		{
			return Title;
		}
	}
}