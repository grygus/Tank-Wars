using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameLogic.AI.FuSM
{
    class FStateApproach : FuSMState
    {
        public FStateApproach(int type,FuSMAIControl parent) : base(type,parent)
        {
            

        }

        public override float CalculateActivation()
        {
            if (_parent.closestEnemy == null) 
                return 0;

            _activationLevel = _parent.FuzzyDecision.Danger;
            _activationLevel /= 100;
            _activationLevel *= _parent.FuzzyDecision.FuzzyEnemyWeight;
            CheckBounds();
            return _activationLevel;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Vector3 distance = _parent.ClosestEnemyPosition - _parent._unit.Transformation.Translation;
            CalculateActivation();
            distance.Normalize();
            _parent._unit.FuzzyAccumulateForce(distance,"Approach");
        }

        
    }
}
