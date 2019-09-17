using Microsoft.ML.Data;

namespace BlazorML.Model.DataModels.Sentement2
{
    public class SampleObservation
    {
        [ColumnName("col0"), LoadColumn(0)]
        public string Col0 { get; set; }


        [ColumnName("Label"), LoadColumn(1)]
        public bool Label { get; set; }
    }
}
