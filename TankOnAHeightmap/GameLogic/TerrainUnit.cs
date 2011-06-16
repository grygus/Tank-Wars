using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using TanksOnAHeightmap.GameBase;
using TanksOnAHeightmap.GameBase.Cameras;
using TanksOnAHeightmap.GameBase.Effects;
using TanksOnAHeightmap.GameBase.Effects.ParticleSystems;
using TanksOnAHeightmap.GameBase.Physics;
using TanksOnAHeightmap.GameBase.Shapes;
using TanksOnAHeightmap.GameLogic.AI.FuSM;
using TanksOnAHeightmap.Helpers.Drawing;

namespace TanksOnAHeightmap.GameLogic
{
    public class TerrainUnit : GameObject
    {
        public const float TankTurnSpeed = .025f;
        public static float MIN_GRAVITY = -1.5f;
        public static float GRAVITY_ACCELERATION = 4.0f;
        public static bool DEBUG_MODE;
        private readonly ParticleEmitter emiter;
        private readonly ParticleSystem part1;
        protected bool boost;
        public bool playerFlag;

        private BoundingBox boundingBox;
        private BoundingSphere boundingSphere;
        protected ChaseCamera camera;
        protected ParticleSystem explosionParticles;
        protected ParticleSystem explosionSmokeParticles;
        private bool needUpdateCollision;
        public Physics phisics;
        public Tank tank;
        protected Terrain terrain;
        private Color boundingColor;
        public HealthManager healthManager;

        #region AI

        public static float DETECTION_DISTANCE = -80.0f;
        private BoundingBox _detectionBox;
        private GameObject closesdObstacle;
        public float detectionDistance;
        public float minDetectionDistance = -120.0f;

        public float DetectionDistance
        {
            get { return detectionDistance; }
            set
            {
                _detectionBox.Min.Z = value;
                detectionDistance = value;
            }
        }

        public BoundingBox DetectionBox
        {
            get { return _detectionBox; }
            set { _detectionBox = value; }
        }

        public float Velocity
        {
            get { return tank.Velocity; }
            set
            {
                tank.Velocity = value;
                DetectionDistance = minDetectionDistance +
                                    (tank.Velocity/5.0f)*
                                    minDetectionDistance;
            }
        }

        public Vector3 SteeringForce { get; set; }
        public Vector3[] Feelers { get; set; }

        #endregion


        #region FuzzyStateMachine

        public FuSMAIControl FuzzyControl;


        #endregion


        #region World

        public Trees[] WorldTrees { set; get; }
        public List<TerrainUnit> Oponents;

        #endregion

        #region Properties

        public override Vector3 Position
        {
            get { return tank.Position; }
            set { tank.Position = Position; }
        }

        public int Life { get; set; }

        public int MaxLife { get; set; }

        public float Speed { get; set; }

        public Vector3 LinearVelocity { get; set; }

        public Vector3 AngularVelocity { get; set; }

        public float GravityVelocity { get; set; }

        public virtual Matrix Transformation
        {
            get
            {
                Matrix helper = Matrix.CreateTranslation(tank.Position);
                return helper;
            }
            set
            {
                //    tank.Transformation = value;

                //    // Upate
                tank.Position = value.Translation;
                UpdateHeight(0);
            }
        }

        public override BoundingBox BoundingBox
        {
            get
            {
                if (needUpdateCollision)
                    UpdateCollision();

                return boundingBox;
            }
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                if (needUpdateCollision)
                    UpdateCollision();

                return boundingSphere;
            }
        }

        public bool IsOnTerrain { get; private set; }

        public bool IsDead { get; set; }

        #endregion

        /*TODO: make class with all informations about world*/

        public TerrainUnit(Game game)
            : base(game)
        {
            Oponents = new List<TerrainUnit>();
            FuzzyControl = new FuSMAIControl(this);
            tank = new Tank(game,this);
        }

        public TerrainUnit(Game game, ContentManager content, GraphicsDeviceManager graphics)
            : base(game)
        {
            FuzzyControl = new FuSMAIControl(this);


            GravityVelocity = 0.0f;
            IsOnTerrain = false;
            IsDead = false;
            needUpdateCollision = true;
            boost = false;
            boundingColor = Color.Black;
            tank = new Tank(game, content, graphics,this);

            part1 = new DustParticleSystem(game, content);
            part1.Initialize();

            game.Components.Add(part1);
            emiter = new ParticleEmitter(part1, 60, tank.Position);

            part1.DrawOrder = 400;
        }

