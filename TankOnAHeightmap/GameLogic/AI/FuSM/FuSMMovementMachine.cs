using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TanksOnAHeightmap.GameLogic.AI.FuSM
{
    class FuSMMovementMachine : FuSMState
    {
        private readonly List<FuSMState> _states;
        private int _activeIndex;

        public FuSMMovementMachine(int type,FuSMAIControl parent):base(type,parent)
        {
            _states = new List<FuSMState>();
            _activeIndex = -1;
        }

        public override float CalculateActivation()
        {
            float currentActivation = 0f;
            float tmpActivation;

            List<FuSMState> nonActiveStates = new List<FuSMState>();
            for (int i = 0; i < _states.Count; i++)
            {
                tmpActivation = _states[i].CalculateActivation();
                if (tmpActivation > currentActivation)
                {
                    currentActivation = tmpActivation;
                    _activeIndex = i;
                }

            }

            return currentActivation;
        }

        public override void Update(float dt)
        {
            if (_states.Count == 0)
                return;
            if (_activeIndex > -1)
            {
                _states[_activeIndex].Update(dt);
            }

            _activeIndex = -1;
        }

        public virtual void AddState(FuSMState state)
        {
            _states.Add(state);
        }
    }
}
