using OpenTK.Mathematics;

namespace DDR.Obejct;

public class Detector : GameObject, IColisedObject
{
    public AABB AABB {get; set; }
    public Action<GameObject> callback;
    public Detector(Vector3 position, AABB aabb, Action<GameObject> callback) : base(position, new Vector3())
    {
        Position = position;
        AABB = aabb;
        Model = DDR.Model.Model.FromAabb(aabb);
        this.callback = callback;
    }

    public override void Tick(Game game)
    {
        var aabb = TransformAABB(AABB, Position);
        game.ObjectLayer.FindAll(obj => obj is IColisedObject).ForEach(obj =>
        {
            if (obj != this && obj is IColisedObject colObj)
            {
                if (TransformAABB(colObj.AABB, obj.Position).Intersects(aabb))
                {
                    callback(obj);
                }
            }
        });
    }

    public static AABB TransformAABB(AABB aabb, Vector3 position)
    {
        return new AABB
        {
            Min = aabb.Min + position,
            Max = aabb.Max + position,
        };
    }
}