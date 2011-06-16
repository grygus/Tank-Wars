using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameLogic.AI.FuSM
{
    class FStateAttack : FuSMState
    {
        public float BulletSpeed = 10;

        public FStateAttack(int type,FuSMAIControl parent) : base(type, parent)
        {
                        
        }

        public override float CalculateActivation()
        {
            return 1;
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            Vector3 futureEnemyPosition = _parent.ClosestEnemyPosition;
            Vector3 distance = futureEnemyPosition - _parent._unit.Transformation.Translation;

            float dist = distance.Length();
            float time = dist/BulletSpeed;

            futureEnemyPosition += time*_parent.enemyVelocity;
            distance = futureEnemyPosition - _parent._unit.Transformation.Translation;



            float newDir = FuSMAIControl.GetSignedAngle3D(_parent._unit.tank.ShootingDirection
                                                            , distance
                                                            ,Vector3.Transform(_parent._unit.tank.turretBone.Transform.Right
                                                            ,_parent._unit.tank.Orientation)
                                                            );
            _parent._unit.ShootAt(newDir);
            if(newDir < 0.1 && distance.Length() < 500)
                _parent._unit.phisics.Cannon.Fire();


            //_parent._unit.SetTarget();
            //_parent._unit.DebugMessage("Attack");
            
        }
    }
}
