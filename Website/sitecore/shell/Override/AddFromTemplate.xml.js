var Watcher = Class.create({
  initialize: function(control) {
    this.control = $(control);
    this.control.observe("keydown", this.onKeyDown.bindAsEventListener(this));
    this.control.select();
  },
  
  onKeyDown: function() {
    this.control.removeClassName("not-dirty");
    
    if (window.event.keyCode == 13) {
      $("OK").click();
    }
  },
  
  setText: function(text) {
    if (!this.control.hasClassName("not-dirty")) {
      return;
    }
    
    this.control.value = text;
    this.control.select();
  }
})

var watcher;
Event.observe(window, "load", function() { watcher = new Watcher($('ItemName')) });