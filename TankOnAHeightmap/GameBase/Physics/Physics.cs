using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.ResourceManagement;
using System;
using BEPUphysics.DataStructures;
using Microsoft.Xna.Framework.Content;
using TanksOnAHeightmap.GameLogic;

namespace TanksOnAHeightmap.GameBase.Physics
{
    /// <summary>
    ///  Player Physics
    /// </summary>
   public class Physics
    {
        #region Fields

        public CannonBallManager CannonBallManager;
        public HealthManager healthManager;
        public Cannon Cannon;
        public Space space;
        Entity tank_box;
        
        public readonly List<Wall> WallStreet = new List<Wall>();
        private readonly List<HardWall> HardWallStreet = new List<HardWall>(); 

        ContentManager Content;
        GameComponentCollection Components;
        Game game;
        Tank tank;
        Player player;
        
        TanksOnAHeightmap.GameBase.Shapes.Terrain terrain;

        #endregion

        public Physics(Game game, Player player, ContentManager Content, TanksOnAHeightmap.GameBase.Shapes.Terrain terrain, Space space) 
        {
            Cannon = new Cannon(game, player.tank);
            CannonBallManager = new CannonBallManager(game);
            EnemyCannon.playerTank = player.tank;

            healthManager = new HealthManager(game, space);
            this.Content = Content;
            this.Components = game.Components;
            this.game = game;
            this.player = player;
            this.tank = player.tank;
            this.terrain = terrain;
            this.space = space;           
        }

        public void LoadContent ()
        {

            #region BEPU Physic

           

            #region MultiThread
            //Give the space some threads to work with.
#if XBOX360
                        //Note that not all four available hardware threads are used.
            //Currently, BEPUphysics will allocate an equal amount of work to each thread.
            //If two threads are put on one core, it will bottleneck the engine and run significantly slower than using 3 hardware threads.
            Space.ThreadManager.AddThread(delegate(object information) { Thread.CurrentThread.SetProcessorAffinity(new[] { 1 }); }, null);
            Space.ThreadManager.AddThread(delegate(object information) { Thread.CurrentThread.SetProcessorAffinity(new[] { 3 }); }, null);
            Space.ThreadManager.AddThread(delegate(object information) { Thread.CurrentThread.SetProcessorAffinity(new[] { 5 }); }, null);

            //Enable the multithreading system!
            //space.useMultithreadedUpdate = true;
#else
            if (Environment.ProcessorCount > 1)
            {
                //On windows, just throw a thread at every processor.  The thread scheduler will take care of where to put them.
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    space.ThreadManager.AddThread();
                }
                //Enable the multithreading system!
                space.UseMultithreadedUpdate = true;
            }
#endif
            #endregion      
            
            
            space.SimulationSettings.MotionUpdate.Gravity = new Vector3(0, -98.81f, 0);
            space.SimulationSettings.CollisionResponse.Iterations = 10;
            space.SimulationSettings.CollisionResponse.IterationsBeforeEarlyOut = 0;
            space.SimulationSettings.TimeStep.TimeStepDuration = 1 / 70f;
           
            Model CubeModel;
            CubeModel = Content.Load<Model>("Models/cube");
            Wall.BlockModel = Content.Load<Model>("Models/wall_cube");
            HardWall.BlockModel = Content.Load<Model>("Models/wall_cube");            
            Health.BlockModel = Content.Load<Model>("Models/health");
            CannonBall.Model = Content.Load<Model>("Models/sphere");
            EnemyCannonBall.Model = Content.Load<Model>("Models/sphere");
            CannonBall.game = game;
            EnemyCannonBall.game = game;

            game.Components.Add(CannonBallManager);
            CannonBallManager.DrawOrder = 102;
            // adding walls // Content.Load<Model>("Models/detectorMiddleOuter")
            for (int i = 0; i < 1001; i = i + 200)
            {
                WallStreet.Add(new Wall(game, space, Matrix.Identity, Matrix.CreateTranslation(new Vector3(i - 1210, -380 + (float)i / 60f, -460)), 10, 5, 200, 60, 10));
                WallStreet.Add(new Wall(game, space, Matrix.Identity, Matrix.CreateRotationY(1.57f) * Matrix.CreateTranslation(new Vector3(-1010, -380, -1070+i)), 10, 5, 200, 60, 10));
                HardWallStreet.Add(new HardWall(game, space, Matrix.Identity, Matrix.CreateTranslation(new Vector3(1810+i, -389, 2960)), 6, 3, 24, 200, 24));
            }


