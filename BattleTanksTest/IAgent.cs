using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BattleTanksTest
{
    interface IAgent
    {
        ActionSet GetAction(ClientGameState myState);
    }
}
