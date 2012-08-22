function scContentEditor() {
    this.lastVisibleStrip = 0;
    this.fontSize = 0;
    this.isContextualTab = false;
    this.currentSmartTag = null;
    this.currentGallery = null;
    this.validationTimer = null;
    this.validationTimer2 = null;
    this.contentFocus = null;
    this.treeFocus = null;
    this.ribbonFocus = null;
    this.validatorUpdateDelay = null;
  
    scForm.Content = this;

    scForm.browser.attachEvent(document, "onkeyup", function (evt) { scContent.onKeyUp(evt ? evt : window.event) });
    scForm.browser.attachEvent(document, "oncut", function (evt) { scContent.onStartValidators(evt ? evt : window.event) });
    scForm.browser.attachEvent(document, "onpaste", function (evt) { scContent.onStartValidators(evt ? evt : window.event) });
    scForm.browser.attachEvent(window, "onunload", function (evt) { scContent.onUnload(evt ? evt : window.event) });
    scForm.browser.attachEvent(window, "onload", function (evt) { scContent.onLoad(evt ? evt : window.event) });
}

scContentEditor.prototype.onLoad = function (evt) {
    scContentEditorUpdated();
    var ctl = scForm.browser.getControl("ContentTreeSplitter");
    if (ctl != null) {
        ctl.style.display = scForm.browser.getControl("ContentTreePanel").style.display;
    }

    if (!Prototype.Browser.Gecko) {
        return;
    }

    var tree = $$("#ContentTreePanel > div")[0];
    if (tree) {
        tree.observe("scroll", function () {
            if (scContent.treeRelayoutTimer || scContent.ignoreTreeScroll) {
                return;
            }

            scContent.treeRelayoutTimer = setTimeout(function () {
                scContent.ignoreTreeScroll = true;
                tree.setStyle({ opacity: tree.getStyle("opacity") == 1 ? 0.999 : 1 });
                scContent.treeRelayoutTimer = null;
                setTimeout("scContent.ignoreTreeScroll = null", 100);
            }, 250);
        });
    }
}

scContentEditor.prototype.onKeyUp = function (evt) {
    evt = (evt ? evt : window.event);

    if (evt.keyCode == 27) {
        if (this.currentGallery != null) {
            this.closeGallery(evt, false, "key up");
        }
    }
}

scContentEditor.prototype.onStartValidators = function (evt) {
    var srcElement = scForm.browser.getSrcElement(evt);

    if (srcElement.tagName == "INPUT" || srcElement.tagName == "SELECT" || srcElement.tagName == "TEXTAREA" || srcElement.isContentEditable) {
        if (!(srcElement.readOnly == true || srcElement.disabled == true)) {
            if (!scForm.isFunctionKey(evt, true)) {
                this.startValidators();
            }
        }
    }
}

scContentEditor.prototype.onUnload = function (evt) {
    if (window.location.href.indexOf("mo=preview") >= 0) {
        var postAction = "";

        var ctl = scForm.browser.getControl("scPostAction");
        if (ctl != null) {
            postAction = ctl.value;
        }

        var opener = window.opener;

        try {
            if (opener != null) {
                opener = opener.parent;
            }

            if (opener != null) {
                if (postAction.substr(0, 5) == "goto:") {
                    opener.location.href = postAction.substr(5);
                }
                else if (postAction.substr(0, 5) == "back:") {
                    opener.history.back();
                }
                else {
                    opener.location.reload(true);
                }
            }
        }
        catch (e) {
            var text = $("scPostActionText").innerHTML;
            if (text != "") {
                alert(text);
            }
        }
    }
}

scContentEditor.prototype.onEditorClick = function (sender, evt) {
    try {
        if (window.event != null && window.event.scClickReason == "ignore") {
            scForm.browser.clearEvent(evt, true, false);
            return;
        }
    }
    catch (e) {
        // silent
    }

    this.smartTag(this);
    scForm.browser.closePopups('EditorClick');
    this.closeGallery(evt, false, 'EditorClick');
}

scContentEditor.prototype.getID = function (id) {
    var n = id.indexOf("_");

    if (n >= 0) {
        return id.substr(0, n);
    }

    return id;
}

// ------------------------------------------------------------------
// Communication
// ------------------------------------------------------------------

scContentEditor.prototype.execute = function (command, callback) {
    var request = this.getExecuteRequest.apply(this, arguments);
    request.send();
}

scContentEditor.prototype.getExecuteRequest = function (command, callback) {
    var request = new scRequest();

    var url = "/sitecore/shell/Applications/Content Manager/Execute.aspx?cmd=" + command;

    for (var n = 2; n < arguments.length - 1; n += 2) {
        url += "&" + arguments[n] + "=" + encodeURIComponent(arguments[n + 1]);
    }

    var content = window.location.href;
    var n = content.indexOf("sc_content=");
    if (n >= 0) {
        content = content.substr(n);

        n = content.indexOf("&");
        if (n >= 0) {
            content = content.substr(0, n);
        }

        n = content.indexOf("#");
        if (n >= 0) {
            content = content.substr(0, n);
        }

        url += "&" + content;
    }

    request.url = url;
    request.async = true;
    request.callback = callback;
    request.handle = this.executeHandler;

    return request;
}

scContentEditor.prototype.executeHandler = function (parameters, callback) {
    if (this.httpRequest != null) {
        if (this.httpRequest.status != "200") {
            scForm.browser.showModalDialog("/sitecore/shell/controls/error.htm", new Array(this.httpRequest.responseText), "center:yes;help:no;resizable:yes;scroll:yes;status:no;");
            return;
        }

        if (this.httpRequest.responseText.indexOf("/sitecore/login/login.js") >= 0) {
            window.top.location = "/sitecore";
            return;
        }

        this.response = this.httpRequest.responseText;
    }

    if (this.response != null) {
        if (this.callback != null) {
            this.callback(this.response, this);
        }
    }
}

// ------------------------------------------------------------------
// Ribbon
// ------------------------------------------------------------------

scContentEditor.prototype.canToggleRibbon = function () {
    var doc = window.top.document;
    return !doc.getElementById("scWebEditRibbon");
}

scContentEditor.prototype.getRibbonStrips = function (id) {
    var result = new Array();

    var parent = scForm.browser.getControl(id + "_Toolbar");

    if (parent != null) {
        for (var e = scForm.browser.getEnumerator(parent.getElementsByTagName("DIV")); !e.atEnd(); e.moveNext()) {
            var ctl = e.item();

            if (ctl.className == "scRibbonToolbarStrip" || ctl.className == "scRibbonToolbarContextualStrip") {
                result.push(ctl);
            }
        }
    }

    return result;
}

scContentEditor.prototype.ribbonNavigatorButtonClick = function (sender, evt, id) {
    this.setActiveStrip(id, false);
    return false;
}



scContentEditor.prototype.ribbonNavigatorButtonDblClick = function (sender, evt, id) {
    this.setActiveStrip(id, this.canToggleRibbon());
    return false;
}

scContentEditor.prototype.ribbonNavigatorWheel = function (sender, evt) {
    var strips = this.getRibbonStrips(this.getID(sender.id));
    var index = -1;

    for (var n = 0; n < strips.length; n++) {
        var ctl = strips[n];

        if (ctl.style.display == "") {
            index = n;
            break;
        }
    }

    var delta = (evt.wheelDelta > 0 ? 1 : -1);

    if (index >= 0) {
        index = index + delta;
    }
    else {
        index = this.lastVisibleStrip;
    }

    index = (index < 0 ? 0 : index >= strips.length ? index = strips.length - 1 : index);

    this.setActiveStrip(strips[index].id, false);

    return false;
}

scContentEditor.prototype.setActiveStrip = function (id, toggleRibbon) {
    var ribbonID = this.getID(id);

    var strips = this.getRibbonStrips(ribbonID);

    for (var n = 0; n < strips.length; n++) {
        var ctl = strips[n];

        var nav = scForm.browser.getControl(ribbonID + "_Nav_" + ctl.id.substr(ctl.id.lastIndexOf("_") + 1));

        if (nav != null) {
            if (ctl.id == id) {
                var display = toggleRibbon ? (ctl.style.display == "none" ? "" : "none") : "";

                var row = scForm.browser.getControl(ribbonID + "_Toolbar");

                ctl.style.display = display;
                row.style.display = display;

                if (ctl.firstChild.tagName == "TEXTAREA") {
                    ctl.innerHTML = ctl.firstChild.value;
                }

                if (display == "") {
                    this.lastVisibleStrip = n;
                }

                nav.className = nav.className.replace(/Normal/gi, "Active");

                this.isContextualTab = nav.className.indexOf("Contextual") >= 0;

                if (!this.isContextualTab) {
                    this.lastNoneContextualStrip = id;
                }
            }
            else {
                if (this.lastNoneContextualStrip == null && ctl.style.display == "") {
                    this.lastNoneContextualStrip = ctl.id;
                }

                ctl.style.display = "none";
                nav.className = nav.className.replace(/Active/gi, "Normal");
            }
        }
    }

    var ctl = scForm.browser.getControl("scActiveRibbonStrip");
    if (ctl != null) {
        ctl.value = id;
    }

    return false;
}

// ------------------------------------------------------------------
// Tree
// ------------------------------------------------------------------

