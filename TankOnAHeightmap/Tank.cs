#region File Description
//-----------------------------------------------------------------------------
// Tank.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using TanksOnAHeightmap.GameBase.Shapes;
using TanksOnAHeightmap.GameLogic;
using TanksOnAHeightmap.Helpers.Drawing;
#endregion

namespace TanksOnAHeightmap
{
	public class Tank : DrawableGameComponent
	{
		#region Constants
		public static bool DEBUG_MODE = false;

		// The radius of the tank's wheels. This is used when we calculate how fast they
		// should be rotating as the tank moves.
		const float tankWheelRadius = 52;

		// controls how quickly the tank can turn from side to side.
		const float TankTurnSpeed = .025f;

		const float max_speed = 200;
		const float mass = 1;
		#endregion


		#region Properties

		// This constant controls how quickly the tank can move forward and backward
		
		/// <summary>
		/// The position of the tank. The camera will use this value to position itself.
		/// </summary>
		public Vector3 Position
		{
			get { return position; }
			set { position = value; }
		}

		private Vector3 position;

		private float TankVelocity;
		public float Velocity
		{
			get { return TankVelocity; }
			set { TankVelocity = value; }
		}
		public Vector3 SteeringForce
		{
			get
			{
				return steeringForce;
			}
			set
			{
				steeringForce = value;
			}
		}
		public float TankWheelRadius
		{
			get { return tankWheelRadius; }
		}
		
		Vector3 steeringForce;
		public Vector3 movement;
		private Vector3 velocity;
		public bool isRotating;

		public bool IsEnemy
		{
			get { return isEnemy; }
			set { isEnemy = value; }
		}
		private bool isEnemy;
		/// <summary>
		/// The direction that the tank is facing, in radians. This value will be used
		/// to position and and aim the camera.
		/// </summary>
		public float FacingDirection
		{
			get { return facingDirection; }
			set { facingDirection = value; }
		}
		private float facingDirection;

		public Vector3 UpVector
		{
			get { return orientation.Up; }
		}
		public Vector3 ForwardVector
		{
			get { return orientation.Forward; }
		}

		public Matrix Orientation
		{
			get { return orientation; }
			set { orientation = value; }
		}
		public Matrix WorldMatrix
		{
			set { worldMatrix = value; }
			get { return worldMatrix; }
		}
		Matrix worldMatrix;
		
		public Vector3 ShootingDirection
		{
			get { return Vector3.Transform(turretBone.Transform.Forward,Orientation); }
			
		}

		public float ShootAt;
		//The amount of rotate on the turret of the tank
		public float TurretRotate
		{
			set { turretRotate = value; }
			get { return turretRotate; }
		}
		private float turretRotate;
		public float SteerRotationValue
		{
			set { steerRotationValue = value; }
			get { return steerRotationValue; }
		}
		private float steerRotationValue;
		//The amound of rotate on the cannon of the tank
		public float CannonRotate
		{
			set { cannonRotate = value; }
			get { return cannonRotate; }
		}
		private float cannonRotate;
		#endregion

		//Model of the tank
		public Model Model
		{
			set { model = value; }
			get { return model; }
		}

		#region Fields

		// The tank's model - a fearsome sight.
		Model model;

		// how is the tank oriented? We'll calculate this based on the user's input and
		// the heightmap's normals, and then use it when drawing.
		Matrix orientation = Matrix.Identity;

		// we'll use this value when making the wheels roll. It's calculated based on 
		// the distance moved.

		// we'll use this value when making the wheels roll. It's calculated based on 
		// the distance moved.
		public Matrix wheelRollMatrix = Matrix.Identity;
		public Matrix cannonMoveMatrix = Matrix.Identity;
		public Matrix turretMoveMatrix = Matrix.Identity;
		public Matrix steerRotationMatrix = Matrix.Identity;

