using System;

namespace NewsGet_Android
{
	public class Provider
	{
		public string Name { get; set; }
		public string DisplayName { get; set; }
		public string Language { get; set; }
		public string Url { get; set; }
		public string Version { get; set; }
		public string Author { get; set; }
		public string Logo { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}
}

