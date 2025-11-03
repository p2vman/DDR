using System.Diagnostics;
using DDR.Model;
using DDR.Obejct;
using NLog;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DDR;

public class Game : GameWindow
{
    Random random = new Random();
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    public IResourceMannager ResourceMannager { get; private set; }
    public ModelLoader ModelLoader { get; private set; }
    public List<GameObject> WorldLayer = [];
    public List<GameObject> ObjectLayer = [];

    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    public Camera camera;
    public Player player;
    
    public Game()
        : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            Size = new Vector2i(800, 600), 
            Title = "Game", 
            Flags = ContextFlags.Default, 
            Vsync = VSyncMode.On,
            DepthBits = 24
        })
    {
        Resize += OnResize;
        ResourceMannager = new DevResourceMannager("./assets");
        ModelLoader = new ModelLoader(ResourceMannager);
        camera = new Camera()
        {
            Position = new Vector3((float)-3.3107831, (float)5.9009223, (float)-7.9504223)
        };
        player = new Player()
        {
            Model = ModelLoader.LoadVariant(ResourceLocation.ParseOrThrow("core:player")),
            Position = new Vector3(0, 1, 0),
            AABB = new AABB()
            {
                Min = new Vector3(0.2f, 0, 00.1f),
                Max = new Vector3(0.8f, 0.5f, 0.8f),
            },
            Velocity = new Vector3(0, 0, 0)
        };
        UpdateFrequency = 30;
    }

    public void UpdateCactusFlow()
    {
        int cactusAhead = ObjectLayer.Count(o => o is Cactus && o.Position.Z > player.Position.Z);
        
        if (cactusAhead < 100)
        {
            int need = 100 - cactusAhead;

            for (int i = 0; i < need; i++)
            {
                float x = random.NextSingle() * 10f - 5f;
                float z = player.Position.Z + 30f + random.NextSingle() * 80f;

                var pos = new Vector3(x, 1f, z);

                var cactus = new Cactus(Cactus.CactusType.C_0, pos)
                {
                    Model = ModelLoader.LoadVariant(ResourceLocation.ParseOrThrow("core:cactus")),
                    Velocity = u + 0
                };

                var keys = ((ModelVariant)cactus.Model).Variants.Keys;
                cactus.State = keys.ElementAt(random.Next(keys.Count));
                cactus.AABB = Model.Model.ComputeAABB(cactus.Model.GetModel(cactus.State).vertices);

                ObjectLayer.Add(cactus);
            }
        }
    }



    
    public Shader WorldShader { get; private set; }
    protected override void OnLoad()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(TriangleFace.Back);
        GL.FrontFace(FrontFaceDirection.Ccw);
        
        WorldShader = new Shader(ResourceMannager.ReadToEndOrThrow(ResourceMannager["core:shaders/world/vertex.glsl"]), ResourceMannager.ReadToEndOrThrow(ResourceMannager["core:shaders/world/fragment.glsl"]), ResourceMannager);
        
        Gen();
        
        CursorState = CursorState.Grabbed;
        base.OnLoad();
    }

    public void Gen()
    {
        u = new Vector3(0, 0, -(0.1f * 16f));
        //WorldLayer.Add(new StaticObject()
        //{
        //    Position = new Vector3(0, 0, 0),
        //    Model = ModelLoader.Load(ResourceLocation.ParseOrThrow("core:world"))
        //});
        ObjectLayer.Add(new Detector(new Vector3(0, 1, -4),
            new AABB(new Vector3(-9, 0, 0), new Vector3(4, 18, (float)0.5)),
            o =>
            {
            //    o.Position += new Vector3(random.Next(4)-2, 0, 32+random.Next(25));
            //    var keys = ((ModelVariant)o.Model).Variants.Keys;
            //    o.State = keys.ElementAt(random.Next(keys.Count));
            //    o.Position.X = Math.Max(Math.Min(o.Position.X, 5), -5);
            ObjectLayer.Remove(o);
            }));
    }
    
    private static void OnResize(ResizeEventArgs e)
    {
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    private float _pitch = 0;
    private float _yaw = 0;
    private bool _firstMouse = true;
    private float _speed = 12.5f;
    private float _sensitivity = 0.1f;
    private Vector2 _lastMousePos;
    private long ticks = 0;

    public Vector3 u = new Vector3(0, 0, -(0.1f * 16f));
    private void Tick()
    {
        
        ticks++;
        player.Tick(this);
        UpdateCactusFlow();

        if (ticks % 30 == 0)
        {
            //player.state = "move";
        }
        if (ticks % 20 == 0)
        {
            player.state = "idle";
        }

        camera.Position = Vector3.Lerp(camera.Position, player.Position + new Vector3(0.5f, 3, -3),  5f * (float)16/1000);
        //camera.Position = player.Position + new Vector3(0.5f, 12, -12);

        foreach (var o in ObjectLayer.ToArray())
        {
            o.Tick(this);
        }


        if (ticks % 140 == 0)
        {
            u -= new Vector3(0, 00, (0.1f*16f) * 0.6f);
            ObjectLayer.ForEach(o =>
            {
                if (o is Cactus)
                {
                    o.Velocity = u + 0;
                }
            });
            
        }
    }
    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        
        if (KeyboardState.IsKeyPressed(Keys.Escape))
        {
            CursorState = CursorState.Normal;
            _firstMouse = true;
        }
        if (MouseState.IsButtonPressed(MouseButton.Left))
        {
            CursorState = CursorState.Grabbed;
            _firstMouse = true;
        }

        if (CursorState != CursorState.Grabbed)
            return;
        
        player.Update(this);
        WorldLayer.ForEach(o =>
        {
            o.Update(this);
        });
    
        Tick(); 

        var mouse = MousePosition;
        if (_firstMouse)
        {
            _lastMousePos = mouse;
            _firstMouse = false;
        }

        
        
        camera.UpdateCameraVectors(90.399994, -23.899998);
        //camera.UpdateCameraVectors(_yaw, _pitch);
        //log.Info(_yaw + ":" + _pitch);
        
        base.OnUpdateFrame(args);
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        //camera.Position = player.Position + new Vector3(-1, 2, 0);
        
        WorldShader.Use();
        
        var view = Matrix4.LookAt(camera.Position, camera.Position + camera.CameraFront, camera.CameraUp);
        var proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);
        
        WorldShader.SetMatrix4("view", view);
        WorldShader.SetMatrix4("projection", proj);
        WorldShader.SetVector3("cam", camera.Position);
        
        var loc = GL.GetUniformLocation(WorldShader.Handle, "model");
        var world_position = GL.GetUniformLocation(WorldShader.Handle, "world_position");
        var color = GL.GetUniformLocation(WorldShader.Handle, "color");


        IModel _model = null;
        int hash;
        foreach (var GObject in WorldLayer)
        {
            var _color = new Vector3(1, 1, 1);
            
        
            var objectMatrix = Matrix4.CreateTranslation(0, 0, 0);
            GL.UniformMatrix4f(loc, 1, false, ref objectMatrix);
            GL.Uniform3f(world_position, 1, ref GObject.Position);
            GL.Uniform3f(color, 1, ref _color);
            
            if (GObject.Model != _model)
            {
                GL.BindVertexArray(GObject.Model.GetModel(GObject.State)._vao);
                _model = GObject.Model.GetModel(GObject.State);
            }

            GL.DrawElements(PrimitiveType.LineStrip, _model.indices.Length, DrawElementsType.UnsignedInt, 0);
        }
        
        foreach (var GObject in ObjectLayer)
        {

            if (GObject.Position.Length < 200)
            {
                hash = Math.Abs(GObject.GetHashCode());
                var g = 0.5f + (hash % 128) / 255f;
                var r = (hash >> 4 & 0x0F) / 255f;
                var b = (hash >> 8 & 0x0F) / 255f;
                var _color = new Vector3(r, g, b);
        
                var objectMatrix = Matrix4.CreateTranslation(0, 0, 0);
                GL.UniformMatrix4f(loc, 1, false, ref objectMatrix);
                GL.Uniform3f(world_position, 1, ref GObject.Position);
                GL.Uniform3f(color, 1, ref _color);
            
                if (GObject.Model != _model)
                {
                    GL.BindVertexArray(GObject.Model.GetModel(GObject.State)._vao);
                    _model = GObject.Model.GetModel(GObject.State);
                }

                GL.DrawElements(PrimitiveType.LineStrip, _model.indices.Length, DrawElementsType.UnsignedInt, 0);

                //_color = new Vector3(200, 0, 0);
                //GL.Uniform3f(color, 1, ref _color);
                //if (GObject is IColisedObject o)
                //{
                //    var mod = Model.Model.FromAabb(o.AABB);
                //    GL.BindVertexArray(mod._vao);
                //    GL.DrawElements(PrimitiveType.LineStrip, mod.indices.Length, DrawElementsType.UnsignedInt, 0);
                //}
            }
        }

        {
            var _color = new Vector3(2, 200, 8);

            GL.UniformMatrix4f(loc, 1, false, ref player.Transform);
            GL.Uniform3f(world_position, 1, ref player.Position);
            GL.Uniform3f(color, 1, ref _color);

            var model = player.Model.Variants[player.state];

            GL.BindVertexArray(model._vao);

            GL.DrawElements(PrimitiveType.LineStrip, model.indices.Length, DrawElementsType.UnsignedInt, 0);

            //_color = new Vector3(200, 0, 0);
            //GL.Uniform3f(color, 1, ref _color);
            //var mod = Model.Model.FromAabb(player.AABB);
            //GL.BindVertexArray(mod._vao);
            //GL.DrawElements(PrimitiveType.LineStrip, mod.indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        SwapBuffers();
        base.OnRenderFrame(args);
    }
}