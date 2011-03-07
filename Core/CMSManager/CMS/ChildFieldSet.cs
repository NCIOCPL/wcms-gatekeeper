using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCI.WCM.CMSManager.CMS
{
    public class ChildFieldSet
    {
        public ChildFieldSet(string name) :
            base()
        {
            Name = name;
        }

        private List<FieldSet> _childFields;
        public List<FieldSet> Fields
        {
            get
            {
                if (_childFields == null)
                    _childFields = new List<FieldSet>();
                return _childFields;
            }
        }

        public string Name { get; private set; }
    }
}
