using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TanksOnAHeightmap.GameLogic.AI
{
    abstract class State
    {
        //this will execute when the state is entered
        public abstract void Enter(Enemy entity);

        public abstract void Execute(Enemy entity);
        //this will execute when the state is exited
        public abstract void Exit(Enemy entity);
    }
}
