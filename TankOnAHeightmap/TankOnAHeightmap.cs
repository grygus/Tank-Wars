#region File Description
//-----------------------------------------------------------------------------
// TanksOnAHeightmap.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TanksOnAHeightmap.GameBase.Cameras;
using TanksOnAHeightmap.GameBase.Effects;
using System.Collections.Generic;
using TanksOnAHeightmap.GameBase.Effects.ParticleSystems;
using TanksOnAHeightmap.GameBase.Shapes;
using TanksOnAHeightmap.GameBase.Lights;
using TanksOnAHeightmap.GameBase.Materials;
using TanksOnAHeightmap.GameLogic;
using LTreesLibrary.Trees;
using TanksOnAHeightmap.GameBase.Helpers;
using TanksOnAHeightmap.Helpers.Drawing;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.DataStructures;
using TanksOnAHeightmap.GameBase.Physics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
#endregion

namespace TanksOnAHeightmap
{
    /// <summary>
    /// Sample showing how to use get the height of a programmatically generated
    /// heightmap.
    /// </summary>
    public class TanksOnAHeightmapGame : Microsoft.Xna.Framework.Game
    {
        #region Constants

        // This vector controls how much the camera's position is offset from the
        // tank. This value can be changed to move the camera further away from or
        // closer to the tank.
        readonly Vector3 CameraPositionOffset = new Vector3(0, 40, 150);

        // This value controls the point the camera will aim at. This value is an offset
        // from the tank's position.
        readonly Vector3 CameraTargetOffset = new Vector3(0, 30, 0);



        //Particles
        ParticleSystem explosionParticles;
        ParticleSystem explosionSmokeParticles;
        ParticleSystem projectileTrailParticles;

        // The explosions effect works by firing projectiles up into the
        // air, so we need to keep track of all the active projectiles.
        List<Projectile> projectiles = new List<Projectile>();

        TimeSpan timeToNextProjectile = TimeSpan.Zero;
        // Random number generator for the fire effect.
        Random random = new Random();

        const int NUMBER_OF_ENEMYS = 8;
        const int NUMBER_OF_TREES = 4;
        #endregion

        #region Fields
        GraphicsDeviceManager graphics;
        //Bloom bloom;

        TanksOnAHeightmap.GameBase.Shapes.Terrain terrain;
        Player player;
        Enemy[] badGuys;
        Trees[] trees;
        //HeightMapInfo heightMapInfo;
        ChaseCamera camera;
        LightManager lightManager;
        Song music;

        #region Fields Physic

        public Physics Physic;
        Space space;
        public Matrix viewMatrix;
        public Matrix projectionMatrix;

        #endregion

        //Sky
        Texture2D cloudMap;
        Model skyDome;
        Effect effect;

        Skybox skybox;

        GraphicsDevice device;

        Random rand;
        InputHelper inputHelper;
        GameScreen gameScreen;
        #endregion


        #region Initialization