scContentEditor.prototype.onTreeClick = function (sender, evt) {
    var ctl = scForm.browser.getSrcElement(evt);

    if (ctl != null) {
        if (ctl.id != null && ctl.id.indexOf("_Glyph_") >= 0) {
            return this.onTreeGlyphClick(ctl, ctl.id.substr(ctl.id.lastIndexOf("_") + 1), this.getID(ctl.id));
        }

        while (ctl != null && ctl.tagName != "A") {
            ctl = ctl.parentNode;
        }

        if (ctl != null) {
            if ($(ctl).hasClassName("scBeenDragged")) {
                console.info("been dragged click");
                Event.stop(evt);
                $(ctl).removeClassName("scBeenDragged");
                return;
            }

            if (ctl.id != null && ctl.id.indexOf("_Node_") >= 0) {
                return this.onTreeNodeClick(ctl, ctl.id.substr(ctl.id.lastIndexOf("_") + 1));
            }
        }
    }

    this.onEditorClick(sender, evt);

    scForm.browser.clearEvent(evt, true, false);

    return false;
}

scContentEditor.prototype.onTreeNodeClick = function (sender, id) {
    sender = $(sender);

    sender.appendChild(new Element("img", { src: "/sitecore/images/blank.gif", "class": "scLoadingGlyph", alt: "", border: "0" }));

    setTimeout(function () {
        scForm.disableRequests = true;

        scForm.postRequest("", "", "", "LoadItem(\"" + id + "\")");

        sender.removeChild(sender.lastChild)
    }, 1);

    return false;
}

scContentEditor.prototype.collapseTreeNode = function (sender) {
    if (typeof (sender) == "string") {
        sender = scForm.browser.getControl(sender);
    }

    if (sender != null) {
        node = sender.parentNode;
    }

    if (node != null) {
        while (node.childNodes.length > 3) {
            node.removeChild(node.childNodes[3]);
        }

        scForm.browser.setOuterHtml(node.childNodes[0], scForm.browser.getOuterHtml(node.childNodes[0]).replace("/collapse15x15", "/expand15x15"));
    }
}

scContentEditor.prototype.expandTreeNode = function (sender, result) {
    var node = null;

    if (typeof (sender) == "string") {
        sender = scForm.browser.getControl(sender);
    }

    if (sender != null) {
        node = sender.parentNode;
    }

    if (node != null) {
        this.collapseTreeNode(sender);

        if (result != "") {
            var container = node.ownerDocument.createElement("div");

            node.appendChild(container);

            container.innerHTML = result;

            scForm.browser.setOuterHtml(node.childNodes[0], scForm.browser.getOuterHtml(node.childNodes[0]).replace("/expand15x15", "/collapse15x15").replace("/noexpand15x15", "/collapse15x15").replace("/loading15x15", "/collapse15x15"));

            document.fire("sc:contenttreeupdated", node);
        }
        else {
            var container = node.ownerDocument.createElement("div");

            node.appendChild(container);

            container.innerHTML = "<div class=\"scContentTreeNode\"><img src=\"/sitecore/shell/themes/standard/images/noexpand15x15.gif\" id=\"Tree_Glyph_DE94472DBCFA4B159F1504ECEF5FA9CC\" class=\"scContentTreeNodeGlyph\" alt=\"\" border=\"0\"><div id=\"GutterDE94472DBCFA4B159F1504ECEF5FA9CC\" class=\"scContentTreeNodeGutter\"></div><a id=\"\" href=\"#\" class=\"scContentTreeNodeNormal\" style=\"position: relative; \"><span style=\"opacity:0.3;\"><img src=\"/temp/IconCache/business/16x16/chest_add.png\" width=\"16\" height=\"16\" class=\"scContentTreeNodeIcon\" alt=\"\" border=\"0\">You have hidden items...</span></a></div>";

            scForm.browser.setOuterHtml(node.childNodes[0], scForm.browser.getOuterHtml(node.childNodes[0]).replace("/expand15x15", "/noexpand15x15").replace("/collapse15x15", "/noexpand15x15").replace("/loading15x15", "/noexpand15x15"));

            document.fire("sc:contenttreeupdated", node);
        }
    }
}

scContentEditor.prototype.onTreeGlyphClick = function (sender, id, treeid) {
    if (sender.src.indexOf("expand15x15") >= 0) {
        function expandTreeNode(result) {
            scContent.expandTreeNode(sender, result);
        }

        var parent = sender.parentNode;

        scForm.browser.setOuterHtml(sender, scForm.browser.getOuterHtml(sender).replace("/expand15x15", "/loading15x15").replace("/collapse5x15", "/loading15x15"));

        sender = parent.childNodes[0];

        var language = $F("scLanguage");

        this.execute("GetTreeviewChildren", expandTreeNode, "id", id, "treeid", treeid, "ro", this.getQueryString("ro"), "la", language);
    }
    else {
        this.collapseTreeNode(sender);
    }

    return false;
}

scContentEditor.prototype.getActiveTreeNode = function (control) {
    while (control != null && control.className != "scContentTree") {
        control = control.parentNode;
    }

    if (control != null) {
        for (var e = scForm.browser.getEnumerator(control.getElementsByTagName("A")); !e.atEnd(); e.moveNext()) {
            var ctl = e.item();

            if (ctl.className == "scContentTreeNodeActive") {
                return ctl;
            }
        }
    }

    return null;
}

scContentEditor.prototype.setActiveTreeNode = function (id, path, treeid) {
    var active = scForm.browser.getControl(id);

    if (active != null) {
        var ctl = this.getActiveTreeNode(active);

        if (ctl != null) {
            ctl.className = "scContentTreeNodeNormal";
        }

        active.className = "scContentTreeNodeActive";
    }
    else if (path != null && path != "") {
        var parts = path.split("/");

        var ctl = this.getActiveTreeNode(scForm.browser.getControl(parts[1]));

        if (ctl != null) {
            ctl.className = "scContentTreeNodeNormal";
        }

        for (var n = 1; n < parts.length; n++) {
            var ctl = scForm.browser.getControl(parts[n]);

            if (ctl == null) {
                function expandTreeToNode(result) {
                    scContent.expandTreeNode(parts[n - 1], result);
                }

                var language = $F("scLanguage");

                this.execute("ExpandTreeviewToNode", expandTreeToNode, "root", parts[n - 1], "id", id, "treeid", treeid, "ro", this.getQueryString("ro"), "la", language);

                break;
            }
        }
    }
}

scContentEditor.prototype.getTreeDragParameters = function (tag, evt) {
    var control = scForm.browser.getSrcElement(evt);

    while (control != null && control.tagName != "A") {
        control = control.parentNode;
    }

    if (control != null) {
        return "sitecore:item:" + control.id;
    }

    return null;
}

scContentEditor.prototype.onTreeDrag = function (tag, evt) {
    var control = scForm.browser.getSrcElement(evt);

    if (evt.button == 1 || evt.type == "dragstart") {
        while (control != null && control.tagName != "A") {
            control = control.parentNode;
        }

        if (control != null) {
            scForm.drag(tag, evt, "item:" + control.id);
        }
    }
}

scContentEditor.prototype.onTreeDrop = function (tag, evt) {
    var control = document.elementFromPoint(evt.clientX, evt.clientY);

    while (control != null && control.tagName != "A") {
        control = control.parentNode;
    }

    if (control != null && control.id != null && control.id != "") {
        var parameters = null;

        if (evt.type == "drop") {
            parameters = 'Drop("$Data,' + control.id + '")';
        }

        scForm.drop(tag, evt, parameters);
    }
}

scContentEditor.prototype.onTreeContextMenu = function (tag, evt) {
    if (evt.ctrlKey) {
        return;
    }

    var control = scForm.browser.getSrcElement(evt);

    while (control != null && control.tagName != "A") {
        control = control.parentNode;
    }

    if (control != null) {
        scForm.lastEvent = evt;
        scForm.invoke('Tree_ContextMenu("' + control.id + '")');
        scForm.lastEvent = null;
    }
    else if (evt.clientX < 20) {
        scForm.postRequest("", "", "", "Gutter_ContextMenu");
    }

    scForm.browser.clearEvent(evt, true, false);
}

scContentEditor.prototype.findNextTreeNode = function (control, useChildren) {
    if (control != null) {
        if (useChildren) {
            if (control.childNodes.length > 2) {
                return control.childNodes[2].childNodes[0];
            }
        }

        var nextSibling = scForm.browser.getNextSibling(control);
        if (nextSibling != null) {
            return nextSibling;
        }

        if (control.parentNode.className != "scContentTree") {
            var parent = control.parentNode.parentNode;

            return this.findNextTreeNode(parent, false);
        }
    }

    return null;
}

scContentEditor.prototype.findPreviousTreeNode = function (control, useChildren) {
    if (control.previousSibling != null) {
        return this.getLastChildTreeNode(control.previousSibling);
    }

    if (control.parentNode.className != "scContentTree") {
        var parent = control.parentNode.parentNode;

        return parent;
    }

    return null;
}

scContentEditor.prototype.getLastChildTreeNode = function (control) {
    if (control.childNodes.length > 2) {
        var ctl = control.childNodes[2];

        return this.getLastChildTreeNode(ctl.childNodes[ctl.childNodes.length - 1]);
    }

    return control;
}

