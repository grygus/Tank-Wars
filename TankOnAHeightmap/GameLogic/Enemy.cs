using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.DataStructures;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using TanksOnAHeightmap.GameLogic.AI;
using TanksOnAHeightmap.Helpers;
using TanksOnAHeightmap.Helpers.Drawing;

namespace TanksOnAHeightmap.GameLogic
{
    public class Enemy : TerrainUnit
    {
        #region EnemyState enum

        public enum EnemyState
        {
            Wander = 0,
            ChasePlayer,
            AttackPlayer,
            AttackPrey,
            //Dead
        }

        #endregion

        private const int CurrentAnimationId = 1;

        private static float DISTANCE_EPSILON          = 100;
        private static float LINEAR_VELOCITY_CONSTANT  = 10.0f;
        private static float ANGULAR_VELOCITY_CONSTANT = 30.0f;
        private static int WANDER_MAX_MOVES            = 30;
        private static int WANDER_DISTANCE             = 300;
        private static float WANDER_DELAY_SECONDS      = 0.1f;
        private static float ATTACK_DELAY_SECONDS      = 1.5f;
        private static float FUZZY_DELAY_SECONDS       = 1.5f;

        public static bool DEBUG_ENEMY;
        public static Enemy[] Units = {};
        public static int selectedUnit = -1;
        private readonly UnitTypes.EnemyType enemyType;
        private readonly Game game;
        public EnemyCannon EnemyCannon;
        public EnemyCannonBallManager EnemyCannonBallManager;
        public bool Selected;
        private int attackDamage;
        private float attackDistance;
        private Vector3 chaseVector;


        private float fuzzyActionTime;
        
        // Wander

        // Attack
        private bool isHited;
        private float nextActionTime;
        private float perceptionDistance;
        private Player player;

        private Vector3 rotate;

        private Space space;
        private EnemyState state;
        private Entity tank_box;
        private int wanderMovesCount;
        private Vector3 wanderPosition;
        private Vector3 wanderStartPosition;

        public Enemy()
            : base(new Game())
        {
        }

        public Enemy(Game game, ContentManager content, GraphicsDeviceManager graphics, UnitTypes.EnemyType enemyType,
                     Space space, Vector3 StartingPosition)
            : base(game, content, graphics)
        {
            tank.IsEnemy = true;
            this.enemyType = enemyType;
            this.space = space;
            rotate = Vector3.Zero;
            isHited = false;
            wanderMovesCount = 0;

            tank_box = new Sphere(StartingPosition, 35, 50); //new Box(StartingPosition, 60, 40, 70, 50);
            space.Add(tank_box);
            tank_box.Tag = this;
            tank_box.EventManager.InitialCollisionDetected += tankCollision;
            tank_box.CollisionRules.Group = EnemyTankCollisionGroup;
            this.game = game;

            Velocity = 2;
            tank.Velocity = 10;

            EnemyCannon = new EnemyCannon(game, tank, this);
            EnemyCannonBallManager = new EnemyCannonBallManager(game);
            game.Components.Add(EnemyCannonBallManager);

            randomWanderTarget = Vector3.Zero;
            MaxSteeringForce = 1000;
            WanderOn = true;
            ObstacleAvoidOn = true;
            ChasingPlayerOn = false;
            wallAvoidanceOn = true;
            TurnToPlayer = false;

            FuzzyBrain = new FuzzyEngine();
        }

        public bool IsHited
        {
            get { return isHited; }
            set { isHited = value; }
        }

        public Entity Tank_box
        {
            get { return tank_box; }
            set { tank_box = value; }
        }

        #region AI

        private const float wanderRadius = 2f;
        private const float wanderJitter = 1f;
        private const int wanderDistance = 120;
        private State currentState;
        private Vector3 randomWanderTarget;
        private bool wallAvoidanceOn;
        public FuzzyEngine FuzzyBrain { get; set; }

        public bool WanderOn { get; set; }

        public bool ObstacleAvoidOn { get; set; }

        public bool ChasingPlayerOn { get; set; }
        public bool ChasingPreyOn { get; set; }
        public bool TurnToPlayer { get; set; }
        public bool TurnToPrey { get; set; }


        public bool WallAvoidanceOn
        {
            get { return wallAvoidanceOn; }
            set { wallAvoidanceOn = value; }
        }

        public float MaxSteeringForce { get; set; }

        private void ChangeState(State newState)
        {
            currentState.Exit(this);
            currentState = newState;
            currentState.Enter(this);
        }

        #endregion

        #region Properties

        public EnemyState State
        {
            get { return state; }
            set { state = value; }
        }

