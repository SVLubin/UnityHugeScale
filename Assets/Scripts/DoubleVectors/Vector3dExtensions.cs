namespace UnityEngine
{
    public static class Vector3dExtensions
    {
        public static Vector3 ToVector3(this Vector3d vector3d) =>
            new Vector3((float)vector3d.x, (float)vector3d.y, (float)vector3d.z);
    }
}