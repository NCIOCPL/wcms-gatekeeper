<!--
//creates appropriate pop-up window
function popWindow(type, urlargs){
	if (type == "privacy") {
		window.open('common/popUps/popPrivacy.aspx','','scrollbars=no,resizable=yes,width=300,height=300');				
	} else if (type == "livehelp") {
		window.open('common/popUps/popLiveHelp.aspx','LiveHelp','scrollbars=yes,resizable=yes,menubar=yes,toolbar=yes,location=yes,width=425,height=500');				
	} else if (type == "definition") {
		urlargs = urlargs.replace(/\s/g, '+');
		window.open('common/popUps/popDefinition.aspx?term=' + urlargs,'','scrollbars=yes,resizable=yes,width=350,height=450');
	} else if (type == "defbyid") {
		window.open('common/popUps/popDefinition.aspx?id=' + urlargs,'','scrollbars=yes,resizable=yes,width=350,height=450');
	} else if (type == "file") {
		window.open(urlargs, '', 'scrollbars=yes,resizable=yes,width=550,height=550');
	} else if (type == "fullbrowser") {
		window.open(urlargs, '', 'menubar=yes,location=yes,status=yes,toolbar=yes,titlebar=yes,scrollbars=yes,resizable=yes,width=675,height=510');
    } else if (type == "small") {
		window.open(urlargs, '', 'scrollbars=no,resizable=no,menubar=no,status=no,toolbar=no,titlebar=no,width=200,height=100,left=400,screenX=400,top=300,screenY=300');
	}
}

function dynPopWindow(url, name, windowAttributes)
{
	options = '';
	optWidth = 'width=500';
	optHeight = 'height=500';
	optScrollbar = 'scrollbars=yes';
	optResizable = 'resizable=yes';
	optMenubar = 'menubar=yes';
	optLocation = 'location=yes';
	optStatus = 'status=yes';
	optToolbar = 'toolbar=yes';
	
	windowOptions = windowAttributes.split(',');

	for(i = 0; i < windowOptions.length; i++)
	{
		attribute = windowOptions[i].substring(0, windowOptions[i].indexOf('=')).toLowerCase();
		
		if(attribute == 'width'){
			optWidth = windowOptions[i];
		} else if(attribute == 'height'){
			optHeight = windowOptions[i];
		} else if(attribute == 'scrollbars'){
			optScrollbar = windowOptions[i];	
		} else if(attribute == 'resizable'){
			optResizable = windowOptions[i];	
		} else if(attribute == 'menubar'){
			optMenubar = windowOptions[i];	
		} else if(attribute == 'location'){
			optLocation = windowOptions[i];	
		} else if(attribute == 'status'){
			optStatus = windowOptions[i];	
		} else if(attribute == 'toolbar'){
			optToolbar = windowOptions[i];	
		}
	}
	
	options = optWidth + ',' + optHeight + ',' + optScrollbar + ',' + optResizable + ',' + optMenubar + ',' + optLocation + ',' + optStatus + ',' + optToolbar;
	
	window.open(url, name, options);
	
}
//-->