        public Player Player
        {
            set 
            { 
                player = value;
                if(Oponents != null)
                    Oponents.Add(player);
                else
                {
                    Oponents = new List<TerrainUnit>();
                    Oponents.Add(player);
                }
            }
        }


        public override Matrix Transformation
        {
            get
            {
                Matrix helper = Matrix.CreateTranslation(tank.Position);
                return helper;
            }
            set
            {
                tank.Position = value.Translation;
                //wanderPosition = value.Translation;
                //wanderStartPosition = value.Translation;
            }
        }

        public static CollisionGroup EnemyTankCollisionGroup { get; set; }

        public static bool UseFuzzy { get; set; }

        #endregion

        public static Enemy GetSelectedUnit()
        {
            if (selectedUnit > -1)
                return Units[selectedUnit];
            else
                return null;
        }

        protected override void LoadContent()
        {
            // Load(UnitTypes.EnemyModelFileName[(int)enemyType]);

            // Unit configurations
            Life = UnitTypes.EnemyLife[(int) enemyType];
            MaxLife = Life;
            Speed = UnitTypes.EnemySpeed[(int) enemyType];
            perceptionDistance = UnitTypes.EnemyPerceptionDistance[(int) enemyType];
            attackDamage = UnitTypes.EnemyAttackDamage[(int) enemyType];
            attackDistance = UnitTypes.EnemyAttackDistance[(int) enemyType];

            wanderPosition = tank.Position;


            base.LoadContent();
        }

        public override void ReceiveDamage(int damageValue)
        {
            base.ReceiveDamage(damageValue);

            // Chase
            isHited = true;

            if (Life > 0)
            {
            }
            else
            {
            }
        }


        private void Move(Vector3 direction)
        {
            if (direction.Length() != 1)
                direction.Normalize();
            LinearVelocity = direction*LINEAR_VELOCITY_CONSTANT;

            // Angle between heading and move direction
            var radianAngle = (float) Math.Acos(Vector3.Dot(tank.ForwardVector, direction));
            if (radianAngle >= 0.1f)
            {
                // Find short side to rodade CW or CCW
                float sideToRotate = Vector3.Dot(tank.Orientation.Right, direction);

                var rotationVector = new Vector3(0, ANGULAR_VELOCITY_CONSTANT*radianAngle, 0);
                if (sideToRotate > 0)
                {
                    AngularVelocity = -rotationVector;
                }
                else
                {
                    AngularVelocity = rotationVector;
                }
            }
        }

        private Vector3 RandomWander(GameTime time)
        {
            randomWanderTarget = RandomHelper.GeneratePositionXZ(200)*wanderJitter;
            randomWanderTarget.Normalize();
            randomWanderTarget *= wanderRadius;
            Vector3 targetLocal = randomWanderTarget + new Vector3(0, 0, -wanderDistance);
            Vector3 targetWorld = Vector3.Transform(targetLocal, tank.WorldMatrix);
            wanderPosition = targetWorld;

            if (Math.Abs(targetWorld.X) < 2048 && Math.Abs(targetWorld.Z) < 2048)
                //terrain.MapInfo.IsOnHeightmap(targetWorld))
                return Vector3.Normalize(targetWorld - tank.Position);
            else
                return -40*Vector3.Normalize((targetWorld - tank.Position));
        }

        private void Wander(GameTime time)
        {
            // Calculate wander vector on X, Z axis
            Vector3 wanderVector = wanderPosition - tank.Position;
            wanderVector.Y = 0;
            float wanderVectorLength = wanderVector.Length();

            // Reached the destination position
            if (wanderVectorLength < DISTANCE_EPSILON)
            {
                // Generate new random position
                if (wanderMovesCount < WANDER_MAX_MOVES)
                {
                    wanderPosition = tank.Position +
                                     RandomHelper.GeneratePositionXZ(WANDER_DISTANCE);

                    //System.Console.WriteLine(wanderPosition);
                    wanderMovesCount++;
                }
                    // Go back to the start position
                else
                {
                    wanderPosition = wanderStartPosition;
                    wanderMovesCount = 0;
                }

                // Next time wander
                nextActionTime = (float) time.TotalGameTime.TotalSeconds + WANDER_DELAY_SECONDS +
                                 WANDER_DELAY_SECONDS*(float) RandomHelper.RandomGenerator.NextDouble();
            }

            // Wait for the next action time
            if (time.TotalGameTime.TotalSeconds > nextActionTime)
            {
                wanderVector *= (1.0f/wanderVectorLength);

                Move(wanderVector);
            }
        }