scContentEditor.prototype.onTreeKeyDown = function (tag, evt) {
    if (evt.altKey || evt.shiftKey || evt.ctrlKey) {
        return;
    }

    var control = this.getActiveTreeNode(scForm.browser.getSrcElement(evt));

    while (control != null && control.tagName != "A") {
        control = control.parentNode;
    }

    if (control != null) {
        control = control.parentNode;
    }

    if (control != null) {
        switch (evt.keyCode) {
            case 37:
                control = control.childNodes[0];

                if (control.src.indexOf("/collapse15x15") >= 0) {
                    control.click();
                    scForm.browser.clearEvent(evt, true, false);
                    scForm.focus(control);
                }
                break;

            case 39:
                control = control.childNodes[0];

                if (control.src.indexOf("/expand15x15") >= 0) {
                    control.click();
                    scForm.browser.clearEvent(evt, true, false);
                    scForm.focus(control);
                }
                break;

            case 38:
                control = this.findPreviousTreeNode(control, true);

                if (control != null) {
                    control.childNodes[1].click();
                    scForm.browser.clearEvent(evt, true, false);
                    scForm.focus(control.childNodes[1]);
                }
                break;

            case 40:
                control = this.findNextTreeNode(control, true);

                if (control != null) {
                    control.childNodes[1].click();
                    scForm.browser.clearEvent(evt, true, false);
                    scForm.focus(control.childNodes[1]);
                }
                break;
        }
    }
}

scContentEditor.prototype.initializeTree = function () {
    var ctl = scForm.browser.getControl("ContentTreePanel");

    if (ctl != null) {
        var visible = (scForm.getCookie("scContentEditorFolders") != "0") && (window.location.href.indexOf("mo=preview") < 0) && (window.location.href.indexOf("mo=mini") < 0) && (window.location.href.indexOf("mo=popup") < 0) || (window.location.href.indexOf("mo=template") >= 0);

        ctl.style.display = visible ? "" : "none";

        var width = scForm.getCookie("scContentEditorFoldersWidth");

        if (width != null && width != "") {
            ctl.style.width = width;
        }
    }
}

// ------------------------------------------------------------------
// Galleries
// ------------------------------------------------------------------

scContentEditor.prototype.showGallery = function (sender, evt, id, src, parameters, width, height, where) {
    scForm.focus(sender); // IE bug work-around
    this.closeGallery(evt, true, 'showGallery');
    Event.stop(evt);

    // To do: insert more proper logic than silent try/catch
    if ($("scGalleries")) {
        try {
            var size = $F("scGalleries").toQueryParams()[id];
            if (size != null && size != "") {
                var parts = size.split("q");
                width = parts[0];
                height = parts[1];
            }
        } catch (ex) {
            console.error(ex);
        }
    }

    var url = "/sitecore/shell/default.aspx?xmlcontrol=" + src + "&" + parameters;

    var iframe = document.createElement("IFRAME");

    iframe.id = id;
    iframe.className = "scGalleryFrame";
    iframe.frameBorder = 0;
    iframe.allowTransparency = true;
    iframe.ondeactivate = function () { return scContent.closeGallery(id, null, 'ondeactivate') };
    iframe.src = url;
    // iframe.style.display = "none";

    iframe.width = width;
    iframe.height = height;

    var bounds = new scRect();
    bounds.clientToScreen(sender);

    if (where == "expanding") {
        iframe.style.left = (bounds.left + 2) + "px";
        iframe.style.top = (bounds.top) + "px";
        if (iframe.width == null || iframe.width == "") {
            iframe.width = sender.offsetWidth + "px";
        }
    }
    else {
        iframe.style.left = (bounds.left + 3) + "px";
        iframe.style.top = (bounds.top + sender.offsetHeight + 2) + "px";
    }

    sender.insertBefore(iframe, sender.firstChild);

    this.currentGallery = id;

    return false;
}

scContentEditor.prototype.closeGallery = function (evt, clearEvent, reason) {
    var ctl = scForm.browser.getControl(this.currentGallery);

    if (ctl != null) {
        ctl.parentNode.removeChild(ctl);
    }

    this.currentGallery = null;

    if (scForm.browser.isIE && clearEvent && evt != null) {
        scForm.browser.clearEvent(evt, true, false);
    }

    return false;
}

scContentEditor.prototype.showOutOfFrameGallery = function (sender, evt, destination, dimensions, params, httpVerb) {
    Event.stop(evt);
    var galleryObj = null;
    for (var win = window.self; ; win = win.parent) {
        if (typeof (win.Sitecore) != "undefined" && typeof (win.Sitecore.OutOfFrameGallery) != "undefined") {
            galleryObj = win.Sitecore.OutOfFrameGallery;
            break;
        }

        if (win == window.top) {
            break;
        }
    }

    if (galleryObj == null) {
        console.error("OutOfFrameGallery object not found.");
        return;
    }

    scForm.focus(sender); // IE bug work-around     
    galleryObj.open(sender, destination, dimensions, params, httpVerb);
}

scContentEditor.prototype.showMenu = function (sender, evt, id) {
    scForm.showPopup(null, sender.id, id, "below");
    scForm.browser.clearEvent(evt, true, false);
    return false;
}

// ------------------------------------------------------------------
// Editor
// ------------------------------------------------------------------

scContentEditor.prototype.toggleSection = function (id, sectionName) {
    var e = $(id);
    if (e == null) {
        return;
    }

    var nextSibling = e.next();
    var visible = nextSibling.style.display == "none";

    e.className = visible ? "scEditorSectionCaptionExpanded" : "scEditorSectionCaptionCollapsed";
    var displayStyle = "block";
    if (Prototype.Browser.Gecko && nextSibling.nodeName.toLowerCase() == "table") {
        displayStyle = "table";
    }
    nextSibling.style.display = visible ? displayStyle : "none";

    var scSections = $("scSections");
    var value = scSections.value;

    var p = value.toQueryParams();
    p[sectionName] = visible ? "0" : "1";
    scSections.value = Object.toQueryString(p);

    var glyph = $(id + "_Glyph");
    if (glyph != null) {
        var replace = (visible ? "/expand15x15" : "/collapse15x15");
        var replaceWith = (visible ? "/collapse15x15" : "/expand15x15");

        scForm.browser.setOuterHtml(glyph, scForm.browser.getOuterHtml(glyph).replace(replace, replaceWith));
    }

    if (this.onToggleSection) {
        for (var k = 0; k < this.onToggleSection.length; k++) {
            this.onToggleSection[k](id, sectionName);
        }
    }
}

// ------------------------------------------------------------------
// Folders
// ------------------------------------------------------------------

scContentEditor.prototype.toggleFolders = function() {
    var ctl = scForm.browser.getControl("ContentTreePanel");

    if (ctl != null) {
        var visible = !(ctl.style.display == "");

        ctl.style.display = visible ? "" : "none";

        scForm.setCookie("scContentEditorFolders", visible ? "1" : "0");

        var button = scForm.browser.getControl("RibbonToggleFolders");

        button.childNodes[0].checked = visible;

        ctl = scForm.browser.getControl("ContentTreeSplitter");
        ctl.style.display = visible ? "" : "none";
    }

    if (typeof(scGeckoRelayout) != "undefined") {
        scForm.browser.initializeFixsizeElements();
    }
};

// ------------------------------------------------------------------
// Item Buckets Fullscreen
// ------------------------------------------------------------------

scContentEditor.prototype.fullScreen = function () {
     if (!scForm.browser.isIE) {
    window.$sc(window.parent.document).fullScreen(true);
}
};

scContentEditor.prototype.mouseDown = function (tag, evt) {
    if (!this.dragging) {
        this.bounds = new scRect();
        this.bounds.getControlRect(tag);
        this.bounds.move(0, 0);
        this.bounds.clientToScreen(tag);

        if (!scForm.browser.isIE) {
            this.bounds.height -= 4;
        }

        this.trackCursor = new scPoint();
        this.trackCursor.setPoint(evt.screenX, evt.screenY);

        this.dragging = true;
        this.delta = 0;

        scForm.browser.setCapture(tag, function (tag, evt) { scContent.mouseMove(tag, evt) });

        scForm.browser.clearEvent(evt, false);

        return false;
    }
}

scContentEditor.prototype.mouseMove = function (tag, evt) {
    if (this.dragging) {
        if (evt.type == "mouseup") {
            return this.mouseUp(tag, evt);
        }

        if (this.outline == null) {
            this.outline = scForm.browser.getControl("outline");
            this.outline.style.display = "";
        }

        var dx = evt.screenX - this.trackCursor.x;

        this.bounds.offset(dx, 0);

        this.delta += dx;

        this.bounds.apply(this.outline);

        this.trackCursor.setPoint(evt.screenX, evt.screenY);

        scForm.browser.clearEvent(evt, false);

        return false;
    }
}

scContentEditor.prototype.mouseUp = function (tag, evt) {
    if (this.dragging) {
        this.dragging = false;

        scForm.browser.clearEvent(evt, false);

        scForm.browser.releaseCapture(tag);

        if (this.outline != null) {
            this.outline.style.display = "none";
            this.outline = null;
        }

        var ctl = tag;

        while (ctl != null && ctl.tagName != "TD") {
            ctl = ctl.parentNode;
        }

        if (ctl != null) {
            var prev = scForm.browser.getPreviousSibling(ctl);
            var next = scForm.browser.getNextSibling(ctl);

            var left = prev.offsetWidth;
            var right = next.offsetWidth;

            var v = left + this.delta;

            if (v < 32) {
                this.delta = -(left - 32);
            }
            else if (v > left + right - 32) {
                this.delta = right - 32;
            }

            prev.style.width = "" + (left + this.delta) + "px";

            scForm.setCookie("scContentEditorFoldersWidth", prev.offsetWidth.toString());
        }

        return false;
    }
}

