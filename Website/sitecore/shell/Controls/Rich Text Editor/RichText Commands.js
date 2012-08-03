/* This file is shared between older developer center rich text editor and the new EditorPage, that is used exclusively by Content Editor */

RadEditorCommandList["Save"] = function(commandName, editor, tool) {
  var form = scGetForm();

  if (form != null) {
    form.postEvent("", "", "item:save");
  }
};

var scEditor = null;
var scTool = null;

RadEditorCommandList["InsertSitecoreLink"] = function(commandName, editor, args) {
  var d = Telerik.Web.UI.Editor.CommandList._getLinkArgument(editor);
  Telerik.Web.UI.Editor.CommandList._getDialogArguments(d, "A", editor, "DocumentManager");

  var html = editor.getSelectionHtml();

  var id;

  // internal link in form of <a href="~/link.aspx?_id=110D559FDEA542EA9C1C8A5DF7E70EF9">...</a>
  if (html) {
    id = GetMediaID(html);
  }

  // link to media in form of <a href="~/media/CC2393E7CA004EADB4A155BE4761086B.ashx">...</a>
  if (!id) {
    var regex = /~\/media\/([\w\d]+)\.ashx/;
    var match = regex.exec(html);
    if (match && match.length >= 1 && match[1]) {
      id = match[1];
    }
  }

  if (!id) {
    id = scItemID;
  }

  scEditor = editor;

  editor.showExternalDialog(
    "/sitecore/shell/default.aspx?xmlcontrol=RichText.InsertLink&la=" + scLanguage + "&fo=" + id,
    null, //argument
    500,
    400,
    scInsertSitecoreLink, //callback
    null, // callback args
    "Insert Link",
    true, //modal
    Telerik.Web.UI.WindowBehaviors.Close, // behaviors
    false, //showStatusBar
    false //showTitleBar
  );
};

RadEditorCommandList["InsertSitecoreBucketLink"] = function (commandName, editor, args) {
    var d = Telerik.Web.UI.Editor.CommandList._getLinkArgument(editor);
    Telerik.Web.UI.Editor.CommandList._getDialogArguments(d, "A", editor, "DocumentManager");

    var html = editor.getSelectionHtml();

    var id;

    // internal link in form of <a href="~/link.aspx?_id=110D559FDEA542EA9C1C8A5DF7E70EF9">...</a>
    if (html) {
        id = GetMediaID(html);
    }

    // link to media in form of <a href="~/media/CC2393E7CA004EADB4A155BE4761086B.ashx">...</a>
    if (!id) {
        var regex = /~\/media\/([\w\d]+)\.ashx/;
        var match = regex.exec(html);
        if (match && match.length >= 1 && match[1]) {
            id = match[1];
        }
    }

    if (!id) {
        id = scItemID;
    }

    scEditor = editor;
    var SearchFilter = window.location.search;
    SearchFilter = window.location.search.substring(window.location.search.indexOf("&@"))
    SearchFilter = SearchFilter.replace("@", "");
    editor.showExternalDialog(
    "/sitecore/shell/default.aspx?xmlcontrol=RichText.InsertBucketLink&la=" + scLanguage + "&fo=" + id + SearchFilter,
    null, //argument
    800,
    500,
    scInsertSitecoreLink, //callback
    null, // callback args
    "Insert Link",
    true, //modal
    Telerik.Web.UI.WindowBehaviors.Close, // behaviors
    false, //showStatusBar
    false //showTitleBar
  );
};




function scInsertSitecoreLink(sender, returnValue) {
  if (!returnValue) {
    return;
  }

  var d = scEditor.getSelection().getParentElement();

  if ($telerik.isFirefox && d.tagName == "A") {
    d.parentNode.removeChild(d);
  } else {
    scEditor.fire("Unlink");
  }

  var text = scEditor.getSelectionHtml();

  if (text == "" || text == null || ((text != null) && (text.length == 15) && (text.substring(2, 15).toLowerCase() == "<p>&nbsp;</p>"))) {
    text = returnValue.text;
  }
  else {
    // if selected string is a full paragraph, we want to insert the link inside the paragraph, and not the other way around.
    var regex = /^[\s]*<p>(.+)<\/p>[\s]*$/i;
    var match = regex.exec(text);
    if (match && match.length >= 2) {
      scEditor.pasteHtml("<p><a href=\"" + returnValue.url + "\">" + match[1] + "</a></p>", "DocumentManager");
      return;
    }
  }

  scEditor.pasteHtml("<a href=\"" + returnValue.url + "\">" + text + "</a>", "DocumentManager");
}

