using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework.Graphics;

namespace TanksOnAHeightmap.GameBase.Materials
{
    public class TerrainMaterial
    {
        // Surface material
        LightMaterial lightMaterial;

        // Diffuse Textures
        TextureMaterial diffuseTexture1;
        TextureMaterial diffuseTexture2;
        TextureMaterial diffuseTexture3;
        TextureMaterial diffuseTexture4;
        TextureMaterial alphaMapTexture;

        // Normal map
        TextureMaterial normalMapTexture;

        #region Properties
    public LightMaterial LightMaterial
    {
        get
        {
            return lightMaterial;
        }
        set
        {
            lightMaterial = value;
        }
    }

    public TextureMaterial DiffuseTexture1
    {
        get
        {
            return diffuseTexture1;
        }
        set
        {
            diffuseTexture1 = value;
        }
    }

    public TextureMaterial DiffuseTexture2
    {
        get
        {
            return diffuseTexture2;
        }
        set
        {
            diffuseTexture2 = value;
        }
    }

    public TextureMaterial DiffuseTexture3
    {
        get
        {
            return diffuseTexture3;
        }
        set
        {
            diffuseTexture3 = value;
        }
    }

    public TextureMaterial DiffuseTexture4
    {
        get
        {
            return diffuseTexture4;
        }
        set
        {
            diffuseTexture4 = value;
        }
    }

    public TextureMaterial AlphaMapTexture
    {
        get
        {
            return alphaMapTexture;
        }
        set
        {
            alphaMapTexture = value;
        }
    }

    public TextureMaterial NormalMapTexture
    {
        get
        {
            return normalMapTexture;
        }
        set
        {
            normalMapTexture = value;
        }
    }
        #endregion

        public TerrainMaterial()
        {
        }
    }
}
