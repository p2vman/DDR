using OpenTK.Mathematics;

namespace DDR;

public class Camera
{
    public Vector3 Position { get; set; }
    public Vector3 CameraFront = -Vector3.UnitZ;
    public Vector3 CameraUp = Vector3.UnitY;
    
    
    public Camera()
    {
        
    }
    
    public void UpdateCameraVectors(float yaw, float pitch)
    {
        Vector3 front;
        front.X = MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
        front.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
        front.Z = MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
        CameraFront = Vector3.Normalize(front);
    }


    public void UpdateCameraVectors(double yaw, double pitch)
    {
        UpdateCameraVectors((float)yaw, (float)pitch);
    }
}