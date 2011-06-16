using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameLogic.AI.FuSM
{
    class FStateGetPowerUp : FuSMState
    {
        public FStateGetPowerUp(int type, FuSMAIControl parent)
            : base(type, parent)
        {
            
        }

        public override float CalculateActivation()
        {
            _activationLevel = _parent.FuzzyDecision.HealthPrior*_parent.FuzzyDecision.FuzzyHealthWeight;
            return _activationLevel;
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            Vector3 distanceToPary = _parent.HealthPosition - _parent._unit.Transformation.Translation;
            distanceToPary.Normalize();
            _parent._unit.FuzzyAccumulateForce(distanceToPary);
            
        }
    }
}