        protected override void LoadContent()
        {
            //model = new Content.Load<mode(Game);
            tank.Load();
            BoundingBox box = tank.BoundingBox; // new BoundingBox(tank.BoundingBox.Min, tank.BoundingBox.Max);
            box.Min += new Vector3(0, 0, DetectionDistance);
            DetectionBox = box;
            //Velocity = 2;
            // Put the player above the terrain
            UpdateHeight(0);
            IsOnTerrain = true;
        }

        public override void Initialize()
        {
            camera = Game.Services.GetService(typeof (ChaseCamera)) as ChaseCamera;
            terrain = Game.Services.GetService(typeof (Terrain)) as Terrain;
            explosionParticles = Game.Services.GetService(typeof (ExplosionParticleSystem)) as ExplosionParticleSystem;
            explosionSmokeParticles =
                Game.Services.GetService(typeof (ExplosionSmokeParticleSystem)) as ExplosionSmokeParticleSystem;


            base.Initialize();
        }


        public override void Update(GameTime time)
        {
            var elapsedTimeSeconds = (float) time.ElapsedGameTime.TotalSeconds;
            //tank.Update(time);
            // Update the height and collision volumes
            if (LinearVelocity != Vector3.Zero || GravityVelocity != 0.0f)
            {
                UpdateHeight(elapsedTimeSeconds);
                needUpdateCollision = true;
            }

            if (boundingSphere.Center != Transformation.Translation)
                needUpdateCollision = true;

            if (boost)
                emiter.Update(time, tank.Position);


            //explosionParticles.Update(time);

            base.Update(time);
        }


        private void UpdateHeight(float elapsedTimeSeconds)
        {
            // Get terrain height
            float terrainHeight;
            Vector3 norm;
            terrain.MapInfo.GetHeightAndNormal(Transformation.Translation, out terrainHeight, out norm);
            Vector3 newPosition = Transformation.Translation;

            // Unit is on terrain
            float HEIGHT_EPSILON = 2.0f;
            if (Transformation.Translation.Y <= (terrainHeight + HEIGHT_EPSILON) && GravityVelocity <= 0)
            {
                IsOnTerrain = true;
                GravityVelocity = 0.0f;
                newPosition.Y = terrainHeight;

                //// Update camera chase speed and unit movement speed (hack)
                //if (adjustJumpChanges)
                //{
                //    ThirdPersonCamera camera = cameraManager.ActiveCamera as ThirdPersonCamera;
                //    camera.ChaseSpeed /= 4.0f;

                //    speed /= 1.5f;
                //    adjustJumpChanges = false;
                //}
            }
                // Unit is above the terrain
            else
            {
                IsOnTerrain = false;
                // Gravity
                if (GravityVelocity > MIN_GRAVITY)
                    GravityVelocity -= GRAVITY_ACCELERATION*elapsedTimeSeconds;
                newPosition.Y = Math.Max(terrainHeight, Transformation.Translation.Y + GravityVelocity);
            }
            //Matrix tmp = Transformation;
            //tmp.Translation = newPosition;
            //Transformation = tmp;
        }

        public void  SetBoundingSphereColor(Color color)
        {
            boundingColor = color;
        }

