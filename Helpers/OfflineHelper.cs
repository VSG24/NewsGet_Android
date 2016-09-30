using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Security.Cryptography;
using System.IO;

using Newtonsoft.Json;
using NewsGet_Android.Models;

namespace NewsGet_Android.Helpers
{
	public static class OfflineHelper
	{
		public static string OfflineArticlesDirectory = System.IO.Path.Combine( System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "saved_articles");
		public static string OfflineArticlesPrefix = OfflineHelper.OfflineArticlesDirectory + "/_offline_";

		public static string GenerateOfflineFileName(string url, string save_location)
		{
			return save_location + GenerateHash (url) + ".json";
		}

		public static string GenerateHash(string input)
		{
			MD5 md5Hasher = MD5.Create();
			byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
			return BitConverter.ToString(data);
		}

		public static bool OfflineArticleExist(string offlinearticleaddr)
		{
			if (File.Exists (offlinearticleaddr))
				return true;
			else
				return false;
		}

		public static Article ReadOfflineArticle(string offlinearticleaddr)
		{
			var json = File.ReadAllText (offlinearticleaddr);
			return JsonConvert.DeserializeObject<Article> (json);
		}

		public static void SaveOfflineArticle(Article article)
		{
			try
			{
				File.WriteAllText (GenerateOfflineFileName (article.Url, OfflineArticlesPrefix), JsonConvert.SerializeObject(article));
			}
			catch (Exception)
			{}
		}

		public static void DeleteSavedArticleNoCheck(string offlinearticleaddr)
		{
			File.Delete (offlinearticleaddr);
		}

		public static void DeleteAllSavedArticles()
		{
			var filePaths = Directory.GetFiles(OfflineArticlesDirectory);
			foreach (string filePath in filePaths)
				File.Delete(filePath);
		}
	}
}

