using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using TanksOnAHeightmap.GameBase;
using Microsoft.Xna.Framework.Content;
using TanksOnAHeightmap.Helpers;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.DataStructures;
using TanksOnAHeightmap.GameBase.Physics;
using Microsoft.Xna.Framework.Graphics;
using TanksOnAHeightmap.GameLogic.AI;
using TanksOnAHeightmap.Helpers.Drawing;
using System.Diagnostics;

namespace TanksOnAHeightmap.GameLogic
{
    public class Enemy : TerrainUnit
    {

        public enum EnemyState
        {
            Wander = 0,
            ChasePlayer,
            AttackPlayer,
            //Dead
        }

        static float DISTANCE_EPSILON          = 100;
        static float LINEAR_VELOCITY_CONSTANT  = 10.0f;
        static float ANGULAR_VELOCITY_CONSTANT = 30.0f;
        static int WANDER_MAX_MOVES            = 30;
        static int WANDER_DISTANCE             = 300;
        static float WANDER_DELAY_SECONDS      = 0.1f;
        static float ATTACK_DELAY_SECONDS      = 1.5f;
        static float FUZZY_DELAY_SECONDS       = 1.5f;

        EnemyState state;
        float nextActionTime;
        float fuzzyActionTime;
        public HealthManager healthManager;
        // Wander
        int wanderMovesCount;
        Vector3 wanderPosition;
        Vector3 wanderStartPosition;

        // Chase
        float perceptionDistance;
        Vector3 chaseVector;

        // Attack
        bool isHited;
        Player player;
        float attackDistance;
        int attackDamage;

        Vector3 rotate;

        UnitTypes.EnemyType enemyType;
        const int CurrentAnimationId = 1;

        Entity tank_box;
        Space space;
        public EnemyCannonBallManager EnemyCannonBallManager;
        public EnemyCannon EnemyCannon;
        Game game;



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

        public FuzzyEngine FuzzyBrain { get; set; }
        State currentState;

        void ChangeState(State newState)
        {
            currentState.Exit(this);
            currentState = newState;
            currentState.Enter(this);
        }
        public bool WanderOn
        {
            get { return wanderOn; }
            set { wanderOn = value; }
        }
        bool wanderOn;
        public bool ObstacleAvoidOn
        {
            get { return obstacleAvoidOn; }
            set { obstacleAvoidOn = value; }
        }
        bool obstacleAvoidOn;
        public bool ChasingOn
        {
            get { return chasingOn; }
            set { chasingOn = value; }
        }
        bool chasingOn;
        public bool Turn
        {
            get { return turn; }
            set { turn = value; }
        }
        bool turn;
        public bool WallAvoidanceOn
        {
            get { return wallAvoidanceOn; }
            set { wallAvoidanceOn = value; }
        }
        bool wallAvoidanceOn;
        public float MaxSteeringForce
        {
            get { return maxSteeringForce; }
            set { maxSteeringForce = value; }
        }
        float maxSteeringForce;
        Vector3 randomWanderTarget;
        const float wanderRadius = 2f;
        const float wanderJitter = 1f;
        const int wanderDistance = 120;

        

        static public Dictionary<string, float> SelectedTankParameters
        {
            get { return selectedTankParameters; }
            set { selectedTankParameters = value; }
        }
        static Dictionary<string, float> selectedTankParameters;

       
        #endregion


        #region Properties