        public override void Draw(GameTime time)
        {
            tank.setOnGround(terrain.MapInfo);
            tank.Draw(time);
            part1.SetCamera(camera.View, camera.Projection);
            part1.Draw(time);
            
            if (DEBUG_MODE)
            {
                Renderer.BoundingBox3D.Draw(DetectionBox, Color.Black, tank.WorldMatrix);
                Renderer.Line3D.Draw(new Vector3(0, 25, 0), new Vector3(0, 25, -150f), Color.Red, tank.WorldMatrix);
                if (SteeringForce != Vector3.Zero)
                    Renderer.Line3D.Draw(new Vector3(0, 50, 0), SteeringForce + new Vector3(0, 50, 0), Color.Red,
                                         tank.WorldMatrix);

                if (closesdObstacle != null)
                    Renderer.Line3D.Draw(tank.Position + new Vector3(0, 50, 0),
                                         closesdObstacle.Position + new Vector3(0, 50, 0), Color.Red, null);

                if (Feelers != null)
                {
                    Renderer.Line3D.Draw(tank.Position + new Vector3(0, 50, 0),
                                         tank.Position + Feelers[0] + new Vector3(0, 50, 0), Color.Black, null);
                    Renderer.Line3D.Draw(tank.Position + new Vector3(0, 50, 0),
                                         tank.Position + Feelers[1] + new Vector3(0, 50, 0), Color.Black, null);
                    Renderer.Line3D.Draw(tank.Position + new Vector3(0, 50, 0),
                                         tank.Position + Feelers[2] + new Vector3(0, 50, 0), Color.Black, null);
                }
                if (closestVec != null)
                    Renderer.Line3D.Draw(tank.Position + new Vector3(0, 50, 0),
                                         tank.Position + closestVec + new Vector3(0, 50, 0), Color.Red, null);
                if (detectVec != null)
                    Renderer.Line3D.Draw(tank.Position + new Vector3(0, 50, 0),
                                         tank.Position + detectVec + new Vector3(0, 50, 0), Color.Blue, null);
            }

            Renderer.BoundingBox3D.Draw(BoundingBox, Color.Black, null);
        }

        public virtual void ReceiveDamage(int damageValue)
        {
            Life = Math.Max(0, Life - damageValue);
            if (Life <= 0)
                IsDead = true;

            /*isHit = true;

            if (isHit)
            {
                for (int i = 0; i < 30; i++)
                    //explosionParticles.AddParticle(tank.Position+new Vector3(0,40,0), new Vector3(0.0f,0.0f,0.0f));
                    explosionParticles.AddParticle(tank.Position + new Vector3(0, 45, 0), -direction * 30);
                for (int i = 0; i < 50; i++)
                    explosionSmokeParticles.AddParticle(tank.Position + new Vector3(0, 45, 0), -direction * 60);

                isHit = false;
            }*/
        }
        //TODO: Detect why BoundingBox do not rotate properly
        private void UpdateCollision()
        {
            // Do not support scale

            // Update bounding box
            boundingBox = tank.BoundingBox;
            //boundingBox.Min = Vector3.Transform(boundingBox.Max, tank.WorldMatrix);
            //boundingBox.Max = Vector3.Transform(tmp, tank.WorldMatrix);
            
            boundingBox.Min += Transformation.Translation;
            boundingBox.Max += Transformation.Translation;

            
            // Update bounding sphere
            boundingSphere = tank.BoundingSphere;
            boundingSphere.Center = Transformation.Translation;

            needUpdateCollision = false;
        }

        public float? BoxIntersects(Ray ray)
        {
            Matrix inverseTransformation = Matrix.Invert(Transformation);
            ray.Position = Vector3.Transform(ray.Position, inverseTransformation);
            ray.Direction = Vector3.TransformNormal(ray.Direction, inverseTransformation);

            return tank.BoundingBox.Intersects((ray));
        }

        #region AI

        private Vector3 closestVec;
        private Vector3 detectVec;

        public void CreateFeelers()
        {
            int feelersLength = 120;
            Feelers = new Vector3[3];

            Feelers[0] = Vector3.Normalize(new Vector3(0, 0, -1));
            Feelers[1] = Vector3.Normalize(new Vector3(-1, 0, -1));
            Feelers[2] = Vector3.Normalize(new Vector3(1, 0, -1));

            Feelers[0] = Vector3.Transform(Feelers[0], tank.Orientation)*feelersLength;
            Feelers[1] = Vector3.Transform(Feelers[1], tank.Orientation)*feelersLength*0.5f;
            Feelers[2] = Vector3.Transform(Feelers[2], tank.Orientation)*feelersLength*0.5f;
        }

