using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using NUnit.Framework;


namespace GateKeeper.UnitTest.DocumentObjects.Summary
{
    /// <summary>
    /// Tests for the Summary Preprocessor's RewriteSummaryRefAttributes() method.
    /// </summary>
    [TestFixture]
    class SummaryPreprocessorRewriteSummaryRefAttributes
    {
        /// <summary>
        /// Test for correct handling of a reference to a piloted summary (Reference to the actual summary, not a section).
        /// </summary>
        [Test]
        public void ReferenceToPilotSummary()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Test for correct handling of a reference to a non-piloted summary (Reference to the actual summary, not a section).
        /// </summary>
        [Test]
        public void ReferenceToNonpilotSummary()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Test for correct handling of a reference to a section within a piloted summary.
        /// </summary>
        [Test]
        public void ReferenceToPilotSummarySection()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Test for correct handling of a reference to a section within a non-piloted summary.
        /// </summary>
        [Test]
        public void ReferenceToNonpilotSummarySection()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Test for correct handling when there is no summary reference.
        /// </summary>
        [Test]
        public void NoSummaryReference()
        {
            throw new NotImplementedException();
        }
    }
}
