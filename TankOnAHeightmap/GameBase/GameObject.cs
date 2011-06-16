using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameBase
{
    public abstract class GameObject : DrawableGameComponent
    {
        public Transformation Transformation;

        public abstract Vector3 Position { get; set; }
        public abstract BoundingBox BoundingBox { get;}

        public GameObject(Game game) 
            : base(game)
        {
            Transformation = new Transformation();
            
        }


    }
}
