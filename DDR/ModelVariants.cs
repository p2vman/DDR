namespace DDR;

public class ModelVariant
{
    public Dictionary<string, Model> Variants { get; set; } = new Dictionary<string, Model>();
}

public class ModelVariantRaw
{
    public Dictionary<string, string> Variants { get; private set; } = new Dictionary<string, string>();
}