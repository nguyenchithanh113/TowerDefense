using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace EcsExtension
{
    public static class MathHelper 
    {
        public static bool HasReachedDestination(this float value, float value1, float value2)
        {
            return value <= value1 || value <= value2;
        }
        
        public static quaternion RotateTowards(
            quaternion from,
            quaternion to,
            float maxDegreesDelta)
        {
            float num = Angle(from, to);
            return num < float.Epsilon ? to : math.slerp(from, to, math.min(1f, maxDegreesDelta / num));
        }
     
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(this quaternion q1, quaternion q2)
        {
            var dot    = math.dot(q1, q2);
            return !(dot > 0.999998986721039) ? (float) (math.acos(math.min(math.abs(dot), 1f)) * 2.0) : 0.0f;
        }
    }
}