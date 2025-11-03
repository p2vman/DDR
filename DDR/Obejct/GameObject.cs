using OpenTK.Mathematics;

namespace DDR.Obejct;

public class GameObject(Vector3 Position, Vector3 Velocity)
{
    public Vector3 Position;
    public Vector3 Velocity;
    public Vector3 Color = new Vector3(1, 1, 1);

    public Model.IModelAccess Model { get; set; }
    
    public virtual void Tick(Game game)
    {
        Position += Velocity * Player.update_s;
    }

    public virtual void Update(Game game)
    {
        
    }
    
    public string State {get; set;}
}