scContentEditor.prototype.activateContextualTab = function (id) {
    var contextuals = scForm.browser.getControl(id + "_ContextualToolbar");

    for (var s = scForm.browser.getEnumerator(contextuals.childNodes); !s.atEnd(); s.moveNext()) {
        if (s.item().className == "scRibbonToolbarStrip" || s.item().className == "scRibbonToolbarContextualStrip") {
            scContent.setActiveStrip(s.item().id, false);
            return true;
        }
    }

    return false
}

scContentEditor.prototype.activate = function (tag, evt, parameters) {
    if (evt != null) {
        scForm.activate(tag, evt);
    }

    if (parameters != this.lastActivated) {
        if (parameters.indexOf("&rib=1") >= 0) {
            this.updateContextualTabs(tag, evt, parameters);
        }
        else {
            this.clearContextualTabs();
        }
    }

    this.lastActivated = parameters;
    this.activeField = tag.id;
}

scContentEditor.prototype.clearContextualTabs = function (reset) {
    var reset = false;

    var ctl = scForm.browser.getControl("Ribbon_ContextualToolbar");
    if (typeof (ctl) == "undefined" || !ctl) {
        return;
    }

    reset = ctl.childNodes.length > 0;
    ctl.innerHTML = "";

    $$("#Ribbon_Navigator .scRibbonNavigatorButtonsContextualGroup").invoke("remove");

    if (reset && this.lastNoneContextualStrip != null) {
        this.setActiveStrip(this.lastNoneContextualStrip, false);
    }
}

scContentEditor.prototype.updateContextualTabs = function (tag, evt, parameters) {

    var ctl = scForm.browser.getControl("Ribbon_Navigator");
    if (ctl == null) {
        return;
    }


    function setTabs(result) {
        if (result != "" && result != null) {
            eval("var data = " + result);

            var ctl = scForm.browser.getControl("Ribbon_Navigator");
            if (ctl != null) {
                var div = document.createElement("div");

                div.innerHTML = data.navigator;

                while (div.childNodes.length > 0) {
                    ctl.appendChild(div.childNodes[0]);

                }
            }

            ctl = scForm.browser.getControl("Ribbon_ContextualToolbar");
            if (ctl != null) {
                ctl.innerHTML = data.strips;
            }

            if (scContent.isContextualTab) {
                scContent.setActiveStrip(scContent.lastNoneContextualStrip, false);
            }
        }
    }

    for (var key in scForm.suspended) {
        if (key != null) {
            window.setTimeout(function () { scContent.updateContextualTabs(tag, null, parameters) }, 0);
            return;
        }
    }

    this.execute("GetContextualTabs", setTabs, "parameters", parameters);
}

scContentEditor.prototype.deactivate = function (tag, evt, parameters) {
    scForm.activate(tag, evt);
}

scContentEditor.prototype.fire = function (command) {
    if (this.activeField != null) {
        var frame = scForm.browser.getControl(this.activeField);

        if (frame != null) {
            try {
                frame.contentWindow.scFire(command);
            } catch (ex) {
                console.log("Failed to accees frame. This typically happens due to a Permission Denied exception in IE9 caused by an intricate issue with Popup and ShowModalDialog calls. " + ex.message);
            }
        }
    }

    var evt = window.event;
    if (evt != null) {
        scForm.browser.clearEvent(evt, true, false);
    }
}

scContentEditor.prototype.fireCombobox = function (tag, evt, command) {
    if (this.activeField != null) {
        var frame = scForm.browser.getControl(this.activeField);

        if (frame != null) {
            var value = tag.options[tag.selectedIndex].value;

            try {
                frame.contentWindow.scFireCombobox(command, value);
            } catch (ex) {
                console.log("Failed to accees frame. This typically happens due to a Permission Denied exception in IE9 caused by an intricate issue with Popup and ShowModalDialog calls. " + ex.message);
            }
        }
    }
}

scContentEditor.prototype.getQueryString = function (name) {
    var qs = window.location.href;

    var n = qs.indexOf("?");
    if (n < 0) {
        return "";
    }

    qs = "&" + qs.substr(n + 1);

    var n = qs.indexOf("&" + name + "=");
    if (n < 0) {
        return "";
    }

    qs = "&" + qs.substr(name.length + 2);

    var n = qs.indexOf("&");
    if (n >= 0) {
        qs = qs.substr(0, n);
    }

    return qs;
}

scContentEditor.prototype.closeSearch = function (sender, evt) {
    var search = scForm.browser.getControl("SearchResultHolder");
    var tree = scForm.browser.getControl("ContentTreeHolder");

    search.style.display = "none";
    tree.style.display = "";

    scForm.browser.clearEvent(evt, true, false);
}

scContentEditor.prototype.watermarkFocus = function (sender, evt) {
    sender = sender || evt.target;

    if (!sender.dirty) {
        sender.watermark = sender.value;
        sender.value = "";
        sender.style.color = "black";
    }
}

scContentEditor.prototype.watermarkBlur = function (sender, evt) {
    if (sender.value == "") {
        sender.value = sender.watermark;
        sender.style.color = "#999999";
    }
    else {
        sender.dirty = true;
    }
}

