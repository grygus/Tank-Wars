using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameBase.Materials
{
    public class LightMaterial
    {
        // Material properties (Diffuse and Specular)
        Vector3 diffuseColor;
        Vector3 specularColor;
        float specularPower;

        #region Properties
        public Vector3 DiffuseColor
        {
            get
            {
                return diffuseColor;
            }
            set
            {
                diffuseColor = value;
            }
        }

        public Vector3 SpecularColor
        {
            get
            {
                return specularColor;
            }
            set
            {
                specularColor = value;
            }
        }

        public float SpecularPower
        {
            get
            {
                return specularPower;
            }
            set
            {
                specularPower = value;
            }
        }
        #endregion

        public LightMaterial()
            : this(new Vector3(0.8f), new Vector3(0.3f), 32.0f)
        {
        }

        public LightMaterial(Vector3 diffuseColor, Vector3 specularColor, float specularPower)
        {
            this.diffuseColor = diffuseColor;
            this.specularColor = specularColor;
            this.specularPower = specularPower;
        }
    }
}
