using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.GameLogic.AI.FuSM
{
    class FuSMMachine : FuSMState
    {
        private List<FuSMState> _states;
        private List<FuSMState> _activeStates;

        public FuSMMachine(int type,FuSMAIControl parent) :  base(type, parent)
        {
           _states = new List<FuSMState>();
           _activeStates = new List<FuSMState>();
        }

        public virtual void UpdateMachine(float dt)
        {
            if (_states.Count == 0)
                return;

            _activeStates.Clear();
            
            List<FuSMState> nonActiveStates = new List<FuSMState>();
            for (int i = 0; i < _states.Count; i++)
            {

                if (_states[i].CalculateActivation() > 0)
                    _activeStates.Add(_states[i]);
                else
                    nonActiveStates.Add(_states[i]);
            }

            if(nonActiveStates.Count != 0)
                for (int i = 0; i < nonActiveStates.Count; i++)
                {
                    nonActiveStates[i].Exit();
                }

            _parent._unit.FuzzySteeringForce = Vector3.Zero;

            if(_activeStates.Count != 0)
                for (int i = 0; i < _activeStates.Count; i++)
                {
                    _activeStates[i].Update(dt);
                }
        }

        public virtual void AddState(FuSMState state)
        {
            _states.Add(state);
        }

        public virtual  void IsActive(FuSMState state)
        {
            _activeStates.Contains(state);
        }

        public virtual void Resest()
        {
            _states.Clear();
        }

    }
}
