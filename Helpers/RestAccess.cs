using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NewsGet_Android.Models;
using RestSharp;
using RestSharp.Deserializers;
using Android.App;

namespace NewsGet_Android.Helpers
{
	public sealed class RestAccess
	{
//		public string RestHost = "http://10.71.34.1:8080";
//		public string RestHost = "http://192.168.43.199:8080";
		public string RestHost;
		public RestClient Client;

//		public RestAccess (string resthost)
//		{
//			// Use this constructor only for testing on localhost
//			this.Client = new RestClient (this.RestHost);
//		}

		public RestAccess ()
		{
			var app = (NewsGetApplication) Application.Context;
			var server = app.GetServer ();
			this.RestHost = server;
			this.Client = new RestClient (this.RestHost);
		}

		public RestAccess (string restHost)
		{
			this.RestHost = restHost;
			this.Client = new RestClient (this.RestHost);
		}

		public async Task<List<Provider>> ListProvidersAsync (string serviceAction)
		{
			var client = this.Client;
			var request = new RestRequest("service/"+serviceAction, Method.GET) { RequestFormat = DataFormat.Json };
			var cancellationTokenSource = new CancellationTokenSource();
			var deserial = new JsonDeserializer();

			var response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

			return deserial.Deserialize<List<Provider>>(response);
		}

		public async Task<Provider> ProviderInfoAsync (string serviceAction)
		{
			var client = this.Client;
			var request = new RestRequest("service/"+serviceAction, Method.GET) { RequestFormat = DataFormat.Json };
			var cancellationTokenSource = new CancellationTokenSource();
			var deserial = new JsonDeserializer();

			var response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

			return deserial.Deserialize<Provider>(response);
		}

		public async Task<List<Article>> ListArticlesAsync (string source, string images, string page)
		{
			var client = this.Client;
			RestRequest request;
			if(source != "getlatestcombine")
			{
				request = new RestRequest("getall/"+source+"/"+images+"/"+page);
			}
			else
			{
				request = new RestRequest("getlatest/"+images+"/"+page);
			}
			var cancellationTokenSource = new CancellationTokenSource();
			var deserial = new JsonDeserializer();

			// In ms
			request.Timeout = 8000;

			var response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

			return deserial.Deserialize<List<Article>>(response);
		}

		public async Task<List<Article>> ListMostViewedArticlesAsync ()
		{
			var client = this.Client;
			var request = new RestRequest("gettop");
			var cancellationTokenSource = new CancellationTokenSource();
			var deserial = new JsonDeserializer();

			request.Timeout = 8000;

			var response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

			return deserial.Deserialize<List<Article>>(response);
		}

		public async Task<Article> GetArticleAsync (string source, string url, string returnRules = "0")
		{
			var client = this.Client;
			var request = new RestRequest("getsingle/"+source+"/"+url+"/"+returnRules);
			var cancellationTokenSource = new CancellationTokenSource();
			var deserial = new JsonDeserializer();

			request.Timeout = 8000;

			var response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token);

			return deserial.Deserialize<List<Article>>(response)[0];
		}

		public async Task<ServiceResponse> GetLatestVersionNumber ()
		{
			var client = this.Client;
			var request = new RestRequest("service/getlatestclientversion");
			var cancellationTokenSource = new CancellationTokenSource();
			var deserial = new JsonDeserializer();

			request.Timeout = 10000;

			var response = await client.ExecuteTaskAsync(request, cancellationTokenSource.Token).ConfigureAwait (false);

			return deserial.Deserialize<ServiceResponse>(response);
		}
	}
}