        private TextureMaterial LoadTextureMaterial(string textureFilename, Vector2 tile)
        {
            Texture2D texture = Content.Load<Texture2D>(GameAssetsPath.TEXTURES_PATH + textureFilename);
            return new TextureMaterial(texture, tile);
        }
        public TanksOnAHeightmapGame()
        {
            graphics = new GraphicsDeviceManager(this);
            device = GraphicsDevice;
            Content.RootDirectory = "Content";
            
            music = Content.Load<Song>("Sound/windbell");
            MediaPlayer.IsMuted = true;
            
            // Light Manager
            lightManager = new LightManager();
            lightManager.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
            Services.AddService(typeof(LightManager), lightManager);
            // Light 1
            lightManager.Add("Light1", new PointLight(new Vector3(0, 1000.0f, 0), Vector3.One));

            //tank = new Tank(this, Content, graphics);
            terrain = new TanksOnAHeightmap.GameBase.Shapes.Terrain(this, Content, graphics);


            //InputHelper

            inputHelper = new InputHelper(PlayerIndex.One);
            Services.AddService(typeof(InputHelper), inputHelper);

            // Create the chase camera
            camera = new ChaseCamera(this);

            // Set the camera offsets
            //camera.DesiredPositionOffset = new Vector3(0.0f, 600.0f, 800.0f);//150,150
            //camera.LookAtOffset = new Vector3(0.0f, 80.0f, 0.0f);//80

            camera.DesiredPositionOffset = new Vector3(0.0f, 45.0f, 20.0f);//150,150
            camera.LookAtOffset = new Vector3(0.0f, 35.0f, -50.0f);//80

            // Set camera perspective
            camera.NearPlaneDistance = 10.0f;
            camera.FarPlaneDistance = 4000.0f;


            this.Services.AddService(typeof(TanksOnAHeightmap.GameBase.Shapes.Terrain), terrain);
            this.Services.AddService(typeof(ChaseCamera), camera);


            rand = new Random(0);

            // Construct our particle system components.
            explosionParticles = new ExplosionParticleSystem(this, Content);
            ParticleSettings settings = new ParticleSettings();


            this.Services.AddService(typeof(ExplosionParticleSystem), explosionParticles);

            explosionSmokeParticles = new ExplosionSmokeParticleSystem(this, Content);

            this.Services.AddService(typeof(ExplosionSmokeParticleSystem), explosionSmokeParticles);

            projectileTrailParticles = new ProjectileTrailParticleSystem(this, Content);

            //Trees
            trees = new Trees[NUMBER_OF_TREES];
            for (int i = 0; i < trees.Length; i++)
            {
                trees[i] = new Trees(this, Content, graphics);
                //Components.Add(trees[i]);
            }

            explosionSmokeParticles.DrawOrder = 200;
            projectileTrailParticles.DrawOrder = 300;
            explosionParticles.DrawOrder = 400;
            terrain.DrawOrder = 102;



            //Trees

            this.graphics.PreferredBackBufferWidth = 1024;
            this.graphics.PreferredBackBufferHeight = 768;

            start();



        }
        private void start()
        {
            badGuys = new Enemy[NUMBER_OF_ENEMYS];


            player = new Player(this, Content, graphics, UnitTypes.PlayerType.TankPlayer);
            player.tank.setPosition(player.tank.Orientation.Translation + new Vector3(300, 0, 150));
            player.WorldTrees = trees;
            
            #region  Constructor Physic
            space = new Space();
            EnemyCannonBall.EnemyCannonBallCollisionGroup = new CollisionGroup();
            Enemy.EnemyTankCollisionGroup = new CollisionGroup();
            Physic = new Physics(this, player, Content, terrain, space);
            player.phisics = Physic;
            
            #endregion

            //World Buildings
            Building church = new Building(this, space
                , new Vector3(-2100, -40, 250)
                , new Box(new Vector3(-70, -210, 0), 685, 400, 365)
                , 15f
                , "Models/88cathedral"
                );
            //church.BlockModel = Content.Load<Model>("Models/88cathedral");
            church.DrawOrder = 102;
            Components.Add(church);

            Prey eagle = new Prey(this, space
                , new Vector3(-1200, -280, 300)
                , new Box(new Vector3(0, 10, 0), 60, 120, 160)
                , 2f
                , "Models/eagle-adler"
                );
            /*HardWall eagle_legs = new HardWall(this, space
                , Matrix.Identity
                , Matrix.CreateTranslation(new Vector3(-1210, -350, 300))
                , 1, 1, 50, 50, 65
                );*/
            //eagle.BlockModel = Content.Load<Model>("Models/eagle-adler");
            eagle.DrawOrder = 102;
            Components.Add(eagle);
            //
            player.healthManager = Physic.healthManager;
            player.Prey = eagle;
            player.Church = church;

            Vector3 temp;
            Components.Add(terrain);
            for (int i = 0; i < badGuys.Length; i++)
            {
                temp = new Vector3(((float)rand.NextDouble() - 0.5f) * 512 * 8, 0, ((float)rand.NextDouble() - 0.5f) * 512 * 8);
                badGuys[i] = new Enemy(this, Content, graphics, UnitTypes.EnemyType.TankEnemy, space, temp);
                badGuys[i].tank.setPosition(temp);
                badGuys[i].Player = player;
                badGuys[i].State = Enemy.EnemyState.Wander;
                badGuys[i].WorldTrees = trees;
                badGuys[i].phisics = Physic;
                badGuys[i].healthManager = Physic.healthManager;
                badGuys[i].Prey = eagle;
                badGuys[i].Church = church;
                //badGuys[i].DrawOrder = 102;
                Components.Add(badGuys[i]);
            }

            Enemy.Units = badGuys;
            //Enemy.DEBUG_ENEMY = true;
            player.EnemyList = badGuys;


            //tank.DrawOrder = 100;
            player.DrawOrder = 101;

            // Register the particle system components.
            //Components.Add(tank);


            Components.Add(player);
            Components.Add(explosionParticles);
            Components.Add(explosionSmokeParticles);
            Components.Add(projectileTrailParticles);
            for (int i = 0; i < trees.Length; i++)
            {
                trees[i].DrawOrder = 102;
                Components.Add(trees[i]);
            }
            // Game Screen
            //Components.Add(new GameScreen(this, player));
            gameScreen = new GameScreen(this, player);
            Components.Add(gameScreen);
        }
        protected override void Initialize()
        {
            this.IsMouseVisible = true;
            base.Initialize();
            // Set the camera aspect ratio
            // This must be done after the class to base.Initalize() which will
            // initialize the graphics device.
            Renderer.Initialize(this, graphics.GraphicsDevice);
            camera.AspectRatio = (float)graphics.GraphicsDevice.Viewport.Width /
                graphics.GraphicsDevice.Viewport.Height;
            // Perform an inital reset on the camera so that it starts at the resting
            // position. If we don't do this, the camera will start at the origin and
            // race across the world to get behind the chased object.
            // This is performed here because the aspect ratio is needed by Reset.
            UpdateCameraChaseTarget();
            camera.Reset();

            //Trees Initialization
            Vector3 normal;
            Vector3 position;
            float height;
            for (int i = 0; i < trees.Length; i++)
            {
                position = new Vector3(((float)rand.NextDouble() - 0.5f) * 512 * 8, 0, ((float)rand.NextDouble() - 0.5f) * 512 * 8);
                terrain.MapInfo.GetHeightAndNormal(position, out height, out normal);
                position.Y = height;
                trees[i].Position = position;

            }


        }

