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
		public float price { get; set; }
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

		public void CreateCSV(string path)
		{
			List<PriceData> datas = FindConnectionPoints();

			//TODO: less hardcoded way
			string header = string.Empty;
			header += nameof(PriceData.manufacturer) + ',';
			header += nameof(PriceData.model) + ',';
			header += nameof(PriceData.bitcoinCource) + ',';
			header += nameof(PriceData.price) + Environment.NewLine;
			
			foreach (var data in datas)
			{
				string values = string.Empty;
				values += data.manufacturer + ",";
				values += data.model + ",";
				values += data.bitcoinCource.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + ",";
				values += data.price.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + Environment.NewLine;
				header += values;
			}

			File.WriteAllText(path, header);
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
			var interval = bitcoinCources.Count / videoCards.Count;
			var videoCardIndex = 0;
			List<PriceData> priceDatas = new List<PriceData>();
			for (int i = 0; i < videoCards.Count * interval;)
			{
				priceDatas.Add(new PriceData(){bitcoinCource = bitcoinCources[i].price, price = videoCards[videoCardIndex].price, manufacturer = "NVIDIA", model = "gtx1060"});
				videoCardIndex++;
				i += interval;
			}

			return priceDatas;
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
