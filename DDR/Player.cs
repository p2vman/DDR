using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace DDR;

public class Player : GameObject
{
    public Model Model { get; set; }
    public AABB AABB { get; set; }
    public Player() : base(new Vector3(), new Vector3())
    {
        
    }

}