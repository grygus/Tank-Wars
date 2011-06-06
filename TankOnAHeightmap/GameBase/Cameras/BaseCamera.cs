using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameBase.Cameras
{
    public abstract class BaseCamera
    {
        // Perspective projection parameters
        float fovy;
        float aspectRatio;
        float nearPlane;
        float farPlane;

        // Position and target
        Vector3 position;
        Vector3 target;

        // orientation vectors
        Vector3 headingVec;
        Vector3 strafeVec;
        Vector3 upVec;

        // Matrices and updates
        protected bool needUpdateView;
        protected bool needUpdateProjection;
        protected bool needUpdateFrustum;
        protected Matrix viewMatrix;
        protected Matrix projectionMatrix;

        // Frustum
        BoundingFrustum frustum;

        #region Properties
        public float FovY
        {
            get
            {
                return fovy;
            }
            set
            {
                fovy = value;
                needUpdateProjection = true;
            }
        }

        public float AspectRatio
        {
            get
            {
                return aspectRatio;
            }
            set
            {
                aspectRatio = value;
                needUpdateProjection = true;
            }
        }

        public float NearPlane
        {
            get
            {
                return nearPlane;
            }
            set
            {
                nearPlane = value;
                needUpdateProjection = true;
            }
        }

        public float FarPlane
        {
            get
            {
                return farPlane;
            }
            set
            {
                farPlane = value;
                needUpdateProjection = true;
            }
        }

        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                SetLookAt(value, target, upVec);
            }
        }

        public Vector3 Target
        {
            get
            {
                return target;
            }
            set
            {
                SetLookAt(position, value, upVec);
            }
        }

        public Vector3 UpVector
        {
            get
            {
                return upVec;
            }
            set
            {
                SetLookAt(position, target, value);
            }
        }

        public Vector3 HeadingVector
        {
            get
            {
                return headingVec;
            }
        }

        public Vector3 RightVector
        {
            get
            {
                return strafeVec;
            }
        }

        public Matrix View
        {
            get
            {
                if (needUpdateView)
                    UpdateView();

                return viewMatrix;
            }
        }

        public Matrix Projection
        {
            get
            {
                if (needUpdateProjection)
                    UpdateProjection();

                return projectionMatrix;
            }
        }

        public BoundingFrustum Frustum
        {
            get
            {
                if (needUpdateView)
                    UpdateView();
                if (needUpdateProjection)
                    UpdateProjection();
                if (needUpdateFrustum)
                    UpdateFrustum();

                return frustum;
            }
        }
        #endregion

        public BaseCamera()
        {
            // Default camera configuration
            SetPerspectiveFov(45.0f, 1.0f, 0.1f, 10000.0f);
            SetLookAt(new Vector3(10.0f, 10.0f, 10.0f), Vector3.Zero, new Vector3(0.0f, 1.0f, 0.0f));

            needUpdateView = true;
            needUpdateProjection = true;
        }

        public void SetPerspectiveFov(float fovy, float aspectRatio, float nearPlane, float farPlane)
        {
            this.fovy = fovy;
            this.aspectRatio = aspectRatio;
            this.nearPlane = nearPlane;
            this.farPlane = farPlane;

            needUpdateProjection = true;
        }

        public void SetLookAt(Vector3 cameraPos, Vector3 cameraTarget, Vector3 cameraUp)
        {
            this.position = cameraPos;
            this.target = cameraTarget;
            this.upVec = cameraUp;

            headingVec = cameraTarget - cameraPos;
            headingVec.Normalize();
            upVec = cameraUp;
            strafeVec = Vector3.Cross(headingVec, upVec);

            needUpdateView = true;
        }

        protected virtual void UpdateView()
        {
            viewMatrix = Matrix.CreateLookAt(position, target, upVec);

            needUpdateView = false;
            needUpdateFrustum = true;
        }

        protected virtual void UpdateProjection()
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                        MathHelper.ToRadians(fovy), aspectRatio, nearPlane, farPlane);

            needUpdateProjection = false;
            needUpdateFrustum = true;
        }

        protected virtual void UpdateFrustum()
        {
            frustum = new BoundingFrustum(viewMatrix * projectionMatrix);

            needUpdateFrustum = false;
        }

        public abstract void Update(GameTime time);
    }

}
