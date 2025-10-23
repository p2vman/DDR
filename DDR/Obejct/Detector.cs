using OpenTK.Mathematics;

namespace DDR.Obejct;

public class Detector : GameObject, IColisedObject
{
    public AABB AABB {get; set; }
    public Detector(Vector3 position, AABB aabb) : base(position, new Vector3())
    {
        Position = position;
        AABB = aabb;
        Model = DDR.Model.Model.FromAabb(aabb);
    }
}