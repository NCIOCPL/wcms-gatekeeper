using System;
using System.Xml;
using System.Xml.Serialization;

using GateKeeper.Common;
using GateKeeper.ContentRendering;
using GateKeeper.DataAccess.CDR;
using GateKeeper.DataAccess.DataAccessWrappers;

namespace GKManagers.Processors
{
    public class ProcessingTarget : IComparable
    {
        [XmlAttribute()]
        public TargetedDevice TargetedDevice { get; set; }

        public DocumentExtractor DocumentExtractor { get; set; }
        public DocumentRenderer DocumentRenderer { get; set; }
        public DocumentDataAccess DocumentDataAccess { get; set; }


        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is ProcessingTarget)
            {
                return TargetedDevice.CompareTo(((ProcessingTarget)obj).TargetedDevice);
            }
            else
            {
                throw new ArgumentException("object is not a ProcessingTarget.");
            }
        }

        #endregion
    }
}
