using OpenTK.Mathematics;

namespace DDR.Obejct;

public class GameObject(Vector3 Position, Vector3 Velocity)
{
    public Vector3 Position;
    public Vector3 Velocity;

    public Model.IModelAccess Model { get; set; }
    
    public void Tick(Game game)
    {
        Position += Velocity;
    }
    
    public string State {get; set;}
}