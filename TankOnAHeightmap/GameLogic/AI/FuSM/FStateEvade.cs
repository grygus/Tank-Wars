using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameLogic.AI.FuSM
{
    class FStateEvade : FuSMState
    {

        public FStateEvade(int type, FuSMAIControl parent) : base(type,parent)
        {
            
        }

        public override float CalculateActivation()
        {
            _activationLevel = _parent.FuzzyDecision.Danger;
            _activationLevel /= 100;
            _activationLevel = 0.75f - _activationLevel;
            _activationLevel *= 1.0f - _parent.FuzzyDecision.FuzzyEnemyWeight;
            CheckBounds();
            return _activationLevel;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            
            Vector3 brake = _parent._unit.Transformation.Translation - _parent.ClosestEnemyPosition;
            if(brake.Length() < 500)
            {
                brake.Normalize();
                _parent._unit.FuzzyAccumulateForce(brake * _activationLevel);
            };

        }
    }
}