        public Vector3 WallAvoidance()
        {
            CreateFeelers();
            Vector3 force = Vector3.Zero;
            float DistToThisIP = 0.0f;
            float DistToClosestIP = float.MaxValue;
            //this will hold an index into the vector of walls
            int ClosestWall = -1;
            Vector3 ClosestPoint = Vector3.Zero; //holds the closest intersection point
            Wall currentWall;
            //examine each feeler in turn

            for (int flr = 0; flr < Feelers.Length; flr++)
            {
                //run through each wall checking for any intersection points
                for (int w = 0; w < phisics.WallStreet.Count; w++)
                {
                    //int w = 1;

                    currentWall = phisics.WallStreet[w];
                    //tmp = currentWall.BoundingBox;
                    Vector3 start = tank.Position;
                    start.Y = currentWall.BoundingBox.Min.Y - currentWall.BoundingBox.Max.Y;
                    start.Y /= 2;
                    start.Y += currentWall.BoundingBox.Max.Y;
                    var ray = new Ray(start, Vector3.Normalize(Feelers[flr]));

                    float? dist = ray.Intersects(currentWall.BoundingBox);

                    if (dist != null)
                    {
                        DistToThisIP = (float) dist;
                        DistToThisIP =
                            ((tank.Position + (Vector3.Normalize(Feelers[flr])*(float) dist)) - tank.Position).Length();
                        detectVec = (Vector3.Normalize(Feelers[flr])*(float) dist);
                        if (Feelers[flr].Length() >= DistToThisIP)
                        {
                            closestVec = (Vector3.Normalize(Feelers[flr])*(float) dist);
                            //is this the closest found so far? If so keep a record
                            if (DistToThisIP < DistToClosestIP)
                            {
                                DistToClosestIP = DistToThisIP;
                                ClosestWall = w;
                                ClosestPoint = tank.Position + (Vector3.Normalize(Feelers[flr])*(float) dist);
                            }
                        }
                    }
                } //next wall
                //if an intersection point has been detected, calculate a force
                //that will direct the agent away
                if (ClosestWall >= 0)
                {
                    //calculate by what distance the projected position of the agent
                    //will overshoot the wall
                    //Vector3 OverShoot = (Feelers[flr] - ClosestPoint)/100;
                    Vector3 OverShoot = (tank.Position + Feelers[flr]) - ClosestPoint;
                    OverShoot /= 40*(flr + 1)*0.35f;
                    //create a force in the direction of the wall normal, with a
                    //magnitude of the overshoot
                    float angle;
                    float angle2;
                    Vector3 normal = phisics.WallStreet[ClosestWall].Normal();
                    Vector3 invNormal = -phisics.WallStreet[ClosestWall].Normal();
                    Vector3 forward = tank.ForwardVector;
                    Vector3.Dot(ref normal, ref forward, out angle);
                    Vector3.Dot(ref invNormal, ref forward, out angle2);
                    if (angle2 > angle)
                        force += phisics.WallStreet[ClosestWall].Normal()*OverShoot.Length();
                    else
                        force += -phisics.WallStreet[ClosestWall].Normal()*OverShoot.Length();
                }
            } //next feeler
            return force;
        }

        public Vector3 FindObstacles()
        {
            var nearTrees = new List<GameObject>();
            foreach (Trees tree in WorldTrees)
            {
                if ((tree.Position - Transformation.Translation).Length() < Math.Abs(DetectionBox.Min.Z))
                {
                    //System.Console.WriteLine((tree.Position - Transformation.Translation).Length());
                    nearTrees.Add(tree);
                }
            }

            foreach (TerrainUnit unit in Oponents)
            {
                if ((unit.Position - Transformation.Translation).Length() < Math.Abs(DetectionBox.Min.Z))
                {
                    nearTrees.Add(unit);
                }
            }

            return FindNextObstacle(nearTrees);
        }

