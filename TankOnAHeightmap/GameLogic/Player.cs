using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using TanksOnAHeightmap.GameBase.Cameras;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using BEPUphysics;
using TanksOnAHeightmap.Helpers.Drawing;

namespace TanksOnAHeightmap.GameLogic
{
	public class Player : TerrainUnit
	{
		static float MAX_WAIST_BONE_ROTATE = 0.50f;
		static int WAIST_BONE_ID = 2;
		static int RIGHT_HAND_BONE_ID = 15;
		// controls how quickly the tank can turn from side to side.


		public enum PlayerAnimations
		{
			Idle = 0,
			Run,
			Aim,
			Shoot
		}

		// Player type
		UnitTypes.PlayerType playerType;
		// Player weapon
		//PlayerWeapon playerWeapon;
		// Camera chase position
		Vector3[] chaseOffsetPosition;
		// Rotate torso bone
		//float rotateWaistBone;
		//float rotateWaistBoneVelocity;
		HeightMapInfo heightMapInfo;
		Enemy[] enemyList;

		Enemy aimEnemy;
		bool shot;
		int numEnemiesAlive;

		public bool enemyCollision;
		float start; // collision start

		#region Properties

		public bool EnemyCollision
		{
			get { return enemyCollision; }
			set { enemyCollision = value; }
		}
		public Int32 NumEnemiesAlive
		{
			get
			{
				return numEnemiesAlive;
			}
			set
			{
				numEnemiesAlive = value;
			}
		}

		public Enemy AimEnemy
		{
			get
			{
				return aimEnemy;
			}
			set
			{
				aimEnemy = value;
			}
		}

		public Enemy[] EnemyList
		{
			get
			{
				return enemyList;
			}
			set
			{
				enemyList = value;
			}
		}

		public Vector3[] ChaseOffsetPosition
		{
			get
			{
				return chaseOffsetPosition;
			}
			set
			{
				chaseOffsetPosition = value;
			}
		}

		#endregion

		public Player(Game game, ContentManager content, GraphicsDeviceManager graphics, UnitTypes.PlayerType playerType)
			: base(game, content, graphics)
		{
			this.playerType = playerType;
			//heightMapInfo = game.Services.GetService(typeof(HeightMapInfo)) as HeightMapInfo;
			shot = false;

			EnemyCollision = false;
			start = 0;
			//tank.Velocity = 3;
			Velocity = 3;
		}

