using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace DDR;

public class StringReader(string input)
{
    private readonly string _input = input ?? throw new ArgumentNullException(nameof(input));
    private int _cursor = 0;

    public int Cursor => _cursor;
    public bool CanRead(int length = 1) => _cursor + length <= _input.Length;
    public char Peek() => CanRead() ? _input[_cursor] : '\0';
    public char Read() => CanRead() ? _input[_cursor++] : '\0';
    public string Remaining => _cursor < _input.Length ? _input.Substring(_cursor) : string.Empty;

    public void Skip() { if (CanRead()) _cursor++; }

    public void SkipWhitespace()
    {
        while (CanRead() && char.IsWhiteSpace(Peek()))
            _cursor++;
    }

    public string ReadWhile(Func<char, bool> condition)
    {
        int start = _cursor;
        while (CanRead() && condition(Peek()))
            _cursor++;
        return _input.Substring(start, _cursor - start);
    }

    public string ReadWord()
    {
        SkipWhitespace();
        return ReadWhile(ch => !char.IsWhiteSpace(ch));
    }

    public int ReadInt()
    {
        SkipWhitespace();
        var number = ReadWhile(ch => char.IsDigit(ch) || ch == '-');
        return int.Parse(number);
    }

    public float ReadFloat()
    {
        SkipWhitespace();
        var number = ReadWhile(ch => char.IsDigit(ch) || ch == '-' || ch == '.');
        return float.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
    }

    public string ReadQuotedString()
    {
        SkipWhitespace();
        if (!CanRead() || Peek() != '"')
            throw new Exception("Expected '\"'");

        Skip();
        var str = ReadWhile(ch => ch != '"');
        if (!CanRead())
            throw new Exception("Unclosed quoted string");

        Skip();
        return str;
    }
}

public class Preprocessor(IResourceMannager resourceMannager)
{
    public string Process(string text)
    {
        var lines = text.Split("\n").ToList();
        for (var i = 0; i < lines.Count; i++)
        {
            var line =  lines[i];

            if (line.StartsWith("#"))
            {
                var reader = new StringReader(line);
                
                reader.Skip();
                var kl = "";
                while (reader.CanRead() && reader.Peek() != ' ')
                {
                    kl += reader.Read();
                }
                
                if (kl.Length == 0) continue;
                if (kl == "include")
                {
                    reader.SkipWhitespace();

                    ResourceLocation location = reader.ReadQuotedString();

                    lines[i] = Process(resourceMannager.ReadToEndOrThrow(resourceMannager[location-"shaders/"]));
                }
            }
        }

        return string.Join("\n", lines);
    }
}

public class Shader : IDisposable
{
    public int Handle;
    public Shader(string vertex, string fragment, IResourceMannager resourceMannager)
    {

        var pre = new Preprocessor(resourceMannager);
        
        var v = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(v, pre.Process(vertex));
        GL.CompileShader(v);

        var f = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(f, pre.Process(fragment));
        GL.CompileShader(f);
        
        
        GL.GetShaderi(v, ShaderParameterName.CompileStatus, out var vStatus);
        if (vStatus == 0)
        {
            GL.GetShaderInfoLog(v, out var log);
            Console.WriteLine(log);
        }
        

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, v);
        GL.AttachShader(Handle, f);
        GL.LinkProgram(Handle);
        
        GL.DetachShader(Handle, v);
        GL.DetachShader(Handle, f);
        GL.DeleteShader(v);
        GL.DeleteShader(f);
    }

    public void Use() => GL.UseProgram(Handle);

    public void SetMatrix4(string name, Matrix4 mat)
    {
        GL.UniformMatrix4f(GL.GetUniformLocation(Handle, name), 1, false, ref mat);
    }
    
    public void SetVector3(string name, Vector3 vec)
    {
        GL.Uniform3f(GL.GetUniformLocation(Handle, name), 1, ref vec);
    }

    public int GetUniformLocation(string name)
    {
        return GL.GetUniformLocation(Handle, name);
    }
    
    public void SetQuaternion(string name, Quaternion quaternion)
    {
        GL.Uniform4f(GL.GetUniformLocation(Handle, name), 
            quaternion.X, 
            quaternion.Y, 
            quaternion.Z, 
            quaternion.W
        );
    }

    public void Dispose()
    {
        GL.DeleteProgram(Handle);
        Handle = 0;
    }
}