        public EnemyState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        public Player Player
        {
            set
            {
                player = value;
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

        #endregion
        public Enemy()
            : base(new Game())
        {

        }

        public Enemy(Game game, ContentManager content, GraphicsDeviceManager graphics, UnitTypes.EnemyType enemyType, Space space, Vector3 StartingPosition)
            : base(game, content, graphics)
        {
            tank.IsEnemy = true;
            this.enemyType = enemyType;
            this.space = space;
            rotate = Vector3.Zero;
            isHited = false;
            wanderMovesCount = 0;

            tank_box = new Sphere(StartingPosition, 35, 50);//new Box(StartingPosition, 60, 40, 70, 50);
            space.Add(tank_box);
            tank_box.Tag = this;
            tank_box.EventManager.InitialCollisionDetected += tankCollision;
            tank_box.CollisionRules.Group = EnemyTankCollisionGroup;
            this.game = game;

            Velocity = 2;
            tank.Velocity = 10;
            
            EnemyCannon = new EnemyCannon(game, tank,this);
            EnemyCannonBallManager = new EnemyCannonBallManager(game);
            game.Components.Add(EnemyCannonBallManager);
            
			randomWanderTarget = Vector3.Zero;
            MaxSteeringForce = 1000;
            WanderOn = true;
            ObstacleAvoidOn = true;
            ChasingOn = false;
            wallAvoidanceOn = true;
            Turn = false;

            FuzzyBrain = new FuzzyEngine();

            SelectedTankParameters = FuzzyBrain.FuzzyParameters;
        }

        protected override void LoadContent()
        {
           // Load(UnitTypes.EnemyModelFileName[(int)enemyType]);

            // Unit configurations
            Life = UnitTypes.EnemyLife[(int)enemyType];
            MaxLife = Life;
            Speed = UnitTypes.EnemySpeed[(int)enemyType];
            perceptionDistance = UnitTypes.EnemyPerceptionDistance[(int)enemyType];
            attackDamage = UnitTypes.EnemyAttackDamage[(int)enemyType];
            attackDistance = UnitTypes.EnemyAttackDistance[(int)enemyType];

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
            LinearVelocity = direction * LINEAR_VELOCITY_CONSTANT;

            // Angle between heading and move direction
            float radianAngle = (float)Math.Acos(Vector3.Dot(tank.ForwardVector, direction));
            if (radianAngle >= 0.1f)
            {
                // Find short side to rodade CW or CCW
                float sideToRotate = Vector3.Dot(tank.Orientation.Right, direction);

                Vector3 rotationVector = new Vector3(0, ANGULAR_VELOCITY_CONSTANT * radianAngle, 0);
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

            randomWanderTarget =  RandomHelper.GeneratePositionXZ(200)*wanderJitter;
            randomWanderTarget.Normalize();
            randomWanderTarget *= wanderRadius;
            Vector3 targetLocal = randomWanderTarget + new Vector3(0,0, -wanderDistance);
            Vector3 targetWorld = Vector3.Transform(targetLocal, tank.WorldMatrix);
            wanderPosition = targetWorld;

            if (Math.Abs(targetWorld.X) < 2048 && Math.Abs(targetWorld.Z) < 2048)//terrain.MapInfo.IsOnHeightmap(targetWorld))
                return Vector3.Normalize(targetWorld - tank.Position);
            else
                return -40* Vector3.Normalize((targetWorld - tank.Position));
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
                nextActionTime = (float)time.TotalGameTime.TotalSeconds + WANDER_DELAY_SECONDS +
                    WANDER_DELAY_SECONDS * (float)RandomHelper.RandomGenerator.NextDouble();
            }

            // Wait for the next action time
            if (time.TotalGameTime.TotalSeconds > nextActionTime)
            {
                wanderVector *= (1.0f / wanderVectorLength);

                Move(wanderVector);
            }
        }

        private void ChasePlayer(GameTime time)
        {
            Move(chaseVector);
        }

        private void AttackPlayer(GameTime time)
        {
            //ChasePlayer(time);

            float elapsedTimeSeconds = (float)time.TotalGameTime.TotalSeconds;
            if (elapsedTimeSeconds > nextActionTime)
            {
                Ray ray = new Ray(this.tank.Position, this.tank.ForwardVector);
                float? distance = player.BoxIntersects(ray);
                if (distance != null)
                {
                    //player.ReceiveDamage(attackDamage, this.tank.Orientation.Forward);
                    EnemyCannon.Fire();
                    if (Turn)
                        Turn = false;
                }
                else
                {
                    if (!Turn)
                        Turn = true;
                }
                
                nextActionTime = elapsedTimeSeconds + ATTACK_DELAY_SECONDS;
            }
        }

        public override void Update(GameTime time)
        {
            float elapsedTimeSeconds = (float)time.ElapsedGameTime.TotalSeconds;

            if (IsDead)
            {
                game.Components.Remove(this);
				//this.Dispose(); //Check this!!
                //return;
            }
            EnemyCannon.Update(time);
            EnemyCannonBallManager.Update(time);

            LinearVelocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
            //AI

            if ((float)time.TotalGameTime.TotalSeconds > fuzzyActionTime)
            {
                FuzzyBrain.UpdateParameters(player.Life,Life);
                fuzzyActionTime = (float)time.TotalGameTime.TotalSeconds + FUZZY_DELAY_SECONDS;
            }

            //
            chaseVector = player.tank.Position - tank.Position;
            float distanceToPlayer = chaseVector.Length();

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
                    if (distanceToPlayer <= attackDistance)
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
                            if (!ChasingOn)
                            { 
                                ChasingOn = true;
                                WanderOn = false;
                            }
                            if (!wallAvoidanceOn)
                                wallAvoidanceOn = true;
                    }
                    break;

                case EnemyState.AttackPlayer:
                    if (distanceToPlayer > attackDistance * 1.5f)
                        // Change state
                        state = EnemyState.ChasePlayer;
                    else
                    {
                        if (distanceToPlayer < 100)
                        { 
                            if (ChasingOn)
                                ChasingOn = false;

                            if (!Turn)
                                Turn = true;

                            tank.movement.Z = 0;
                        }
                        else
                        {
                            if (!ChasingOn)
                                ChasingOn = true;

                            if (Turn)
                                Turn = false;

                        }
                        if (wallAvoidanceOn)
                            wallAvoidanceOn = false;

                        if(WanderOn)
                            WanderOn = false;
                        AttackPlayer(time);
                    }
                    break;

                default:
                    break;
            }

           // if (state != EnemyState.Wander)
                ;// System.Console.WriteLine(state);

            /*if (AngularVelocity != Vector3.Zero)
            {
                rotate = AngularVelocity * elapsedTimeSeconds * 6;// *TankTurnSpeed;

                float facingDirection = tank.FacingDirection;
                //facingDirection += MathHelper.ToRadians(rotate.Y);
                facingDirection += MathHelper.ToRadians(rotate.Y);
                tank.FacingDirection = facingDirection;

                if ((player.tank.Position - tank.Position).Length() > 80)
                {
                    tank.movement.Z = -1;
                }
                else
                {
                    tank.movement.Z = 0;
                }
                    
                    //tank.Ride(new Vector3(0, 0, -1), terrain.MapInfo);
            }
            else if ((wanderPosition - tank.Position).Length() > 20.0f &&
                ((player.tank.Position - tank.Position).Length() > 80))
            {
                tank.movement.Z = -1;
            }*/
            //FindObstacles();
            
            tank.SteeringForce = Calculate();
            tank.Update(time);
            base.Update(time);


            tank_box.LinearVelocity = new Vector3((tank.Position.X - tank_box.CenterPosition.X) * 16, (tank.Position.Y + 30 - tank_box.CenterPosition.Y) * 16, (tank.Position.Z - tank_box.CenterPosition.Z) * 16);
            tank_box.OrientationMatrix = tank.Orientation;
            if (distanceToPlayer > 100)
            { 
                
                tank.setPosition(tank_box.CenterPosition);
            }
            //tank.Orientation = tank_box.OrientationMatrix;

            base.Update(time);
        }

