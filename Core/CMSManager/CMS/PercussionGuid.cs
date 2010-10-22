using System;

namespace NCI.WCM.CMSManager.CMS
{
    public class PercussionGuid
    {
        public PercussionGuid()
        {

        }

        public PercussionGuid(int id)
        {
            ID = id;
            Revision = -1;
            Type = 101;
        }

        public PercussionGuid(long guid)
        {
            Guid = guid;
        }

        public long Guid { get; set; }

        public int ID
        {
            get { return (int)(Guid & 0xFFFFFFFFL); }
            set
            {
                unchecked
                {
                    long mask = (long)0xFFFFFFFF00000000L;
                    Guid = (mask & Guid) | (uint)value;
                }
            }
        }

        public int Revision
        {
            get { return (int)(Guid >> 40); }
            set
            {
                long mask = 0x000000FFFFFFFFFFL;
                Guid = (mask & Guid) | (uint)value;
            }
        }

        public int Type
        {
            get { return (int)(Guid >> 32) & 0xFF; }
            set
            {
                unchecked
                {
                    long mask = (long)0xFFFFFF00FFFFFFFFL;
                    Guid = (mask & Guid) | (uint)value << 32;
                }
            }
        }

        public override String ToString()
        {
            return string.Format("{0}-{1}-{2}", Revision, Type, ID);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            PercussionGuid rhs = obj as PercussionGuid;
            if ((Object)rhs == null)
            {
                return false;
            }

            return Revision == rhs.Revision
                && Type == rhs.Type
                && ID == rhs.ID;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

    }
}
