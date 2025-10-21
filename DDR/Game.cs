using System.Diagnostics;
using NLog;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DDR;

public class Game : GameWindow
{
    private static readonly Logger log = LogManager.GetCurrentClassLogger();
    public IResourceMannager ResourceMannager { get; private set; }
    public ModelLoader ModelLoader { get; private set; }
    public List<GameObject> WorldLayer = [];
    
    public Camera camera;
    public Player player;
    public Game()
        : base(GameWindowSettings.Default, new NativeWindowSettings()
        {
            Size = new Vector2i(800, 600), Title = "Game", Flags = ContextFlags.Default, Vsync = VSyncMode.On
        })
    {
        Resize += OnResize;
        ResourceMannager = new DevResourceMannager("../../../assets");
        ModelLoader = new ModelLoader(ResourceMannager);
        camera = new Camera()
        {
            Position = new Vector3((float)-3.3107831, (float)5.9009223, (float)-7.9504223)
        };
        player = new Player()
        {
            Model = ModelLoader.LoadVariantCs(ResourceLocation.ParseOrThrow("core:player")),
            Position = new Vector3(0, 1, 0),
            AABB = new AABB()
            {
                Min = new Vector3(-1, 0, -1),
                Max = new Vector3(1, 1, 1),
            },
            Velocity = new Vector3(0, 0, (float)0.01)
        };
    }
    
    public Shader WorldShader { get; private set; }
    protected override void OnLoad()
    {
        GL.Enable(EnableCap.DepthTest);
        
        WorldShader = new Shader(ResourceMannager.ReadToEndOrThrow(ResourceMannager["core:shaders/world/vertex.glsl"]), ResourceMannager.ReadToEndOrThrow(ResourceMannager["core:shaders/world/fragment.glsl"]), ResourceMannager);

        WorldLayer.Add(new StaticObject()
        {
            Position = new Vector3(0, 0, 0),
            Model = ModelLoader.Load(ResourceLocation.ParseOrThrow("core:world"))
        });
        
        CursorState = CursorState.Grabbed;
        UpdateFrequency = 30;
        base.OnLoad();
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

    private void Tick()
    {
        ticks++;
        player.Tick(this);

        if (ticks % 30 == 0)
        {
            player.state = "move";
        }
        if (ticks % 20 == 0)
        {
            player.state = "idle";
        }

        if (ticks % 10 == 0)
        {
            WorldLayer = WorldLayer.OrderBy(o => o.Model._vao).ToList();
        }
        
        camera.Position = player.Position + new Vector3((float)-3.3107831, (float)5.9009223, (float)-7.9504223);
    }
    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        Tick();
        
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

        var mouse = MousePosition;
        if (_firstMouse)
        {
            _lastMousePos = mouse;
            _firstMouse = false;
        }

        var xOffset = mouse.X - _lastMousePos.X;
        var yOffset = _lastMousePos.Y - mouse.Y;
        _lastMousePos = mouse;

        xOffset *= _sensitivity;
        yOffset *= _sensitivity;

        _yaw += xOffset;
        _pitch += yOffset;
        _pitch = Math.Clamp(_pitch, -89f, 89f);

        Vector3 front;
        front.X = MathF.Cos(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(_pitch));
        front.Z = MathF.Sin(MathHelper.DegreesToRadians(_yaw)) * MathF.Cos(MathHelper.DegreesToRadians(_pitch));
        camera.CameraFront = Vector3.Normalize(front);

        var velocity = _speed * (float)args.Time;
        if (KeyboardState.IsKeyDown(Keys.W)) camera.Position += camera.CameraFront * velocity;
        if (KeyboardState.IsKeyDown(Keys.S)) camera.Position -= camera.CameraFront * velocity;
        if (KeyboardState.IsKeyDown(Keys.A)) camera.Position -= Vector3.Normalize(Vector3.Cross(camera.CameraFront, camera.CameraUp)) * velocity;
        if (KeyboardState.IsKeyDown(Keys.D)) camera.Position += Vector3.Normalize(Vector3.Cross(camera.CameraFront, camera.CameraUp)) * velocity;
        
        if (KeyboardState.IsKeyDown(Keys.Space)) camera.Position += new Vector3(0, 1, 0) * velocity / 2;
        if (KeyboardState.IsKeyDown(Keys.LeftShift)) camera.Position += new Vector3(0, -1, 0) * velocity / 2;
        
        camera.UpdateCameraVectors(76.10004, -23.299994);
        
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


        Model _model = null;
        foreach (var GObject in WorldLayer)
        {
            
            var _color = new Vector3(255, 255, 255);
        
            var objectMatrix = Matrix4.CreateTranslation(0, 0, 0);
            GL.UniformMatrix4f(loc, 1, false, ref objectMatrix);
            GL.Uniform3f(world_position, 1, ref GObject.Position);
            GL.Uniform3f(color, 1, ref _color);
            
            if (GObject.Model != _model)
            {
                GL.BindVertexArray(GObject.Model._vao);
                _model = GObject.Model;
            }

            GL.DrawElements(PrimitiveType.Triangles, GObject.Model.indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        {
            var _color = new Vector3(255, 0, 0);
        
            var objectMatrix = Matrix4.CreateTranslation(0, 0, 0);
            GL.UniformMatrix4f(loc, 1, false, ref objectMatrix);
            GL.Uniform3f(world_position, 1, ref player.Position);
            GL.Uniform3f(color, 1, ref _color);
            
            var model = player.Model.Variants[player.state];
            
            GL.BindVertexArray(model._vao);

            GL.DrawElements(PrimitiveType.Triangles, model.indices.Length, DrawElementsType.UnsignedInt, 0);
        }
        
        SwapBuffers();
        base.OnRenderFrame(args);
    }
}