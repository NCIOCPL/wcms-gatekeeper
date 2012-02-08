//selects all checkboxes
function SelectOrClearAll(bSelect) {
    //debugger;

    var tblMain = document.all["tblMain"];
    for (i = 0; i < tblMain.rows.length; i++) {
        if (tblMain.rows[i].children[0].children[0].tagName.toLowerCase() == "input" && tblMain.rows[i].children[0].children[0].type.toLowerCase() == "checkbox")
            tblMain.rows[i].children[0].children[0].checked = bSelect;
    }
}

function ValidateDetails(oSrc, args) {
    var theForm = document.aspnetForm;
    if (!theForm)
        return;
    re = /ActionDropDown/i
    for (i = 0; i < theForm.elements.length; i++) {
        if (theForm.elements[i].tagName.toLowerCase() == "select" && theForm.elements[i].id.search(re) > -1) {
            if (theForm.elements[i].options[theForm.elements[i].selectedIndex].text != "") {
                args.IsValid = true;
                return;
            }
            else {
                args.IsValid = false;
                return;
            }
        }
    }
    args.IsValid = false;
}

function ValidateCDRID(oSrc, args) {
    if (document.all["lblPushChkbx1"].children[0].checked || document.all["lblPushChkbx2"].children[0].checked)
        args.IsValid = true;
    else
        args.IsValid = false;
}


function confirm_deactivate() {
    if (confirm("Are you sure you want to deactivate processing of queued batches?") == true)
        return true;
    else
        return false;
}

function ConfirmRequestAbort() {
    var okToAbort = false;

    if (confirm("Do you really want to abort this request?"))
        okToAbort = true;

    return okToAbort;
}

function checkBoxSelected(sender, args) {
    var checkBox = document.getElementById(sender.RB1);
    args.IsValid = checkBox.checked;
}

function checkBoxSelectedInList(sender, args) {

    //debugger;+
    var tblMain = null;

    if (document.all) {
        tblMain = document.all["tblMain"];
    }
    else {
        tblMain = document.getElementById("tblMain");
    }
    
    for (i = 0; i < tblMain.rows.length; i++) {
        if (tblMain.rows[i].children[0].children[0].tagName.toLowerCase() == "input" && tblMain.rows[i].children[0].children[0].type.toLowerCase() == "checkbox") {
            if (tblMain.rows[i].children[0].children[0].checked == true) {
                args.IsValid = true;
                return true;
            }
        }
    }

    args.IsValid = false;
    return false;
}