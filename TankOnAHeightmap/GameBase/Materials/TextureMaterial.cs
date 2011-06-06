using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TanksOnAHeightmap.GameBase.Materials
{
    public class TextureMaterial
    {
        // Texture
        Texture2D texture;
        // Texture UV tile
        Vector2 uvTile;

        #region Properties
        public Texture2D Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
            }
        }

        public Vector2 UVTile
        {
            get
            {
                return uvTile;
            }
            set
            {
                uvTile = value;
            }
        }
        #endregion

        public TextureMaterial()
        {
        }

        public TextureMaterial(Texture2D texture, Vector2 uvTile)
        {
            this.texture = texture;
            this.uvTile = uvTile;
        }
    }
}
