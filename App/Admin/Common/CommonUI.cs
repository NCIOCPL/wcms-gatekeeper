using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace GateKeeperAdmin.Common
{
    public class CommonUI
    {
        //populate a drop down with values bases on the enumerated type 
        //if bInsertBlank is true, insert a blank line with -1 index 
        public static void PopulateDropDownFromEnum(DropDownList dl, Type enumType, bool bInsertBlank)
        {

            if (!enumType.IsSubclassOf(typeof(Enum)))
                throw new Exception("Exception in PopulateDropDownFromEnum. Must be an Enum");

            if (bInsertBlank)
                dl.Items.Add(new ListItem("", "-1"));

            foreach (int eVal in Enum.GetValues(enumType))
            {
                if (eVal > 0)
                {
                    string nm = Enum.ToObject(enumType, eVal).ToString();
                    dl.Items.Add(new ListItem(nm, eVal.ToString()));
                }
            }
        }

        public static void SelectOrClearAllCheckboxes(Repeater rptRepeater, string strChkBoxId, bool bCheck)
        {
            CheckBox CheckBx;
            foreach (RepeaterItem ri in rptRepeater.Items)
            {
                CheckBx = (CheckBox)ri.FindControl(strChkBoxId);
                if (CheckBx != null)
                    CheckBx.Checked = bCheck;
            }
        }
    }
}