        Vector3 ModelPosition;
        float ModelRotation = 0.0f;
        Model Model;
        Model SkySphere;
        Effect SkySphereEffect;

        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            ///
            /// Physics Load Content
            ///

            Cannon.fire = Content.Load<SoundEffect>("Sound/fire");
            EnemyCannon.fire = Content.Load<SoundEffect>("Sound/fire");

            CannonBall.blast = Content.Load<SoundEffect>("Sound/blast");
            EnemyCannonBall.blast = Content.Load<SoundEffect>("Sound/blast");

            Physic.LoadContent();

            // Terrain material
            TerrainMaterial material = new TerrainMaterial();
            material.DiffuseTexture1 = LoadTextureMaterial("Terrain1", new Vector2(50, 50));
            material.DiffuseTexture2 = LoadTextureMaterial("Terrain2", new Vector2(50, 50));
            material.DiffuseTexture3 = LoadTextureMaterial("Terrain3", new Vector2(30, 30));
            material.DiffuseTexture4 = LoadTextureMaterial("Terrain4", Vector2.One);
            material.AlphaMapTexture = LoadTextureMaterial("AlphaMap", Vector2.One);
            material.NormalMapTexture = LoadTextureMaterial("Rockbump", new Vector2(196, 196));
            material.LightMaterial = new LightMaterial(new Vector3(0.8f), new Vector3(0.3f), 32);
            terrain.Material = material;


            //Load Sky Content
            effect = Content.Load<Effect>(GameAssetsPath.EFFECTS_PATH + "Series4Effects");
            skyDome = Content.Load<Model>("dome");
            skyDome.Meshes[0].MeshParts[0].Effect = effect.Clone();

            cloudMap = Content.Load<Texture2D>("cloudMap");

