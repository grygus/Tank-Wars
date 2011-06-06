using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameBase
{
    public class Transformation
    {
        // Translation, rotate and scale in the x, y, z axis
        Vector3 translate;
        Vector3 rotate;
        Vector3 scale;

        // Matrix handling
        bool needUpdate;
        Matrix matrix;

        #region Properties
        public Vector3 Translate
        {
            get
            {
                return translate;
            }
            set
            {
                translate = value;
                needUpdate = true;
            }
        }
        public Vector3 Rotate
        {
            get
            {
                return rotate;
            }
            set
            {
                rotate = value;
                needUpdate = true;
            }
        }
        public Vector3 Scale
        {
            get
            {
                return scale;
            }
            set
            {
                scale = value;
                needUpdate = true;
            }
        }
        public Matrix Matrix
        {
            get
            {
                if (needUpdate)
                {
                    matrix =
                        Matrix.CreateScale(scale) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(rotate.Y)) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(rotate.X)) *
                        Matrix.CreateRotationZ(MathHelper.ToRadians(rotate.Z)) *
                        Matrix.CreateTranslation(translate);

                    needUpdate = false;
                }

                return matrix;
            }
        }
        #endregion

        public Transformation()
            : this(Vector3.Zero, Vector3.Zero, Vector3.One)
        {
            matrix = Matrix.Identity;
            needUpdate = false;
        }

        public Transformation(Vector3 translate, Vector3 rotate, Vector3 scale)
        {
            this.translate = translate;
            this.rotate = rotate;
            this.scale = scale;

            needUpdate = true;
            matrix = Matrix.Identity;
        }
    }
}
