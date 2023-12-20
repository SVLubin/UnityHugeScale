using UnityEngine;

namespace HugeScale.RenderSystems
{
    public static class Converter
    {
        public static float MeterToUnit(double coordinate, double ctu) =>
            (float)(coordinate * ctu);
        
        public static Vector3 MeterToUnit(Vector3d coordinate, double ctu) => 
            (coordinate * ctu).ToVector3();
        
        public static double UnitToMeter(double unit, double ctu) => 
            unit / ctu;
        
        public static Vector3d UnitToMeter(Vector3 unit, double ctu) => 
            new Vector3d(unit) * ctu;
    }
}