		protected override void LoadContent()
		{
			//Load(UnitTypes.PlayerModelFileName[(int)playerType]);

			// Unit configurations
			Life = UnitTypes.PlayerLife[(int)playerType];
			MaxLife = Life;
			Speed = UnitTypes.PlayerSpeed[(int)playerType];

			//SetAnimation(Player.PlayerAnimations.Idle, false, true, false);

			base.LoadContent();
		}
		/// <summary>
		/// This function is called when the game is Updating in response to user input.
		/// It'll move the tank around the heightmap, and update all of the tank's 
		/// necessary state.
		/// </summary>
		public void HandleInput(GamePadState currentGamePadState,
			KeyboardState currentKeyboardState, HeightMapInfo heightMapInfo)
		{
            
			// First, we want to check to see if the tank should turn. turnAmount will 
			// be an accumulation of all the different possible inputs.
			float turnAmount = -currentGamePadState.ThumbSticks.Left.X;
			if (currentKeyboardState.IsKeyDown(Keys.A) ||
				//currentKeyboardState.IsKeyDown(Keys.Left) ||
				currentGamePadState.DPad.Left == ButtonState.Pressed)
			{
				//turnAmount += 1;
				tank.movement.X = -1;
			}
			else if (currentKeyboardState.IsKeyDown(Keys.D) ||
				//currentKeyboardState.IsKeyDown(Keys.Right) ||
				currentGamePadState.DPad.Right == ButtonState.Pressed)
			{
				//turnAmount -= 1;
				tank.movement.X = +1;
			}
			else
			{
				tank.movement.X = 0;
			}

			// clamp the turn amount between -1 and 1, and then use the finished
			// value to turn the tank.
			turnAmount = MathHelper.Clamp(turnAmount, -1, 1);
			float facingDirection = tank.FacingDirection;
			facingDirection += turnAmount * TankTurnSpeed;

			tank.FacingDirection = facingDirection;

			// Next, we want to move the tank forward or back. to do this, 
			// we'll create a Vector3 and modify use the user's input to modify the Z
			// component, which corresponds to the forward direction.
			Vector3 movement = Vector3.Zero;
			//movement.Z = -currentGamePadState.ThumbSticks.Left.Y;

			if (currentKeyboardState.IsKeyDown(Keys.W) ||
				//currentKeyboardState.IsKeyDown(Keys.Up) ||
				currentGamePadState.DPad.Up == ButtonState.Pressed)
			{
				tank.movement.Z = -1;
			}
			else if (currentKeyboardState.IsKeyDown(Keys.S) ||
				//currentKeyboardState.IsKeyDown(Keys.Down) ||
				currentGamePadState.DPad.Down == ButtonState.Pressed)
			{
				tank.movement.Z = 1;
			}
			else
			{
				tank.movement.Z = 0;
			}

			if (currentKeyboardState.IsKeyDown(Keys.LeftShift))
			{
				if(boost != true)
					Velocity *= 1.5f;
				boost = true;
			}
			else
			{
				if (boost == true)
				{
					boost = false;
					Velocity /= 1.5f;
				}
				
			}

			/*if (currentKeyboardState.IsKeyDown(Keys.Space) && !shot)
			{
				if(AimEnemy != null)
					AimEnemy.ReceiveDamage(UnitTypes.BulletDamage[(int)UnitTypes.PlayerWeaponType.Canon], 
						tank.ForwardVector);
				shot = true;

			}
			else if (currentKeyboardState.IsKeyUp(Keys.Space))
			{
				shot = false;
			}*/
            if (currentKeyboardState.IsKeyDown(Keys.M))
            {
                if (enemyList[0] != null)
                {
                    float radians = MathHelper.ToDegrees(enemyList[0].FuzzyBrain.DecideDirection(tank.WorldMatrix,
                        enemyList[0].Transformation.Translation, enemyList[0].healthManager.Health.Position, WorldTrees[0].Position));
                    if (radians < 0)
                        radians = -180 - radians;
                    else if(radians > 0)
                        radians = 180 - radians;


                    Matrix rot = Matrix.CreateRotationY(MathHelper.ToRadians(-radians));
                    
                    tank.Move(MathHelper.ToRadians(-radians),new Vector3(0,0,-1),heightMapInfo);
                    

                }
            }
            else
            {
                MovePlayer(currentGamePadState, currentKeyboardState, heightMapInfo);
            }
			//tank.Move(facingDirection, movement, heightMapInfo,SteeringForce);
			
		}

	    

		public override void Update(GameTime time)
		{
			float elapsedTimeSeconds = (float)time.ElapsedGameTime.TotalSeconds;
			//UpdateWaistBone(elapsedTimeSeconds);
			//System.Console.WriteLine(Life);

			UpdateTarget();
			FindObstacles();
			//tank.SteeringForce = WallAvoidance();
			if (enemyCollision)
			{
				start += (float)time.ElapsedGameTime.TotalSeconds; 
			}
			if (start > 0.3f)
			{
				enemyCollision = false;
				start = 0;
				tank.Velocity = -tank.Velocity;
			}
			tank.Update(time);
			base.Update(time);

			// Update player weapon
			//Matrix transformedHand = AnimatedModel.BonesAnimation[RIGHT_HAND_BONE_ID] * Transformation.Matrix;
			//playerWeapon.TargetDirection = HeadingVector + UpVector * rotateWaistBone;
			//playerWeapon.Update(time, transformedHand);
		}

