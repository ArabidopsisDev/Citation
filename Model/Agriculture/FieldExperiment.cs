using Citation.Properties;
using System.Collections.ObjectModel;

namespace Citation.Model.Agriculture;

public class FieldExperiment
{
    public string Name { get; set; } = Resources.FieldExperimentData_DefaultName;
    public string Description { get; set; } = "";
    public DateTime CreateDate { get; set; } = DateTime.Now;
    public string Author { get; set; } = "";
    public string Institution { get; set; } = "";

    public enum ExperimentDesignType
    {
        CompletelyRandomized,
        RandomizedCompleteBlock,
        SplitPlot,
        LatinSquare
    }

    public ExperimentDesignType DesignType { get; set; } = ExperimentDesignType.RandomizedCompleteBlock;

    public ObservableCollection<Field> Fields { get; set; } = new ObservableCollection<Field>();
    public ObservableCollection<Treatment> Treatments { get; set; } = new ObservableCollection<Treatment>();
    public ObservableCollection<Signboard> Signboards { get; set; } = new ObservableCollection<Signboard>();
}
