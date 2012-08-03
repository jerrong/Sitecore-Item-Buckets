function GetDialogArguments() {
    return getRadWindow().ClientParameters;
}

function getRadWindow() {


  if (window.parent.radWindow) {
        return window.parent.radWindow;
  }
    
    if (window.parent.frameElement && window.parent.frameElement.radWindow) {
        return window.parent.frameElement.radWindow;
    }
    
    return null;
}

var isRadWindow = true;

var radWindow = getRadWindow();

if (radWindow) { 
  if (window.parent.dialogArguments) { 
    radWindow.window.parent = window.parent;
  } 
}

function scClose(url, text) {
	var returnValue = {
		url:url,
		text:text
	};

	window.parent.close(returnValue);

}

function scCancel() {
  getRadWindow().close();
}

function scCloseWebEdit(url) {
  window.parent.returnValue = url;
  window.parent.close();
}

if (window.parent.focus) { //&& Prototype.Browser.Gecko
  window.parent.focus();
}