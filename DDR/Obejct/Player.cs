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
    public Player() : base(new Vector3(), new Vector3())
    {
        
    }

    public void jump()
    {
        log.Info("Jump.");
    }

    public void Tick(Game game)
    {
        if (game.KeyboardState.IsKeyPressed(Keys.Space))
        {
            jump();
        }
        base.Tick(game);
    }
    
}