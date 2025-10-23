

using OpenTK.Graphics.OpenGL;

namespace LGL;

public class VertexBuffer
{
    public record MeshPtr(int offset, int size);
    
    private int _vbo;
    
    public VertexBuffer()
    {
        _vbo = GL.GenBuffer();
    }

    public void Bind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
    }

    public MeshPtr Push()
    {
        return new MeshPtr(0, 0);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }
    
    
}