        private void ChasePlayer(GameTime time)
        {
            Move(chaseVector);
        }

        private void AttackPrey(GameTime time)
        {
            var elapsedTimeSeconds = (float)time.TotalGameTime.TotalSeconds;
            if (elapsedTimeSeconds > nextActionTime)
            {
                var ray = new Ray(tank.Position, tank.ForwardVector);
                Microsoft.Xna.Framework.BoundingSphere sphere = new BoundingSphere(Prey.Position,150);
                
                //float? distance = Prey.BoundingBox.Intersects(ray);
                float? distance = sphere.Intersects(ray);
                if (distance != null)
                {
                    //player.ReceiveDamage(attackDamage, this.tank.Orientation.Forward);
                    EnemyCannon.Fire();
                    if (TurnToPrey)
                        TurnToPrey = false;
                }
                else
                {
                    if (!TurnToPrey)
                        TurnToPrey = true;
                }

                nextActionTime = elapsedTimeSeconds + ATTACK_DELAY_SECONDS;
            }
        }

        private void AttackPlayer(GameTime time)
        {
            //ChasePlayer(time);

            var elapsedTimeSeconds = (float) time.TotalGameTime.TotalSeconds;
            if (elapsedTimeSeconds > nextActionTime)
            {
                var ray = new Ray(tank.Position, tank.ForwardVector);
                float? distance = player.BoxIntersects(ray);
                if (distance != null)
                {
                    //player.ReceiveDamage(attackDamage, this.tank.Orientation.Forward);
                    EnemyCannon.Fire();
                    if (TurnToPlayer)
                        TurnToPlayer = false;
                }
                else
                {
                    if (!TurnToPlayer)
                        TurnToPlayer = true;
                }

                nextActionTime = elapsedTimeSeconds + ATTACK_DELAY_SECONDS;
            }
        }