		// The Simple Animation Sample at creators.xna.com explains the technique that 
		// we will be using in order to roll the tanks wheels. In this technique, we
		// will keep track of the ModelBones that control the wheels, and will manually
		// set their transforms. These next eight fields will be used for this
		// technique.
		ModelBone leftBackWheelBone;
		ModelBone rightBackWheelBone;
		ModelBone leftFrontWheelBone;
		ModelBone rightFrontWheelBone;
		ModelBone leftSteerBone;
		ModelBone rightSteerBone;
		public ModelBone turretBone;
		public ModelBone cannonBone;


		Matrix leftBackWheelTransform;
		Matrix rightBackWheelTransform;
		Matrix leftFrontWheelTransform;
		Matrix rightFrontWheelTransform;
		Matrix leftSteerTransform;
		Matrix rightSteerTransform;
		Matrix turretTransform;
		Matrix cannonTransform;

		Matrix viewMatrix;
		Matrix projectionMatrix;
		GraphicsDeviceManager graphics;
		#endregion
		ContentManager content;
		Terrain terrain;
		HeightMapInfo heightMapInfo;
		Game game;
		private TerrainUnit _parent;

		public Tank(Game game, TerrainUnit parent)
			: base(game)
		{
			this.game = game;
			this.TankVelocity = 2000;
			this._parent = parent;
			movement = new Vector3(0, 0, 0);
			terrain = game.Services.GetService(typeof(Terrain)) as Terrain;
			velocity = Vector3.Zero;
			isRotating = false;
			isEnemy = false;
		}

		public Tank(Game game, ContentManager content, GraphicsDeviceManager graphics,TerrainUnit parent)
			: base(game)
		{
			this.game = game;
			this.content = content;
			this.graphics = graphics;
			this.TankVelocity = 2000;
			this._parent = parent;
			movement = new Vector3(0, 0, 0);
			terrain = game.Services.GetService(typeof(Terrain)) as Terrain;
			velocity = Vector3.Zero;
			isRotating = false;
			isEnemy = false;
		}

		#region
		//Initialization
		public Matrix CanonTransform
		{
			get
			{
				return cannonTransform;
			}
		}
		public Matrix LeftBackWheelTransform
		{
			get
			{
				return leftBackWheelTransform;
			}
		}
		BoundingSphere boundingSphere;
		BoundingBox boundingBox;
		public BoundingSphere BoundingSphere
		{
			get
			{
				return boundingSphere;
			}
		}
		public BoundingBox BoundingBox
		{
			get
			{
				return boundingBox;
			}
		}
		public override void Initialize()
		{


			base.Initialize();
		}
		/// <summary>
		/// Called when the Game is loading its content. Pass in a ContentManager so the
		/// tank can load its model.
		/// </summary>
		public void Load()
		{
			LoadContent();
		}
		protected override void LoadContent()
		{
				   
			if(isEnemy)
				model = content.Load<Model>("Models/Tank");
			else
				model = content.Load<Model>("Tank");

			  foreach (ModelMesh mesh in model.Meshes)
			  {
				  boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mesh.BoundingSphere);
			  }
			  boundingBox = BoundingBox.CreateFromSphere(boundingSphere);

			// as discussed in the Simple Animation Sample, we'll look up the bones
			// that control the wheels.
			leftBackWheelBone = model.Bones["l_back_wheel_geo"];
			rightBackWheelBone = model.Bones["r_back_wheel_geo"];
			leftFrontWheelBone = model.Bones["l_front_wheel_geo"];
			rightFrontWheelBone = model.Bones["r_front_wheel_geo"];
			leftSteerBone = model.Bones["l_steer_geo"];
			rightSteerBone = model.Bones["r_steer_geo"];
			turretBone = model.Bones["turret_geo"];
			cannonBone = model.Bones["canon_geo"];

			// Also, we'll store the original transform matrix for each animating bone.
			leftBackWheelTransform = leftBackWheelBone.Transform;
			rightBackWheelTransform = rightBackWheelBone.Transform;
			leftFrontWheelTransform = leftFrontWheelBone.Transform;
			rightFrontWheelTransform = rightFrontWheelBone.Transform;
			leftSteerTransform = leftSteerBone.Transform;
			rightSteerTransform = rightSteerBone.Transform;
			turretTransform = turretBone.Transform;
			cannonTransform = cannonBone.Transform;
		}

