using GateKeeper.Common;

namespace GateKeeper.DocumentObjects
{
    /// <summary>
    /// Helper class for event logging in the DocumentObjects assembly.
    /// </summary>
    class DocumentLogBuilder : LogBuilder
    {
        public DocumentLogBuilder()
            : base("DocumentObjects")
        {
        }
    }
}
