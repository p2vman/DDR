using System.Diagnostics;
using System.Globalization;
using System.Numerics;
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
    
    public Model.Model LoadObj(ResourceLocation location)
    {
        ArgumentNullException.ThrowIfNull(location);
        if (Cache.ContainsKey(location))
        {
            return Cache[location];
        }
        return Cache[location] = LoadObj(resourceMannager.ReadToEndOrThrow(resourceMannager[location
            .StartPrefix("models/")
            .EndPrefix(".obj")
        ] ?? resourceMannager.GetResourceOrThrow(Cube)));
    }
    
    public Model.Model LoadObj(string text)
    {
        var vertices = new List<Vector3>();
        var indices = new List<uint>();

        foreach (var rawLine in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(rawLine)) continue;
            var line = rawLine.Trim();
            
            if (line.StartsWith("#") || line.StartsWith("o ") || line.StartsWith("g ") ||
                line.StartsWith("s ") || line.StartsWith("usemtl ") || line.StartsWith("mtllib ")) continue;

            if (line.StartsWith("v "))
            {
                var p = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (p.Length < 4) continue;

                float x = float.Parse(p[1], CultureInfo.InvariantCulture);
                float y = float.Parse(p[2], CultureInfo.InvariantCulture);
                float z = float.Parse(p[3], CultureInfo.InvariantCulture);

                vertices.Add(new Vector3(x, y, z));
            }
            else if (line.StartsWith("f "))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 4) continue;

                var faceIndices = new List<uint>();
                for (int i = 1; i < parts.Length; i++)
                {
                    var token = parts[i];
                    if (string.IsNullOrWhiteSpace(token)) continue;

                    var split = token.Split('/');
                    if (!int.TryParse(split[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out int idx)) continue;

                    int vertexIndex = idx < 0 ? vertices.Count + idx : idx - 1;
                    if (vertexIndex < 0 || vertexIndex >= vertices.Count)
                        throw new InvalidDataException($"Invalid vertex index {idx} in line: {line}");

                    faceIndices.Add((uint)vertexIndex);
                }
                
                for (int i = 1; i < faceIndices.Count - 1; i++)
                {
                    indices.Add(faceIndices[0]);
                    indices.Add(faceIndices[i]);
                    indices.Add(faceIndices[i + 1]);
                }
            }
        }

        var arr = new float[vertices.Count * 3];
        for (var i = 0; i < vertices.Count; i++)
        {
            var v = vertices[i];
            var j = i * 3;
            arr[j] = v.X;
            arr[j + 1] = v.Y;
            arr[j + 2] = v.Z;
        }
        return new Model.Model(indices.ToArray(), arr);
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
}