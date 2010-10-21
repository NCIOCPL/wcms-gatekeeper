using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCI.WCM.CMSManager.CMS
{
    //internal class PercussionGuid
    //{
    //    private long guid;

    //    public PercussionGuid()
    //    {

    //    }
    //    public PercussionGuid(int id)
    //    {
    //        setId(id);
    //        setRevision(-1);
    //        setType(101);
    //    }
    //    public PercussionGuid(Long guid)
    //    {
    //        this.guid = guid;
    //    }

    //    public long getGuid()
    //    {
    //        return guid;
    //    }

    //    public void setGuid(long guid)
    //    {
    //        this.guid = guid;
    //    }

    //    public int getId()
    //    {
    //        return getId(guid);
    //    }
    //    public void setId(int id)
    //    {
    //        ulong mask = 0xFFFFFFFF00000000L;
    //        guid = (mask & guid) | (long)id);
    //    }

    //    public int getRevision()
    //    {

    //        return getRevision(guid);
    //    }

    //    public void setRevision(int revision)
    //    {
    //        long mask = 0x000000FFFFFFFFFFL;
    //        guid = (mask & guid) | (long)revision << 40;
    //    }

    //    public int getType()
    //    {
    //        return getType(guid);
    //    }

    //    public void setType(int type)
    //    {
    //        ulong mask = 0xFFFFFF00FFFFFFFFL;
    //        guid = (mask & guid) | (long)type << 32;

    //    }

    //    public String toString()
    //    {
    //        return "" + getType() + "-" + getId() + "-" + getRevision();
    //    }

    //    public int getId(long id)
    //    {
    //        return (int)(id & 0xFFFFFFFFL);
    //    }
    //    public int getRevision(long id)
    //    {

    //        return (int)(id >> 40);
    //    }
    //    public int getType(long id)
    //    {
    //        return (int)(id >> 32) & 0xFF;
    //    }
    //}
}
