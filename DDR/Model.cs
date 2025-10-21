using System.Numerics;
using OpenTK.Graphics.OpenGL;

namespace DDR;

public class Model
{
    public uint[] indices;
    public float[] vertices;
    public int _vao, _vbo, _ebo;

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
}

public class ModelGroup
{
    public uint[] indices;
    public float[] vertices;
    public int _vao, _vbo, _ebo;
    
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