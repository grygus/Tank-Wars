using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TanksOnAHeightmap.GameLogic.AI
{
    abstract class BaseGameEntity
    {
        int m_ID;

        public int M_ID
        {
            get { return m_ID;}
        }

        static int m_iNextValidID;

        public BaseGameEntity(int id)
        {
            m_ID = id;
        }

        public abstract void Update();
    }
}