        public override void Update(GameTime time)
        {
            var elapsedTimeSeconds = (float) time.ElapsedGameTime.TotalSeconds;

            if (IsDead)
            {
                game.Components.Remove(this);
                player.Oponents.Remove(this);
            }

            EnemyCannon.Update(time);
            EnemyCannonBallManager.Update(time);

            LinearVelocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
            //AI

            if ((float) time.TotalGameTime.TotalSeconds > fuzzyActionTime)
            {
                FuzzyBrain.UpdateParameters(player.Life, Life);
                fuzzyActionTime = (float) time.TotalGameTime.TotalSeconds + FUZZY_DELAY_SECONDS;
            }


            if (UseFuzzy)
            {
                float direction = FuzzyBrain.DecideDirection(tank.WorldMatrix, player.Transformation.Translation,
                                           healthManager.GetNearestHealthPosition(Transformation.Translation), WorldTrees[0].Position);


            }
            else
            {
                //
                chaseVector = player.tank.Position - tank.Position;
                float distanceToPlayer = chaseVector.Length();
                float distanceToPray = (Prey.Position - Position).Length();
                // Normalize chase vector
                //chaseVector *= (1.0f / distanceToPlayer);
                tank.movement.Z = -1;
                switch (state)
                {
                    case EnemyState.Wander:
                        if (isHited || distanceToPlayer < perceptionDistance)
                        {
                            state = EnemyState.ChasePlayer;
                            WanderOn = false;
                        }
                        else if ( distanceToPray < 500)
                        {
                            state = EnemyState.AttackPrey;
                        }
                        else
                        {
                            //Wander(time);
                            if ((float)time.TotalGameTime.TotalSeconds > nextActionTime)
                            {
                                SteeringForce = RandomWander(time);
                                nextActionTime = (float)time.TotalGameTime.TotalSeconds + WANDER_DELAY_SECONDS +
                                                 WANDER_DELAY_SECONDS * (float)RandomHelper.RandomGenerator.NextDouble();
                                //SteeringForce = tank.SteeringForce;
                            }
                            //Move(tank.SteeringForce);
                        }
                        break;

                    case EnemyState.ChasePlayer:
                        if (distanceToPlayer > perceptionDistance)
                        {
                            state = EnemyState.Wander;
                            if (ChasingPlayerOn)
                            {
                                ChasingPlayerOn = false;
                                WanderOn = true;
                            }

                        }
                        else if (distanceToPlayer <= attackDistance)
                        {
                            // Change state
                            state = EnemyState.AttackPlayer;
                            nextActionTime = 0;
                        }
                        else
                        {
                            //if(tank.SteeringForce != null && tank.SteeringForce != Vector3.Zero)
                            //Move(tank.SteeringForce);
                            //else
                            //ChasePlayer(time);
                            if (!ChasingPlayerOn)
                            {
                                ChasingPlayerOn = true;
                                WanderOn = false;
                            }
                            if (!wallAvoidanceOn)
                                wallAvoidanceOn = true;
                        }
                        break;

                    case EnemyState.AttackPlayer:
                        if (ChasingPreyOn)
                            ChasingPreyOn = false;
                        if (TurnToPrey)
                            TurnToPrey = false;

                        if (distanceToPlayer > attackDistance * 1.5f)
                            // Change state
                            state = EnemyState.ChasePlayer;
                        else
                        {
                            if (distanceToPlayer < 100)
                            {
                                if (ChasingPlayerOn)
                                    ChasingPlayerOn = false;

                                if (!TurnToPlayer)
                                    TurnToPlayer = true;

                                tank.movement.Z = 0;
                            }
                            else
                            {
                                if (!ChasingPlayerOn)
                                    ChasingPlayerOn = true;

                                if (TurnToPlayer)
                                    TurnToPlayer = false;
                            }
                            if (wallAvoidanceOn)
                                wallAvoidanceOn = false;

                            if (WanderOn)
                                WanderOn = false;
                            AttackPlayer(time);
                        }
                        break;

                    case EnemyState.AttackPrey:
                        if (ChasingPlayerOn)
                            ChasingPlayerOn = false;
                        if (TurnToPlayer)
                            TurnToPlayer = false;

                        if (isHited && distanceToPlayer < attackDistance)
                        {
                            state = EnemyState.AttackPlayer;
                        }
                        else
                        {
                            if (distanceToPray < 200)
                            {
                                if (!TurnToPrey)
                                    TurnToPrey = true;

                                tank.movement.Z = 0;
                            }
                            else
                            {
                                if (!ChasingPreyOn)
                                    ChasingPreyOn = true;

                                if (TurnToPrey)
                                    TurnToPrey = false;
                            }
                            if (wallAvoidanceOn)
                                wallAvoidanceOn = false;

                            if (WanderOn)
                                WanderOn = false;
                            AttackPrey(time);
                        }

                        break;
                    default:
                        break;
                }

                tank_box.LinearVelocity = new Vector3((tank.Position.X - tank_box.CenterPosition.X) * 16,
                                                 (tank.Position.Y + 30 - tank_box.CenterPosition.Y) * 16,
                                                 (tank.Position.Z - tank_box.CenterPosition.Z) * 16);
                tank_box.OrientationMatrix = tank.Orientation;
                if (distanceToPlayer > 100 && distanceToPray > 200)
                {
                    tank.setPosition(tank_box.CenterPosition);
                }

                tank.SteeringForce = Calculate();
            }
            

            
            

            // Draw enemies
            var frustum = new BoundingFrustum(camera.View*camera.Projection);
            Visible = BoundingSphere.Intersects(frustum) ? true : false;

            tank.Update(time);
            isHited = false;
            base.Update(time);
        }

        public static void Select(int index)
        {
            if (index > -1)
            {
                if (selectedUnit > -1)
                    Units[selectedUnit].Selected = false;

                selectedUnit = index;
                Units[selectedUnit].Selected = true;
            }
        }

        public static void ClearSelection()
        {
            if (selectedUnit > -1)
                Units[selectedUnit].Selected = false;

            selectedUnit = -1;
        }

        public override void Draw(GameTime time)
        {
            base.Draw(time);
            if (Selected)
            {
                //Renderer.BoundingSphere3D.Draw(tank.BoundingSphere, Color.Black, Transformation * Matrix.CreateTranslation(new Vector3(0, 25, 0)));

                //Renderer.BoundingSphere3D.Draw(BoundingSphere, Color.Red, null);
                Renderer.BoundingSphere3D.DrawWheel(Transformation.Translation,50,Color.GreenYellow,tank.WorldMatrix * Matrix.CreateTranslation(0,5,0));
            }
            //Renderer.Line3D.Draw(tank.Position + new Vector3(0, 50, 0), wanderPosition+ new Vector3(0, 50, 0), Color.Red, null);
            if (DEBUG_MODE)
            {
                Renderer.Line3D.Draw(tank.Position + new Vector3(0, 50, 0),
                                     tank.Position + SteeringForce*1000 + new Vector3(0, 50, 0), Color.Tomato, null);

                //Renderer.BoundingSphere3D.Draw(BoundingSphere,Color.Black,Transformation);
                Renderer.BoundingSphere3D.Draw(tank.BoundingSphere, Color.Black,
                                               Transformation*Matrix.CreateTranslation(new Vector3(0, 25, 0)));

                float radians = MathHelper.ToDegrees(FuzzyBrain.DecideDirection(tank.WorldMatrix,
                                                                            player.Transformation.Translation,
                                                                            healthManager.GetNearestHealthPosition(Transformation.Translation),
                                                                            WorldTrees[0].Position));
                if (radians < 0)
                    radians = -180 - radians;
                else
                    radians = 180 - radians;
                Matrix rot = Matrix.CreateRotationY(MathHelper.ToRadians(-radians));
                Renderer.Line3D.Draw(new Vector3(0, 50, 0), new Vector3(0, 50, -200), Color.Red, rot * tank.WorldMatrix);
            }

            
        }

