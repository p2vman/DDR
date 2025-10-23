
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Numerics;
using DDR.Model;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp;
using Newtonsoft.Json;

namespace DDR;

public class JsonModelElement
{
    public float[] from;
    public float[] to;
}

public class JsonModel
{
    public List<JsonModelElement> elements;
}

public class ModelLoader
{
    private IResourceMannager resourceMannager;
    public Dictionary<ResourceLocation, Model.Model> Cache;
    public Dictionary<ResourceLocation, ModelVariant> CacheVariants;
    public ResourceLocation Cube = ResourceLocation.ParseOrThrow("core:cube")
        .StartPrefix("models/")
        .EndPrefix(".json");
    
    public ModelLoader(IResourceMannager resourceMannager)
    {
        ArgumentNullException.ThrowIfNull(resourceMannager);
        this.resourceMannager = resourceMannager;
        Cache = new Dictionary<ResourceLocation, Model.Model>();
        CacheVariants = new Dictionary<ResourceLocation, ModelVariant>();
    }
        
    public Model.Model Load(ResourceLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (Cache.ContainsKey(location))
        {
            return Cache[location];
        }
        return Cache[location] = Load(resourceMannager.ReadToEndOrThrow(resourceMannager[location
            .StartPrefix("models/")
            .EndPrefix(".json")
        ] ?? resourceMannager.GetResourceOrThrow(Cube)));
    }

    public Model.Model Load(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var obj = JsonSerializer.CreateDefault()
            .Deserialize<JsonModel>(new JsonTextReader(new System.IO.StringReader(text)));
        

        var vertices = new List<float>();
        var indices = new List<uint>();
        uint vertexOffset = 0;

        Debug.Assert(obj != null, nameof(obj) + " != null");
        foreach (var element in obj.elements)
        {
            var f = element.from;
            var t = element.to;
            
            vertices.AddRange(new float[]
            {
                f[0], f[1], f[2], // 0
                t[0], f[1], f[2], // 1
                t[0], t[1], f[2], // 2
                f[0], t[1], f[2], // 3
                f[0], f[1], t[2], // 4
                t[0], f[1], t[2], // 5
                t[0], t[1], t[2], // 6
                f[0], t[1], t[2], // 7
            }.Select(i => i / 16));
            
            indices.AddRange(new uint[]
            {
                0,1,2,2,3,0,  4,5,6,6,7,4,
                0,1,5,5,4,0,  2,3,7,7,6,2,
                0,3,7,7,4,0,  1,2,6,6,5,1
            }.Select(i => i + vertexOffset));

            vertexOffset += 8;
        }

        return new Model.Model(indices.ToArray(), vertices.ToArray());
    }
    
    public ModelVariant LoadVariant(ResourceLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (CacheVariants.ContainsKey(location))
        {
            return CacheVariants[location];
        }
        return CacheVariants[location] = LoadVariant(resourceMannager.ReadToEndOrThrow(resourceMannager[location
            .StartPrefix("model_variants/")
            .EndPrefix(".json")
        ] ?? resourceMannager.GetResourceOrThrow(Cube)));
    }
    
    public ModelVariant LoadVariant(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var obj = JsonSerializer.CreateDefault()
            .Deserialize<ModelVariantRaw>(new JsonTextReader(new System.IO.StringReader(text)));
        
        
        var variants = new Dictionary<string, Model.Model>();
        Debug.Assert(obj != null, nameof(obj) + " != null");
        foreach (var element in obj.Variants)
        {
           variants.Add(element.Key, Load(ResourceLocation.ParseOrThrow(element.Value)));
        }

        return new ModelVariant()
        {
            Variants = variants,
        };
    }
    
    public ModelVariant LoadVariantCs(ResourceLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (CacheVariants.ContainsKey(location))
        {
            return CacheVariants[location];
        }
        return CacheVariants[location] = LoadVariantCs(resourceMannager.ReadToEndOrThrow(resourceMannager[location
            .StartPrefix("model_variants/")
            .EndPrefix(".cs")
        ] ?? resourceMannager.GetResourceOrThrow(Cube)));
    }
    
    public ModelVariant LoadVariantCs(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        
        var options = ScriptOptions.Default
            .AddReferences(typeof(Model.Model).Assembly, typeof(ModelBuilder).Assembly)
            .AddImports("System", "DDR", "System.Collections.Generic");
       
        return new ModelVariant()
        {
            Variants = CSharpScript.EvaluateAsync<Dictionary<string, Model.Model>>(text, options).GetAwaiter().GetResult(),
        };
    }
}