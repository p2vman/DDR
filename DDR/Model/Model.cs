using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace DDR.Model;


public interface IModel
{
    public uint[] indices {get; set;}
    public float[] vertices {get; set;}
    public int _vao {get; set;}
    public int _vbo {get; set;}
    public int _ebo {get; set;}
}
public interface IModelAccess
{
    IModel GetModel(string name);
}

public class Model : IModelAccess, IModel
{
    public uint[] indices {get; set;}
    public float[] vertices {get; set;}
    public int _vao {get; set;}
    public int _vbo {get; set;}
    public int _ebo {get; set;}
    
    public Model ApplyMatrix(Matrix4 mat)
    {
        for (int i = 0; i < vertices.Length; i += 3)
        {
            Vector3 v = new Vector3(vertices[i], vertices[i + 1], vertices[i + 2]); // гомогенные координаты
            Vector3 tv = Vector3.TransformPosition(v, mat);
            vertices[i]     = tv.X;
            vertices[i + 1] = tv.Y;
            vertices[i + 2] = tv.Z;
        }
        return this;
    }

    public static AABB ComputeAABB(float[] vertices)
    {
        if (vertices == null || vertices.Length < 3)
            throw new ArgumentException("Vertices array is empty or invalid.");

        Vector3 min = new Vector3(float.MaxValue);
        Vector3 max = new Vector3(float.MinValue);

        for (int i = 0; i < vertices.Length; i += 3)
        {
            var v = new Vector3(vertices[i], vertices[i + 1], vertices[i + 2]);

            if (v.X < min.X) min.X = v.X;
            if (v.Y < min.Y) min.Y = v.Y;
            if (v.Z < min.Z) min.Z = v.Z;

            if (v.X > max.X) max.X = v.X;
            if (v.Y > max.Y) max.Y = v.Y;
            if (v.Z > max.Z) max.Z = v.Z;
        }

        return new AABB(min, max);
    }



    public Model(uint[] indices, float[] vertices)
    {

        this.indices = indices;
        this.vertices = vertices;
        
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, this.vertices.Length * sizeof(float), this.vertices, BufferUsage.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, this.indices.Length * sizeof(uint), this.indices, BufferUsage.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
    }

    public IModel GetModel(string name)
    {
        return this;
    }
    
    public static Model FromAabb(AABB aabb)
    {
        return new Model([
            0,1,2,2,3,0,  4,5,6,6,7,4,
            0,1,5,5,4,0,  2,3,7,7,6,2,
            0,3,7,7,4,0,  1,2,6,6,5,1
        ], [
            aabb.Min.X, aabb.Min.Y, aabb.Min.Z, // 0
            aabb.Max.X, aabb.Min.Y, aabb.Min.Z, // 1
            aabb.Max.X, aabb.Max.Y, aabb.Min.Z, // 2
            aabb.Min.X, aabb.Max.Y, aabb.Min.Z, // 3
            aabb.Min.X, aabb.Min.Y, aabb.Max.Z, // 4
            aabb.Max.X, aabb.Min.Y, aabb.Max.Z, // 5
            aabb.Max.X, aabb.Max.Y, aabb.Max.Z, // 6
            aabb.Min.X, aabb.Max.Y, aabb.Max.Z // 7
        ]);
    }
}

public class ModelGroup : IModelAccess, IModel
{
    public uint[] indices {get; set;}
    public float[] vertices {get; set;}
    public int _vao {get; set;}
    public int _vbo {get; set;}
    public int _ebo {get; set;}
    
    public ModelGroup(Model[] models)
    {
        int totalVertices = models.Sum(m => m.vertices.Length);
        int totalIndices = models.Sum(m => m.indices.Length);
        
        vertices = new float[totalVertices];
        indices = new uint[totalIndices];
        
        int vertexOffset = 0;
        int indexOffset = 0;
        uint currentVertexBase = 0;
        
        foreach (var model in models)
        {
            Array.Copy(model.vertices, 0, vertices, vertexOffset, model.vertices.Length);
            vertexOffset += model.vertices.Length;
            
            for (int i = 0; i < model.indices.Length; i++)
                indices[indexOffset + i] = model.indices[i] + currentVertexBase / 3;

            indexOffset += model.indices.Length;
            currentVertexBase += (uint)(model.vertices.Length);
        }
        
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsage.StaticDraw);

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsage.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindVertexArray(0);
    }
    
    public IModel GetModel(string name)
    {
        return this;
    }
}

public class ModelBuilder
{
    private List<Vector3> _vertices = [];
    private List<uint> _indices = [];

    public ModelBuilder()
    {

    }

    public ModelBuilder Vertex3(float x, float y, float z)
    {
        _vertices.Add(new Vector3(x, y, z));
        return this;
    }
    
    public ModelBuilder Vertex3(Vector3 v)
    {
        _vertices.Add(v);
        return this;
    }

    public ModelBuilder Index(uint i)
    {
        _indices.Add(i);
        return this;
    }
    
    public Model Build()
    {
        var arr = new float[_vertices.Count * 3];
        for (var i = 0; i < _vertices.Count; i++)
        {
            var v = _vertices[i];
            var j = i * 3;
            arr[j] = v.X;
            arr[j + 1] = v.Y;
            arr[j + 2] = v.Z;
        }
        return new Model(_indices.ToArray(), arr);
    }
}