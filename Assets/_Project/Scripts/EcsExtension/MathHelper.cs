namespace EcsExtension
{
    public static class MathHelper 
    {
        public static bool HasReachedDestination(this float value, params float[] compareValues)
        {
            if (compareValues == null || compareValues.Length == 0) return false;

            for (var i = 0; i < compareValues.Length; i++)
            {
                var val = compareValues[i];
                if (value <= val)
                {
                    return true;
                }
            }

            return false;
        }
    }
}