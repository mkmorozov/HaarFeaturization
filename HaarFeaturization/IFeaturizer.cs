namespace HaarFeaturization;

public interface IFeaturizer
{
    public SortedDictionary<string, double> Featurize(List<double> signal);

    public List<SortedDictionary<string, double>> Postprocess(List<SortedDictionary<string, double>> features);
}
