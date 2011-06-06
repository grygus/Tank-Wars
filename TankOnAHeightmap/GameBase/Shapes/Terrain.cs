using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TanksOnAHeightmap.GameBase.Lights;
using TanksOnAHeightmap.GameBase.Effects;
using TanksOnAHeightmap.GameBase.Materials;


namespace TanksOnAHeightmap.GameBase.Shapes
{

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Terrain : Microsoft.Xna.Framework.DrawableGameComponent
    {
        #region Constants
        Model terrain;
        public Model Model
        {
            get { return terrain; }
        }

        HeightMapInfo heightMapInfo;
        ContentManager content;

        Matrix viewMatrix;
        Matrix projectionMatrix;

        GraphicsDeviceManager graphics;

        TerrainEffect effect;
        TerrainMaterial terrainMaterial;

        // Necessary services
        bool isInitialized;
        //CameraManager cameraManager;
        LightManager lightManager;
        #endregion

        public HeightMapInfo MapInfo
        {
            get { return heightMapInfo; }
        }
        public TerrainMaterial Material
        {
            get
            {
                return terrainMaterial;
            }
            set
            {
                terrainMaterial = value;
            }
        }
        public Terrain(Game game, ContentManager content, GraphicsDeviceManager graphics)
            : base(game)
        {
            // TODO: Construct any child components here
            this.content = content;
            this.graphics = graphics;
        }



        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            //cameraManager = Game.Services.GetService(typeof(CameraManager)) as CameraManager;
            lightManager = Game.Services.GetService(typeof(LightManager)) as LightManager;

            //if (cameraManager == null || lightManager == null)
            if (lightManager == null)
                throw new InvalidOperationException();

            // TODO: Add your initialization code here
            viewMatrix = Matrix.Identity;
            projectionMatrix = Matrix.Identity;

            base.Initialize();
        }

        protected override void LoadContent()
        {

            terrain = content.Load<Model>("terrain");

            // The terrain processor attached a HeightMapInfo to the terrain model's
            // Tag. We'll save that to a member variable now, and use it to
            // calculate the terrain's heights later.
            heightMapInfo = terrain.Tag as HeightMapInfo;
            if (heightMapInfo == null)
            {
                string message = "The terrain model did not have a HeightMapInfo " +
                    "object attached. Are you sure you are using the " +
                    "TerrainProcessor?";
                throw new InvalidOperationException(message);


            }

            // Load effect
            effect = new TerrainEffect(Game.Content.Load<Effect>(TerrainEffect.EFFECT_FILENAME));
            terrainMaterial = new TerrainMaterial();

            base.LoadContent();
        }

