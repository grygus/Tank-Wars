using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameLogic.AI.FuSM
{
    public class FuSMAIControl
    {
        private FuSMMachine _machine;
        private FuSMMovementMachine _movementMachine;
        public TerrainUnit _unit;
        private readonly int AttackingDistance = 500;
        #region Helpers

        public static float GetUnsignedAngle3D(Vector3 FromVector, Vector3 DestVector)
        {
            FromVector.Normalize();
            DestVector.Normalize();

            float forwardDot = Vector3.Dot(FromVector, DestVector);

            // Keep dot in range to prevent rounding errors
            forwardDot = MathHelper.Clamp(forwardDot, -1.0f, 1.0f);

            float angleBetween = (float)Math.Acos(forwardDot);
            
            return angleBetween;
        }

        public static float GetSignedAngle3D(Vector3 FromVector, Vector3 DestVector, Vector3 FromVectorRight)
        {
            FromVector.Normalize();
            DestVector.Normalize();
            FromVectorRight.Normalize();

            float forwardDot = Vector3.Dot(FromVector, DestVector);
            float rightDot = Vector3.Dot(FromVectorRight, DestVector);

            // Keep dot in range to prevent rounding errors
            forwardDot = MathHelper.Clamp(forwardDot, -1.0f, 1.0f);

            float angleBetween = (float)Math.Acos(forwardDot);

            if (rightDot > 0.0f)
                angleBetween *= -1.0f;

            return angleBetween;
        }

        public void TestGetSignedAngleBetween2DVectors()
        {
            Vector3 v1 = new Vector3(0,0.5f,-1);
            Vector3 v2 = new Vector3(-1.5f,-1,0.1f);

            float result = GetSignedAngle3D(v1, v2, new Vector3(1, 0, 0));
            Console.WriteLine(MathHelper.ToDegrees(result));
            Console.WriteLine(result);

        }

        #endregion


        #region Perceptions

        public Vector3 ClosestEnemyPosition;

        public Vector3 PrayPosition;
        public Vector3 HealthPosition;
        public Vector3 ClosesObstacle;
        public Vector3 enemyVelocity;
        public int EnemyHealth;
        public BoundingBox enemyBoundingBox;
        public TerrainUnit closestEnemy;
        public FuzzyEngine FuzzyDecision;
        public Matrix enemyWorld;  

        #endregion


        public FuSMAIControl()
        {
        }

        public FuSMAIControl(TerrainUnit unit = null)
        {
            _unit = unit;
            
            _movementMachine = new FuSMMovementMachine(0,this);
            _movementMachine.AddState(new FStateApproach(0, this));
            //_movementMachine.AddState(new FStateEvade(0, this));
            _movementMachine.AddState(new FStateGetPowerUp(0, this));
            _movementMachine.AddState(new FStateDefendPray(0, this));

            _machine = new FuSMMachine(0, this);
            _machine.AddState(new FStateAttack(0, this));
            _machine.AddState(new FStateDodge(0, this));
            _machine.AddState(new FStateAvoidObstacles(0, this));
            _machine.AddState(_movementMachine);

            FuzzyDecision = new FuzzyEngine();
        }

        public void Update(float dt)
        {
            if (_unit.IsDead)
            {
                _machine.Resest();
                return;
            }

            UpdatePerceptions(dt);
            if (closestEnemy != null)
               FuzzyDecision.UpdateParameters(_unit.Life,EnemyHealth,_unit.Prey.Life);
            else
                FuzzyDecision.UpdateParameters(_unit.Life, _unit.Prey.Life);

            _machine.UpdateMachine(dt);
        }

        private void UpdatePerceptions(float dt)
        {
            if (_unit.Oponents.Count == 0)
                return;

            ClosestEnemyPosition = _unit.Oponents[0].Transformation.Translation;
            PrayPosition = _unit.WorldTrees[0].Position;
            HealthPosition = _unit.healthManager.GetNearestHealthPosition(_unit.Transformation.Translation);
            //ClosesObstacle = _unit.FindObstacles();
            Vector3 vecToPray = _unit.Prey.Position - _unit.Position;
            if (vecToPray.Length() <= 500)
            {
                closestEnemy = GetEnemyInDangerZone(_unit.Oponents);
            }
            else
            {
                closestEnemy = GetNearestEnemy(_unit.Oponents);
            }

            if (closestEnemy != null)
            {
                ClosestEnemyPosition = closestEnemy.Transformation.Translation;
                EnemyHealth = closestEnemy.Life;
                enemyWorld = closestEnemy.tank.WorldMatrix;
                enemyVelocity = enemyWorld.Forward;
                enemyBoundingBox = closestEnemy.tank.BoundingBox;
            }
            

        }

        private TerrainUnit GetNearestEnemy(List<TerrainUnit> enemies)
        {
            float distance = AttackingDistance;
            float tmpDistance;
            TerrainUnit nearest = null;
            foreach (var terrainUnit in enemies)
            {
                tmpDistance = Vector3.Distance(_unit.Transformation.Translation, terrainUnit.Transformation.Translation);
                if (tmpDistance < distance)
                {
                    distance = tmpDistance;
                    nearest = terrainUnit;
                }
            }
            return nearest;
        }

        private TerrainUnit GetEnemyInDangerZone(List<TerrainUnit> enemies)
        {
            float tmpDistance;
            foreach (TerrainUnit terrainUnit in enemies)
            {
                tmpDistance = Vector3.Distance(_unit.Prey.Position, terrainUnit.Transformation.Translation);
                if (tmpDistance < 500)
                {
                    return terrainUnit;
                }
            }
            return null;
        }

        public List<TerrainUnit> GetAttackingEnemy(List<TerrainUnit> enemies)
        {
            float distance = float.MaxValue;
            Vector3 tmpDistance;
            float angle;
            List<TerrainUnit> atacking = new List<TerrainUnit>();
            foreach (var terrainUnit in enemies)
            {
                tmpDistance = _unit.Transformation.Translation - terrainUnit.Transformation.Translation;
                angle = GetUnsignedAngle3D(terrainUnit.tank.WorldMatrix.Forward, tmpDistance);
                angle = MathHelper.ToDegrees(angle);
                if (angle < 35)
                    atacking.Add(terrainUnit);
            }
            return atacking;
        }

        private void TestGetAttackingEnemy()
        {
            _unit = new Enemy();
            _unit.tank.setPosition(new Vector3(-1,-1,-1));
            Matrix tmp = new Matrix();
            tmp.Forward = new Vector3(0, 0, 0);
            _unit.tank.WorldMatrix = tmp;

            List<TerrainUnit> atacking = new List<TerrainUnit>();
            Enemy tmpEnemy = new Enemy();
            tmp.Forward = new Vector3(-1, -1, -1);
            tmpEnemy.tank.WorldMatrix = tmp;
            atacking.Add(tmpEnemy);

            tmpEnemy = new Enemy();
            Matrix tmp2 = Matrix.Identity;
            tmpEnemy.tank.setPosition(new Vector3(0, 0, 0));
            tmp2.Forward = new Vector3(1, 1, 1);
            tmpEnemy.tank.WorldMatrix = tmp2;
            atacking.Add(tmpEnemy);

            tmpEnemy = new Enemy();
            tmp.Forward = new Vector3(0, 0, 1);
            tmpEnemy.tank.WorldMatrix = tmp;
            atacking.Add(tmpEnemy);

            tmpEnemy = new Enemy();
            tmp.Forward = new Vector3(-0.9f, -1, -0.9f);
            tmpEnemy.tank.WorldMatrix = tmp;
            atacking.Add(tmpEnemy);

            Console.WriteLine(GetAttackingEnemy(atacking).Count);
        }
    }
}
