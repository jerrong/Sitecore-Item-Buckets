function scHoverSmiley(sender, evt, id, index) {
}

function scClickSmiley(sender, evt, id, index) {
  function setIcon(tag, modifier) {
    var s = scForm.browser.getImageSrc(tag);

    if (s.indexOf("_d.png") >= 0) {
      s = s.replace(/_d.png/gi, modifier + ".png");
    }
    else if (s.indexOf("_h.png") >= 0) {
      s = s.replace(/_h.png/gi, modifier + ".png");
    }
    else {
      s = s.replace(/.png/gi, modifier + ".png");
    }

    scForm.browser.setImageSrc(tag, s);
  }

  var profileId = id.substr(0, id.indexOf("_"));
  var panel = $("profile_panel_" + profileId);
  if (panel.disabled) {
    return;
  }

  setIcon($(id + "_0"), "_d");
  setIcon($(id + "_1"), "_d");
  setIcon($(id + "_2"), "_d");

  setIcon($(id + "_" + index), "");

  $(id).value = index - 1;
}

function scHoverStar(sender, evt, id) {
  var profileId = id.substr(0, id.indexOf("_"));
  var panel = $("profile_panel_" + profileId);
  if (panel.disabled) {
    return;
  }

  var element = Event.element(evt);
  var value = 0;

  if (evt.type == "mouseover") {
    if (element.tagName == "IMG") {
      value = parseInt(element.id.substr(element.id.length - 1), 10);
    }
  }
  else {
    value = parseInt($(id).value, 10);
  }

  scShowStars(id, value);
  
  return false;
}

function scClickStar(sender, evt, id, index) {
  var element = Event.element(evt);
  var value = 0;

  var profileId = id.substr(0, id.indexOf("_"));
  var panel = $("profile_panel_" + profileId);
  if (panel.disabled) {
    return;
  }

  if (element.tagName == "IMG") {
    value = parseInt(element.id.substr(element.id.length - 1), 10);
  }

  scShowStars(id, value);

  $(id).value = value;

  return false;
}

function scShowStars(id, value) {
  function setIcon(tag, isYellow) {
    var s = scForm.browser.getImageSrc(tag);

    if (isYellow) {
      s = s.replace(/_grey.png/gi, "_yellow.png");
    }
    else {
      s = s.replace(/_yellow.png/gi, "_grey.png");
    }

    scForm.browser.setImageSrc(tag, s);
  }

  for (var n = 1; n < 6; n++) {
    setIcon($(id + "_" + n), value >= n);
  }
}
