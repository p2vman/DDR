

using OpenTK.Graphics.OpenGL.Compatibility;

namespace DDR.Obejct;

public interface IColisedObject
{
    AABB AABB {get; set;}

    public void Draw()
    {
        
    }
}