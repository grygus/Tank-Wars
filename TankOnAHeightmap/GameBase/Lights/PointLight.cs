using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameBase.Lights
{
    public class PointLight : BaseLight
    {
        public static PointLight NoLight = new PointLight(Vector3.One, Vector3.Zero);

        // Light position
        Vector3 position;

        #region Properties
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }
        #endregion

        public PointLight()
        {
        }

        public PointLight(Vector3 position, Vector3 color)
            : base(color)
        {
            this.position = position;
        }
    }
}
