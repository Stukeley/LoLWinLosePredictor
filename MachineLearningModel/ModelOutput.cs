﻿using Microsoft.ML.Data;

namespace MachineLearningModel
{
	public class ModelOutput
	{
		[ColumnName("PredictedLabel")]
		public bool Prediction { get; set; }

		public float Score { get; set; }
	}
}
