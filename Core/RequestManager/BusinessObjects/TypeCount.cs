using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using GateKeeper.Common;

namespace GKManagers.BusinessObjects
{
    public class TypeCount : IComparable
    {
        public string _type;
        string _val;

        int IComparable.CompareTo(object o)
        {
            TypeCount tmp = (TypeCount)o;

            return (String.Compare(this._type, tmp._type, true));
        }

        #region Constructors
        public TypeCount()
        {
        }

        public TypeCount(string strType, string strVal)
        {
            _type = strType;
            _val = strVal;
        }
        #endregion

        #region Properties
        public string Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Value
        {
            get { return _val; }
            set { _val = value; }
        }

        #endregion
    }
}