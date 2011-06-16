using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TanksOnAHeightmap.GameLogic.AI.FuSM
{
    class FuSMState
    {
        protected readonly int _type;
        protected readonly FuSMAIControl _parent;
        protected float _activationLevel;

        public FuSMState(int type = 0, FuSMAIControl parent = null)
        {
            _type = type;
            _parent = parent;
            _activationLevel = 0.0f;
        }

        public virtual void Update(float dt){}
        public virtual void Enter(){}
        public virtual void Exit(){}
        public virtual void Init(){}

        public virtual float CalculateActivation()
        {
            return 0f;
        }

        protected virtual void CheckLowerBound(float lBound = 0.0f)
        {
            if (_activationLevel < lBound) _activationLevel = lBound; 
        }

        protected virtual void CheckUpperBound(float uBound = 0.0f)
        {
            if (_activationLevel > uBound) _activationLevel = uBound; 
        }

        protected virtual void CheckBounds(float lB = 0.0f,float uB = 1.0f)
        {
            CheckLowerBound(lB);
            CheckUpperBound(uB);

        }

    }
}