            skybox = new Skybox(GameAssetsPath.TEXTURES_PATH + "t", Content);
            /*
            //skysphere
           // Load the effect, the texture it uses, and 
           // the model used for drawing it
           SkySphereEffect = Content.Load<Effect>(GameAssetsPath.EFFECTS_PATH +"SkySphere");
           TextureCube SkyboxTexture =
               Content.Load<TextureCube>(GameAssetsPath.TEXTURES_PATH + "t");
           SkySphere = Content.Load<Model>(GameAssetsPath.MODELS_PATH + "SphereHighPoly");

           // Set the parameters of the effect
           SkySphereEffect.Parameters["ViewMatrix"].SetValue(
               camera.View);
           SkySphereEffect.Parameters["ProjectionMatrix"].SetValue(
               camera.Projection);
           SkySphereEffect.Parameters["SkyboxTexture"].SetValue(
               SkyboxTexture);
           // Set the Skysphere Effect to each part of the Skysphere model
           foreach (ModelMesh mesh in SkySphere.Meshes)
           {
               foreach (ModelMeshPart part in mesh.MeshParts)
               {
                   part.Effect = SkySphereEffect;
               }
           }*/

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(music);
            MediaPlayer.Volume = 0.5f;
        }

        #endregion



        #region Update and Draw

        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {



            inputHelper.Update();
            // Update the camera to chase the new target
            UpdateCameraChaseTarget();
            camera.Update(gameTime);
            base.Update(gameTime);
            if (player.IsDead || player.NumEnemiesAlive == 0)
            {
                Components.Clear();

            }
            HandleInput(gameTime);
            UpdateProjectiles(gameTime);
            explosionParticles.Update(gameTime);

            #region Update Physic

            // needed to draw cannonballs
            viewMatrix = camera.View;
            projectionMatrix = camera.Projection;
            Physic.Update(gameTime);

            #endregion
        }

        /// <summary>
        /// Helper for updating the explosions effect.
        /// </summary>
        void UpdateExplosions(GameTime gameTime)
        {
            timeToNextProjectile -= gameTime.ElapsedGameTime;

            Vector3 Position = Vector3.Transform(player.tank.cannonBone.Transform.Translation + player.tank.turretBone.Transform.Translation,
                                                                                                        player.tank.WorldMatrix);

            Vector3 initialVelocity = 1000 * Vector3.Transform(player.tank.cannonBone.Transform.Forward + player.tank.turretBone.Transform.Forward,
                                                                                                        player.tank.Orientation);

            if (timeToNextProjectile <= TimeSpan.Zero)
            {
                // Create a new projectile once per second. The real work of moving
                // and creating particles is handled inside the Projectile class.
                projectiles.Add(new Projectile(explosionParticles,
                                               explosionSmokeParticles,
                                               projectileTrailParticles,
                                              Position,
                                              initialVelocity));

                timeToNextProjectile += TimeSpan.FromSeconds(0.01);
            }
        }
        /// <summary>
        /// Helper for updating the list of active projectiles.
        /// </summary>
        void UpdateProjectiles(GameTime gameTime)
        {
            int i = 0;

            while (i < projectiles.Count)
            {
                if (!projectiles[i].Update(gameTime))
                {
                    // Remove projectiles at the end of their life.
                    projectiles.RemoveAt(i);
                }
                else
                {
                    // Advance to the next projectile.
                    i++;
                }
            }
        }