		private void UpdateTarget()
		{
			aimEnemy = null;
			numEnemiesAlive = 0;

			// Shoot ray
			Ray ray = new Ray(tank.Position, tank.ForwardVector);
			
			// Distance from the ray start position to the terrain
			float? distance = terrain.Intersects(ray);


			// Test intersection with enemies
			foreach (Enemy enemy in enemyList)
			{
				if (!enemy.IsDead)
				{
					numEnemiesAlive++;
					
					float? enemyDistance = enemy.BoxIntersects(ray);
  
					if (enemyDistance != null)
					{
						
						if (distance == null || enemyDistance <= distance)
						{
							distance = enemyDistance;
							aimEnemy = enemy;
							//System.Console.WriteLine(numEnemiesAlive);
						}
					}
				}
			}

			// Weapon target position
			//weaponTargetPosition = gameLevel.Player.Weapon.FirePosition +
			//gameLevel.Player.Weapon.TargetDirection * 300;
		}

		public override void Draw(GameTime time)
		{
			base.Draw(time);
			//TODO: Check why this canno be draw in Enemy class
			/*Renderer.BoundingSphere3D.DrawCircle(tank.BoundingSphere, 
													Color.Black,
													Transformation*Matrix.CreateTranslation(new Vector3(0,25,0)));
			Renderer.BoundingSphere3D.DrawCircle(tank.BoundingSphere,
													Color.Black,
													 Matrix.CreateRotationX(MathHelper.ToRadians(90)) * tank.WorldMatrix * Matrix.CreateTranslation(new Vector3(0, 25, 0))
													);
			Renderer.BoundingSphere3D.DrawCircle(tank.BoundingSphere,
													Color.Black,
													Matrix.CreateRotationZ(MathHelper.ToRadians(90)) * tank.WorldMatrix * Matrix.CreateTranslation(new Vector3(0, 25, 0))
													);*/

			
			Renderer.BoundingSphere3D.DrawCircle(new BoundingSphere(new Vector3(0, 0, 0), 1200),
													Color.Blue,
													Transformation * Matrix.CreateTranslation(new Vector3(0, 25, 0)));

			Renderer.BoundingSphere3D.DrawCircle(new BoundingSphere(new Vector3(0, 0, 0), 600),
													Color.Red,
													Transformation * Matrix.CreateTranslation(new Vector3(0, 25, 0)));
			
			if (enemyList[0] != null)
			{ 
				float radians = MathHelper.ToDegrees(enemyList[0].FuzzyBrain.DecideDirection(tank.WorldMatrix,
					enemyList[0].Transformation.Translation,enemyList[0].healthManager.Health.Position,WorldTrees[0].Position));
				if (radians < 0)
					radians = -180 - radians;
				else
					radians = 180 - radians;
			

			Matrix rot = Matrix.CreateRotationY(MathHelper.ToRadians(-radians));
			Renderer.Line3D.Draw(new Vector3(0,50,0), new Vector3(0, 50, -200), Color.Red,rot*tank.WorldMatrix);
			}
			
		}