		#endregion


		#region Update and Draw
		public void Move(float facingDirection, Vector3 movement, HeightMapInfo heightMapInfo)
		{
			// next, we'll create a rotation matrix from the direction the tank is 
			// facing, and use it to transform the vector.

			Matrix tmpOrientation = Matrix.CreateRotationY(facingDirection*0.05f);
			Vector3 velocity = Vector3.Transform(movement, tmpOrientation);
			
			velocity = Vector3.Transform(movement, Orientation);
			velocity *= Velocity;
			tmpOrientation *= Orientation;
			Orientation = tmpOrientation;
			
			// Now we know how much the user wants to move. We'll construct a temporary
			// vector, newPosition, which will represent where the user wants to go. If
			// that value is on the heightmap, we'll allow the move.
			Vector3 newPosition = Position + velocity;
			if (heightMapInfo.IsOnHeightmap(newPosition))
			{
				// now that we know we're on the heightmap, we need to know the correct
				// height and normal at this position.
				Vector3 normal;
				heightMapInfo.GetHeightAndNormal(newPosition,
					out newPosition.Y, out normal);

				// As discussed in the doc, we'll use the normal of the heightmap
				// and our desired forward direction to recalculate our orientation
				// matrix. It's important to normalize, as well.
				orientation.Up = normal;

				orientation.Right = Vector3.Cross(orientation.Forward, orientation.Up);
				orientation.Right = Vector3.Normalize(orientation.Right);

				orientation.Forward = Vector3.Cross(orientation.Up, orientation.Right);
				orientation.Forward = Vector3.Normalize(orientation.Forward);

				// now we need to roll the tank's wheels "forward." to do this, we'll
				// calculate how far they have rolled, and from there calculate how much
				// they must have rotated.
				float distanceMoved = Vector3.Distance(Position, newPosition);
				//distanceMoved = distanceMoved / 12.0f;
				float theta = distanceMoved / tankWheelRadius;
				int rollDirection = movement.Z > 0 ? 1 : -1;

				wheelRollMatrix *= Matrix.CreateRotationX(theta * rollDirection);

				
				// once we've finished all computations, we can set our position to the
				// new position that we calculated.
				position = newPosition;
				worldMatrix = orientation * Matrix.CreateTranslation(Position);
			}
		}

