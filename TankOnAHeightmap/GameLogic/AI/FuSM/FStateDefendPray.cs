using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameLogic.AI.FuSM
{
    class FStateDefendPray : FuSMState
    {
        private readonly int Radius = 500;

        public FStateDefendPray(int type,FuSMAIControl parent)
            : base(type,parent)
        {
            
        }

        public override float CalculateActivation()
        {
            /*_activationLevel = (Vector3.Distance(_parent._unit.Position, _parent._unit.Prey.Position) > Radius)
                                   ? _parent.FuzzyDecision.PrayPrior
                                   : 0;*/
            if (Vector3.Distance(_parent._unit.Position, _parent._unit.Prey.Position) > Radius)
            {

                _activationLevel = (_parent._unit.Prey.IsUnderAttack || _parent._unit.Prey.Life < 50)
                                       ? _parent.FuzzyDecision.PrayPrior
                                       : 0.1f;
            }
            else
            {
                _activationLevel = 0;
            }
            return _activationLevel;
        }

        public override void Update(float dt)
        {

            Vector3 vecToPray = _parent._unit.Prey.Position - _parent._unit.Position;
            if (vecToPray.Length() > Radius)
            {
                vecToPray.Normalize();
                _parent._unit.FuzzyAccumulateForce(vecToPray, "DefendPrey");
            }
            else
            {
                _parent._unit.FuzzyAccumulateForce(Vector3.Zero, "DefendPrey");
            }


        }
    }
}
