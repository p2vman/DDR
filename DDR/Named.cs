using OpenTK.Mathematics;

namespace DDR;

public class Named : GameObject
{
    public AABB AABB {get; set; }
    public Named() : base(new Vector3(0,0,0),new Vector3(0, 0, 0))
    {
        
    }
}