		public void Move(GameTime gameTime)
		{
			// next, we'll create a rotation matrix from the direction the tank is 
			// facing, and use it to transform the vector.
			float elapsedTimeSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

			//float directionForce = 0;


		  /*  if (steeringForce != null && (Vector3)steeringForce != Vector3.Zero)
			{
				directionForce = -((Vector3)steeringForce).X;

				float turnAmount = MathHelper.Clamp(directionForce, -1, 1);
				FacingDirection += turnAmount * .045f;
			}*/
		   /* //TEst
			Vector3 rotate;
			if (steeringForce != null && steeringForce != Vector3.Zero)
			{
				steeringForce.X = -steeringForce.X;
				rotate = AngularVelocity(steeringForce) * elapsedTimeSeconds;

				facingDirection += MathHelper.ToRadians(rotate.Y);
			}
			//*/
			//orientation = Matrix.CreateRotationY(FacingDirection);


			//if (steeringForce != null && (Vector3)steeringForce != Vector3.Zero)
			//   steeringForce = Vector3.Transform((Vector3)steeringForce, orientation * 
			//                                        Matrix.CreateTranslation(position));
			//if(!_parent.playerFlag)
				if (movement.Z == 0)
					movement.X = 0;
				else
					movement.X *= 0.03f;

			//Vector3 velocity;
			Vector3 final_velocity = Vector3.Transform(movement, Orientation);
			
			//velocity *= TankVelocity;
			Vector3 acceleration;
			velocity = Vector3.Zero;


				if (steeringForce != null && 
					(Vector3)steeringForce != Vector3.Zero &&
					(movement.Z != 0 || isRotating))//-1
				{

					acceleration = ((Vector3)steeringForce) / mass;
					//if(velocity != Vector3.Zero)
					velocity += acceleration * elapsedTimeSeconds;
				}




			final_velocity += velocity;
			// Now we know how much the user wants to move. We'll construct a temporary
			// vector, newPosition, which will represent where the user wants to go. If
			// that value is on the heightmap, we'll allow the move.
			Vector3 newPosition;
			if (!isRotating)
				newPosition = Position + TankVelocity * 100 * final_velocity * elapsedTimeSeconds;
			else
				newPosition = Position;
			if (terrain.MapInfo.IsOnHeightmap(newPosition))
			{
				// now that we know we're on the heightmap, we need to know the correct
				// height and normal at this position.
				Vector3 normal;
				terrain.MapInfo.GetHeightAndNormal(newPosition,
					out newPosition.Y, out normal);


				// As discussed in the doc, we'll use the normal of the heightmap
				// and our desired forward direction to recalculate our orientation
				// matrix. It's important to normalize, as well.
				//orientation.Forward = Vector3.Normalize(velocity);
				if (final_velocity != Vector3.Zero)
				{
					orientation.Forward = Vector3.Normalize(final_velocity);
					if(movement.Z == 1)
						orientation.Forward = -Vector3.Normalize(final_velocity);
				}

				orientation.Up = normal;

				orientation.Right = Vector3.Cross(orientation.Forward, orientation.Up);
				orientation.Right = Vector3.Normalize(orientation.Right);

				orientation.Forward = Vector3.Cross(orientation.Up, orientation.Right);
				orientation.Forward = Vector3.Normalize(orientation.Forward);
				
				// now we need to roll the tank's wheels "forward." to do this, we'll
				// calculate how far they have rolled, and from there calculate how much
				// they must have rotated.
				float distanceMoved = Vector3.Distance(Position, newPosition);
				float theta = distanceMoved / TankWheelRadius;
				int rollDirection = movement.Z > 0 ? 1 : -1;

				wheelRollMatrix *= Matrix.CreateRotationX(theta * rollDirection);

				// once we've finished all computations, we can set our position to the
				// new position that we calculated.
				position = newPosition;
				worldMatrix = orientation * Matrix.CreateTranslation(Position);
			}
		}