        public override void Draw(GameTime time)
        {
            
            //Renderer.Line3D.Draw(tank.Position + new Vector3(0, 50, 0), wanderPosition+ new Vector3(0, 50, 0), Color.Red, null);
            if (DEBUG_MODE)
            {
                Renderer.Line3D.Draw(tank.Position + new Vector3(0, 50, 0),
                   tank.Position + SteeringForce * 1000 + new Vector3(0, 50, 0), Color.Tomato, null);
            }
            base.Draw(time);
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
            {  } // Enemy was hit                       

            var enemy4 = other.Tag as Wall;
            if (enemy4 != null && (other.IsDynamic == false))
                other.BecomeDynamic(1f);
        }
        #endregion
		
		#region AI
        Vector3 Seek()
        {
            Vector3 DesiredVelocity = Vector3.Normalize(player.tank.Position - tank.Position);
            return 4 * DesiredVelocity;//+tank.ForwardVector*40;//(DesiredVelocity - m_pVehicle->Velocity());
        }

        Vector3 Calculate()
        {

            Vector3 wanderForce = SteeringForce;
            SteeringForce = Vector3.Zero;
            Vector3 force;
            if (Turn)
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
            else
            {
                tank.isRotating = false;
            }
            if(ObstacleAvoidOn && (tank.isRotating == false))
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

            if (ChasingOn)
            {
                force = Seek();
                if (!AccumulateForce(force)) return SteeringForce;
            }
            

            return SteeringForce;

        }
        bool AccumulateForce(Vector3 force)
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
                SteeringForce += Vector3.Normalize(force) * MagnitudeRemaining;
            }

            return true;
        }
        #endregion
    }
}
