using DDR.Obejct;
using NLog;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DDR;

public class Player : GameObject, IColisedObject
{
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    public string state = "idle";
    public ModelVariant Model { get; set; }
    public AABB AABB { get; set; }
    public Matrix4 Transform = Matrix4.Identity;
    public Player() : base(new Vector3(), new Vector3())
    {
        
    }
    
    public void jump()
    {
        if (Position.Y > 1.01) return;
        Velocity += new Vector3(0, (9.81f * update_s)*21, 0);
        log.Info("Jump.");
    }

    public static readonly float update_s = (float)(33.333333333 / 1000);

    public void Tick(Game game)
    {
        var aabb = Detector.TransformAABB(AABB, Position);
        foreach (var gameObject in game.ObjectLayer.FindAll(o => o is IColisedObject))
        {
            if (gameObject is IColisedObject colisedObject)
            {
                if (Detector.TransformAABB(colisedObject.AABB, gameObject.Position).Intersects(aabb))
                {
                    log.Info("Game over!");
                    game.ObjectLayer.Clear();
                    game.WorldLayer.Clear();
                    game.Gen();
                }
            }
        }
        
        
        if (Position.Y > 1.01)
        {
            Velocity -= new Vector3(0, 9.81f * update_s, 0);
        }
        else
        {
            Velocity -= Velocity * update_s;
        }
        Position += Velocity * update_s;
        if (Position.Y < 1f)
        {
            Position.Y = 1f;
            Velocity.Y = 0;
        }

        Position.X = Math.Max(Math.Min(Position.X, 5), -5);
    }

    public void Update(Game game)
    {
        if (game.KeyboardState.IsKeyPressed(Keys.Space))
        {
            jump();
        }

        if (game.KeyboardState.IsKeyDown(Keys.A))
        {
            Velocity += new Vector3((9.81f * update_s) * 1.2f, 0, 0);
        } else if (game.KeyboardState.IsKeyDown(Keys.D))
        {
            Velocity -= new Vector3((9.81f * update_s) * 1.2f, 0, 0);
        }
        base.Update(game);
    }
}