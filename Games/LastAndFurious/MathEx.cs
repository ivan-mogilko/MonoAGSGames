﻿using System;

namespace LastAndFurious
{
    // TODO: suggest adding some of these into the engine.
    public static class MathEx
    {
        /// <summary>
        /// Converts angle to the range of 0-359 degrees
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static float Angle360(float degrees)
        {
            float angle = degrees % 360;
            if (angle >= 0) return angle;
            return 360 - (-angle);
        }

        // Converts angle to the range of [-Pi..+Pi], the angle must be in the range [-3*Pi, 3*Pi]
        public static float AnglePiFast(float rads)
        {
            if (rads > Math.PI)
                return (float)(rads - Math.PI * 2.0);
            else if (rads < -Math.PI)
                return (float)(rads + Math.PI * 2.0);
            return rads;
        }
    }
}