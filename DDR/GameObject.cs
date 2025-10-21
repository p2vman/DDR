using OpenTK.Mathematics;
namespace DDR;

public class GameObject(Vector3 Position, Vector3 Velocity)
{
    public Vector3 Position;
    public Vector3 Velocity;

    public Model Model { get; set; }
    
    public void Tick(Game game)
    {
        Position += Velocity;
    }
}