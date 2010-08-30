using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.Common
{
    public enum DBStatusCodeType
    {
        Success = 0, 

        // Failure codes (negative values)
        RecordNotFound = -1,
        DuplicateRecord = -2,
        RecordAlreadyComplete = -3,
        OpenRequestAlreadyExists = -4,

        // Non-fatal error codes (positive values)
    }
}