        public Vector3 FindNextObstacle(List<GameObject> nearTrees)
        {
            Matrix inverseTransformation = Matrix.Invert(tank.WorldMatrix);
            var localPosition = new Vector3[nearTrees.Count];
            int i = -1;
            foreach (GameObject tree in nearTrees)
            {
                i += 1;
                localPosition[i] = Vector3.Transform(tree.Position, inverseTransformation);
            }

            float objectRadius;
            float DistToClosestIP = float.MaxValue;
            int closest = -1;
            Vector3 boundingMax;
            Vector3 boundingMin;

            for (i = 0; i < localPosition.Length; i++)
            {
                if (localPosition[i].Z <= 100)
                {
                    //1
                    boundingMax = nearTrees[i].BoundingBox.Max;
                    boundingMin = nearTrees[i].BoundingBox.Min;
                    boundingMax.Y = 0;
                    boundingMin.Y = 0;
                    objectRadius = (boundingMax - boundingMin).Length()/2.0f;

                    float ExpandedRadius = objectRadius + (DetectionBox.Max.X - DetectionBox.Min.X)/2.0f;
                    if (Math.Abs(localPosition[i].X) < ExpandedRadius)
                    {
                        float cX = -localPosition[i].Z;
                        float cY = localPosition[i].X;

                        var SqrtPart = (float) Math.Sqrt(ExpandedRadius*ExpandedRadius - cY*cY);

                        float ip = cX - SqrtPart;

                        if (ip <= 0.0) //!
                        {
                            ip = cX + SqrtPart;
                        }

                        if (ip < DistToClosestIP)
                        {
                            DistToClosestIP = ip;
                            closest = i;
                        }
                    }
                }
            }

            var SteeringForce = new Vector3(0, 0, 0);

            if (closest != -1)
            {
                //the closer the agent is to an object, the stronger the 
                //steering force should be
                boundingMax = nearTrees[closest].BoundingBox.Max;
                boundingMin = nearTrees[closest].BoundingBox.Min;
                boundingMax.Y = 0;
                boundingMin.Y = 0;
                objectRadius = (boundingMax - boundingMin).Length()/2.0f;

                float multiplier = 1.0f + (DetectionDistance - localPosition[closest].Z)/
                                   DetectionDistance;

                //calculate the lateral force ClosestIntersectingObstacle->BRadius()

                //SteeringForce.X = (objectRadius - localPosition[closest].X) * multiplier * 0.01f;
                //SteeringForce.X = -1.0f/(localPosition[closest].X) * multiplier*10;
                //SteeringForce.X = -1.0f / (localPosition[closest].X) * multiplier * 10;
                SteeringForce.X = ((DetectionBox.Max.X - DetectionBox.Min.X)/2.0f) + objectRadius;
                if (localPosition[closest].X < 0.0f)
                {
                    SteeringForce.X = SteeringForce.X + localPosition[closest].X;
                }
                else
                {
                    SteeringForce.X = -1.0f*(SteeringForce.X - localPosition[closest].X);
                }
                SteeringForce.X *= multiplier*0.05f;
                //apply a braking force proportional to the obstacles distance from
                //the vehicle. 
                //float BrakingWeight = 0.1f;

                // SteeringForce.Z = (objectRadius -
                //              localPosition[closest].Z) *
                //            BrakingWeight;

                //System.Console.WriteLine(SteeringForce);
                //return Vector3.Transform(SteeringForce, tank.Orientation * Transformation);
                //return SteeringForce;
                tank.SteeringForce = Vector3.Transform(SteeringForce, tank.Orientation);
                closesdObstacle = nearTrees[closest];
                return tank.SteeringForce;
            }
            else
            {
                //return Vector3.Zero;
                tank.SteeringForce = Vector3.Zero;
                closesdObstacle = null;
                return Vector3.Zero;
            }
        }

        #endregion

        #region

        public Vector3 FuzzySteeringForce = Vector3.Zero;
        public float FuzzyMaxSteeringForce = 5;

        public bool FuzzyAccumulateForce(Vector3 force)
        {
            float magnitudeSoFar = FuzzySteeringForce.Length();
            float magnitudeRemaining = FuzzyMaxSteeringForce - magnitudeSoFar;
            if (magnitudeRemaining <= 0.0) return false;

            float magnitudeToAdd = force.Length();

            if (magnitudeToAdd < magnitudeRemaining)
            {

                FuzzySteeringForce += force;
            }
            else
            {
                //add it to the steering force
                FuzzySteeringForce += Vector3.Normalize(force) * magnitudeRemaining;
            }

            return true;
        }

        #endregion

        /*     public bool TankCollision()
             {
                 //bounding sphere for this tank
              /*   BoundingSphere tankBox = model.Meshes[0].BoundingSphere;
                 tankBox.Center = position;
                 tankBox.Radius *= scale;

                 //bounding sphere for enemytank
                 BoundingSphere tankBox2 = tank2.model.Meshes[0].BoundingSphere;
                 tankBox2.Center = tank2.position;
                 tankBox2.Radius *= (scale + .2f);
            
                 bool result = false;
                 tank.BoundingSphere.Intersects(ref Microsoft.Xna.Framework.BoundingSphere sphere, result);
                 return result;
             }*/

        public void ShootAt(float newDir)
        {
            if (Math.Abs(newDir) > 0.2)
                tank.ShootAt = newDir;
        }
    }
}