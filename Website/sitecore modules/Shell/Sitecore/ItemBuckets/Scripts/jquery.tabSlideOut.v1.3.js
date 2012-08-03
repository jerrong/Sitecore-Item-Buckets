(function (a) {
    a.fn.tabSlideOut = function (k) {
        var e = a.extend({
            tabHandle: ".handle",
            speed: 300,
            action: "click",
            tabLocation: "left",
            topPos: "200px",
            leftPos: "20px",
            fixedPosition: false,
            positioning: "absolute",
            pathToTabImage: null,
            imageHeight: null,
            imageWidth: null,
            onLoadSlideOut: false
        }, k || {});
        e.tabHandle = a(e.tabHandle);
        var f = this;
        if (e.fixedPosition === true) {
            e.positioning = "fixed"
        } else {
            e.positioning = "absolute"
        }
        if (document.all && !window.opera && !window.XMLHttpRequest) {
            e.positioning = "absolute"
        }
        if (e.pathToTabImage != null) {
            e.tabHandle.css({
                background: "url(" + e.pathToTabImage + ") no-repeat",
                width: e.imageWidth,
                height: e.imageHeight
            })
        }
        e.tabHandle.css({
            display: "block",
            textIndent: "-99999px",
            outline: "none",
            position: "absolute"
        });
        f.css({
            "line-height": "1",
            position: e.positioning
        });
        var i = {
            containerWidth: parseInt(f.outerWidth(), 10) + "px",
            containerHeight: parseInt(f.outerHeight(), 10) + "px",
            tabWidth: parseInt(e.tabHandle.outerWidth(), 10) + "px",
            tabHeight: parseInt(e.tabHandle.outerHeight(), 10) + "px"
        };
        if (e.tabLocation === "top" || e.tabLocation === "bottom") {
            f.css({
                left: e.leftPos
            });
            e.tabHandle.css({
                right: 0
            })
        }
        if (e.tabLocation === "top") {
            f.css({
                top: "-" + i.containerHeight
            });
            e.tabHandle.css({
                bottom: "-" + i.tabHeight
            })
        }
        if (e.tabLocation === "bottom") {
            f.css({
                bottom: "-" + i.containerHeight,
                position: "fixed"
            });
            e.tabHandle.css({
                top: "-" + i.tabHeight
            })
        }
        if (e.tabLocation === "left" || e.tabLocation === "right") {
            f.css({
                height: i.containerHeight,
                top: e.topPos
            });
            e.tabHandle.css({
                top: 0
            })
        }
        if (e.tabLocation === "left") {
            f.css({
                left: "-" + i.containerWidth
            });
            e.tabHandle.css({
                right: "-" + i.tabWidth
            })
        }
        if (e.tabLocation === "right") {
            f.css({
                right: "-" + i.containerWidth
            });
            e.tabHandle.css({
                left: "-" + i.tabWidth
            });
            a("html").css("overflow-x", "hidden")
        }
        e.tabHandle.click(function (l) {
            l.preventDefault()
        });
        var d = function () {
            if (e.tabLocation === "top") {
                f.animate({
                    top: "-" + i.containerHeight
                }, e.speed).removeClass("open")
            } else {
                if (e.tabLocation === "left") {
                    f.animate({
                        left: "-" + i.containerWidth
                    }, e.speed).removeClass("open")
                } else {
                    if (e.tabLocation === "right") {
                        f.animate({
                            right: "-" + 180
                        }, e.speed).removeClass("open")
                    } else {
                        if (e.tabLocation === "bottom") {
                            f.animate({
                                bottom: "-" + i.containerHeight
                            }, e.speed).removeClass("open")
                        }
                    }
                }
            }
        };
        var g = function () {
            if (e.tabLocation == "top") {
                f.animate({
                    top: "-3px"
                }, e.speed).addClass("open")
            } else {
                if (e.tabLocation == "left") {
                    f.animate({
                        left: "-3px"
                    }, e.speed).addClass("open")
                } else {
                    if (e.tabLocation == "right") {
                        f.animate({
                            right: "-3px"
                        }, e.speed).addClass("open")
                    } else {
                        if (e.tabLocation == "bottom") {
                            f.animate({
                                bottom: "-3px"
                            }, e.speed).addClass("open")
                        }
                    }
                }
            }
        };
        var c = function () {
            f.click(function (l) {
                l.stopPropagation()
            });
            a(document).click(function () {
                d()
            })
        };
        var j = function () {
            e.tabHandle.click(function (l) {
                if (f.hasClass("open")) {
                    d()
                } else {
                    g()
                }
            });
            c()
        };
        var h = function () {
            f.hover(function () {
                g()
            }, function () {
                d()
            });
            e.tabHandle.click(function (l) {
                if (f.hasClass("open")) {
                    d()
                }
            });
            c()
        };
        var b = function () {
            d();
            setTimeout(g, 500)
        };
        if (e.action === "click") {
            j()
        }
        if (e.action === "hover") {
            h()
        }
        if (e.onLoadSlideOut) {
            b()
        }
    }
})(jQuery);