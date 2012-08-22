function scSetMastersType() {
}

scSetMastersType.prototype.moveLeft = function() {
  var all = scForm.browser.getControl("All");
  var selected = scForm.browser.getControl("Selected");

  for(var n = 0; n < selected.options.length; n++) {
    var option = selected.options[n];
    
    if (option.selected) {
      var index = null;
      
      var header = option.innerHTML;
      
      for(var i = 0; i < all.options.length - 1; i++) {
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

  this.removeSelected(selected);
  
  this.update();
}

scSetMastersType.prototype.moveRight = function() {
  var all = scForm.browser.getControl("All");
  var selected = scForm.browser.getControl("Selected");

  for(var n = 0; n < all.options.length; n++) {
    var option = all.options[n];
    
    if (option.selected) {
      var opt = document.createElement("OPTION");
      
      selected.appendChild(opt);

      opt.innerHTML = option.innerHTML;
      opt.value = option.value;
    }
  }

  this.removeSelected(all);

  this.update();
}

scSetMastersType.prototype.moveDown = function() {
  var selected = scForm.browser.getControl("Selected");

  var index = 0;

  for(var n = selected.options.length - 1; n >= 0; n--) {
    var option = selected.options[n];
    
    if (!option.selected) {
      index = n;
      break;
    }
  }
  
  for(var n = index - 1; n >= 0; n--) {
    var option = selected.options[n];
    
    if (option.selected) {
      scForm.browser.swapNode(option, scForm.browser.getNextSibling(option));
    }
  }
  
  this.update();
}

scSetMastersType.prototype.moveUp = function() {
  var selected = scForm.browser.getControl("Selected");

  var index = selected.options.length;

  for(var n = 0; n < selected.options.length; n++) {
    var option = selected.options[n];
    
    if (!option.selected) {
      index = n;
      break;
    }
  }
  
  for(var n = index + 1; n < selected.options.length; n++) {
    var option = selected.options[n];
    
    if (option.selected) {
      scForm.browser.swapNode(option, scForm.browser.getPreviousSibling(option));
    }
  }

  this.update();
}

scSetMastersType.prototype.removeSelected = function(list) {
  for(var n = list.options.length - 1; n >= 0; n--) {
    var option = list.options[n];
    
    if (option.selected) {
      list.removeChild(list.childNodes[n]);
    }
  }
}

scSetMastersType.prototype.update = function() {
  var selected = scForm.browser.getControl("Selected");

  var value = "";

  for(var n = 0; n < selected.options.length; n++) {
    var option = selected.options[n];
    value += (value != "" ? "|" : "") + option.value;
  }
  
  var selectedValues = scForm.browser.getControl("SelectedValues");
  
  selectedValues.value = value;
}

var scSetMasters = new scSetMastersType();