		public void MovePlayer(GamePadState currentGamePadState,
			KeyboardState currentKeyboardState, HeightMapInfo heightMapInfo)
		{
			#region Player Movement
			float cannonRotateAmount = -currentGamePadState.ThumbSticks.Right.Y * .1f;
			if (currentKeyboardState.IsKeyDown(Keys.Y)) //||
			//currentGamePadState.DPad.Left == ButtonState.Pressed)
			{
				cannonRotateAmount += 0.02f;
			}
			if (currentKeyboardState.IsKeyDown(Keys.H)) //||
			//currentGamePadState.DPad.Left == ButtonState.Pressed)
			{
				cannonRotateAmount -= 0.02f;
			}

			float newCannonRotate = tank.CannonRotate + cannonRotateAmount;
			newCannonRotate = MathHelper.Clamp(newCannonRotate, -MathHelper.PiOver4/2.5f , MathHelper.PiOver4/2.5f );

			//Code to rotate the turret with the right thumbstick or J button
			float turretRotateAmount = currentGamePadState.ThumbSticks.Right.X * .1f;
			if (currentKeyboardState.IsKeyDown(Keys.G)) //||
			//currentGamePadState.DPad.Left == ButtonState.Pressed)
			{
				turretRotateAmount -= 0.02f;
			}
			if (currentKeyboardState.IsKeyDown(Keys.J)) //||
			//currentGamePadState.DPad.Left == ButtonState.Pressed)
			{
				turretRotateAmount += 0.02f;
			}
			float newTurretRotate = tank.TurretRotate + turretRotateAmount;
			newTurretRotate = MathHelper.Clamp(newTurretRotate, -MathHelper.PiOver2 / 1, MathHelper.PiOver2 / 1);

			// First, we want to check to see if the tank should turn. turnAmount will 
			// be an accumulation of all the different possible inputs.
			float turnAmount = -currentGamePadState.ThumbSticks.Left.X;     // turn LEFT
			float steerRotationAmount = currentGamePadState.ThumbSticks.Right.X * .1f;
			
			if (currentKeyboardState.IsKeyDown(Keys.Left) ||
				currentGamePadState.DPad.Left == ButtonState.Pressed ||
				currentKeyboardState.IsKeyDown(Keys.A))
			{

				steerRotationAmount -= 0.04f;
				// player is turning holding Foward button
				if (currentKeyboardState.IsKeyDown(Keys.Up) ||
					currentGamePadState.DPad.Up == ButtonState.Pressed ||
					currentKeyboardState.IsKeyDown(Keys.W)
					)
				{
					steerRotationAmount -= 0.05f;
					turnAmount += 1;
				}
				// player is turning holding Bacward button
				if (currentKeyboardState.IsKeyDown(Keys.Down) ||
					currentGamePadState.DPad.Down == ButtonState.Pressed ||
					currentKeyboardState.IsKeyDown(Keys.S))
				{
					steerRotationAmount -= 0.05f;
					turnAmount -= 1;
				}
			}

			if (currentKeyboardState.IsKeyDown(Keys.D) ||          // turn RIGHT
				currentKeyboardState.IsKeyDown(Keys.Right) ||
				currentGamePadState.DPad.Right == ButtonState.Pressed)
			{
				steerRotationAmount += 0.04f;
				// player is turning holding Foward button
				if (currentKeyboardState.IsKeyDown(Keys.Up) ||
				currentGamePadState.DPad.Up == ButtonState.Pressed ||
				currentKeyboardState.IsKeyDown(Keys.W))
				{
					steerRotationAmount += 0.05f;
					turnAmount -= 1;
				}
				// player is turning holding Bacward button
				if (currentKeyboardState.IsKeyDown(Keys.Down) ||
					currentGamePadState.DPad.Down == ButtonState.Pressed ||
					currentKeyboardState.IsKeyDown(Keys.S))
				{
					steerRotationAmount += 0.05f;
					turnAmount += 1;
				}
			}


			// clamp the turn amount between -1 and 1, and then use the finished
			// value to turn the tank.
			turnAmount = MathHelper.Clamp(turnAmount, -1, 1);
			tank.FacingDirection += turnAmount * TankTurnSpeed;
			// Next, we want to move the tank forward or back. to do this, 
			// we'll create a Vector3 and modify use the user's input to modify the Z
			// component, which corresponds to the forward direction.
			Vector3 movement = Vector3.Zero;
			movement.Z = -currentGamePadState.ThumbSticks.Left.Y;

			if (currentKeyboardState.IsKeyDown(Keys.Up) ||
					currentGamePadState.DPad.Up == ButtonState.Pressed || // FOWARD
					currentKeyboardState.IsKeyDown(Keys.W))
			{
				movement.Z = -1;
				// case when player is turning holding Foward button
				if (currentKeyboardState.IsKeyUp(Keys.Left) ||
					currentKeyboardState.IsKeyUp(Keys.A) ||
					currentGamePadState.DPad.Right == ButtonState.Released ||
					currentKeyboardState.IsKeyUp(Keys.Right) ||
					currentKeyboardState.IsKeyUp(Keys.D) ||
					currentGamePadState.DPad.Left == ButtonState.Released
					)
				{
					if (tank.SteerRotationValue > 0 & tank.SteerRotationValue != 0)
						steerRotationAmount -= 0.05f;
					if (tank.SteerRotationValue < 0 & tank.SteerRotationValue != 0)
						steerRotationAmount += 0.05f;
				}

			}
			if (currentKeyboardState.IsKeyDown(Keys.Down) ||      // BACKWARD
				currentKeyboardState.IsKeyDown(Keys.S) ||
				currentGamePadState.DPad.Down == ButtonState.Pressed)
			{
				movement.Z = 0.7f;

				// case when player is turning holding Bacward button
				if (currentKeyboardState.IsKeyUp(Keys.Left) ||
					currentKeyboardState.IsKeyUp(Keys.A) ||
					currentGamePadState.DPad.Right == ButtonState.Released ||
					currentKeyboardState.IsKeyUp(Keys.Right) ||
					currentKeyboardState.IsKeyUp(Keys.D) ||
					currentGamePadState.DPad.Left == ButtonState.Released
					)
				{
					if (tank.SteerRotationValue > 0 & tank.SteerRotationValue != 0)
						steerRotationAmount -= 0.05f;
					if (tank.SteerRotationValue < 0 & tank.SteerRotationValue != 0)
						steerRotationAmount += 0.05f;
				}
			}
			if (currentKeyboardState.IsKeyDown(Keys.LeftShift))
			{
				movement *= 2;
				if (currentKeyboardState.IsKeyDown(Keys.Down) ||
					currentKeyboardState.IsKeyDown(Keys.Up) ||
					currentKeyboardState.IsKeyDown(Keys.S) ||
					currentKeyboardState.IsKeyDown(Keys.W))
					boost = true;
			}
			else
			{
				boost = false;
			}

			// turning tanks front wheels
			float newSteerRotationValue = tank.SteerRotationValue + steerRotationAmount;
			newSteerRotationValue = MathHelper.Clamp(newSteerRotationValue, -MathHelper.PiOver4, MathHelper.PiOver4);

			#endregion


			// next, we'll create a rotation matrix from the direction the tank is 
			// facing, and use it to transform the vector.
			//tank.Orientation = Matrix.CreateRotationY(tank.FacingDirection);
			Vector3 velocity = Vector3.Transform(movement, tank.Orientation);

			velocity *= tank.Velocity;

			// Now we know how much the user wants to move. We'll construct a temporary
			// vector, newPosition, which will represent where the user wants to go. If
			// that value is on the heightmap, we'll allow the move.
			//Vector3 newPosition = tank.Position + velocity;
			//if (heightMapInfo.IsOnHeightmap(newPosition))
			//{
				// now that we know we're on the heightmap, we need to know the correct
				// height and normal at this position.
				//Vector3 normal;
				//heightMapInfo.GetHeightAndNormal(newPosition,
					//out newPosition.Y, out normal);


				// As discussed in the doc, we'll use the normal of the heightmap
				// and our desired forward direction to recalculate our orientation
				// matrix. It's important to normalize, as well.
				//Matrix orientation = tank.Orientation;
				//orientation.Up = normal;

				//orientation.Right = Vector3.Cross(orientation.Forward, orientation.Up);
				//orientation.Right = Vector3.Normalize(orientation.Right);

				//orientation.Forward = Vector3.Cross(orientation.Up, orientation.Right);
				//orientation.Forward = Vector3.Normalize(orientation.Forward);

				// tank.Orientation = orientation;
				// now we need to roll the tank's wheels "forward." to do this, we'll
				// calculate how far they have rolled, and from there calculate how much
				// they must have rotated.
				// float distanceMoved = Vector3.Distance(tank.Position, newPosition);
				// float theta = distanceMoved / tank.TankWheelRadius;
				// int rollDirection = movement.Z > 0 ? 1 : -1;

				//tank.wheelRollMatrix *= Matrix.CreateRotationX(theta * rollDirection);
				tank.cannonMoveMatrix *= Matrix.CreateRotationX((tank.CannonRotate - newCannonRotate));
				tank.turretMoveMatrix *= Matrix.CreateRotationY((tank.TurretRotate - newTurretRotate));
				tank.steerRotationMatrix *= Matrix.CreateRotationY(tank.SteerRotationValue - newSteerRotationValue);

				// once we've finished all computations, we can set our position to the
				// new position that we calculated.
				//tank.Position = newPosition;
				tank.TurretRotate = newTurretRotate;
				tank.CannonRotate = newCannonRotate;
				tank.SteerRotationValue = newSteerRotationValue;
			//}
		}
	}
}