        private void SetEffectMaterial()
        {
            // Get the first two lights from the light manager
            PointLight light0 = PointLight.NoLight;
            PointLight light1 = PointLight.NoLight;
            if (lightManager.Count > 0)
            {
                light0 = lightManager[0] as PointLight;
                if (lightManager.Count > 1)
                    light1 = lightManager[1] as PointLight;
            }

            // Lights
            effect.AmbientLightColor = lightManager.AmbientLightColor;
            effect.Light1Position = light0.Position;
            effect.Light1Color = light0.Color;
            effect.Light2Position = light1.Position;
            effect.Light2Color = light1.Color;

            // Material
            effect.DiffuseColor = terrainMaterial.LightMaterial.DiffuseColor;
            effect.SpecularColor = terrainMaterial.LightMaterial.SpecularColor;
            effect.SpecularPower = terrainMaterial.LightMaterial.SpecularPower;
            // Textures
            effect.DiffuseTexture1 = terrainMaterial.DiffuseTexture1.Texture;
            effect.DiffuseTexture2 = terrainMaterial.DiffuseTexture2.Texture;
            effect.DiffuseTexture3 = terrainMaterial.DiffuseTexture3.Texture;
            effect.DiffuseTexture4 = terrainMaterial.DiffuseTexture4.Texture;
            effect.NormalMapTexture = terrainMaterial.NormalMapTexture.Texture;
            effect.AlphaMapTexture = terrainMaterial.AlphaMapTexture.Texture;
            // Texturas UVs
            effect.TextureUV1Tile = terrainMaterial.DiffuseTexture1.UVTile;
            effect.TextureUV2Tile = terrainMaterial.DiffuseTexture2.UVTile;
            effect.TextureUV3Tile = terrainMaterial.DiffuseTexture3.UVTile;
            effect.TextureUV4Tile = terrainMaterial.DiffuseTexture4.UVTile;
            effect.TextureUVBumpTile = terrainMaterial.NormalMapTexture.UVTile;
            // Camera and world transformations
            effect.World = Matrix.Identity;//transformation.Matrix;
            effect.View = viewMatrix;
            effect.Projection = projectionMatrix;

            effect.SpecularColor = new Vector3(0.0f, 0.0f, 0.1f);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;



            //device.SamplerStates.
            //device.RenderState.AlphaBlendEnable = false
            //device.Clear(ClearOptions.DepthBuffer,Color.Black,100.0f,200);
            //device.Clear(Color.Black);
            //device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);
            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            //GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            //Matrix[] boneTransforms = new Matrix[terrain.Bones.Count];
            //terrain.CopyAbsoluteBoneTransformsTo(boneTransforms);
            //BasicEffect ef = new BasicEffect();
            Vector3 lightDirection = Vector3.Normalize(new Vector3(3, -1, 1));
            Vector3 lightColor = new Vector3(0.3f, 0.4f, 0.2f);

            // Time is scaled down to make things wave in the wind more slowly.
            float time = (float)gameTime.TotalGameTime.TotalSeconds * 0.333f;

            SetEffectMaterial();

            effect.Begin();
            foreach (ModelMesh mesh in terrain.Meshes)
            {
                if (mesh.Name != "Billboards")
                {
                    foreach (BasicEffect effecta in mesh.Effects)
                    {
                        effecta.World = Matrix.Identity;
                        effecta.View = viewMatrix;
                        effecta.Projection = projectionMatrix;

                        effecta.EnableDefaultLighting();
                        effecta.PreferPerPixelLighting = true;

                        // Set the fog to match the black background color
                        effecta.FogEnabled = true;
                        effecta.FogColor = new Vector3(0.5f, 0.5f, 0.8f);
                        effecta.FogStart = 1000;
                        effecta.FogEnd = 3200;

                    }
                    mesh.Draw();
                }

            }

            if (MapInfo.Vegetation)
            {
                foreach (ModelMesh mesh in terrain.Meshes)
                {

                    if (mesh.Name == "Billboards")
                    {
                        // First pass renders opaque pixels.
                        foreach (Effect effecta in mesh.Effects)
                        {
                            effecta.Parameters["View"].SetValue(viewMatrix);
                            effecta.Parameters["Projection"].SetValue(projectionMatrix);
                            effecta.Parameters["LightDirection"].SetValue(lightDirection);
                            effecta.Parameters["WindTime"].SetValue(time);
                            effecta.Parameters["AlphaTestDirection"].SetValue(1f);
                        }

                        device.BlendState = BlendState.Opaque;
                        device.DepthStencilState = DepthStencilState.Default;
                        device.RasterizerState = RasterizerState.CullNone;
                        device.SamplerStates[0] = SamplerState.LinearClamp;

                        mesh.Draw();

                        // Second pass renders the alpha blended fringe pixels.
                        foreach (Effect effecta in mesh.Effects)
                        {
                            effecta.Parameters["AlphaTestDirection"].SetValue(-1f);
                        }

                        device.BlendState = BlendState.NonPremultiplied;
                        device.DepthStencilState = DepthStencilState.DepthRead;

                        mesh.Draw();
                    }
                }
            }
            //foreach (ModelMesh mesh in terrain.Meshes)
            //{
            //    foreach (ModelMeshPart part in mesh.MeshParts)
            //    {

            //        //GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.NumVertices, 0, part.PrimitiveCount);
            //    }
            //}

            //foreach (ModelMesh mesh in terrain.Meshes)
            //{
            //effect.Begin();
            //foreach (EffectPass pass in effect.CurrentTechniquePasses)
            //{
            //pass.Apply();
            // Draw the mesh
            //GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, numTriangles);
            //terrain.Draw(Matrix.Identity, viewMatrix, projectionMatrix);
            //terrain.Meshes[0].Draw();
            // pass.End();
            //}
            //effect.End();
            //}
            base.Draw(gameTime);
        }


        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        public float? Intersects(Ray ray)
        {

            float? collisionDistance = null;
            Vector3 rayStep = ray.Direction * heightMapInfo.TerrainScale * 0.5f;
            Vector3 rayStartPosition = ray.Position;
            Vector3 tmpNormal;
            // Linear search. Loop until find a point inside the terrain
            Vector3 lastRayPosition = ray.Position;
            ray.Position += rayStep;
            float height;
            heightMapInfo.GetHeightAndNormal(ray.Position, out height, out tmpNormal);

            while (ray.Position.Y > height && height >= 0)
            {
                lastRayPosition = ray.Position;
                ray.Position += rayStep;
                heightMapInfo.GetHeightAndNormal(ray.Position, out height, out tmpNormal);
            }

            // If the ray collide with the terrain
            if (height >= 0)
            {
                Vector3 startPosition = lastRayPosition;
                Vector3 endPosition = ray.Position;
                // Binary search. Find the exact collision point
                for (int i = 0; i < 32; i++)
                {
                    // Bynary search pass
                    Vector3 middlePoint = (startPosition + endPosition) * 0.5f;
                    if (middlePoint.Y < height)
                        endPosition = middlePoint;
                    else
                        startPosition = middlePoint;
                }
                Vector3 collisionPoint = (startPosition + endPosition) * 0.5f;
                collisionDistance = Vector3.Distance(rayStartPosition, collisionPoint);
            }
            return collisionDistance;
        }

        /// <summary>
        /// Sets the camera view and projection matrices
        /// that will be used to draw this particle system.
        /// </summary>
        public void SetCamera(Matrix view, Matrix projection)
        {
            viewMatrix = view;
            projectionMatrix = projection;
        }
    }
}
