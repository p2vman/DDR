using DDR.Model;

namespace DDR;

public class ModelVariant : IModelAccess
{
    public Dictionary<string, Model.Model> Variants { get; set; } = new Dictionary<string, Model.Model>();
    
    public IModel GetModel(string name)
    {
        return Variants.ContainsKey(name) ? Variants[name] : null;
    }
}

public class ModelVariantRaw
{
    public Dictionary<string, string> Variants { get; private set; } = new Dictionary<string, string>();
}