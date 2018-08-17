using Microsoft.ML.Runtime.Api;

namespace MLTest
{
	public class PriceData
	{
		[Column("0")]
		public string manufacturer;

		[Column("1")]
		public string model;

		[Column("2")]
		public float bitcoinCource;

		[Column("3")]
		public float price;
	}

	public class PriceDataPrediction
	{
		[ColumnName("Score")]
		public float price;
	}
}