scContentEditor.prototype.addSearchCriteria = function (sender, evt) {
    var input = $("SearchOptionsAddCriteria");
    var name = input.value;

    if (name == "") {
        return;
    }

    input.value = "";

    var table = $("SearchOptionsList");

    var row = table.insertRow(table.rows.length - 1);

    var cell = $(row.insertCell(0));

    cell.innerHTML = "<a href=\"#\" class=\"scSearchOptionName\" onclick=\"javascript:return scForm.postEvent(this,event,'TreeSearchOptionName_Click',true)\">" + name + ":</a>";
    cell.addClassName("scSearchOptionsNameContainer");

    cell = $(row.insertCell(1));

    name = name.replace(/\"/gi, "");

    cell.innerHTML = "<input class=\"scSearchOptionsInput scIgnoreModified\"/><input type=\"hidden\" value=\"" + name + "\"/>";
    cell.addClassName("scSearchOptionsValueContainer");

    this.updateSearchCriteria();

    return false;
}

scContentEditor.prototype.changeSearchCriteria = function (index, name) {
    var table = $("SearchOptionsList");

    var row = table.rows[parseInt(index, 10)];

    row.cells[0].childNodes[0].innerHTML = name + ":";
    row.cells[1].childNodes[1].value = name;

    this.updateSearchCriteria();

    scForm.browser.closePopups();

    return false;
}

scContentEditor.prototype.removeSearchCriteria = function (index) {
    var table = $("SearchOptionsList");

    var row = table.rows[parseInt(index, 10)];

    row.parentNode.removeChild(row);

    this.updateSearchCriteria();

    scForm.browser.closePopups();

    return false;
}

scContentEditor.prototype.updateSearchCriteria = function () {
    var table = $("SearchOptionsList");

    for (var r = 0; r < table.rows.length - 1; r++) {
        var cell = table.rows[r].cells[0];
        cell.childNodes[0].id = "SearchOptionsControl" + r;

        cell = table.rows[r].cells[1];

        cell.childNodes[0].id = "SearchOptionsValue" + r;
        cell.childNodes[1].id = "SearchOptionsName" + r;
    }
}

// ------------------------------------------------------------------
// Multilist
// ------------------------------------------------------------------

scContentEditor.prototype.multilistMoveLeft = function (id, allOptions) {
    var all = scForm.browser.getControl(id + "_unselected");
    var selected = scForm.browser.getControl(id + "_selected");

    for (var n = 0; n < selected.options.length; n++) {
        var option = selected.options[n];

        if (option.selected || allOptions == true) {
            var index = 0;

            var header = option.innerHTML;

            for (var i = 0; i < all.options.length - 1; i++) {
                if (header < all.options[i].innerHTML) {
                    index = i;
                    break;
                }
            }

            var opt = document.createElement("OPTION");

            all.options.add(opt, index);

            opt.innerHTML = header;
            opt.value = option.value;
        }
    }

    this.multilistRemoveSelected(selected, allOptions, id + "_selected_help");

    this.multilistUpdate(id);
}

scContentEditor.prototype.multilistMoveRight = function (id, allOptions) {
    var all = scForm.browser.getControl(id + "_unselected");
    var selected = scForm.browser.getControl(id + "_selected");

    for (var n = 0; n < all.options.length; n++) {
        var option = all.options[n];

        if (option.selected || allOptions == true) {
            var opt = document.createElement("OPTION");

            selected.appendChild(opt);

            opt.innerHTML = option.innerHTML;
            opt.value = option.value;
        }
    }

    this.multilistRemoveSelected(all, allOptions, id + "_all_help");

    this.multilistUpdate(id);
}

scContentEditor.prototype.multilistMoveDown = function (id) {
    var selected = scForm.browser.getControl(id + "_selected");

    var index = 0;

    for (var n = selected.options.length - 1; n >= 0; n--) {
        var option = selected.options[n];

        if (!option.selected) {
            index = n;
            break;
        }
    }

    for (var n = index - 1; n >= 0; n--) {
        var option = selected.options[n];

        if (option.selected) {
            scForm.browser.swapNode(option, scForm.browser.getNextSibling(option));
        }
    }

    this.multilistUpdate(id);
}

scContentEditor.prototype.multilistMoveUp = function (id) {
    var selected = scForm.browser.getControl(id + "_selected");

    var index = selected.options.length;

    for (var n = 0; n < selected.options.length; n++) {
        var option = selected.options[n];

        if (!option.selected) {
            index = n;
            break;
        }
    }

    for (var n = index + 1; n < selected.options.length; n++) {
        var option = selected.options[n];

        if (option.selected) {
            scForm.browser.swapNode(option, scForm.browser.getPreviousSibling(option));
        }
    }

    this.multilistUpdate(id);
}

scContentEditor.prototype.multilistRemoveSelected = function (list, allOptions, id) {
    var index = -1;

    for (var n = list.options.length - 1; n >= 0; n--) {
        var option = list.options[n];

        if (option.selected || allOptions == true) {
            index = n;
            if (Prototype.Browser.IE) {
                list.options.remove(n);
            }
            else {
                list.remove(n);
            }
        }
    }

    if (index >= list.options.length) {
        index = list.options.length - 1;
    }

    var help = scForm.browser.getControl(id);

    if (index >= 0 && index < list.options.length) {
        list.selectedIndex = index;
        help.innerHTML = list.options[index].innerHTML;
    }
    else {
        help.innerHTML = "";
    }

    list.focus();
}

scContentEditor.prototype.multilistUpdate = function (id) {
    var selected = scForm.browser.getControl(id + "_selected");

    // IE listbox hack
    selected.style.position = selected.style.position == 'relative' ? 'static' : 'relative';

    var value = "";

    for (var n = 0; n < selected.options.length; n++) {
        var option = selected.options[n];
        value += (value != "" ? "|" : "") + option.value;
    }

    var selectedValues = scForm.browser.getControl(id + "_Value");

    selectedValues.value = value;
}

/* Switcher */

scContentEditor.prototype.switchTo = function (id, url) {
    location.href = url;

    if (event != null) {
        scForm.browser.clearEvent(event, true, false);
    }

    return false;
}

scContentEditor.prototype.scrollPanel = function (id, direction) {
    var ctl = scForm.browser.getControl(id);

    if (ctl != null && ((direction && ctl.scrollTop > 0) || (!direction && ctl.scrollTop + ctl.clientHeight < ctl.scrollHeight))) {
        var data = new Object();

        data.id = id;
        data.property = "scrollTop";
        data.delta = direction ? -4 : 4;
        data.steps = 11;
        data.speed = 25;

        setTimeout(function () { scContent.animate(data) }, 25);
    }
}

scContentEditor.prototype.animate = function (data) {
    var ctl = scForm.browser.getControl(data.id);

    if (ctl != null) {
        eval("ctl." + data.property + " += " + data.delta);
        data.steps--;

        if (data.steps > 0) {
            setTimeout(function () { scContent.animate(data) }, data.speed);
        }
    }
}

scContentEditor.prototype.smartTag = function (sender, evt, controlID, text, menuCount) {
    if (text != null) {
        if (this.currentSmartTag == controlID) {
            return;
        }

        this.hideSmartTag();

        var smarttag = scForm.browser.getControl("SmartTag_" + controlID);

        if (smarttag != null) {
            var ctl = document.createElement("div");
            ctl.className = "scSmartTag";
            ctl.id = controlID + "_smarttag";
            ctl.title = "";

            var click = document.createElement("a");
            click.href = "#";
            click.onclick = function () { return scContent.smartTagClick(this, evt, controlID, true) };
            click.className = (menuCount < 2 ? "scSmartTagClickOnly" : "scSmartTagClick");
            click.innerHTML = "<img src=\"/sitecore/images/blank.gif\" class=\"scSmartTagIcon\" alt=\"\" />" + text;

            ctl.appendChild(click);

            if (menuCount > 1) {
                var menu = document.createElement("a");
                menu.href = "#";
                menu.id = controlID + "_menu";
                menu.onclick = function () { return scContent.smartTagClick(this, evt, controlID, false) };
                menu.className = "scSmartTagMenu";
                menu.innerHTML = "<img src=\"/sitecore/shell/themes/standard/Images/ribbondropdown.gif\" class=\"scSmartTagGlyph\" alt=\"\" />";

                ctl.appendChild(menu);
            }

            smarttag.insertBefore(ctl, smarttag.firstChild);

            this.currentSmartTag = controlID;
        }
    }
    else {
        this.hideSmartTag();
    }
}

scContentEditor.prototype.hideSmartTag = function () {
    if (this.currentSmartTag != null) {
        var ctl = scForm.browser.getControl(this.currentSmartTag + "_smarttag");
        if (ctl != null) {
            scForm.browser.removeChild(ctl);
            this.currentSmartTag = null;
        }
    }
}

scContentEditor.prototype.smartTagClick = function (sender, evt, controlID, click) {
    var evt = window.event;

    scForm.browser.clearEvent(window.event, true, false);

    if (click) {
        scForm.invoke("FieldMenu_Click(\"" + controlID + "\")");
    }
    else {
        scForm.invoke("FieldMenu_DropDown(\"" + controlID + "\")");
    }

    try {
        evt.scClickReason = "ignore";
    }
    catch (e) {
        // silent
    }

    return false;
}

scContentEditor.prototype.postMessage = function (message) {
    for (var n = 0; n < window.frames.length; n++) {
        try {
            scForm.postMessageToWindow(window.frames[n], message);
        } catch (ex) {
            console.log("Failed to access frame. This typically happens due to a Permission Denied exception in IE9 caused by an intricate issue with Popup and ShowModalDialog calls. " + ex.message);
        }
    }
}

scContentEditor.prototype.scrollTo = function (sender, evt) {
    var srcElement = scForm.browser.getSrcElement(evt);

    var ctl = scForm.browser.getControl(srcElement.id.substr(4));

    alignToTop = srcElement.className == "scEditorHeaderNavigatorSection";

    scForm.focus(ctl);

    if (!scForm.browser.isIE) {
        scForm.browser.scrollIntoView(ctl);
    }
    else if (alignToTop) {
        var ce = scForm.browser.getControl("ContentEditor");
        ce.scrollTop = ctl.offsetTop;
    }

    scForm.browser.closePopups('ScrollTo');

    scForm.browser.clearEvent(evt, true, false);

    return false;
}

/* Validation */

scContentEditor.prototype.startValidators = function (sender, evt) {
    if (this.validatorUpdateDelay == null) {
        var ctl = scForm.browser.getControl("scValidatorsUpdateDelay");

        if (ctl == null) {
            return;
        }

        this.validatorUpdateDelay = parseInt(ctl.value, 10);
    }

    if (this.validatorUpdateDelay == 0) {
        return;
    }

    this.clearValidatorTimeouts();

    if (!this.hasValidators()) {
        return;
    }

    var validator = scForm.browser.getControl("ValidatorPanel");
    if (validator != null) {
        validator.innerHTML = "<div class=\"scValidationResultValidating\"></div>";

        this.validationTimer = window.setTimeout(function () { scContent.validate() }, this.validatorUpdateDelay);
    }
}

scContentEditor.prototype.renderValidators = function (result, frequency) {
    var validator = scForm.browser.getControl("ValidatorPanel");
    validator.innerHTML = result;

    this.updateFieldMarkers()

    if (result.indexOf("Applications/16x16/bullet_square_grey.png") >= 0) {
        this.validationTimer2 = window.setTimeout("scContent.updateValidators()", frequency);
    }
}

scContentEditor.prototype.validate = function () {
    this.clearValidatorTimeouts();

    if (!this.hasValidators()) {
        return;
    }

    var validator = scForm.browser.getControl("ValidatorPanel");
    validator.innerHTML = "<div class=\"scValidationResultValidating\"></div>";

    scForm.postRequest("", "", "", "ValidateItem", null, true);
}

scContentEditor.prototype.hasValidators = function () {
    var hasValidators = scForm.browser.getControl("scHasValidators");
    if (hasValidators == null) {
        return false;
    }
    return hasValidators.value == "1";
}

scContentEditor.prototype.clearValidatorTimeouts = function () {
    if (this.validationTimer != null) {
        window.clearTimeout(this.validationTimer);
    }
    this.validationTimer = null;

    if (this.validationTimer2 != null) {
        window.clearTimeout(this.validationTimer2);
    }
    this.validationTimer2 = null;
}

scContentEditor.prototype.updateValidators = function () {
    scForm.postRequest("", "", "", "UpdateValidators", null, true);
}

scContentEditor.prototype.updateFieldMarkers = function () {
    var validatorPanel = $("ValidatorPanel");

    for (var e = scForm.browser.getEnumerator(document.getElementsByTagName("TD")); !e.atEnd(); e.moveNext()) {
        var ctl = e.item();
        if (ctl.id.substr(0, 11) == "FieldMarker") {
            ctl.className = "scEditorFieldMarkerBarCell";
        }
    }

    for (var e = scForm.browser.getEnumerator(validatorPanel.getElementsByTagName("DIV")); !e.atEnd(); e.moveNext()) {
        var ctl = e.item();

        if (ctl.id.substr(0, 16) == "ValidationMarker") {
            var id = ctl.id.substr(16);

            var marker = $("FieldMarker" + id);

            if (marker != null) {
                marker.className = ctl.firstChild.innerHTML;
                $(marker).title = ctl.childNodes[1].alt ? ctl.childNodes[1].alt : ctl.childNodes[1].title;
            }
        }
    }
}

scForm.activateEx = scForm.activate;

scForm.activate = function (sender, evt) {
    if (evt.type == "deactivate" || (Prototype.Browser.Gecko && evt.type == "blur")) {
        scContent.startValidators();
    }
    scForm.activateEx(sender, evt);
}

/* Editor tabs */

scContentEditor.prototype.getActiveEditorTab = function () {
    var ctl = scForm.browser.getControl("EditorTabs");

    for (var e = scForm.browser.getEnumerator(ctl.childNodes); !e.atEnd(); e.moveNext()) {
        var ctl = e.item();

        if (ctl.className == "scRibbonEditorTabActive") {
            return ctl;
        }
    }

    return null;
}

scContentEditor.prototype.replaceImageSrc = function (element, text, withText) {
    var src = scForm.browser.getImageSrc(element);
    src = src.replace(text, withText);
    scForm.browser.setImageSrc(element, src);
}

scContentEditor.prototype.onEditorTabClick = function (sender, evt, id) {
    var active = this.getActiveEditorTab();

    scForm.browser.clearEvent(evt, true, false);

    if (active != null) {
        active.className = "scRibbonEditorTabNormal";
        if (active.parentNode.childNodes[1] == active) {
            this.replaceImageSrc(active.firstChild, "tab0_h", "tab0");
            active.childNodes[1].className = "scEditorTabHeaderNormal";
        }
        else {
            var sibling = scForm.browser.getPreviousSibling(active);
            this.replaceImageSrc(sibling.lastChild, "tab2_h2", "tab2")
            active.firstChild.className = "scEditorTabHeaderNormal";
        }

        if (active.parentNode.lastChild == active) {
            this.replaceImageSrc(active.lastChild, "tab3_h", "tab3");
        }
        else {
            this.replaceImageSrc(active.lastChild, "tab2_h1", "tab2");
        }

        var frame = scForm.browser.getControl("F" + active.id.substr(1));

        frame.style.display = "none";
    }

    var active = scForm.browser.getControl("B" + id);
    if (active != null) {
        active.className = "scRibbonEditorTabActive";

        if (active.parentNode.childNodes[1] == active) {
            this.replaceImageSrc(active.firstChild, "tab0", "tab0_h");
            active.childNodes[1].className = "scEditorTabHeaderActive";
        } else {
            var sibling = scForm.browser.getPreviousSibling(active);
            this.replaceImageSrc(sibling.lastChild, "tab2", "tab2_h2")
            active.firstChild.className = "scEditorTabHeaderActive";
        }
        if (active.parentNode.lastChild == active) {
            this.replaceImageSrc(active.lastChild, "tab3", "tab3_h");
        } else {
            this.replaceImageSrc(active.lastChild, "tab2", "tab2_h1");
        }

        var frame = scForm.browser.getControl("F" + id);

        frame.style.display = "";

        if (Prototype.Browser.Gecko || Prototype.Browser.WebKit) {
            scForm.browser.initializeFixsizeElements(true);
        }

        var scActiveEditorTab = scForm.browser.getControl("scActiveEditorTab");
        scActiveEditorTab.value = id;

        if (this.isContextualTab) {
            this.setActiveStrip(this.lastNoneContextualStrip);
        }

        this.clearContextualTabs();

        try {
            if (frame.contentWindow != null && frame.contentWindow.scOnShowEditor != null) {
                frame.contentWindow.scOnShowEditor();
            }
        } catch (ex) {
            console.log("Failed to accees frame. This typically happens due to a Permission Denied exception in IE9 caused by an intricate issue with Popup and ShowModalDialog calls. " + ex.message);
        }

        $$("#EditorTabs > .scEditorTabControlsHolder").invoke("hide");

        var e = $("EditorTabControls_" + id);
        if (e != null) {
            Element.show("EditorTabControls_" + id);
        }

        if (this.onEditorTabActivated) {
            for (var k = 0; k < this.onEditorTabActivated.length; k++) {
                this.onEditorTabActivated[k]();
            }
        }
    }
}

scContentEditor.prototype.onEditorTabActivated = [];
scContentEditor.prototype.subscribeToEditorTabActivatedEvent = function (delegate) {
    if (!delegate) {
        return;
    }

    if (!scContentEditor.prototype.onEditorTabActivated) {
        scContentEditor.prototype.onEditorTabActivated = [];
    }

    scContentEditor.prototype.onEditorTabActivated[scContentEditor.prototype.onEditorTabActivated.length] = delegate;
}

scContentEditor.prototype.showEditorTab = function (command, header, icon, url, id, closeable, activate) {
    var editorTabs = scForm.browser.getControl("EditorTabs");
    var editors = scForm.browser.getControl("EditorFrames");

    scForm.browser.setImageSrc(editorTabs.lastChild.lastChild, "/sitecore/shell/themes/standard/Images/Ribbon/tab2.png");

    var div = document.createElement("div");
    /*
    var a = document.createElement("a");
    a.className = "scRibbonEditorTabNormal";
    a.id = "B" + id;
    a.href = "#";
    a.onclick = "return scContent.onEditorTabClick(this,event,'" + id + "')";
    */

    /* Item Bucket Insertion */
    // var html = "<a id=\"B" + id + "\" href=\"#\" class=\"scRibbonEditorTabNormal\" onclick=\"javascript:return scContent.onEditorTabClick(this,event,'" + id + "')\"><span class=\"scEditorTabHeader\"><img src=\"" + icon + "\" width=\"16\" height=\"16\" alt=\"\" border=\"0\" class=\"scEditorTabIcon\" />" + header;
    var html = "<a id=\"B" + id + "\" href=\"#\" class=\"scRibbonEditorTabNormal\" oncontextmenu=\"javascript:return scForm.postEvent(this,event,'ShowTabContextMenu');\" onclick=\"javascript:return scContent.onEditorTabClick(this,event,'" + id + "')\"><span class=\"scEditorTabHeader\"><img src=\"" + icon + "\" width=\"16\" height=\"16\" alt=\"\" border=\"0\" class=\"scEditorTabIcon\" />" + header;
    if (closeable) {
        if (scForm.browser.isIE) {
            var src = " style=\"FILTER: progid:DXImageTransform.Microsoft.AlphaImageLoader(src='/sitecore/shell/themes/standard/Images/Close.png', sizingMethod='scale')\" src=\"/sitecore/images/blank.gif\"";
        }
        else {
            var src = " src=\"/sitecore/shell/themes/standard/Images/Close.png\"";
        }
        html += "<img class=\"scEditorTabClose\" onmouseover=\"javascript:return scForm.rollOver(this, event)\" onclick=\"javascript:scContent.closeEditorTab('" + id + "');\" onmouseout=\"javascript:return scForm.rollOver(this, event)\" height=\"16\" alt=\"\" width=\"16\" border=\"0\"" + src + " />";
    }

    if (scForm.browser.isIE) {
        var src = " style=\"filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(src='/sitecore/shell/themes/standard/Images/Ribbon/tab3.png', sizingMethod='scale')\" src=\"/sitecore/images/blank.gif\"";
    }
    else {
        var src = " src='/sitecore/shell/themes/standard/Images/Ribbon/tab3.png'";
    }

    html += "</span><img width=\"21\" height=\"24\" align=\"top\" alt=\"\" border=\"0\"" + src + " /></a>";

    div.innerHTML = html;

    editorTabs.appendChild(div.firstChild);


    var iframe = document.createElement("iframe");
    iframe.id = "F" + id;
    iframe.src = url + (activate ? "&ar=1" : "");
    iframe.width = "100%";
    iframe.height = "100%";
    iframe.frameBorder = "no";
    iframe.marginWidth = "0";
    iframe.marginHeight = "0";
    iframe.style.display = "none";
    editors.appendChild(iframe);

    var scEditorTabs = scForm.browser.getControl("scEditorTabs");
    var tabs = scEditorTabs.value;
    if (tabs != "") {
        tabs += "|"
    }
    tabs += command + "^" + header + "^" + icon + "^" + url + "^0^" + id;
    scEditorTabs.value = tabs;

    this.onEditorTabClick(null, null, id);
}

/* Item Buckets Code */

scContentEditor.prototype.closeAllEditorTab = function () {

    var allBucketTabs = $$(".scEditorTabIcon");
    var allTabsTogether = "";
    var scEditorTabs = scForm.browser.getControl("scEditorTabs");
    var tabs = scEditorTabs.value.split("|");
    for (var n = 0; n < allBucketTabs.length; n++) {
        if (allBucketTabs[n].src.indexOf("/temp/IconCache/Applications/16x16/text_view.png") != -1) {

            var currentId = allBucketTabs[n].parentElement.parentElement;
            this.closeEditorTab(currentId.id.substr(1, currentId.id.length));
            allTabsTogether = allTabsTogether + currentId.id.substr(1, currentId.id.length) + "|";
           

        }
    }

    allTabsTogether = allTabsTogether.substring(0, allTabsTogether.lastIndexOf("|"));


    jQuery.ajax({
        type: "POST",
        url: "/sitecore%20modules/Shell/Sitecore/ItemBuckets/ItemBucket.asmx/SaveClosedTabs",
        data: "{'ids' : '" + allTabsTogether + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (a) {

        }
    });


}

scContentEditor.prototype.closeEditorTab = function (id) {
    var active = scForm.browser.getControl("B" + id);

    if (active == null) {
        return;
    }

    var id = active.id.substr(1);

    var scEditorTabs = scForm.browser.getControl("scEditorTabs");
    var tabs = scEditorTabs.value.split("|");

    scForm.browser.clearEvent(window.event, true, false);

    for (var n = 0; n < tabs.length; n++) {
        if (tabs[n].indexOf(id) >= 0) {
            if (tabs.length == 1) {
                scEditorTabs.value = "";
            }
            else {
                tabs.splice(n, 1);
                scEditorTabs.value = tabs.join("|");
            }

            var focusId = null;

            var button = scForm.browser.getControl("B" + id);

            var ctl = scForm.browser.getPreviousSibling(button);
            if (ctl == null) {
                ctl = scForm.browser.getNextSibling(button);
            }
            if (ctl != null) {
                focusId = ctl.id.substr(1);
            }

            if (button == button.parentNode.lastChild) {
                scForm.browser.setImageSrc(ctl.lastChild, "/sitecore/shell/themes/standard/Images/Ribbon/tab3.png");
                


            }

            button.parentNode.removeChild(button);

            var frame = scForm.browser.getControl("F" + id);
            frame.parentNode.removeChild(frame);

            if (focusId != null) {
                this.onEditorTabClick(null, null, focusId);
            }
            return;
        }
    }

    alert("This tab cannot be closed.");
}



scContentEditor.prototype.closeAllButActiveEditorTab = function () {
    var allBucketTabs = $$(".scEditorTabIcon");
    var scEditorTabs = scForm.browser.getControl("scEditorTabs");
    var tabs = scEditorTabs.value.split("|");
    for (var n = 0; n < allBucketTabs.length; n++) {
        if (allBucketTabs[n].src.indexOf("/temp/IconCache/Applications/16x16/text_view.png") != -1) {

            if (allBucketTabs[n].parentElement.parentElement.className.indexOf("Active") != -1) {
                var currentId = allBucketTabs[n].parentElement.parentElement;
                this.closeEditorTab(currentId.id.substr(1, currentId.id.length));

            }
        }
    }

}

scContentEditor.prototype.closeAllToRightEditorTab = function () {
    var allBucketTabs = $$(".scEditorTabIcon");
    var scEditorTabs = scForm.browser.getControl("scEditorTabs");
    var tabs = scEditorTabs.value.split("|");
    for (var n = 0; n < allBucketTabs.length; n++) {
        if (allBucketTabs[n].src.indexOf("/temp/IconCache/Applications/16x16/text_view.png") != -1) {

            if (allBucketTabs[n].parentElement.parentElement.className.indexOf("Active") != -1) {
                var currentId = allBucketTabs[n].parentElement.parentElement;
                this.closeEditorTab(currentId.id.substr(1, currentId.id.length));

            }
        }
    }

}


/* Ribbon Proxies */

function scUpdateRibbonProxy(source, target, activateRibbon) {
    var form = scForm.getParentForm();

    if (form.hasSuspendedRequests()) {
        setTimeout("scUpdateRibbonProxy('" + source + "','" + target + "'," + activateRibbon + ")", 5);
        return;
    }

    var sourceNavigator = scForm.browser.getControl(source + "_Navigator");
    var sourceToolbar = scForm.browser.getControl(source + "_ContextualToolbar").parentNode;

    var targetNavigator = form.browser.getControl(target + "_Navigator");
    var targetToolbar = form.browser.getControl(target + "_ContextualToolbar");

    var activeStripID = null;
    var activeStripIndex = -1;

    var activeStrip = form.browser.getControl("scActiveRibbonStrip");
    if (activeStrip != null) {
        activeStripID = activeStrip.value;
    }

    var n = 0;
    var toolbar = sourceToolbar.cloneNode(true);
    toolbar.removeChild(toolbar.lastChild);
    for (var e = scForm.browser.getEnumerator(toolbar.childNodes); !e.atEnd(); e.moveNext()) {
        var ctl = e.item();
        ctl.className = "scRibbonToolbarContextualStrip";

        if (activateRibbon) {
            activeStripID = ctl.id;
            activateRibbon = false;
        }

        ctl.style.display = (ctl.id == activeStripID ? "" : "none");

        if (ctl.id == activeStripID) {
            ctl.style.display = "";
            activeStripIndex = n;
        }
        else {
            ctl.style.display = "none";
        }

        n++;
    }

    window.parent.scContent.clearContextualTabs(true);

    var html = toolbar.innerHTML;

    var frameID = scForm.browser.getFrameElement(window).id;

    function convertEvent($0, $1) {
        var command = $1;

        if (command.substr(0, 11) == "javascript:") {
            command = command.substr(11);
        }

        if (command.substr(0, 7) == "return ") {
            command = command.substr(7);
        }

        if (command.substr(0, 22) == "scContent.showGallery(") {
            return $0;
        }

        command = command.replace(/'/g, "\"");

        var code = "scUpdateRibbonProxyValues(\"" + frameID + "\",\"" + target + "\");return scForm.browser.getControl(\"" + frameID + "\").contentWindow." + command;

        return "_sc_event='javascript:" + code + "'";
    }

    function replaceEvent(eventName) {
        var re = new RegExp(eventName + "=\"([^\"]*)\"", "g");
        html = html.replace(re, convertEvent);

        var re = new RegExp(eventName + "='([^']*)'", "g");
        html = html.replace(re, convertEvent);

        var re = new RegExp(eventName + "=(\S+)", "g");
        html = html.replace(re, convertEvent);

        html = html.replace(/_sc_event/g, eventName);
    }

    replaceEvent("onclick");
    replaceEvent("onchange");

    targetToolbar.innerHTML = html;

    var html = scForm.browser.getOuterHtml(sourceNavigator);
    html = html.replace(/scRibbonNavigatorButtonsNormal/gi, "scRibbonNavigatorButtonsContextualNormal");
    html = html.replace(/scRibbonNavigatorButtonsActive/gi, "scRibbonNavigatorButtonsContextualNormal");
    html = html.replace(/scRibbonNavigatorButtonsContextualActive/gi, "scRibbonNavigatorButtonsContextualNormal");

    var div = targetNavigator.ownerDocument.createElement("div");
    div.innerHTML = html;

    while (div.childNodes.length > 0) {
        targetNavigator.appendChild(div.childNodes[0]);
    }

    window.parent.scContent.setActiveStrip(activeStripID);
}

function scUpdateRibbonProxyValues(frame, source) {
    try {
        var form = scForm.browser.getControl(frame).contentWindow.scForm;
    } catch (ex) {
        console.log("Failed to accees frame. This typically happens due to a Permission Denied exception in IE9 caused by an intricate issue with Popup and ShowModalDialog calls. " + ex.message);
        return;
    }

    var parent = scForm.browser.getControl(source);

    function replaceInputValue(tagName) {
        for (var e = scForm.browser.getEnumerator(parent.getElementsByTagName(tagName)); !e.atEnd(); e.moveNext()) {
            var ctl = e.item();

            var c = form.browser.getControl(ctl.id);
            if (c != null) {
                c.value = ctl.value;
            }
        }
    }

    replaceInputValue("INPUT");
    replaceInputValue("TEXTAREA");
    replaceInputValue("SELECT");
}

function scContentEditorUpdated() {
    scForm.disableRequests = false;

    var body = $(document.body);
    var editor = $("ContentEditor");
    if (editor) {
        body.fire("sc:contenteditorupdated");

        // obsolete. should only fire on body instead(above).
        editor.fire("sc:contenteditorupdated");
    }

    if (typeof (scGeckoRelayout) != "undefined") {
        scForm.browser.initializeFixsizeElements(true);
        scGeckoRelayout();
    }
}

function MultipleTextSource(sources) {
    this.sources = sources;

    this.get_text = function () {
        var texts = [];

        for (var i = 0; i < this.sources.length; i++) {
            texts[texts.length] = this.sources[i].get_text();
        }

        return texts.join("<controlSeparator><br/></controlSeparator>");
    }

    this.set_text = function (text) {
        var texts = text.split("<controlSeparator><br/></controlSeparator>");

        for (var i = 0; i < this.sources.length; i++) {
            this.sources[i].set_text(texts[i]);
        }
    }
}

function StartSpellCheck(elements, language) {
    var radSpell = $find("RadSpell");

    var sources = new Array();

    var parts = elements.split('|');
    for (var n = 0; n < parts.length; n++) {
        var element = scForm.browser.getControl(parts[n]);

        if (element == null) {
            continue;
        }

        // accessing the frame might cause a permission denied exception in IE9
        var elementTestSource = new Telerik.Web.UI.Spell.HtmlElementTextSource(element);
        if (element.contentWindow && element.contentWindow.scSetText) {
            elementTestSource.frameObject = element;
            elementTestSource.set_text = function (value) {
                this.frameObject.contentWindow.scSetText(value);
            }

            if (element.contentWindow.scGetText) {
                elementTestSource.get_text = function () {
                    return this.frameObject.contentWindow.scGetText();
                }
            }
        }

        sources.push(elementTestSource);
    }

    radSpell.set_textSource(new MultipleTextSource(sources));

    radSpell.set_dictionaryLanguage = language;

    radSpell.startSpellCheck();

    return false;
}

scContentEditor.prototype.fieldResizeDown = function (tag, evt) {
    if (!this.dragging) {
        this.dragging = true;

        scForm.browser.setCapture(tag);

        this.resizeFieldY = evt.screenY;
        this.resizeFieldHeight = $(tag).previous().getHeight();

        scForm.browser.clearEvent(evt, true, false);
    }
}

scContentEditor.prototype.fieldResizeMove = function (tag, evt) {
    if (this.dragging) {
        var dy = evt.screenY - this.resizeFieldY;
        var h = this.resizeFieldHeight + dy;

        if (h > 24 && h < 800) {
            $(tag).previous().style.height = h + "px";
            $(tag).previous().down().style.height = h + "px";
        }

        scForm.browser.clearEvent(evt, true, false);
    }
}

scContentEditor.prototype.fieldResizeUp = function (tag, evt, id) {
    if (this.dragging) {
        this.dragging = false;

        var dy = evt.screenY - this.resizeFieldY;
        var h = this.resizeFieldHeight + dy;

        if (h < 24) {
            h = 24;
        }
        else if (h > 800) {
            h = 800;
        }

        scForm.browser.clearEvent(evt, true, false);

        scForm.browser.releaseCapture(tag);

        scForm.postRequest("", "", "", "SaveFieldSize(\"" + id + "\", \"" + h + "\")", null, true);
    }
}

scContentEditor.prototype.editRichText = function (url, key, html) {
    var w = $('overlayWindow');
    this.state = scForm.state.pipeline;
    // Rich text editor is launched from content editor.
    if (w) {
        w.show();
        w.contentWindow.scLoad(url, key, html);
    }
    else {
        w = $('feRTEContainer');
        // Rich text editor is launched from field editor.
        if (w) {
            //w.show();
            if (this.loadHandler) { this.loadHandler.stop(); }
            this.loadHandler = w.on("load", function () {
                w.show();
                if (w.contentWindow.scLoad) {
                    w.contentWindow.scLoad(key, html);
                }
            });

            w.src = url;
            return;
        }

        console.error("Cannot find RTE frame object");
    }
}

scContentEditor.prototype.saveRichText = function (html) {
    scForm.postResult(html, this.state);
}

scContentEditor.prototype.onToggleSection = [];
scContentEditor.prototype.subscribeToToggleSectionEvent = function (delegate) {
    if (!delegate) {
        return;
    }

    if (!scContentEditor.prototype.onToggleSection) {
        scContentEditor.prototype.onToggleSection = [];
    }

    scContentEditor.prototype.onToggleSection[scContentEditor.prototype.onToggleSection.length] = delegate;
}

// scDraggable extends Draggable class from Scriptaculous to fix a couple of methods according to our needs.
// Changes compared to original Scriptaculous 1.9 code are marked in the code by comments.
var scDraggable;

Event.observe(document, "dom:loaded", function () {
    if (Prototype.Browser.IE) {
        return;
    }

    if (typeof (Draggable) == "undefined") {
        if (console) {
            console.info("Scriptaculous Draggable not loaded, skipping content tree drag&drop initialization");
            return;
        }
    }

    scDraggable = Class.create(Draggable, {
        startDrag: function ($super, event) {
            this.dragging = true;
            if (!this.delta)
                this.delta = this.currentDelta();

            if (this.options.zindex) {
                this.originalZ = parseInt(Element.getStyle(this.element, 'z-index') || 0);
                this.element.style.zIndex = this.options.zindex;
            }

            if (this.options.ghosting) {
                this._clone = this.element.cloneNode(true);
                this._originallyAbsolute = (this.element.getStyle('position') == 'absolute');
                if (!this._originallyAbsolute)
                    Position.absolutize(this.element);

                //Added this line to original method taken from Scriptaculous 1.9
                this.delta = this.currentDelta();

                this.element.parentNode.insertBefore(this._clone, this.element);
            }

            if (this.options.scroll) {
                if (this.options.scroll == window) {
                    var where = this._getWindowScroll(this.options.scroll);
                    this.originalScrollLeft = where.left;
                    this.originalScrollTop = where.top;
                } else {
                    this.originalScrollLeft = this.options.scroll.scrollLeft;
                    this.originalScrollTop = this.options.scroll.scrollTop;
                }
            }

            Draggables.notify('onStart', this, event);

            if (this.options.starteffect) this.options.starteffect(this.element);
        },

        finishDrag: function ($super, event, success) {
            this.dragging = false;

            if (this.options.quiet) {
                Position.prepare();
                var pointer = [Event.pointerX(event), Event.pointerY(event)];
                Droppables.show(pointer, this.element);
            }

            if (this.options.ghosting) {
                if (!this._originallyAbsolute)
                //CHANGE - Commented this line in original scriptaculous 1.9
                //Position.relativize(this.element);

                    delete this._originallyAbsolute;
                Element.remove(this._clone);
                this._clone = null;
            }

            var dropped = false;
            if (success) {
                dropped = Droppables.fire(event, this.element);
                if (!dropped) dropped = false;
            }
            if (dropped && this.options.onDropped) this.options.onDropped(this.element);
            Draggables.notify('onEnd', this, event);

            var revert = this.options.revert;
            if (revert && Object.isFunction(revert)) revert = revert(this.element);

            var d = this.currentDelta();
            if (revert && this.options.reverteffect) {
                if (dropped == 0 || revert != 'failure')
                    this.options.reverteffect(this.element, d[1] - this.delta[1], d[0] - this.delta[0]);
            } else {
                this.delta = d;
            }

            if (this.options.zindex)
                this.element.style.zIndex = this.originalZ;

            if (this.options.endeffect)
                this.options.endeffect(this.element);

            Draggables.deactivate(this);
            Droppables.reset();
        }
    });
});

var scContentEditorDragDrop = Class.create({
    initialize: function () {
        if (Prototype.Browser.IE) {
            return;
        }

        this.activeDraggable = null;

        Event.observe(window, "dom:loaded", function () {
            if (!scDraggable) {
                console.warn("scDraggable not defined. Skipping drag&drop initialization.");
                return;
            }

            var root = $("ContentTreeInnerPanel");

            if (!root) {
                if (console) {
                    console.info("#ContentTreeInnerPanel not found, drag&drop not initialized");
                }
            }

            this.initializeTreeNodes(root);

            $("ContentTreeActualSize").observe("mouseleave", function () {
                this.showMarker(null, "hide");
            } .bind(this));

            document.observe("sc:contenttreeupdated", function (e) {
                var node = e.memo;

                this.initializeTreeNodes(node);
            } .bindAsEventListener(this));
        } .bind(this));
    },

    initializeTreeNodes: function (root) {
        var elements = root.select(".scContentTreeNodeNormal, .scContentTreeNodeActive");
        var ids = elements.pluck("id");

        var hoverHandler = this.onNodeHover.bind(this);
        var dropHandler = this.onNodeDrop.bind(this);

        ids.each(function (id) {
            new scDraggable(id, {
                ghosting: true,
                revert: true,
                starteffect: function (element) {
                    element._opacity = Element.getOpacity(element);
                    Draggable._dragging[element] = true;
                    new Effect.Opacity(element, { duration: 0.2, from: element._opacity, to: 0.2 });
                },
                reverteffect: function (element, top_offset, left_offset) {
                    var dur = Math.sqrt(Math.abs(top_offset ^ 2) + Math.abs(left_offset ^ 2)) * 0.02;
                    new Effect.Move(element, { x: -left_offset, y: -top_offset, duration: dur,
                        queue: { scope: '_draggable', position: 'end' },
                        afterFinish: function () { element.relativize(); element.setStyle({ position: 'relative' }); }
                    });
                },
                onEnd: function (draggable, event) {
                    console.info("setting 'scBeenDragged'");
                    draggable.element.addClassName("scBeenDragged");
                    root.select(".scDragHover").invoke("removeClassName", "scDragHover");
                }
            });

            Droppables.add(id, {
                overlap: 'vertical',
                onHover: hoverHandler,
                onDrop: dropHandler
            });

        });
    },

    getMarker: function () {
        var result = $("ContentTreeDragMarker");

        if (!result) {
            result = new Element("div", { id: "ContentTreeDragMarker", "class": "scContentDragMarker" });
            $("ContentTreeInnerPanel").insert(result);
        }

        return result;
    },

    onNodeDrop: function (dragged, target, event) {
        var action = this.lastAction;
        if (!action) {
            return false;
        }

        var sourceid = dragged.id.gsub("Tree_Node_", "");
        var targetid = target.id.gsub("Tree_Node_", "");
        var message = sourceid + "|" + action + "|" + targetid;

        this.showMarker(target, "hide");

        // without timeout the sorting marker gets stuck sometimes
        setTimeout(function () {
            scForm.postEvent(target, event, "DropAndSort(\"" + message + "\")");
        }, 1);

        return true;
    },

    onNodeHover: function (draggable, droppable, overlap) {
        if (this.activeDroppable) {
            this.activeDroppable.setStyle({ backgroundColor: 'white' });
        }

        this.activeDroppable = droppable;

        if (overlap == 1) {
            droppable.removeClassName("scDragHover");
            this.showMarker(droppable, "hide");
            return;
        }

        if (overlap > 0.75) {
            droppable.removeClassName("scDragHover");
            this.showMarker(droppable, "before");
            this.lastAction = "before";
            return;
        }

        if (overlap > 0.25) {
            droppable.addClassName("scDragHover");
            this.showMarker(droppable, "hide");
            this.lastAction = "into";

            return;
        }

        droppable.removeClassName("scDragHover");
        this.showMarker(droppable, "after");
        this.lastAction = "after";
    },

    showMarker: function (target, position) {
        var marker = this.getMarker();

        if (position == "hide") {
            marker.hide();
            this.lastAction = null;
            return;
        }
        else {
            marker.show();
        }

        var offsetParent = target.getOffsetParent();
        var eOffset = target.viewportOffset(),
    pOffset = offsetParent.viewportOffset();

        var offset = eOffset.relativeTo(pOffset);
        var layout = target.getLayout();

        if (position == "before") {
            marker.setStyle({ "top": offset.top + "px", "left": offset.left + "px", width: layout.get("width") + 6 + "px" });
        }
        else if (position == "after") {
            marker.setStyle({ "top": offset.top + layout.get("height") + 4 + "px", "left": offset.left + "px", width: layout.get("width") + 4 + "px" });
        }
    }
});

var scContent = new scContentEditor();
scContent.dragDropManager = new scContentEditorDragDrop();
