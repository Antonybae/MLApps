using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace MLTest
{
	public class BitcoinCource
	{
		[JsonProperty("y")]
		public int price { get; set; }
	}

	public class VideoCard
	{
		public int price { get; set; }
	}


	public class PriceGraphManager
	{
		private static readonly string ProductId = "160";
		private static readonly string Api = @"http://hardprice.ru/api/product/" + ProductId + "/chart";
		private static readonly string BitcoinApi = @"https://api.blockchain.info/charts/transactions-per-second?timespan=48weeks&rollingAverage=5days&format=json";

		private string jsonString = string.Empty;

		//realtime data
		public IList<BitcoinCource> bitcoinCources { get; set; }
		public IList<VideoCard> videoCards { get; set; }

		public void GetBitcoinData()
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BitcoinApi);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				jsonString = reader.ReadToEnd();
			}
		}

		public void GetVideoCardData()
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Api);
			request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader reader = new StreamReader(stream))
			{
				jsonString = reader.ReadToEnd();
			}
		}

		public void CreateCSV()
		{
			throw new NotImplementedException("create csv not implemented");
		}

		public void ConvertJsonToVideoCard()
		{
			JObject obJObject = JObject.Parse(jsonString);
			IList<JToken> tokens = obJObject["1"].Children().ToList();
			videoCards = new List<VideoCard>();
			foreach (var token in tokens)
			{
				videoCards.Add(new VideoCard(){price = (int)token.Last});
			}
		}

		public List<PriceData> FindConnectionPoints()
		{
			var interval = bitcoinCources.Count
		}

		public void ConvertJsonToBitcoinCource()
		{
			JObject obJObject = JObject.Parse(jsonString);
			IList<JToken> results = obJObject["values"].Children().ToList();
			bitcoinCources	= new List<BitcoinCource>();
			foreach (var result in results)
			{
				BitcoinCource priceResult = result.ToObject<BitcoinCource>();
				priceResult.price *= 1000;
				bitcoinCources.Add(priceResult);
			}
		}
	}
}
