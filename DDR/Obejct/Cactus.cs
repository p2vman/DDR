using DDR.Obejct;
using OpenTK.Mathematics;

namespace DDR;

public class Cactus : GameObject, IColisedObject
{
    public record CactusType(AABB aabb, string state);
    
    public AABB AABB { get; set; }
    public Cactus(CactusType type, Vector3 pos) : base(new Vector3(), new Vector3())
    {
        State = type.state;
        AABB = type.aabb;
        Position = pos;
    }
}