        /// <summary>
        /// Update the values to be chased by the camera
        /// </summary>
        private void UpdateCameraChaseTarget()
        {
            camera.ChasePosition = player.tank.Position;//tank.Position; // new Vector3(0,0, 0);//
            Vector3 direct = player.tank.ForwardVector;
            direct.Y = MathHelper.ToRadians(player.tank.FacingDirection);
            camera.ChaseDirection = direct;//tank.ForwardVector;// 
            camera.Up = Vector3.Up;// tank.UpVector;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;

            device.Clear(Color.Black);
            if (player.IsDead || player.NumEnemiesAlive == 0)
            {
                gameScreen.Draw(gameTime);
            }
            else
            {


                terrain.SetCamera(camera.View, camera.Projection);
                //terrain.Draw(gameTime);

                player.tank.SetCamera(camera.View, camera.Projection);
                for (int i = 0; i < badGuys.Length; i++)
                {
                    badGuys[i].tank.SetCamera(camera.View, camera.Projection);
                }

                explosionParticles.SetCamera(camera.View, camera.Projection);
                explosionSmokeParticles.SetCamera(camera.View, camera.Projection);
                projectileTrailParticles.SetCamera(camera.View, camera.Projection);
                //tank.SetCamera(camera.View, camera.Projection);

                #region Draw Physic

                Physic.CannonBallManager.Draw(gameTime);
                #endregion

                //DrawSkyDome(camera.View);
                skybox.Draw(camera.View, camera.Projection, camera.Position);
                /*RasterizerState tmp;
                tmp = graphics.GraphicsDevice.RasterizerState;
                skybox.Draw(camera.View, camera.Projection, camera.Position);
                graphics.GraphicsDevice.RasterizerState = tmp;
                //DrawSkyDome(camera.View);


                // Set the View and Projection matrix for the effect
                SkySphereEffect.Parameters["ViewMatrix"].SetValue(
                    camera.View);
                SkySphereEffect.Parameters["ProjectionMatrix"].SetValue(
                    camera.Projection);
                // Draw the sphere model that the effect projects onto
                foreach (ModelMesh mesh in SkySphere.Meshes)
                {
                    //mesh.Draw();
                }
                */
                // If there was any alpha blended translucent geometry in
                // the scene, that would be drawn here.
                // Pass camera matrices through to the particle system components.
            }
            base.Draw(gameTime);
        }

        private void DrawSkyDome(Matrix currentViewMatrix)
        {
            // device.RenderState.DepthBufferWriteEnable = false;

            Matrix[] modelTransforms = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(modelTransforms);

            Matrix wMatrix = Matrix.CreateTranslation(0, -0.3f, 0) * Matrix.CreateScale(800) * Matrix.CreateTranslation(camera.Position);
            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = modelTransforms[mesh.ParentBone.Index] * wMatrix;
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];
                    currentEffect.Parameters["xWorld"].SetValue(worldMatrix);
                    currentEffect.Parameters["xView"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["xProjection"].SetValue(camera.Projection);
                    currentEffect.Parameters["xTexture"].SetValue(cloudMap);
                    currentEffect.Parameters["xEnableLighting"].SetValue(false);
                }
                mesh.Draw();
            }
            //device.RenderState.DepthBufferWriteEnable = true;
        }
        #endregion

        #region Handle Input
        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        private void HandleInput(GameTime gameTime)
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);


            if (currentKeyboardState.IsKeyDown(Keys.T))
            {
                Physic.Cannon.Fire();
                UpdateExplosions(gameTime);
            }

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }

            if (inputHelper.IsKeyJustPressed(Keys.P))
            {
                Enemy.UseFuzzy = true;
            }

            if (inputHelper.IsKeyJustPressed(Keys.L))
            {
                TerrainUnit.DEBUG_MODE = !TerrainUnit.DEBUG_MODE;
                Trees.DEBUG_MODE = !Trees.DEBUG_MODE;
                Tank.DEBUG_MODE = !Tank.DEBUG_MODE;
            }
            if (inputHelper.IsKeyJustPressed(Keys.R))
            {
                if (player.IsDead || player.NumEnemiesAlive == 0)
                {
                    start();
                    Physic.LoadContent();
                }
            }
            if (inputHelper.IsKeyJustPressed(Keys.J))
            {
                //badGuys[0].DecideDirection();sdsdf
                
            }
            player.HandleInput(currentGamePadState, currentKeyboardState, terrain.MapInfo);
            camera.HandleInput(currentGamePadState, currentKeyboardState, terrain.MapInfo);
            gameScreen.HandleInput(currentGamePadState, currentKeyboardState, terrain.MapInfo);
            

        #endregion
        }


        #region Entry Point

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static class Program
        {
            static void Main()
            {
                using (TanksOnAHeightmapGame game = new TanksOnAHeightmapGame())
                {
                    game.Run();
                }
            }
        }
    }
    #endregion
}