            for (int i = 0; i < WallStreet.Count; i++)
            { 
                game.Components.Add(WallStreet[i]);
                WallStreet[i].DrawOrder = 102;
            }

            

            HardWallStreet.Add(new HardWall(game, space, Matrix.Identity, Matrix.CreateTranslation(new Vector3(-1710, -360, 310)), 1, 5, 24, 200, 24));
            HardWallStreet.Add(new HardWall(game, space, Matrix.Identity, Matrix.CreateTranslation(new Vector3(-1710, -360, 190)), 1, 5, 24, 200, 24));
            for (int i = 0; i < HardWallStreet.Count; i++)
            {
                game.Components.Add(HardWallStreet[i]);
                HardWallStreet[i].DrawOrder = 102;
            }
           // Components.Add(new Wall(game, space, Matrix.Identity, Matrix.CreateTranslation(new Vector3(10, -350, -360)) * Matrix.CreateRotationY(1.57f), 5, 5, 121, 37, 10));
                                                    // Position                     // entity offset                 // entity size
            Building church = new Building(game, space, new Vector3(-2100, -40, 250), new Box( new Vector3(-70, -210, 0), 685, 400, 365),15f);
            church.BlockModel = Content.Load<Model>("Models/88cathedral");
            church.DrawOrder = 102;
            Components.Add(church);
            
            /*Building oldBarn = new Building(game, space, new Vector3(1840, -200, 2390), new Box(new Vector3(-70, -210, 0), 685, 400, 365),120f);
            oldBarn.BlockModel = Content.Load<Model>("Models/street-lamppost");
            Components.Add(oldBarn);*/

            healthManager.AddRandomHealth();

            // Border
           Model BorderCube;
           BorderCube = Content.Load<Model>("Models/border");
           Box entity;

           for (int i = -3600; i < 3601; i += 7200)
           {
                entity = new Box(new Vector3(0, -380, i), 7200, 450, 10);
                entity.EventManager.InitialCollisionDetected += borderCollision;
                Box box = entity as Box;
                Matrix scaling = Matrix.CreateScale(box.Width/10f, box.Height/10f, box.Length/10f);
                EntityModel model1 = new EntityModel(entity, BorderCube, scaling, game);
                entity.Tag = model1;
                space.Add(entity);
                game.Components.Add(model1);
                model1.DrawOrder = 102;
           }
           for (int i = -3600; i < 3601; i += 7200)
           {
               entity = new Box(new Vector3(i, -380, 0), 10, 450, 7200);
               entity.EventManager.InitialCollisionDetected += borderCollision;
               Box box = entity as Box;
               Matrix scaling = Matrix.CreateScale(box.Width / 10f, box.Height / 10f, box.Length / 10f);
               EntityModel model1 = new EntityModel(entity, BorderCube, scaling, game);
               space.Add(entity);
               game.Components.Add(model1);
               model1.DrawOrder = 102;
           }
           
            

            //space.Add(new Box(new Vector3(0, 8, 0), 10, 10, 10, 5));
            //space.Add(new Box(new Vector3(0, 12, 0), 10, 10, 10, 5));
            
            //Create a physical environment from a triangle mesh.
            //First, collect the the mesh data from the model using a helper function.
            //This special kind of vertex inherits from the TriangleMeshVertex and optionally includes
            //friction/bounciness data.
            //The StaticTriangleGroup requires that this special vertex type is used in lieu of a normal TriangleMeshVertex array.
            StaticTriangleGroup.StaticTriangleGroupVertex[] vertices2;
            int[] indices;
            StaticTriangleGroup.GetVerticesAndIndicesFromModel(terrain.Model, out vertices2, out indices);
            //Give the mesh information to a new TriangleMesh.  
            //TriangleMeshes are internally accelerated structures that 
            //help handle raycasts and various other queries on mesh data.
            TriangleMesh triangleMesh = new TriangleMesh(vertices2, indices);
            //Create the StaticTriangleGroup based on the triangle mesh.
            StaticTriangleGroup triangleGroup = new StaticTriangleGroup(triangleMesh);
            //Scoot the mesh down so its below the previously created kinematic box.
            // triangleGroup.WorldMatrix = Matrix.CreateTranslation(new Vector3(0, -40, 0));
            //Add it to the space!
            space.Add(triangleGroup);
            triangleGroup.Tag = triangleMesh;
            //Make it visible too.
            Components.Add(new StaticModel(terrain.Model, triangleGroup.WorldMatrix, game));


