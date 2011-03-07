using System;
using System.Collections.Generic;
using System.Text;

namespace GateKeeper.DocumentObjects.Protocol
{
    class OrganizationRole
    {
    }

    /// <summary>
    /// Represents protocol phase type
    /// </summary>
    [Serializable]
    public enum OrganizationRoleType
    {
        Primary = 1,
        Secondary = 2,
        Organization = 3,
        Person = 4,
    }

}
