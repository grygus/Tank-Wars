using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameBase.Lights
{
    public class LightManager
    {
        // Global ambient light
        Vector3 ambientLightColor;

        // Sorted list containing all lights
        SortedList<string, BaseLight> lights;

        #region Properties
        public Vector3 AmbientLightColor
        {
            get
            {
                return ambientLightColor;
            }
            set
            {
                ambientLightColor = value;
            }
        }

        public BaseLight this[int index]
        {
            get
            {
                return lights.Values[index];
            }
        }

        public BaseLight this[string id]
        {
            get
            {
                return lights[id];
            }
        }

        public int Count
        {
            get
            {
                return lights.Count;
            }
        }
        #endregion

        public LightManager()
            : this(Vector3.Zero)
        {
        }

        public LightManager(Vector3 ambientLightColor)
        {
            this.ambientLightColor = ambientLightColor;
            lights = new SortedList<string, BaseLight>();
        }

        public void Clear()
        {
            lights.Clear();
        }

        public void Add(string id, BaseLight light)
        {
            lights.Add(id, light);
        }

        public void Remove(string id)
        {
            lights.Remove(id);
        }
    }
}
