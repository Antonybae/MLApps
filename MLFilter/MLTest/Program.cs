using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Models;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;

namespace MLTest
{
	class Program
	{
		//contains the path to the file with data used to train
		private static readonly string _dataPath =
			Path.Combine(Environment.CurrentDirectory, "Data", "video_card_train.csv");

		//containt the path to the file with the data set used to evaluate the model
		private static readonly string _testDataPath =
			Path.Combine(Environment.CurrentDirectory, "Data", "video_card_test.csv");

		//contains the path to the file where the trained model is stored
		private static readonly string _modelPath = Path.Combine(Environment.CurrentDirectory, "Data", "Model.zip");

		static async Task Main(string[] args)
		{
			Console.WriteLine("Updating bitcoin data...");
			PriceGraphManager manager = new PriceGraphManager();
			//manager.GetBitcoinData();
			//manager.ConvertJsonToBitcoinCource();
			//Console.WriteLine("Current bitcoin value: {0}", manager.bitcoinCources.Last().price);

			Console.WriteLine("Updating videocard prices...");
			manager.GetVideoCardData();
			manager.ConvertJsonToVideoCard();
			Console.WriteLine("Current videocard value: ", manager.videoCards.Last().price);

			Console.WriteLine("Writing data to disk...");
			manager.CreateCSV();

			Console.WriteLine("Starting learning...");

			PredictionModel<PriceData, PriceDataPrediction> model = await Train();
			Evaluate(model);

			PriceDataPrediction prediction = model.Predict(TestPricing.videocardPrice);
			Console.WriteLine("Predicted price: {0}, actual price: 18207", prediction.price);
		}

		public static async Task<PredictionModel<PriceData, PriceDataPrediction>> Train()
		{
			var pipeline = new LearningPipeline
			{
				new TextLoader(_dataPath).CreateFrom<PriceData>(useHeader: true, separator: ','),
				new ColumnCopier(("price", "Label")),
				new CategoricalOneHotVectorizer("manufacturer", "model"),
				new ColumnConcatenator("Features", "manufacturer", "model", "bitcoinCource"),
				new FastTreeRegressor()
			};

			PredictionModel<PriceData, PriceDataPrediction> model = pipeline.Train<PriceData, PriceDataPrediction>();

			await model.WriteAsync(_modelPath);
			return model;
		}
		
		private static void Evaluate(PredictionModel<PriceData, PriceDataPrediction> model)
		{
			var testData = new TextLoader(_testDataPath).CreateFrom<PriceData>(useHeader: true, separator: ',');
			var evaluator = new RegressionEvaluator();
			RegressionMetrics metrics = evaluator.Evaluate(model, testData);
			Console.WriteLine($"Rms = {metrics.Rms}");
			Console.WriteLine($"RSquared = {metrics.RSquared}");
		}
	}
}
