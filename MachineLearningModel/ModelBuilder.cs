using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System.Diagnostics;

namespace MachineLearningModel
{
	public class ModelBuilder
	{
		private static string DataPath = @"E:\Programowanie\Stukeley\LoLWinLosePredictor\MachineLearningModel\TestData Joined.csv";
		private static string ModelPath = @"E:\Programowanie\Stukeley\LoLWinLosePredictor\MachineLearningModel\TrainedModel.zip";

		private static MLContext mlContext = new MLContext(seed: 1);

		public static void CreateModel()
		{
			IDataView trainingDataView = mlContext.Data.LoadFromTextFile<ModelInput>(DataPath, hasHeader: true, separatorChar: ',');

			IEstimator<ITransformer> trainingPipeline = BuildTrainingPipeline();

			ITransformer model = TrainModel(trainingDataView, trainingPipeline);

			//Evaluate(trainingDataView);

			SaveModel(model, trainingDataView.Schema);
		}

		private static IEstimator<ITransformer> BuildTrainingPipeline()
		{
			var options = new AveragedPerceptronTrainer.Options
			{
				LossFunction = new SmoothedHingeLoss(),
				LearningRate = 0.1f,
				LazyUpdate = false,
				RecencyGain = 0.1f,
				NumberOfIterations = 10
			};

			////var dataProcessPipeline = mlContext.Transforms.Concatenate("Features", "SpecifiedPlayer", "AllyTeam", "EnemyTeam")
			////	.Append(mlContext.BinaryClassification.Trainers.AveragedPerceptron(options));


			var dataProcessPipeline = mlContext.Transforms.CopyColumns("Label", "Label")
				.Append(mlContext.Transforms.Text.FeaturizeText("SpecifiedText", "SpecifiedPlayer"))
				.Append(mlContext.Transforms.Text.FeaturizeText("AllyText", "AllyTeam"))
				.Append(mlContext.Transforms.Text.FeaturizeText("EnemyText", "EnemyTeam"))
				.Append(mlContext.Transforms.Concatenate("Features", "SpecifiedText", "AllyText", "EnemyText"))
				.AppendCacheCheckpoint(mlContext)
				.Append(mlContext.BinaryClassification.Trainers.AveragedPerceptron(options));

			return dataProcessPipeline;
		}

		private static ITransformer TrainModel(IDataView trainingDataView, IEstimator<ITransformer> trainingPipeline)
		{
			ITransformer model = trainingPipeline.Fit(trainingDataView);

			Debug.WriteLine("Model trained");

			return model;
		}

		private static void Evaluate(IDataView trainingDataView)
		{
			var metrics = mlContext.BinaryClassification.EvaluateNonCalibrated(trainingDataView);

			PrintMetrics(metrics);
		}

		private static void SaveModel(ITransformer mlModel, DataViewSchema modelInputSchema)
		{
			mlContext.Model.Save(mlModel, modelInputSchema, ModelPath);

			Debug.WriteLine($"Model saved to {ModelPath}");
		}

		private static void PrintMetrics(BinaryClassificationMetrics metrics)
		{
			Debug.WriteLine($"Accuracy: {metrics.Accuracy:F2}");
			Debug.WriteLine($"AUC: {metrics.AreaUnderRocCurve:F2}");
			Debug.WriteLine($"F1 Score: {metrics.F1Score:F2}");
			Debug.WriteLine($"Negative Precision: " +
				$"{metrics.NegativePrecision:F2}");

			Debug.WriteLine($"Negative Recall: {metrics.NegativeRecall:F2}");
			Debug.WriteLine($"Positive Precision: " +
				$"{metrics.PositivePrecision:F2}");

			Debug.WriteLine($"Positive Recall: {metrics.PositiveRecall:F2}\n");
			Debug.WriteLine(metrics.ConfusionMatrix.GetFormattedConfusionTable());
		}
	}
}
