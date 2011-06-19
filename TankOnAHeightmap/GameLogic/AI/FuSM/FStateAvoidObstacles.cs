using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TanksOnAHeightmap.GameLogic.AI.FuSM
{
    class FStateAvoidObstacles : FuSMState
    {
        public FStateAvoidObstacles(int type, FuSMAIControl parent)
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

            _parent._unit.FuzzyAccumulateForce(_parent._unit.FindObstacles(), "AvoidObstacles");


        }
    }
}
