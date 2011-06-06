using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.Helpers
{
    public static class RandomHelper
    {
        public static Random RandomGenerator = new Random();

        public static Vector3 GeneratePositionXZ(int distance)
        {
            int scaledDistance = distance * 10;
            float posX = (RandomGenerator.Next(2 * scaledDistance + 1) - scaledDistance) * 0.1f;
            float posZ = (RandomGenerator.Next(2 * scaledDistance + 1) - scaledDistance) * 0.1f;

            return new Vector3(posX, 0, posZ);
        }
    }
}
