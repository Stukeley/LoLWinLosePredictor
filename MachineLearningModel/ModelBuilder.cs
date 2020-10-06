using Microsoft.ML;
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

			Evaluate(trainingDataView, trainingPipeline);

			SaveModel(model, trainingDataView.Schema);
		}

		private static IEstimator<ITransformer> BuildTrainingPipeline()
		{
			var dataProcessPipeline = mlContext.Transforms.Conversion.MapValueToKey("Outcome", "Outcome")
				.Append(mlContext.Transforms.CopyColumns("SpecifiedPlayer", "SpecifiedPlayer"))
				.Append(mlContext.Transforms.CopyColumns("AllyTeam", "AllyTeam"))
				.Append(mlContext.Transforms.CopyColumns("EnemyTeam", "EnemyTeam"))
				.AppendCacheCheckpoint(mlContext);

			var trainer = mlContext.MulticlassClassification.Trainers.OneVersusAll(mlContext.BinaryClassification.Trainers.AveragedPerceptron(labelColumnName: "Outcome", numberOfIterations: 10, featureColumnName: "Features"), labelColumnName: "Outcome").Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

			var trainingPipeline = dataProcessPipeline.Append(trainer);

			return trainingPipeline;
		}

		private static ITransformer TrainModel(IDataView trainingDataView, IEstimator<ITransformer> trainingPipeline)
		{
			ITransformer model = trainingPipeline.Fit(trainingDataView);

			Debug.WriteLine("Model trained");

			return model;
		}

		private static void Evaluate(IDataView trainingDataView, IEstimator<ITransformer> trainingPipeline)
		{

		}

		private static void SaveModel(ITransformer mlModel, DataViewSchema modelInputSchema)
		{
			mlContext.Model.Save(mlModel, modelInputSchema, ModelPath);

			Debug.WriteLine($"Model saved to {ModelPath}");
		}
	}
}
