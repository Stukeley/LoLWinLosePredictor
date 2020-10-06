using Microsoft.ML;
using System;

namespace MachineLearningModel
{
	public class ConsumeModel
	{
		private static Lazy<PredictionEngine<ModelInput, ModelOutput>> PredictionEngine = new Lazy<PredictionEngine<ModelInput, ModelOutput>>(CreatePredictionEngine);

		public static PredictionEngine<ModelInput, ModelOutput> CreatePredictionEngine()
		{
			MLContext mlContext = new MLContext();

			string modelPath = @"";
			ITransformer mlModel = mlContext.Model.Load(modelPath, out _);
			var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

			return predEngine;
		}

		public static ModelOutput Predict(ModelInput input)
		{
			ModelOutput result = PredictionEngine.Value.Predict(input);
			return result;
		}
	}
}