		static float LINEAR_VELOCITY_CONSTANT = 10.0f;
		static float ANGULAR_VELOCITY_CONSTANT = 20.0f;
		private Vector3 AngularVelocity(Vector3 direction)
		{
			if (direction.Length() != 1)
				direction.Normalize();

			Vector3 LinearVelocity = direction * LINEAR_VELOCITY_CONSTANT;

			// Angle between heading and move direction
			float radianAngle = (float)Math.Acos(Vector3.Dot(ForwardVector, direction));
			if (radianAngle >= 0.1f)
			{
				// Find short side to rodade CW or CCW
				float sideToRotate = Vector3.Dot(Orientation.Right, direction);

				Vector3 rotationVector = new Vector3(0, ANGULAR_VELOCITY_CONSTANT * radianAngle, 0);
				if (sideToRotate > 0)
				{
					return -rotationVector;
				}
				else
				{
					return rotationVector;

				}
			}
			return Vector3.Zero;

		}
		public override void Draw(GameTime gameTime)
		{
			GraphicsDevice device = graphics.GraphicsDevice;

			device.BlendState = BlendState.Opaque;
			device.DepthStencilState = DepthStencilState.Default;
			device.SamplerStates[0] = SamplerState.LinearWrap;


			// Apply matrices to the relevant bones, as discussed in the Simple 
			// Animation Sample.
			leftBackWheelBone.Transform = wheelRollMatrix * leftBackWheelTransform;
			rightBackWheelBone.Transform = wheelRollMatrix * rightBackWheelTransform;
			leftFrontWheelBone.Transform = wheelRollMatrix * leftFrontWheelTransform;
			rightFrontWheelBone.Transform = wheelRollMatrix * rightFrontWheelTransform;
			leftSteerBone.Transform = steerRotationMatrix * leftSteerTransform;
			rightSteerBone.Transform = steerRotationMatrix * rightSteerTransform;
			cannonBone.Transform = cannonMoveMatrix * cannonTransform;
			turretBone.Transform = turretMoveMatrix * turretTransform;

			// now that we've updated the wheels' transforms, we can create an array
			// of absolute transforms for all of the bones, and then use it to draw.
			Matrix[] boneTransforms = new Matrix[model.Bones.Count];
			model.CopyAbsoluteBoneTransformsTo(boneTransforms);

			// calculate the tank's world matrix, which will be a combination of our
			// orientation and a translation matrix that will put us at at the correct
			// position.
			

			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (BasicEffect effect in mesh.Effects)
				{
					effect.World = boneTransforms[mesh.ParentBone.Index] * worldMatrix;
					effect.View = viewMatrix;
					effect.Projection = projectionMatrix;

					effect.EnableDefaultLighting();
					effect.PreferPerPixelLighting = true;

				   // Set the fog to match the black background color

					effect.FogEnabled = true;
					effect.FogColor = new Vector3(0.5f, 0.5f, 0.8f);
					effect.FogStart = 1000;
					effect.FogEnd = 3500;                    
					effect.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);
				}
				mesh.Draw();
			}

			if (DEBUG_MODE)
			{ 
				if (steeringForce != null && (Vector3)steeringForce != Vector3.Zero)
				Renderer.Line3D.Draw(new Vector3(0, 50, 0),
					steeringForce + new Vector3(0, 50, 0), Color.Blue, Matrix.CreateTranslation(Position));

				Renderer.Line3D.Draw(position + new Vector3(0, 50, 0),
					position+velocity + new Vector3(0, 50, 0), Color.Blue, null);

				//Axis X
				Renderer.Line3D.Draw( Position,
					Position + new Vector3(300, 50, 0), Color.Red, null);
				//Axis X
				Renderer.Line3D.Draw(Position,
					Position + new Vector3(0, 50, 300), Color.Blue, null);
			}
		}

		#endregion

		/// <summary>
		/// Sets the camera view and projection matrices
		/// that will be used to draw this particle system.
		/// </summary>
		public void SetCamera(Matrix view, Matrix projection)
		{
			viewMatrix = view;
			projectionMatrix = projection;
		}

		public override void Update(GameTime gameTime)
		{
				Move(gameTime);
				float elapsedTimeSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (Math.Abs(ShootAt) > 0.05)
				{
					float amountToMove = elapsedTimeSeconds;

					if (ShootAt < 0)
					{
						amountToMove *= -1;
						amountToMove = MathHelper.Clamp(amountToMove, ShootAt, 0);
					}
					else if (ShootAt > 0)
					{
						amountToMove = MathHelper.Clamp(amountToMove, 0, ShootAt);
					}

					turretMoveMatrix *= Matrix.CreateRotationY(amountToMove);

					ShootAt -= amountToMove;
				}
		  
		}

		public void setPosition(Vector3 position)
		{
			this.position = position;

		}

		public void setOnGround(HeightMapInfo heightMapInfo)
		{
			if (heightMapInfo.IsOnHeightmap(position))
			{
				// now that we know we're on the heightmap, we need to know the correct
				// height and normal at this position.
				Vector3 normal;
				heightMapInfo.GetHeightAndNormal(position,
					out position.Y, out normal);


				// As discussed in the doc, we'll use the normal of the heightmap
				// and our desired forward direction to recalculate our orientation
				// matrix. It's important to normalize, as well.
				orientation.Up = normal;

				orientation.Right = Vector3.Cross(orientation.Forward, orientation.Up);
				orientation.Right = Vector3.Normalize(orientation.Right);

				orientation.Forward = Vector3.Cross(orientation.Up, orientation.Right);
				orientation.Forward = Vector3.Normalize(orientation.Forward);

			}
		}
	}
}
