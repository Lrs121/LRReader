﻿using LRReader.Shared.Models.Main;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LRReader.Shared.Models.Api
{
	public class LRRApi
	{
		private string apiKey;

		private RestClient client;

		public LRRApi()
		{
			client = new RestClient();
			client.UseSerializer(() => new JsonNetSerializer());
		}

		public void RefreshSettings(ServerProfile profile)
		{
			client.BaseUrl = new Uri(profile.ServerAddress);
			if (profile.HasApiKey)
			{
				apiKey = profile.ServerApiKey;
				client.RemoveDefaultParameter("key");
				client.AddDefaultParameter("key", apiKey);
			}
			else
			{
				apiKey = "";
			}
		}

		public RestClient GetClient()
		{
			return client;
		}

		public static GenericApiResponse<T> GetResult<T>(IRestResponse restResponse)
		{
			var apiResponse = new GenericApiResponse<T>();
			if (restResponse.StatusCode == HttpStatusCode.OK)
			{
				apiResponse.Data = JsonConvert.DeserializeObject<T>(restResponse.Content);
			}
			else
			{
				apiResponse.Error = JsonConvert.DeserializeObject<GenericApiError>(restResponse.Content);
			}
			return apiResponse;
		}

		public static GenericApiError GetError(IRestResponse restResponse)
		{
			if (restResponse.StatusCode != HttpStatusCode.OK)
			{
				return JsonConvert.DeserializeObject<GenericApiError>(restResponse.Content);
			}
			return null;
		}
	}

	public class JsonNetSerializer : IRestSerializer
	{
		public string Serialize(object obj) =>
			JsonConvert.SerializeObject(obj);

		public string Serialize(Parameter parameter) =>
			JsonConvert.SerializeObject(parameter.Value);

		public T Deserialize<T>(IRestResponse response) =>
			JsonConvert.DeserializeObject<T>(response.Content);

		public string[] SupportedContentTypes { get; } =
		{
			"application/json", "text/json", "text/x-json", "text/javascript", "*+json"
		};

		public string ContentType { get; set; } = "application/json";

		public DataFormat DataFormat { get; } = DataFormat.Json;
	}
}
