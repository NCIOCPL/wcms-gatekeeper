using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Protocol
{
     /// <summary>
    /// Represents protocol phase type
    /// </summary>
    [Serializable]
    public enum PhaseType
    {
        NoPhase = 0,
        PhaseI = 1,
        PhaseII = 2,
        PhaseIII = 3,
        PhaseIV = 4,
        PhaseV = 5,
    }

}

