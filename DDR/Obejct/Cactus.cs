using DDR.Obejct;
using OpenTK.Mathematics;

namespace DDR;

public class Cactus : GameObject, IColisedObject
{


    public record CactusType(AABB aabb, string state)
    {
        public static readonly CactusType C_0 = new CactusType(new AABB()
        {
            Max = new Vector3(1, 1, 1),
            Min = new Vector3(0, 0, 0),
        }, "0");
    }
    
    public AABB AABB { get; set; }
    public Cactus(CactusType type, Vector3 pos) : base(new Vector3(), new Vector3())
    {
        State = type.state;
        AABB = type.aabb;
        Position = pos;
    }
}