            tank_box = new Box(tank.Position, 60, 40, 65, 50);
            //Components.Add(tank_box);            
            space.Add(tank_box);
            tank_box.Tag = player; 
            tank_box.EventManager.InitialCollisionDetected += tankCollision;

            CannonBall.CannonBallCollisionGroup = new CollisionGroup();            

            space.SimulationSettings.CollisionDetection.CollisionGroupRules.Add(new CollisionGroupPair(tank_box.CollisionRules.Group, CannonBall.CannonBallCollisionGroup), CollisionRule.NoPair);
            triangleGroup.CollisionRules.SpecificEntities.Add(tank_box, CollisionRule.NoPair);
            space.SimulationSettings.CollisionDetection.CollisionGroupRules.Add(new CollisionGroupPair(Enemy.EnemyTankCollisionGroup, EnemyCannonBall.EnemyCannonBallCollisionGroup), CollisionRule.NoPair);
            #endregion
        }
       
        #region EventPhysics

        public void borderCollision(Entity sender, Entity other, CollisionPair pair)
        {
            var enemy = other.Tag as Player;
            if (enemy != null)
            {
                if (!player.EnemyCollision)
                    player.tank.Velocity = -player.tank.Velocity;
                player.EnemyCollision = true;
            }


        }
        public void tankCollision(Entity sender, Entity other, CollisionPair pair)
        {
            var enemy = other.Tag as TriangleMesh;
            if (enemy != null)
            {
                // Terrain was hit!
            }

            var enemy2 = other.Tag as Player;
            if (enemy2 != null)
            {
                // Player tank was was hit!
            }

            var enemy3 = other.Tag as Enemy;
            if (enemy3 != null)
            {
                if (!player.EnemyCollision)
                    player.tank.Velocity = -player.tank.Velocity;
                player.EnemyCollision = true;
            } // Enemy was hit                       

            var enemy4 = other.Tag as Wall;
            if (enemy4 != null && (other.IsDynamic == false))
                other.BecomeDynamic(1f);

            var enemy5 = other.Tag as Building;
            if (enemy5 != null)
            {
                if (!player.EnemyCollision)
                    player.tank.Velocity = -player.tank.Velocity;
                player.EnemyCollision = true;
            }

            var enemy6 = other.Tag as HardWall;
            if (enemy6 != null)
            {
                if (!player.EnemyCollision)
                    player.tank.Velocity = -player.tank.Velocity;
                player.EnemyCollision = true;
            } 
        }
        #endregion

        public void Update(GameTime gameTime)
        {
            //Physic player tank movment update
            tank_box.LinearVelocity = new Vector3((tank.Position.X - tank_box.CenterPosition.X) * 16, (tank.Position.Y + 15 - tank_box.CenterPosition.Y) * 16, (tank.Position.Z - tank_box.CenterPosition.Z) * 16);
            tank_box.OrientationMatrix = tank.Orientation;
            //tank.setPosition(tank_box.CenterPosition);
            //tank.Orientation = tank_box.OrientationMatrix;
            // cannon update
            Cannon.Update(gameTime);
            CannonBallManager.Update(gameTime);
            healthManager.Update(gameTime);
            

                
                for(int i=0;i<WallStreet.Count;i++)
                {
                    if (WallStreet[i].blocks.Count.Equals(0))
                    {
                        WallStreet[i].Visible = false;
                        WallStreet[i].Dispose();
                        WallStreet[i].Enabled = false;                        
                        WallStreet.Remove(WallStreet[i]);                        
                    }
                    else
                        WallStreet[i].Update(gameTime);
                
                }


            space.Update(gameTime);


        }



    }
}