        #region EventPhysics

        public void tankCollision(Entity sender, Entity other, CollisionPair pair)
        {
            var enemy = other.Tag as TriangleMesh;
            if (enemy != null)
            {
                // Terrain was hit!
            }

            var enemy2 = other.Tag as Player; // Player tank was was hit!
            if (enemy2 != null)
            {
                if (!enemy2.EnemyCollision)
                    enemy2.tank.Velocity = -enemy2.tank.Velocity;
                enemy2.EnemyCollision = true;
            }

            var enemy3 = other.Tag as Enemy;
            if (enemy3 != null)
            {
            } // Enemy was hit                       

            var enemy4 = other.Tag as Wall;
            if (enemy4 != null && (other.IsDynamic == false))
                other.BecomeDynamic(1f);
        }

        #endregion

        #region AI

        private Vector3 Seek(Vector3 goal)
        {
            Vector3 DesiredVelocity = Vector3.Normalize(goal - tank.Position);
            return 4*DesiredVelocity; //+tank.ForwardVector*40;//(DesiredVelocity - m_pVehicle->Velocity());
        }

        private Vector3 Calculate()
        {
            Vector3 wanderForce = SteeringForce;
            SteeringForce = Vector3.Zero;
            Vector3 force;
            if (TurnToPlayer)
            {
                force = Vector3.Normalize(player.tank.Position - tank.Position);

                tank.isRotating = true;
                //Matrix inverted = Matrix.Invert(tank.WorldMatrix);
                //Vector3 invertedVec = Vector3.Transform(force, inverted);
                //invertedVec.Z = 0;
                //force = Vector3.Normalize(Vector3.Transform(invertedVec, tank.WorldMatrix));
                //(DesiredVelocity - m_pVehicle->Velocity());

                if (!AccumulateForce(force)) return SteeringForce;
            }
            else if (TurnToPrey)
            {
                force = Vector3.Normalize(Prey.Position - tank.Position);

                tank.isRotating = true;
                //Matrix inverted = Matrix.Invert(tank.WorldMatrix);
                //Vector3 invertedVec = Vector3.Transform(force, inverted);
                //invertedVec.Z = 0;
                //force = Vector3.Normalize(Vector3.Transform(invertedVec, tank.WorldMatrix));
                //(DesiredVelocity - m_pVehicle->Velocity());

                if (!AccumulateForce(force)) return SteeringForce;
            }
            else
            {
                tank.isRotating = false;
            }
            if (ObstacleAvoidOn && (tank.isRotating == false))
            {
                force = FindObstacles();
                if (!AccumulateForce(force))
                    return force;
            }
            if (WallAvoidanceOn && (tank.isRotating == false))
            {
                force = WallAvoidance();
                if (!AccumulateForce(force))
                    return force;
            }

            if (WanderOn)
            {
                force = wanderForce;
                if (!AccumulateForce(force)) return SteeringForce;
            }
            if (ChasingPreyOn)
            {
                force = Seek(Prey.Position);
                if (!AccumulateForce(force)) return SteeringForce;
            }
            else if (ChasingPlayerOn)
            {
                force = Seek(player.Position);
                if (!AccumulateForce(force)) return SteeringForce;
            }


            return SteeringForce;
        }

        private bool AccumulateForce(Vector3 force)
        {
            float MagnitudeSoFar = SteeringForce.Length();
            float MagnitudeRemaining = MaxSteeringForce - MagnitudeSoFar;
            if (MagnitudeRemaining <= 0.0) return false;

            float MagnitudeToAdd = force.Length();

            if (MagnitudeToAdd < MagnitudeRemaining)
            {
                SteeringForce += force;
            }
            else
            {
                //add it to the steering force
                SteeringForce += Vector3.Normalize(force)*MagnitudeRemaining;
            }

            return true;
        }

        #endregion

        
    }
}