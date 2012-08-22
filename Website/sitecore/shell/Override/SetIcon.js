function scClick(element, evt) {
  evt = scForm.lastEvent ? scForm.lastEvent : evt;
  var icon = evt.srcElement ? evt.srcElement : evt.target;
  
  var edit = scForm.browser.getControl("IconFile");
  
  if (!icon) {
    return;
  }
   
  var src = null;
  if (icon.tagName && icon.tagName.toLowerCase() == "img" && icon.className == "scRecentIcon") {
    src = icon.src;
  }
  else {
    src = scForm.browser.isIE ? icon["sc_path"] : icon.getAttribute("sc_path");
  }
   
  if (src == null) {
    return;
  }
  
  var n = src.indexOf("/~/icon/");
  
  if (n >= 0) {
    src = src.substr(n + 8); 
  }
  else if (src.substr(0, 32) == "/sitecore/shell/themes/standard/") {
    src = src.substr(32);
  }
  
  n = src.indexOf("/temp/IconCache/");
  if (n >= 0) {
    src = src.substr(n + 16);
  }
  
  if (src.substr(src.length - 5, 5) == ".aspx") {
    src = src.substr(0, src.length - 5);
  }
  
  edit.value = src;
}

function scChange(element, evt) {
  var element = scForm.browser.getControl("Selector");
  
  var id = element.options[element.selectedIndex].value + "List";
  
  var list = scForm.browser.getControl("List");
  
  var childNodes = list.childNodes;
  
  for(var n = 0; n < childNodes.length; n++) {
    var element = childNodes[n];
    
    element.style.display = (element.id == id ? "" : "none");
  }

  scUpdateControls();
}

function scUpdateControls() {
  if (!scForm.browser.isIE) {
    scForm.browser.initializeFixsizeElements();
  }
}