RadEditorCommandList["InsertSitecoreMedia"] = function(commandName, editor, args) {
  var html = editor.getSelectionHtml();
  var id;

  // inserted media in form of <img src="~/media/CC2393E7CA004EADB4A155BE4761086B.ashx" />
  if (!id) {
    id = GetMediaID(html);
  }

  scEditor = editor;

  editor.showExternalDialog(
    "/sitecore/shell/default.aspx?xmlcontrol=RichText.InsertImage&la=" + scLanguage + (id ? "&fo=" + id : ""),
    null, //argument
    680,
    500,
    scInsertSitecoreMedia,
    null,
    "Insert Media",
    true, //modal
    Telerik.Web.UI.WindowBehaviors.Close, // behaviors
    false, //showStatusBar
    false //showTitleBar
  );
};
GetMediaID = function(html)
{
  var id = null;
  var list = prefixes.split('|');
  if(!list)
  {
    id = GetIDByMediaPrefix('~\\/media\\/([\\w\\d]+)\\.ashx', html);
  }
  else
  {
    for(i = 0; i < list.length; i++)
    {
      if(list[i] != '')
      {
        id = GetIDByMediaPrefix(list[i] +'([\\w\\d]+)\\.ashx', html);
        if(id)
        {
          break;
        }
      }
    }
  }
  
  return id;
}

GetIDByMediaPrefix = function(pattern, html)
{
    var regex = new RegExp(pattern, 'm');
    var match = regex.exec(html);
    if (match && match.length >=1 && match[1]) {
      return match[1];
    }
    
    return null;
}

function scInsertSitecoreMedia(sender, returnValue) {
  if (returnValue) {
    scEditor.pasteHtml(returnValue.media);
  }
}

function PrototypeAwayFilter() {
  PrototypeAwayFilter.initializeBase(this);
  this.set_isDom(true);
  this.set_enabled(true);
  this.set_name("Sitecore PrototypeAwayFilter filter");
  this.set_description("Sitecore PrototypeAwayFilter filter removes prototype attributes from DOM");
}

PrototypeAwayFilter.prototype =
{
  getHtmlContent: function (content) {
    this.getHtml(content);
    return content;
  },

  getHtml: function (node) {
    var children = node.childNodes;

    for (var i = children.length - 1; i >= 0; i--) {
      var n = children[i];

      if (n.nodeType != 1) {
        continue;
      }

      if (n.removeAttribute) {
        n._extendedByPrototype = null;
        n.removeAttribute("_extendedByPrototype");
      }

      this.getHtml(n);
    }
  }
}

WebControlFilter = function() {
  WebControlFilter.initializeBase(this);
  this.set_isDom(true);
  this.set_enabled(true);
  this.set_name("Sitecore WebControl filter");
  this.set_description("Sitecore WebControl filter displays ASP.NET web controls");
}

WebControlFilter.prototype =
{
  getHtmlContent: function(content) {
    this.getHtml(content);
    return content;
  },

  getHtml: function(node) {
    var children = node.childNodes;

    for (var i = children.length - 1; i >= 0; i--) {
      //Do not use here Prototype. This will cause issues like 329238, when Flash object getting extended by 
      //prototype methods.
      var n = children[i];

      if (n.nodeType != 1) {
        continue;
      }

      if (n.tagName != "IMG" || n.className != "scWebControl") {
        this.getHtml(n);
        continue;
      }

      Element.replace(n, n.title);
    }
  },

  getDesignContent: function(content) {
    this.getDesign(content);
    return content;
  },

  getDesign: function(node) {
    var children = node.childNodes;

    for (var i = children.length - 1; i >= 0; i--) {
      var n = children[i];

      if (n.nodeType != 1) {
        continue;
      }

      var prefix = n.scopeName != null ? n.scopeName : n.prefix;

      if (prefix == null || prefix == "" || prefix == "HTML") {
        this.getDesign(n);
        continue;
      }

      var webcontrol = n.outerHTML;
      var j = webcontrol.indexOf("<?xml:namespace");
      if (j >= 0) {
        var k = webcontrol.substr(j).indexOf(">") + j + 1;
        webcontrol = webcontrol.substr(k);
      }

      var e = new Element("img", { 'width': 32, 'height': 32, 'class': 'scWebControl', 'title': webcontrol, 'style': 'background:#F8EED0;margin:4px;border:1px solid #F0CCA5', 'src': '/sitecore/shell/~/icon/Software/32x32/Elements1.png' });

      Element.replace(n, e);
    }
  }
}

WebControlFilter.registerClass('WebControlFilter', Telerik.Web.UI.Editor.Filter);
PrototypeAwayFilter.registerClass('PrototypeAwayFilter', Telerik.Web.UI.Editor.Filter);