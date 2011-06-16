using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameLogic.AI.FuSM
{
    class FStateDodge : FuSMState
    {
        public float BulletSpeed = 10;
        private readonly int dangerAngle = 35;

        public FStateDodge(int type, FuSMAIControl parent)
            : base(type, parent)
        {
                        
        }

        public override float CalculateActivation()
        {
            return 1;
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            Vector3 enemyShootingDirection = _parent.enemyVelocity;

            List<TerrainUnit> attackingEnemies = _parent.GetAttackingEnemy(_parent._unit.Oponents);
            Vector3 finalForce = Vector3.Zero;

            foreach (var attackingEnemy in attackingEnemies)
            {
                Vector3 directionToPlayer = _parent._unit.Transformation.Translation - attackingEnemy.Transformation.Translation;
                directionToPlayer.Normalize();
                if (directionToPlayer.Length() < 500)
                {
                    float angle = FuSMAIControl.GetSignedAngle3D(directionToPlayer, enemyShootingDirection, attackingEnemy.tank.WorldMatrix.Right);
                    float absDegreeAngle = Math.Abs(MathHelper.ToDegrees(angle));

                    if (absDegreeAngle < dangerAngle)
                    {
                        _activationLevel = (absDegreeAngle - dangerAngle) / (-dangerAngle);
                    }

                    Vector3 dodgeForce = (angle < 5) ? attackingEnemy.tank.WorldMatrix.Left : attackingEnemy.tank.WorldMatrix.Right;
                    finalForce += dodgeForce * _activationLevel;
                }
            }

            _parent._unit.FuzzyAccumulateForce(finalForce);

        }
    }
}
