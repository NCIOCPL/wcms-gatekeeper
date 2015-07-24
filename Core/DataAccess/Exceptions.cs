using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GateKeeper.DataAccess
{
    /// <summary>
    /// Exception thrown when GateKeeper encounters problems with assumptions involving the database
    /// objects.  This is intended for use when trying to convert from the generic DbConnection and
    /// related objects returned by the Enterprise Library into the more specific SqlConnection, etc.
    /// objects used by SQLHelper.
    /// Thrown
    /// </summary>
    [global::System.Serializable]
    public class DatabaseAssumptionException : Exception
    {
        public DatabaseAssumptionException() { }
        public DatabaseAssumptionException(string message) : base(message) { }
        public DatabaseAssumptionException(string message, Exception inner) : base(message, inner) { }
        protected DatabaseAssumptionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
