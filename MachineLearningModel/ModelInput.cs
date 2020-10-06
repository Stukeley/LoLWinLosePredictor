using Microsoft.ML.Data;

namespace MachineLearningModel
{
	public class ModelInput
	{
		[LoadColumn(0)]
		public string SpecifiedPlayer { get; set; }

		[LoadColumn(1, 4)]
		[VectorType(4)]
		public string[] AllyTeam { get; set; }

		[LoadColumn(5, 9)]
		[VectorType(5)]
		public string[] EnemyTeam { get; set; }

		[ColumnName("Label"), LoadColumn(10)]
		public bool Outcome { get; set; }
	}
}
