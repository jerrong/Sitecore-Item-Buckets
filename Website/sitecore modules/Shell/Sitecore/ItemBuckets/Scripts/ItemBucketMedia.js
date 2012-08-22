function getQueryVariable(variable, qs) {

    var vars = qs.split("&");
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split("=");
        if (pair[0] == variable) {
            return unescape(pair[1]);
        }
    }

}


function GetItemPathFromMediaLibrary(id) {

    jQuery.ajax({
        type: "POST",
        url: "/sitecore%20modules/Shell/Sitecore/ItemBuckets/ItemBucket.asmx/GetMediaPath",
        data: "{'id' : '" + id + "'}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (a) {
            return a.d;
        }
    });

}

function BindItemResult(a) {

    $('#Filename', parent.document.body).val(a);
    $('#ItemName', parent.document.body).val(a);
    
}
function AddFilter() {

    var cleanFilter = filterForSearch;
    var o = new Array;

    var locationsFilter = getQueryVariable("location", cleanFilter);
    if (locationsFilter != undefined) {
        if (locationsFilter.length > 0) {
            o.push({
                type: "location",
                value: locationsFilter
            })

        }
    }

    var textFilter = getQueryVariable("text", cleanFilter);
    if (textFilter != undefined) {
        if (textFilter.length > 0) {
            o.push({
                type: "text",
                value: textFilter
            })

        }
    }

    var templateFilter = getQueryVariable("template", cleanFilter);
    if (templateFilter != undefined) {
        if (templateFilter.length > 0) {
            o.push({
                type: "template",
                value: templateFilter
            })

        }
    }

    return o;
}



var ContinueSearch = true;
var QueryServer = "";
var Expanded = false;
var CurrentView = "";


//This will check if you have designate a URL for all quieries to be run from and switch to that. Default is empty. (Local)
retrieveScalabilitySettings();

function detectViewMode() {
    return $("#views").find(".active").attr("id");
}

function autoSuggestText(element, filterName, data, characterCount) {
    var a = $("#ui_element");
    if (a.find(".addition").val().indexOf(filterName) > -1) {
        if (element.val().length >= characterCount) {
            jQuery.ajax({
                type: "POST",
                url: "/sitecore%20modules/Shell/Sitecore/ItemBuckets/ItemBucket.asmx/GetNames",
                data: data,
                cache: false,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (a) {
                    var b = a.d;
                    b = b.toString().split(",");
                    var c = new Array;
                    $.each(b, function () {
                        c.push(this.toString());
                    });
                    $(".ui-corner-all").live("click", function () {
                        ConvertSearchQuery();
                    });
                    $(".addition").autocomplete({
                        source: c,
                        autoFocus: true
                        
                    });
//                    $(".addition").autocomplete("hide");
//                    $(".ui-autocomplete").css("top", "75px").css("left", "536px").css("width", "428px");
//                    $(".ui-menu-item").css("height", "45px");
//                    $(".ui-corner-all").css("height", "45px").css("text-indent", "19px").css("font-size", "24px");
                    $(".addition").autocomplete("show");
                    $("#token-input-demo-input-local").show();
                }
            });
        }
    }
}

function buildTipsMenu(a) {

    $("#navBeta").html("");
    $(".slide-out-div2").show();
    $(".slide-out-div2").html("");
    $(".slide-out-div2").append('<a class="handle2" href="#"></a>');
    $(".slide-out-div2").tabSlideOut
        ({
            tabHandle: ".handle2",
            pathToTabImage: "images/thin-arrow-left.png",
            imageHeight: "522px",
            imageWidth: "20px",
            tabLocation: "right",
            speed: 300,
            action: "click",
            topPos: "17px",
            leftPos: "17px",
            fixedPosition: false
        });
        if ($.browser.msie) {
            $(".handle2").css("left", "-82px");
        }
        else {
            $(".handle2").css("left", "-52px");
        }
    $('.handle, .handle2').css("background-position", "50% 50%");
    if (a.tips.length == 0) {
        var b = '<div class="side">' + '<div class="sb_filter">' + "Tips Not Enabled" + "</div>" + "<div><ul>";
        b = b + "</ul></div>" + "</div>";
        $("#navBeta").append(b);
    };

    $.each(a.tips, function () {
        $("#navBeta").append('<div class="side">' + ' <div class="tip">Random Tip</div><p>' + this.TipName + "</p>" + '<div style="display: block;width:142px;word-wrap:break-word;">' + "<p>" + this.TipText + "</p>" + "</div>" + "</div>")
    });
}

function retrieveScalabilitySettings() {

    jQuery.ajax({
        type: "POST",
        url: "/sitecore%20modules/Shell/Sitecore/ItemBuckets/ItemBucket.asmx/QueryServer",
        data: "",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (a) {
            QueryServer = a.d;
        }
    });
}

function autoSuggest(filterName, serviceName, data) {
    var a = $("#ui_element");
    if (a.find(".addition").val().indexOf(filterName + ":") > -1) {
        jQuery.ajax({
            type: "POST",
            url: "/sitecore%20modules/Shell/Sitecore/ItemBuckets/ItemBucket.asmx/" + serviceName,
            data: "{" + data + "}",
            cache: false,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (a) {
                var b = a.d;
                b = b.toString().split(",");
                var c = new Array;
                $.each(b, function () {
                    c.push(filterName + ":" + this);
                });
                $(".ui-corner-all").live("click", function () {
                    ConvertSearchQuery();
                });
                $(".addition").autocomplete({
                    source: c,
                    autoFocus: true
                });
            }
        });
        $(".addition").autocomplete("destroy");
    }
}

function autoSuggestDate(filterName) {
    var a = $("#ui_element");
    if (a.find(".addition").val().indexOf(filterName + ":") > -1) {
        a.find(".addition").datepicker({
            onClose: function (b) {
                var d = a.find(".addition").val().replace(filterName + ":", "").replace(b, "");
                a.find(".addition").val(d);
                var bCulture;
                if (b == filterName + ":") {
                    var now = new Date();
                    var year = now.getYear();
                    if (year < 2000) {
                        year = year + 1900;
                    }
                    b = ("0" + (now.getMonth() + 1)).slice(-2) + "/" + ("0" + now.getDate()).slice(-2) + "/" + year;
                    bCulture = ("0" + now.getDate()).slice(-2) + "/" + ("0" + (now.getMonth() + 1)).slice(-2) + "/" + year;
                }
                bCulture = Date.parse(b).toString('dd/MM/yyyy');
                a.find(".boxme").prepend('<li class="token-input-token-facebook" title="' + b + '"><span style="background: url(\'images/' + filterName + '.png\') no-repeat center center;padding: 0px 11px;"></span><span>' + upperFirstLetter(filterName) + ' Date:</span><p class="' + filterName + ' type">' + bCulture + '</p><p class="' + filterName + 'hidden" style="display:none">' + b + '</p><span class="token-input-delete-token-facebook remove">×</span></li>');
                $(".remove").live("click", function () {
                    $(this).parents("li:first").remove();
                    a.find(".addition").focus();
                });
                a.find(".addition").datepicker("destroy");
            }
        });
        a.find(".token-input-token-facebook").bind("dblclick", function () {
            var b = $(this).find(".type");
            console.log(b);
            var cc = b.html();
            var dd = b.attr("class").split(/\s+/)[0];

            if (a.find(".addition").val().indexOf(dd + ":" + cc) < 0) {
                a.find(".addition").val(a.find(".addition").val() + (dd + ":" + cc));
                $(this).parents("li:first").remove();
            }
        });
        a.find(".addition").datepicker("show");
        $(".addition").bind("datepickercreate", function () {
            $(this).show();
        });
    }
}

function upperFirstLetter(str) {
    var string = str;
    string = string.toLowerCase().replace(/\b[a-z]/g, function (letter) {
        return letter.toUpperCase();
    });
    return string;
}

function meme(a) {
    $(".navAlpha").html("");
    $(".slide-out-div").show();
    $(".slide-out-div").html("");
    $(".slide-out-div").prepend('<div id="ajaxBusyFacet"><p><img src="images/loading.gif"></p><p>Loading Facets...</p></div>');
    $("#ajaxBusyFacet").css
        ({

            margin: "0px auto",
            width: "44px"
        });

    $(".slide-out-div").append('<a class="handle" href="#"></a>');
    $(".slide-out-div").tabSlideOut
        ({
            tabHandle: ".handle",
            pathToTabImage: "images/thin-arrow-right.png",
            imageHeight: "522px",
            imageWidth: "20px",
            tabLocation: "left",
            speed: 300,
            action: "click",
            topPos: "17px",
            rightPos: "20px",
            fixedPosition: false
        });
    $('.handle, .handle2').css("background-position", "50% 50%");
    $.each
        (a.facets,
            function () {

                if (typeof (this[0]) == 'undefined') { }
                else {
                    var b = '<div class="side">' + '<div class="sb_filter">' + (typeof (this[0]) == 'undefined' ? "No Results Found" : this[0].Type + "") + "</div>" + "<div><ul>";
                    $.each
                    (this,
                        function () {
                            if (this.Type == "field") {
                                b = b + '<li class="filter"><a href="#" title="' + this.KeyName + '" class="facetClick" onclick="javascript:IsClone(\'' + this.KeyName + '|' + this.Template + "', '" + this.Type + "','" + this.ID + "');\">" + (this.KeyName.length > 16 ? (this.KeyName.substring(0, 16) + "...") : this.KeyName) + (this.Template != null ? " (" + (this.Template.length > 4 ? (this.Template.substring(0, 4) + "...") : this.Template) + ")" : "") + " (" + this.Value + ")" + "</a></li>"
                            }
                            else if (this.Type == "location") {
                                b = b + '<li class="filter"><a href="#" title="' + this.KeyName + '" class="facetClick" onclick="javascript:IsClone(\'' + this.KeyName + '|' + this.ID + "', '" + this.Type + "','" + this.ID + "');\">" + (this.KeyName.length > 16 ? (this.KeyName.substring(0, 16) + "...") : this.KeyName) + (this.Template != null ? " (" + (this.Template.length > 4 ? (this.Template.substring(0, 4) + "...") : this.Template) + ")" : "") + " (" + this.Value + ")" + "</a></li>"
                            }
                            else if (this.Type == "date range") {
                                b = b + '<li class="filter"><a href="#" title="' + this.KeyName + '" class="facetClick" onclick="javascript:IsClone(\'' + this.KeyName + '|' + this.ID + "', '" + this.Type + "','" + this.ID + "');\">" + (this.KeyName.length > 16 ? (this.KeyName.substring(0, 16) + "...") : this.KeyName) + (this.Template != null ? " (" + (this.Template.length > 4 ? (this.Template.substring(0, 4) + "...") : this.Template) + ")" : "") + " (" + this.Value + ")" + "</a></li>"
                            }
                            else if (this.Type == "template") {
                                b = b + '<li class="filter"><a href="#" title="' + this.KeyName + '" class="facetClick" onclick="javascript:IsClone(\'' + this.KeyName + '|' + this.ID + "', '" + this.Type + "','" + this.ID + "');\">" + (this.KeyName.length > 16 ? (this.KeyName.substring(0, 16) + "...") : this.KeyName) + (this.Template != null ? " (" + (this.Template.length > 4 ? (this.Template.substring(0, 4) + "...") : this.Template) + ")" : "") + " (" + this.Value + ")" + "</a></li>"
                            }
                            else if (this.Type == "author") {
                                var replaceKeyName = this.KeyName.replace("\\", "|");
                                b = b + '<li class="filter"><a href="#" title="' + this.KeyName + '" class="facetClick" onclick="javascript:IsClone(\'' + replaceKeyName + "', '" + this.Type + "','" + this.ID + "');\">" + (this.KeyName.length > 16 ? (this.KeyName.substring(0, 16) + "...") : this.KeyName) + (this.Template != null ? " (" + (this.Template.length > 4 ? (this.Template.substring(0, 4) + "...") : this.Template) + ")" : "") + " (" + this.Value + ")" + "</a></li>"
                            }
                            else {
                                b = b + '<li class="filter"><a href="#" title="' + this.KeyName + '" class="facetClick" onclick="javascript:IsClone(\'' + this.KeyName + "', '" + this.Type + "','" + this.ID + "');\">" + (this.KeyName.length > 16 ? (this.KeyName.substring(0, 16) + "...") : this.KeyName) + (this.Template != null ? " (" + (this.Template.length > 4 ? (this.Template.substring(0, 4) + "...") : this.Template) + ")" : "") + " (" + this.Value + ")" + "</a></li>"
                            }
                        }
                    );

                    b = b + "</ul></div>" + "</div>";
                    $(".navAlpha").append(b);
                }

                $(".facetClick").click(function () {
                    $(".facetClick").css("padding-bottom", "0px").css("border-bottom", "0px").css("background-color", "#FFF");
                    $(this).css("background-color", "#A8DBF3").css("height", "18px").css("-moz-border-radius", "7px 7px 7px 7px").css("-webkit-border-radius", "7px 7px 7px 7px").css("border-radius", "7px 7px 7px 7px").css("padding", "3px 1px");
                });

            }
        );

    $(".pagination").remove();
    var c = a.PageNumbers;
    var e = Page(a.CurrentPage, c);
    $(".pageSection").append(e);
    $(".side .sb_filter").bind
        ('click',
            function () {
                if ($(this).hasClass('on')) {
                    $(this).removeClass('on').addClass('off');
                    //$(this).css("background-image", "url('/ItemBuckets/images/down.png')"); //Add the .. back into the link
                }
                else if ($(this).hasClass('off')) {
                    $(this).removeClass('off');
                    //$(this).css("background-image", "url('/ItemBuckets/images/up.png')"); //Add the .. back into the link
                }
                else {
                    $(this).addClass('on');
                }

                $(this).next("div").slideToggle(100);
            }
        );

    $(this).removeClass("pageClickLoad");

    if ($(".navAlpha .side").length == 0) {
        var b = '<div class="side">' + '<div class="sb_filter">' + "No Facets" + "</div>" + "<div><ul>";
        b = b + "</ul></div>" + "</div>";
        $(".navAlpha").append(b);
    };

    $("#ajaxBusyFacet").css
        ({
            display: "none",
            margin: "0px auto",
            width: "24px"
        });

    $('.handle').toggle
        (
            function () {
                $('.handle').css("background-image", "url(images/thin-arrow-left.png)");
                $('.handle').css("background-position", "50% 50%");
                $('.handle').addClass("toggled");
                return false;
            },

            function () {
                $('.handle').css("background-image", "url(images/thin-arrow-right.png)");
                $('.handle').css("background-position", "50% 50%");
                $('.handle').removeClass("toggled");
                return false;
            }
        );

    $('.handle2').toggle
        (
            function () {

                $('.handle2').addClass("toggled");
                return false;
            },

            function () {

                $('.handle2').removeClass("toggled");
                return false;
            }
        );

   // $('.addition').removeAttr('disabled');
}

function autoSuggestWithWait(element, filterName, serviceName, data, characterCount) {
    var a = $("#ui_element");
    if (a.find(".addition").val().indexOf(filterName + ":") > -1) {
        if (element.value.replace(filterName + ":", "").length >= characterCount) {
            jQuery.ajax({
                type: "POST",
                url: "/sitecore%20modules/Shell/Sitecore/ItemBuckets/ItemBucket.asmx/" + serviceName,
                data: data,
                cache: false,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (a) {
                    var b = a.d;
                    b = b.toString().split(",");
                    var c = new Array;
                    $.each(b, function () {
                        c.push(filterName + ":" + this);
                    });
                    $(".ui-corner-all").live("click", function () {
                        ConvertSearchQuery();
                    });
                    $(".addition").autocomplete({
                        source: c,
                        autoFocus: true
                    });
                    $(".addition").autocomplete("show");
                    $("#token-input-demo-input-local").show();
                }
            });
        }
    }
}

function runFacet(o, pageNumber, onSuccessFunction, onErrorFunction) {

    $.ajax
        ({
            type: "GET",
            url: QueryServer + "/sitecore%20modules/Shell/Sitecore/ItemBuckets/Services/Search.ashx?callback=?",
            contentType: "application/json; charset=utf-8",
            dataType: "jsonp",
            cache: false,
            data:
            {
                selections: o.concat(AddFilter()),
                pageNumber: pageNumber,
                type: "facet"
            },
            responseType: "json",
            success: onSuccessFunction,
            error: onErrorFunction
        });
}

function runQuery(o, pageNumber, onSuccessFunction, onErrorFunction) {

    $.ajax({
        type: "GET",
        url: QueryServer + "/sitecore%20modules/Shell/Sitecore/ItemBuckets/Services/Search.ashx?callback=?",
        contentType: "application/json; charset=utf-8",
        dataType: "jsonp",
        cache: false,
        data: {
            selections: o.concat(AddFilter()),
            pageNumber: pageNumber,
            type: "Query"
        },
        responseType: "json",
        success: onSuccessFunction,
        error: onErrorFunction
    });
}

function retrieveFiltersWithDoubleClickToEdit() {

    var a = $("#ui_element");
    var filterArray = ["extension", "version", "debug", "id", "ref", "custom", "sort", "site", "author", "language", "text", "template", "tag", "location"];
    $.each(filterArray, function (index, filter) {
        if (a.find(".addition").val().indexOf(filter + ":") > -1) {
            var d = a.find(".addition").val().split(":")[1];
            var e = a.find(".addition").val().replace(filter + ":" + d, "");
            a.find(".addition").val(e);
            a.find(".boxme").prepend('<li class="token-input-token-facebook" title="' + d + '"><span style="background: url(\'images/' + filter + '.gif\') no-repeat center center;padding: 0px 11px;"></span><p class="' + filter + '">' + d + '</p><span class="token-input-delete-token-facebook remove">×</span></li>');
            $(".remove").live("click", function () {
                $(this).parents("li:first").remove();
                a.find(".addition").focus();
            });
            a.find(".token-input-token-facebook p").live("dblclick", function () {
                var b = $(this);
                console.log(b);
                var cc = b.html();
                var dd = b.attr("class").split(/\s+/)[0];

                if (a.find(".addition").val().indexOf(dd + ":" + cc) < 0) {
                    a.find(".addition").val(a.find(".addition").val() + (dd + ":" + cc));
                    $(this).parents("li:first").remove();
                }
            });
        }
    });
}

//This will fetch all the available filters and will handle solving them.
function retrieveFilters() {

    var a = $("#ui_element");
    var filterArray = ["extension", "version", "debug", "id", "ref", "custom", "sort", "site", "author", "language", "text", "template", "tag", "start", "end", "recent", "location"];
    $.each(filterArray, function (index, filter) {

        if (a.find(".addition").val().indexOf(filter + ":") > -1) {
            var p = a.find(".addition").val().split(":")[1];
            var q = a.find(".addition").val().replace(filter + ":" + p, "");
            a.find(".addition").val(q);
            if (filter == "start" || filter == "end") {
                a.find(".boxme").prepend('<li class="token-input-token-facebook" title="' + p + '"><span style="background: url(\'images/' + filter + '.png\') no-repeat center center;padding: 0px 11px;" class="booleanOperation"></span><p class="' + filter + 'hidden">' + p + '</p><span class="token-input-delete-token-facebook remove">×</span></li>');
            }
            else if (filter == "sort") {
                a.find(".boxme").prepend('<li class="token-input-token-facebook" title="' + p + '"><span style="background: url(\'images/' + filter + '.png\') no-repeat center center;padding: 0px 11px;" class="sortDirection"></span><p class="' + filter + '">' + p + '</p><span class="token-input-delete-token-facebook remove">×</span></li>');
            }
            else if (filter == "text") {
                a.find(".boxme").prepend('<li class="token-input-token-facebook" title="' + p + '"><span style="background: url(\'images/' + filter + '.png\') no-repeat center center;padding: 0px 11px;" class="booleanOperation"></span><p class="' + filter + '">' + p + '</p><span class="token-input-delete-token-facebook remove">×</span></li>');
            }

            else {
                a.find(".boxme").prepend('<li class="token-input-token-facebook" title="' + p + '"><span style="background: url(\'images/' + filter + '.png\') no-repeat center center;padding: 0px 11px;"></span><p class="' + filter + '">' + p + '</p><span class="token-input-delete-token-facebook remove">×</span></li>');
            }
            $(".remove").live("click", function () {
                $(this).parents("li:first").remove();
                a.find(".addition").focus();
            });
            $(".addition").autocomplete("destroy");
        }
    });
}

//This will fetch all the available filters and will handle solving them.
function retrieveFiltersGalleryView() {

    var a = $("#ui_element");
    var filterArray = ["extension", "version", "debug", "id", "ref", "custom", "sort", "site", "author", "language", "text", "template", "tag", "start", "end", "recent", "location"];
    $.each(filterArray, function (index, filter) {

        if (a.find(".addition").val().indexOf(filter + ":") > -1) {
            var p = a.find(".addition").val().split(":")[1];
            var q = a.find(".addition").val().replace(filter + ":" + p, "");
            a.find(".addition").val(q);
            if (filter == "start" || filter == "end") {
                a.find(".boxme").prepend('<li class="token-input-token-facebook" title="' + p + '"><span style="background: url(\'images/' + filter + '.png\') no-repeat center center;padding: 0px 11px;" class="booleanOperation"></span><p class="' + filter + 'hidden">' + p + '</p><span class="token-input-delete-token-facebook remove">×</span></li>');
            }
            else if (filter == "sort") {
                a.find(".boxme").prepend('<li class="token-input-token-facebook" title="' + p + '"><span style="background: url(\'images/' + filter + '.png\') no-repeat center center;padding: 0px 11px;" class="sortDirection"></span><p class="' + filter + '">' + p + '</p><span class="token-input-delete-token-facebook remove">×</span></li>');

            }
            else if (filter == "text") {
                a.find(".boxme").prepend('<li class="token-input-token-facebook" title="' + p + '"><span style="background: url(\'images/' + filter + '.png\') no-repeat center center;padding: 0px 11px;" class="booleanOperation"></span><p class="' + filter + '">' + p + '</p><span class="token-input-delete-token-facebook remove">×</span></li>');

            } else {
                a.find(".boxme").prepend('<li class="token-input-token-facebook" title="' + p + '"><span style="background: url(\'images/' + filter + '.png\') no-repeat center center;padding: 0px 11px;"></span><p class="' + filter + '">' + p + '</p><span class="token-input-delete-token-facebook remove">×</span></li>');
            }
            $(".remove").live("click", function () {
                $(this).parents("li:first").remove();
                a.find(".addition").focus();
            });
            $(".addition").autocomplete("destroy");
        }
    });
}

function buildQuery() {
    var a = $("#ui_element");
    var findArray = ["extension", "text", "id", "custom", "template", "tag", "ref", "starthidden", "endhidden", "author", "version", "debug", "language", "site"];
    var n = new Array;
    $.each(findArray, function (index, filter) {
        var b = a.find(".boxme li ." + filter);
        $.each(b, function () {
            n.push({
                type: filter.replace("hidden", ""),
                value: $(this).text()
            });
        });
    });

    var customSort = a.find(".boxme li .sort");
    var recent = a.find(".boxme li .recent");
    var location = a.find(".boxme li .location");
    var o = a.find(".addition").val();
    if (o != null && o != "" && o != "Search for an Item") {
        n.push({
            type: "text",
            value: o
        });
    }

    $.each(customSort, function () {
        n.push({
            type: "sort",
            value: $(this).text()
        });
        n.push({
            type: "orderby",
            value: $(this).prev().hasClass('desc') ? "desc" : "asc"
        });
    });

    $.each(recent, function () {
        n.push({
            type: "recent",
            value: $(this).text().split('|')[1]
        });
    });

    $.each(location, function () {
        n.push({
            type: "location",
            value: $(this).text().split('|')[1]
        });
    });

    return n;
}

$('.content').live("click",
    function () {
        $('.content').animate({ opacity: '1.0' }, 5000);
    }
 );

$('.sb_clear').live("click", function () {

    $('.sb_clear').toggle
    ({
        display: 'inline'
    });

        if ($.browser.msie) {
            document.getElementById("token-input-demo-input-local").value = "";
        }
        else {

            $(".addition").text("");
        }
        $(".addition").val("");
        $(".boxme").children(".token-input-token-facebook").remove();
        //$('.addition').removeAttr('disabled');
    });

function IsClone(a, b, c) {
    var d = $("#ui_element");
    $(".grid").removeClass("active");
    $(".list").addClass("active");
    var e = d.find(".boxme li .extension");
    var tt = d.find(".boxme li .id");
    var zz = d.find(".boxme li .custom");
    var f = d.find(".boxme li .text");
    var customSort = d.find(".boxme li .sort");
    var g = d.find(".boxme li .template");
    var h = d.find(".boxme li .tag");
    var ref = d.find(".boxme li .ref");
    var i = d.find(".boxme li .starthidden");
    var j = d.find(".boxme li .endhidden");
    var k = d.find(".boxme li .author");
    var l = d.find(".boxme li .version");
    var debugMode = d.find(".boxme li .debug");
    var m = d.find(".boxme li .site");
    var n = d.find(".boxme li .language");
    var o = new Array;
    var p = d.find(".addition").val();

    $("#loadingSection").prepend('<div id="ajaxBusy"><p><img src="images/loading.gif"></p></div>');
    $("#ajaxBusy").css
    ({
        padding: "0px 122px 0px 0px",
        margin: "0px auto",
        width: "24px"
    });

    if (b == "location") {
        o.push({
            type: b,
            value: c
        });
        if (c == "{11111111-1111-1111-1111-111111111111}") {
            ContinueSearch = false;
            $(".continue").fadeOut("slow");
        }
    }

    if (b == "field") {
        o.push
        ({
            type: "field",
            value: a.split('|')[0]
        });
    }

    if (b == "date range") {
        var now = new Date();
        var year = now.getYear();
        if (year < 2000) {
            year = year + 1900;
        }
        var month = ("0" + (now.getMonth() + 1)).slice(-2);
        var day = ("0" + now.getDate()).slice(-2);

        switch (a.split('|')[0]) {
            case "Within a Day":
                o.push
                    ({
                        type: "start",
                        value: Date.today().add(-1).days().toString("MM/dd/yyyy")
                    });

                break;

            case "Within a Week":
                o.push
                    ({
                        type: "start",
                        value: Date.today().add(-7).days().toString("MM/dd/yyyy")
                    });

                o.push
                    ({
                        type: "end",
                        value: Date.today().add(-1).days().toString("MM/dd/yyyy")
                    });

                break;

            case "Within a Month":
                o.push
                    ({
                        type: "start",
                        value: Date.today().add(-1).months().toString("MM/dd/yyyy")
                    });

                o.push
                    ({
                        type: "end",
                        value: Date.today().add(-7).days().toString("MM/dd/yyyy")
                    });

                break;

            case "Within a Year":
                o.push
                    ({
                        type: "start",
                        value: Date.today().add(-1).years().toString("MM/dd/yyyy")
                    });

                o.push
                    ({
                        type: "end",
                        value: Date.today().add(-1).months().toString("MM/dd/yyyy")
                    });

                break;

            case "Older":
                o.push
                    ({
                        type: "start",
                        value: Date.today().add(-3).years().toString("MM/dd/yyyy")
                    });

                o.push
                    ({
                        type: "end",
                        value: Date.today().add(-1).years().toString("MM/dd/yyyy")
                    });

                break;

            default:
                o.push
                    ({
                        type: "start",
                        value: month + "/" + (((day - 1) <= 0) ? 1 : (day - 1)) + "/" + year
                    });
        } //Switch
    } //If

    if (b == "template") {
        o.push
        ({
            type: b,
            value: a.split('|')[1]
        });
    }

    if (b == "author") {
        o.push
        ({
            type: b,
            value: a.replace('|', '\\')
        });
    }

    if (p != null && p != "" && p != "Search for an Item") {
        o.push
        ({
            type: "text",
            value: p
        });
    }

    $.each(customSort,
    function () {
        o.push
        ({
            type: "sort",
            value: $(this).text()
        });

        o.push
        ({
            type: "orderby",
            value: $(this).prev().hasClass('desc') ? "desc" : "asc"
        });
    });

    $.each(f,
    function () {
        o.push
        ({
            type: "text",
            value: $(this).text()
        });
    });

    $.each(e,
    function () {
        o.push
        ({
            type: "extension",
            value: $(this).text()
        });
    });

    $.each(tt,
    function () {
        o.push
        ({
            type: "id",
            value: $(this).text()
        });
    });

    $.each(ref,
    function () {
        o.push
        ({
            type: "ref",
            value: $(this).text()
        });
    });

    $.each(zz,
    function () {
        o.push
        ({
            type: "custom",
            value: $(this).text()
        });
    });

    $.each(g,
    function () {
        o.push
        ({
            type: "template",
            value: $(this).text()
        });
    });

    $.each(h,
        function () {
            o.push({
                type: "tag",
                value: this.id
            });
        });

    $.each(i,
    function () {
        o.push
        ({
            type: "start",
            value: $(this).text()
        });
    });

    $.each(n,
    function () {
        o.push
        ({
            type: "language",
            value: $(this).text()
        });
    });

    $.each(j,
    function () {
        o.push
        ({
            type: "end",
            value: $(this).text()
        });
    });

    $.each(k,
    function () {
        o.push
        ({
            type: "author",
            value: $(this).text()
        });
    });

    $.each(l,
    function () {
        o.push
        ({
            type: "version",
            value: $(this).text()
        });
    });

    $.each(debugMode,
    function () {
        o.push
        ({
            type: "debug",
            value: $(this).text()
        });
    });

    $.each(m,
    function () {
        o.push
        ({
            type: "site",
            value: $(this).text()
        });
    });

    retrieveFilters();
    runQuery(o, 0, OnComplete, OnFail);
    runFacet(o, 0, meme, g);

    $(".navAlpha").html("");
    $(".slide-out-div").html("");
    $(".slide-out-div").prepend('<div id="ajaxBusyFacet"><p><img src="images/loading.gif"></p><p>Loading Facets...</p></div>');
    $("#ajaxBusyFacet").css
    ({
        margin: "0px auto",
        width: "44px"
    });
}

function OnFail(a) {
    $("#results").html("");
    $("#results").append('<div style="padding-bottom: 20px;text-align: center;padding-right: 85px;padding-right: 156px;">Apologies, we could not resolve your search. Please try to refine your search, as your query is too broad.</div>');
    if (ContinueSearch) {
        //   $("#results").append('<div style="padding-bottom: 20px;text-align: center;padding-right: 156px;" class="continue"><a href="#" onclick="javascript:IsClone(\'' + 'location' + '|' + '{11111111-1111-1111-1111-111111111111}' + "', '" + "location" + "','" + '{11111111-1111-1111-1111-111111111111}\')\";>Continue Searching All Other Bucket Locations....</a></div>');
    }
   // $('.addition').removeAttr('disabled');
    ContinueSearch = true;
}

function OnComplete(a) {
    $("#ajaxBusy").css
    ({
        display: "none",
        margin: "0px auto",
        width: "24px"
    });

    if ($("#grid-content").hasClass('mainmargin')) {
        $("#results").html("");
        var b = "";
        $("#results").append('<div style="padding-bottom: 20px;text-align: center;padding-right: 156px;">Your search has returned <strong>' + a.SearchCount + "</strong> result/s in <strong>" + a.SearchTime + "</strong> seconds under the <strong class='bucketLocation'>" + a.Location + "</strong> item</div>");
        if (ContinueSearch) {
            //  $("#results").append('<div style="padding-bottom: 20px;text-align: center;padding-right: 156px;" class="continue"><a href="#" onclick="javascript:IsClone(\'' + 'location' + '|' + '{11111111-1111-1111-1111-111111111111}' + "', '" + "location" + "','" + '{11111111-1111-1111-1111-111111111111}\')\";>Continue Searching All Other Bucket Locations....</a></div>');
        }
        else {
            $(".bucketLocation")[0].innerText = 'Sitecore Start Node';
        }

        ContinueSearch = true;
        b = b + '<div class="mainmargin" id="grid-content" style="position: relative; width: auto;overflow-x: hidden; overflow-y: hidden;">';
        $.each
        (a.items,
            function () {
                if (a.items != 0) {
                    b = b + '<div onclick="' + "BindItemResult('" + this.Path + "'); return false;" + '"  class="post-1 post type-post status-publish format-standard hentry category-inspiration category-landscapes category-portraits category-typography category-web-design category-weddings tag-image tag-lightbox tag-sample post_float rounded" id="post-1" style="' + Meta(this) + '">' + "<a class=\"ceebox imgcontainer\" title=\"Lightbox Example\" href=\"\"  onclick=\"BindItemResult('" + this.Path + "'); return false;" + '">' + ' <img width="142" onerror="this.onerror=null;this.src=\'../ItemBuckets/images/default.jpg\';" height="100" src="' + this.ImagePath + '?w=142&h=100&db=master " class="attachment-post-thumbnail wp-post-image" ' + '  alt="' + this.Name + '" title="' + this.Name + '" /></a>' + " <h2> " + " <a class=\"ceebox\" title=\"Lightbox Example\" href=\"\" onclick=\"BindItemResult('" + this.Path + "'); return false;" + '">' + this.Name + "  </a></h2> " + ' <div class="post_tags"> ' + '<strong>Template: </strong>' + this.TemplateName + ' <strong>Location: </strong>' + this.Bucket + "<br/><p>" + (this.Content.length > 40 ? (this.Content.substring(0, 40) + "...") : this.Content) + "</p> <strong>Version: </strong>" + this.Version + " <strong>Created:</strong> " + this.Cre + " <strong>By:</strong> " + this.CreBy + "<br />" + " </div>" + " </div>"
                }
            }
        );

        b = b + "</div>";
        $("#results").append(b);
        $(".pagination").remove();
        var c = a.PageNumbers;
        var e = Page(a.CurrentPage, c);
        $(".pageSection").append(e);
        $("#results").fadeIn
        ("slow",
            function () {
            }
        );

        buildTipsMenu(a);

        $("#ajaxBusy").hide();
    }
    else {
        $("#results").html("");
        $(".pagination").remove();
        $("#results").append('<div style="padding-bottom: 20px;text-align: center;padding-right: 156px;">Your search has returned <strong>' + a.SearchCount + "</strong> result/s in <strong>" + a.SearchTime + "</strong> seconds under the <strong class='bucketLocation'>" + a.Location + "</strong> item</div>");
        if (ContinueSearch) {
            // $("#results").append('<div style="padding-bottom: 20px;text-align: center;padding-right: 156px;" class="continue"><a href="#" onclick="javascript:IsClone(\'' + 'location' + '|' + '{11111111-1111-1111-1111-111111111111}' + "', '" + "location" + "','" + '{11111111-1111-1111-1111-111111111111}\')\";>Continue Searching All Other Bucket Locations....</a></div>');
        }
        else {
            $(".bucketLocation")[0].innerText = 'Sitecore Start Node';
        }

        ContinueSearch = true;
        $("#results").append('<div id="resultAppendDiv" style="overflow: auto; height: auto;"><ul>');
        $.each
        (a.items,
            function () {
                if (a.items != 0) {
                    if (this.Name != null) {
                        $("#results").append('<li class="BlogPostArea" onclick="' + "BindItemResult('" + this.Path + "'); return false;" + '"  style="margin-left:' + InnerItem(this) + '">' + '<div class="BlogPostViews">' + "<a class=\"ceebox imgcontainer\" title=\"Lightbox Example\" href=\"\"  onclick=\"BindItemResult('" + this.Path + "'); return false;" + '">' + ' <img width="80" onerror="this.onerror=null;this.src=\'../ItemBuckets/images/default.jpg\';" height="60" src="' + this.ImagePath + '?w=80&h=60&db=master " class="attachment-post-thumbnail wp-post-image" ' + '  alt="' + this.Name + '" title="' + this.Name + '" /></a>' + "</div>" + '<h5 class="BlogPostHeader">' + '   <a href="#" onclick="' + "BindItemResult('" + this.Path + "'); return false;" + '">' + this.Name + "</a></h5>" + '<div class="BlogPostContent"><strong>Template: </strong>' + this.TemplateName + ' - <strong>Location: </strong>' + this.Bucket + "</div>" + '<div class="BlogPostFooter">' + this.Content + "   <div>" + " <strong>Version: </strong>" + this.Version + '      <strong>Created: </strong>' + this.Cre + "        <strong> by</strong>" + '    ' + this.CreBy + " </div>" + "<div>" + "</div>" + "</li>")
                    }
                    else {
                        $("#results").append('<li class="BlogPostArea" style="margin-left:' + InnerItem(this) + ';color: transparent;text-shadow: 0px 0px 10px #3D393D;">' + '<div class="BlogPostViews style="color: transparent;text-shadow: 0px 0px 10px #3D393D;">' + "<a class=\"ceebox imgcontainer\" title=\"Lightbox Example\" href=\"\" style=\"color: transparent;text-shadow: 0px 0px 10px #3D393D;\"" + '">' + ' <img width="80" height="60" src="' + "./images/defaultblur.jpg" + '?w=80&h=60&db=master " class="attachment-post-thumbnail wp-post-image" ' + '  alt="' + this.Name + '" title="' + this.Name + '" style="color: transparent;text-shadow: 0px 0px 10px #3D393D;"/></a>' + "</div>" + '<h5 style="color: transparent;text-shadow: 0px 0px 10px #3D393D;" class="BlogPostHeader">' + '   <a href="#"" style="color: transparent;text-shadow: 0px 0px 10px #3D393D;>' + this.Name + "</a></h5>" + '<div class="BlogPostContent" style="color: transparent;text-shadow: 0px 0px 10px #3D393D;">Template:' + this.TemplateName + ' - Location:' + this.Bucket + "</div>" + '<div class="BlogPostFooter" style="color: transparent;text-shadow: 0px 0px 10px #3D393D;">' + this.Content + "   <div style=\"color: transparent;text-shadow: 0px 0px 10px #3D393D;\">" + " Version:" + this.Version + '      Created: <a href="#" style="color: transparent;text-shadow: 0px 0px 10px #3D393D;">' + this.Cre + "        </a> by" + '    <a href="#" style="color: transparent;text-shadow: 0px 0px 10px #3D393D;">' + this.CreBy + " </a></div>" + "<div>" + "</div>" + "</li>")
                    }
                }
            }
        );

        $("#results").append("</ul></div>");
        $(".pagination").remove();
        var b = a.PageNumbers;
        var c = Page(a.CurrentPage, b);
        $(".pageSection").append(c);

        buildTipsMenu(a);
    }
}

function Page(a, b) {
    if (b < 2) {
        return '<div class="pagination empty"><div>';
    }
    var c = '<div class="pagination"><ul>';
    var d = Math.floor((a - 1) / maxPageCount) * maxPageCount + 1;
    var e = Math.min(d + maxPageCount - 1, b);
    if (d > 1) {
        c += '<li class="previous-pages">' + '<a class="pageLink" href="javascript:void(0);" data-page="' + (d - 1) + '" ><</a>' + "</li>";
    }

    for (var f = d; f <= e; f++) {
        c = c + "<li " + (f == a ? 'class="active"' : "") + ">" + '<a class="pageLink" data-page="' + f + '" href="javascript:void(0)">' + f + "</a>" + "</li>";
    }
    if (e < b) {
        c += '<li class="next-pages">' + '<a class="pageLink" href="javascript:void(0);" data-page="' + (e + 1) + '">></a>' + "</li>";
    }

    c += "</ul>" + "</div>";

    return c;
}

function Meta(a) {
    if (a.IsClone == true) {
        return "opacity:0.4;";
    }
    else {
        return "";
    }
}

function ResolveSearches(query) {
    var searchArray = new Array();
    var searchList = query.split(';');
    for (var i = 0; i < searchList.length; i++) {
        var ss = searchList[i].split(':');
        if (ss != "") {
            searchArray.push(ss[0]);
            if (ss.length > 1) {
                searchArray.push(ss[1]);
            }
        }
    }
    return searchArray;
}

$.fn.outerHTML = function (a) {
    return a ? this.before(a).remove() : $("<p>").append(this.eq(0).clone()).html();
};

function InnerItem(a) {
    if (a.IsClone == true) {
        return "105px;opacity:0.4;";
    }
    else {
        return "70px;";
    }
}

$.fn.outerHTML = function (a) {
    return a ? this.before(a).remove() : $("<p>").append(this.eq(0).clone()).html();
};

var pageNumber = 0;
var maxPageCount = 10;
$(function () {
    function i() {
    }

    function h(a) {

        var mode = detectViewMode();

        $("#results").html("");

        $("#results").append('<div style="padding-bottom: 20px;text-align: center;padding-right: 156px;">Your search has returned <strong>' + a.SearchCount + "</strong> result/s in <strong>" + a.SearchTime + "</strong> seconds under the <strong class='bucketLocation'>" + a.Location + "</strong> item</div>");
        if (ContinueSearch) {
            // $("#results").append('<div style="padding-bottom: 20px;text-align: center;padding-right: 156px;" class="continue"><a href="#" onclick="javascript:IsClone(\'' + 'location' + '|' + '{11111111-1111-1111-1111-111111111111}' + "', '" + "location" + "','" + '{11111111-1111-1111-1111-111111111111}\')\";>Continue Searching All Other Bucket Locations....</a></div>');
        }
        else {
            $(".bucketLocation")[0].innerText = 'Sitecore Start Node';
        }
        var modeObject;
        jQuery.ajax({
            type: "POST",
            url: "/sitecore%20modules/Shell/Sitecore/ItemBuckets/ItemBucket.asmx/GetView",
            data: "{'viewName' : '" + mode + "'}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (serverResponse) {
                modeObject = serverResponse.d;
                var b = "";
                if (modeObject != undefined && modeObject.HeaderTemplate != "") {

                    ContinueSearch = true;
                    b = b + modeObject.HeaderTemplate;
                    $.each(a.items,
                        function (serverItem) {
                            if (a.items != 0) {
                                var templateFiller = this.TemplateName;
                                var metaFiller = Meta(this);
                                var launchTypeFiller = a.launchType;
                                var itemIdFiller = this.ItemId;
                                var imagePathFiller = this.ImagePath;
                                var nameFiller = this.Name;
                                var bucketFiller = this.Bucket;
                                var contentFiller = (this.Content.length > 40 ? (this.Content.substring(0, 40) + "...") : this.Content);
                                var versionFiller = this.Version;
                                var createdFiller = this.Cre;
                                var createdbyFiller = this.CreBy;

                                var templateText = modeObject.ItemTemplate;
                                b = b + templateText.replace(/TemplatePlaceholder/g, templateFiller).replace(/NamePlaceholder/g, nameFiller).replace(/MetaPlaceholder/g, metaFiller).replace(/LaunchTypePlaceholder/g, launchTypeFiller).replace(/ItemIdPlaceholder/g, itemIdFiller).replace(/ImagePathPlaceholder/g, imagePathFiller).replace(/NamePlaceholder/g, nameFiller).replace(/BucketPlaceholder/g, bucketFiller).replace(/ContentPlaceholder/g, contentFiller).replace(/VersionPlaceholder/g, versionFiller).replace(/CreatedPlaceholder/g, createdFiller).replace(/CreatedByPlaceholder/g, createdbyFiller);
                            }
                        }
                    );

                    b = b + modeObject.FooterTemplate;
                }
                else {
                    ContinueSearch = true;
                    b = b + '<div class="mainmargin" id="grid-content" style="position: relative; width: auto;overflow-x: hidden; overflow-y: hidden;">';
                    $.each(a.items,
                function () {
                    if (a.items != 0) {
                        b = b + '<div onclick="' + "BindItemResult('" + this.Path + "'); return false;" + '"  class="post-1 post type-post status-publish format-standard hentry category-inspiration category-landscapes category-portraits category-typography category-web-design category-weddings tag-image tag-lightbox tag-sample post_float rounded" id="post-1" style="' + Meta(this) + '">' + "<a class=\"ceebox imgcontainer\" title=\"Lightbox Example\" href=\"\"  onclick=\"BindItemResult('" + this.Path + "'); return false;" + '">' + ' <img width="142" onerror="this.onerror=null;this.src=\'../ItemBuckets/images/default.jpg\';" height="100" src="' + this.ImagePath + '?w=142&h=100&db=master " class="attachment-post-thumbnail wp-post-image" ' + '  alt="' + this.Name + '" title="' + this.Name + '" /></a>' + " <h2> " + " <a class=\"ceebox\" title=\"Lightbox Example\" href=\"\" onclick=\"BindItemResult('" + this.Path + "'); return false;" + '">' + this.Name + "  </a></h2> " + ' <div class="post_tags"> ' + '<strong>Template:</strong>' + this.TemplateName + ' <strong>Location: </strong>' + this.Bucket + "<br/><p>" + (this.Content.length > 40 ? (this.Content.substring(0, 40) + "...") : this.Content) + "</p><strong>Version:</strong> " + this.Version + " <strong>Created:</strong>  " + this.Cre + " <strong>By: </strong> " + this.CreBy + "<br />" + " </div>" + " </div>";
                    }
                }
            );

                    b = b + "</div>";
                }
                $("#results").append(b);
                $(".pagination").remove();
                var c = a.PageNumbers;
                var e = Page(a.CurrentPage, c);
                $(".pageSection").append(e);
                $("#results").fadeIn
        ("slow",
            function () {
            }
        );

                buildTipsMenu(a);

                $("#ajaxBusy").hide();
                //$('.addition').removeAttr('disabled');
            }
        });


    }

    function g() {
        //        $("#results").html("");
        //        $("#results").append('<div style="padding-bottom: 20px;text-align: center;padding-right: 85px;">Apologies, we could not resolve your search. Please try to refine your search, as your query is too broad.</div>');
        //        if (ContinueSearch) {
        //            //  $("#results").append('<div style="padding-bottom: 20px;text-align: center;padding-right: 156px;" class="continue"><a href="#" onclick="javascript:IsClone(\'' + 'location' + '|' + '{11111111-1111-1111-1111-111111111111}' + "', '" + "location" + "','" + '{11111111-1111-1111-1111-111111111111}\')\";>Continue Searching All Other Bucket Locations....</a></div>');
        //        } else {
        //            $(".bucketLocation")[0].innerText = 'Sitecore Start Node';
        //        }

        //        ContinueSearch = true;
        $("#ajaxBusy").hide();
    }

    function d(a, b) {
        if (b < 2) {
            return '<div class="pagination empty"><div>';
        }

        var c = '<div class="pagination"><ul>';
        var d = Math.floor((a - 1) / maxPageCount) * maxPageCount + 1;
        var e = Math.min(d + maxPageCount - 1, b);
        if (d > 1) {
            c += '<li class="previous-pages">' + '<a class="pageLink" href="javascript:void(0);" data-page="' + (d - 1) + '" ><</a>' + "</li>";
        }

        for (var f = d; f <= e; f++) {
            c = c + "<li " + (f == a ? 'class="active"' : "") + ">" + '<a class="pageLink" data-page="' + f + '" href="javascript:void(0)">' + f + "</a>" + "</li>";
        }

        if (e < b) {
            c += '<li class="next-pages">' + '<a class="pageLink" href="javascript:void(0);" data-page="' + (e + 1) + '">></a>' + "</li>";
        }

        c += "</ul>" + "</div>";
        return c;
    }

    function c(a) {
        $("#results").html("");
        $(".pagination").remove();
        $("#results").append('<div style="padding-bottom: 20px;text-align: center;padding-right: 156px;">Your search has returned <strong>' + a.SearchCount + "</strong> result/s in <strong>" + a.SearchTime + "</strong> seconds under the <strong class='bucketLocation'>" + a.Location + "</strong> item</div>");
        if (ContinueSearch) {
            // $("#results").append('<div style="padding-bottom: 20px;text-align: center;padding-right: 156px;" class="continue"><a href="#" onclick="javascript:IsClone(\'' + 'location' + '|' + '{11111111-1111-1111-1111-111111111111}' + "', '" + "location" + "','" + '{11111111-1111-1111-1111-111111111111}\')\";>Continue Searching All Other Bucket Locations....</a></div>');
        }
        else {
            $(".bucketLocation")[0].innerText = 'Sitecore Start Node';
        }

        ContinueSearch = true;
        $("#results").append('<div id="resultAppendDiv" style="overflow: auto; height: auto;"><ul>');
        $.each
        (a.items,
        function () {
            if (a.items != 0) {
                if (this.Name != null) {
                    $("#results").append('<li class="BlogPostArea" onclick="' + "BindItemResult('" + this.Path + "'); return false;" + '"  style="margin-left:' + InnerItem(this) + '">' + '<div class="BlogPostViews">' + "<a class=\"ceebox imgcontainer\" title=\"Lightbox Example\" href=\"\"  onclick=\"BindItemResult('" + this.Path + "'); return false;" + '">' + ' <img width="80" onerror="this.onerror=null;this.src=\'../ItemBuckets/images/default.jpg\';" height="60" src="' + this.ImagePath + '?w=80&h=60&db=master " class="attachment-post-thumbnail wp-post-image" ' + '  alt="' + this.Name + '" title="' + this.Name + '" /></a>' + "</div>" + '<h5 class="BlogPostHeader">' + '   <a href="#" onclick="' + "BindItemResult('" + this.Path + "'); return false;" + '">' + this.Name + "</a></h5>" + '<div class="BlogPostContent"><strong>Template: </strong>' + this.TemplateName + '- <strong>Location: </strong>' + this.Bucket + "</div>" + '<div class="BlogPostFooter">' + this.Content + "   <div>" + " <strong>Version: </strong>" + this.Version + '      <strong>Created: </strong>' + this.Cre + "        <strong> by</strong>" + '    ' + this.CreBy + " </div>" + "<div>" + "</div>" + "</li>")
                }
                else {
                    $("#results").append('<li class="BlogPostArea" style="margin-left:' + InnerItem(this) + ';color: transparent;text-shadow: 0px 0px 10px #3D393D;">' + '<div class="BlogPostViews style="color: transparent;text-shadow: 0px 0px 10px #3D393D;">' + "<a class=\"ceebox imgcontainer\" title=\"Lightbox Example\" href=\"#\" style=\"color: transparent;text-shadow: 0px 0px 10px #3D393D;\"" + '">' + ' <img width="80" height="60" src="' + "./images/defaultblur.jpg" + '?w=80&h=50&db=master " class="attachment-post-thumbnail wp-post-image" ' + '  alt="' + this.Name + '" title="' + this.Name + '" style="color: transparent;text-shadow: 0px 0px 10px #3D393D;"/></a>' + "</div>" + '<h5 style="color: transparent;text-shadow: 0px 0px 10px #3D393D;" class="BlogPostHeader">' + '   <a href="#"" style="color: transparent;text-shadow: 0px 0px 10px #3D393D;>' + this.Name + "</a></h5>" + '<div class="BlogPostContent" style="color: transparent;text-shadow: 0px 0px 10px #3D393D;">Template:' + this.TemplateName + ' - Location:' + this.Bucket + "</div>" + '<div class="BlogPostFooter" style="color: transparent;text-shadow: 0px 0px 10px #3D393D;">' + this.Content + "   <div style=\"color: transparent;text-shadow: 0px 0px 10px #3D393D;\">" + " Version:" + this.Version + '      Created: <a href="#" style="color: transparent;text-shadow: 0px 0px 10px #3D393D;">' + this.Cre + "        </a> by" + '    <a href="#" style="color: transparent;text-shadow: 0px 0px 10px #3D393D;">' + this.CreBy + " </a></div>" + "<div>" + "</div>" + "</li>")
                }
            }
        });

        $("#results").append("</ul></div>");
        $(".pagination").remove();
        var b = a.PageNumbers;
        var c = a.CurrentPage;
        var e = d(c, b);
        $(".pageSection").append(e);

        buildTipsMenu(a);

        $('.handle2').toggle
        (
            function () {
                if ($('.content').css('opacity') == '1.0') {
                    $('.handle2').css("background-image", "url(images/thin-arrow-right.png)");
                    $('.handle2').css("background-position", "50% 50%");
                    $('.handle2').addClass("toggled");
                } else {
                    $('.handle2').css("background-image", "url(images/thin-arrow-right.png)");
                    $('.handle2').css("background-position", "50% 50%");
                    $('.handle2').removeClass("toggled");
                }

                return false;
            },

            function () {
                if ($('.content').css('opacity') == '0.4') {
                    $('.handle2').css("background-image", "url(images/thin-arrow-left.png)");
                    $('.handle2').css("background-position", "50% 50%");
                    $('.handle2').addClass("toggled");
                } else {
                    $('.handle2').css("background-image", "url(images/thin-arrow-left.png)");
                    $('.handle2').css("background-position", "50% 50%");
                    $('.handle2').removeClass("toggled");
                }

                return false;
            });

        if (a.CurrentPage > 1) {
           // $('.addition').removeAttr('disabled');
        }

        if (!$.browser.msie) {
            $(".handle").css("right", "-52px");
        }
        $("#ajaxBusy").hide();
    }



    var a = $("#ui_element");
    a.find(".addition").focus();
    a.find(".addition").bind
    ("focus",
        function () {
            if ((a.find(".addition").val().length > 0) || ($('.boxme').children('li').length > 1) || (a.find(".addition").text().length > 0)) {
                if ($(".addition").val().indexOf("Search for an Item") > -1) {
                    var tempAddition = $(".addition").val().replace("Search for an Item", "");
                    $(".addition").val(tempAddition).css("font-style", "italic").css("opacity", "0.3");
                }
            }
            else {
                $(".addition").css("font-style", "normal").css("opacity", "1.0");
            }

            a.find(".boxme").addClass("myInputbox");

            $('.content').css
            ({
                'opacity': 1.0
            });
        }
    );

    a.find(".addition").bind
    ("blur",
        function () {
            if ((a.find(".addition").val().length <= 0) && $('.boxme').children('li').length <= 1 && (a.find(".addition").text().length <= 0)) {
                $(".addition").val("Search for an Item").css("font-style", "italic").css("opacity", "0.3");
            }
            else {
                $(".addition").css("font-style", "normal").css("opacity", "1.0");
            }

            a.find(".boxme").removeClass("myInputbox");
        }
    );

    $(".msg_body5, .msg_body1, .msg_body2, .msg_body2, .msg_body3, .msg_body4").hide();

    $(".msg_head5").click
    (
        function () {
            $(this).next(".msg_body5").slideToggle(100);
        }
    );

    $(".msg_head1").click
    (
        function () {
            $(this).next(".msg_body1").slideToggle(100);
        }
    );

    function ConvertSearchQuery() {

        retrieveFilters();
    }

    function ParseSearchForQuery() {

        var u = buildQuery();
        var returnString = "";
        $.each(u, function () {
            returnString = returnString + this.type + ":" + this.value + ";";
        });

        return returnString;
    }

    $(".SearchOperation").live("click", function () {
        scForm.getParentForm().postRequest('', '', '', 'bucket:' + this.id + '(url="' + ParseSearchForQuery() + '")');
        return false;
    });

    a.find(".addition").live("keydown", function (b) {
        var d = b.keyCode || b.which;
        if (d == 13) {


            if (CurrentView != "") {
                $("." + CurrentView).click();

            }
            else {

                if ((!$('.addition').val().replace(/\s/g, '').length && $('.boxme').children('li').length <= 1) || (($('.boxme').children('li').length == 2) && $('.boxme').children('li')[0].innerHTML.indexOf('p class="end type"') > 0) || $('.addition').val() == "Search for an Item") {
                    if (!($.browser.msie)) {

                        $('.boxme').stop().css("background-color", "#EE0000").animate({ backgroundColor: "#FFFFFF" }, 3000);
                    }
                    // $('.addition').removeAttr('disabled');
                } else {
                    b.preventDefault();
                    $(".sb_dropdown").hide();
                    pageNumber = 0;
                    $("#ajaxBusy").css({
                        display: "block"
                    });

                retrieveFilters();
                var u = buildQuery();
                runQuery(u, pageNumber, c, g);
                runFacet(u, pageNumber, meme, g);

                    $(".navAlpha").html("");
                    $(".slide-out-div").html("");
                    $(".slide-out-div").prepend('<div id="ajaxBusyFacet"><p><img src="images/loading.gif"></p><p>Loading Facets...</p></div>');
                    $("#ajaxBusyFacet").css({
                        margin: "0px auto",
                        width: "44px"
                    });
                }
            }
        }
    });

    $(".slide-out-div").hide();
    $(".sb_search_container").click(function () {
        a.find(".addition").focus();
    });

    function toggleDropDown() {

        a.find(".sb_down").addClass("sb_up").removeClass("sb_down");
        jQuery.ajax({
            type: "POST",
            url: "/sitecore%20modules/Shell/Sitecore/ItemBuckets/ItemBucket.asmx/RunLookup",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (b) {
                var c = b.d;
                var e = "";
                $.each(c, function () {
                    var showMe = this.Name.replace(" ", "");
                    showMe = showMe.replace(" ", "");
                    if ($.browser.msie) {
                        e = e + '<div title="Click to load/reload the data" class=\"sb_filter recent ' + showMe + '\" style=\"font-weight:bold;\">' + this.Name + " - <span style=\"font-size:8px;\">" + this.DisplayText + '</span></div><div class="' + showMe + "body" + '" style="display:none"></div>';
                    }
                    else {
                        e = e + '<div title="Click to load/reload the data" class=\"sb_filter recent ' + showMe + '\" style=\"font-weight:bold;background: url(\'/temp/iconCache/' + this.Icon + '\') no-repeat left center;padding-left:25px;background-size:16px 16px;background-position-x:3px;\">' + this.Name + " - <span style=\"font-size:8px;\">" + this.DisplayText + '</span></div><div class="' + showMe + "body" + '" style="display:none"></div>';
                    }
                    $("." + showMe).die("click");
                    $("." + showMe).live('click',
                        function () {
                            var toggled = $("." + showMe).next("." + showMe + "body").is(":visible");
                            if (!toggled) {
                                //$("." + showMe).css("background-image", "url(./ItemBuckets/images/up.png)"); //Add the .. back into the link
                                jQuery.ajax({
                                    type: "POST",
                                    url: "/sitecore%20modules/Shell/Sitecore/ItemBuckets/ItemBucket.asmx/GenericCall",
                                    data: "{'ServiceName' : '" + showMe + "'}",
                                    contentType: "application/json; charset=utf-8",
                                    dataType: "json",
                                    success: function (b) {
                                        var c = b.d;
                                        c = c.toString().split(",");
                                        var e = "";
                                        var scope = this;
                                        $.each(c, function () {
                                            if (this != "") {
                                                if ((scope.data.indexOf("RecentlyModified") > 0) || (scope.data.indexOf("RecentlyCreated")) > 0 || (scope.data.indexOf("RecentTabs")) > 0) {
                                                    var splitMe = this.split("|");
                                                    e = e + "<li><a href=\"#\" onclick=\"BindItemResult(" + GetItemPathFromMediaLibrary(splitMe[1]) + "); return false;\" title='Click this to launch a search based on the '" + this + '" class=\"command\" id="' + splitMe[1] + "' style=\"background: url(\'images/pin.png\') no-repeat left center;padding: 0px 18px;\">" + (splitMe[0].length > 20 ? (splitMe[0].substring(0, 20) + "...") : splitMe[0]) + "</a></li>"
                                                } else if (scope.data.indexOf("SearchOperations") > 0) {
                                                    if ($.browser.msie) {
                                                        e = e + '<li><a href="#" id="' + this.split("|")[0].toString().replace(/\s/g, '') + '" title="Click this to launch a search based on the ' + this.split("|")[0] + '" class="SearchOperation ' + this.split("|")[0].toString().replace(/\s/g, '') + '" style="">' + (this.split("|")[0].length > 20 ? (this.split("|")[0].substring(0, 20) + "...") : this.split("|")[0]) + "</a></li>"
                                                    }
                                                    else {
                                                        e = e + '<li><a href="#" id="' + this.split("|")[0].toString().replace(/\s/g, '') + '" title="Click this to launch a search based on the ' + this.split("|")[0] + '" class="SearchOperation ' + this.split("|")[0].toString().replace(/\s/g, '') + '" style="background: url(\'/temp/iconCache/' + this.split("|")[1] + '\') no-repeat left center;padding: 0px 18px;background-size:16px 16px;">' + (this.split("|")[0].length > 20 ? (this.split("|")[0].substring(0, 20) + "...") : this.split("|")[0]) + "</a></li>"
                                                    } 
                                                } else if (scope.data.indexOf("MyRecentSearches") > 0) {

                                                    e = e + '<li><a href="#" title="Click this to launch a search based on the ' + this + '" class="command">' + (this.length > 30 ? (this.substring(0, 30) + "...") : this) + "</a></li>";

                                                } else {
                                                    if ($.browser.msie) {
                                                        e = e + '<li><a href="#" title="Click this to launch a search based on the ' + this.split("|")[0] + '" class="command" style="no-repeat left center;padding: 0px 18px;">' + (this.split("|")[1].length > 30 ? (this.split("|")[1].substring(0, 30) + "...") : this.split("|")[1]) + "</a></li>";
                                                    } else {
                                                        e = e + '<li><a href="#" title="Click this to launch a search based on the ' + this.split("|")[0] + '" class="command" style="background: url(\'/temp/iconCache/' + this.split("|")[2] + '\') no-repeat left center;padding: 0px 18px;background-size:16px 16px;">' + (this.split("|")[1].length > 30 ? (this.split("|")[1].substring(0, 30) + "...") : this.split("|")[1]) + "</a></li>";
                                                    }
                                                }
                                            }
                                        });

                                        $("." + showMe + "body").html("");
                                        $("." + showMe + "body").append(e);
                                        $("." + showMe + "body").show();
                                        $(".command").first().attr("id", "keySelect");

                                    }
                                });
                            } else {
                                $("." + showMe).next("." + showMe + "body").toggle('slow');
                            }
                        });

                    $(".command, .topsearch").live("click",
                        function () {
                            if ((this.parentElement.parentElement.className.replace("body", "") == "RecentlyModified") || (this.parentElement.parentElement.className.replace("body", "") == "RecentlyCreated") || (this.parentElement.parentElement.className.replace("body", "") == "RecentTabs")) {

                            } else {

                                var listOfSearches = ResolveSearches(this.title.replace("Click this to launch a search based on the ", "").replace(new RegExp(",", 'g'), ""));

                                $(".addition").val("");
                                if (listOfSearches.length == 1) {
                                    $(".addition").val(listOfSearches[0]);
                                    $(".addition").focus();
                                    $(".addition").val($(".addition").val() + ":");
                                    var press = jQuery.Event("keyup");
                                    press.shiftKey = true;
                                    press.which = 58;
                                    $(".addition").trigger(press);
                                } else {
                                    for (var iii = 0; iii < listOfSearches.length; iii = iii + 2) {
                                        if (listOfSearches[iii + 1] != "") {
                                            var childCheck = $(".boxme").children(".token-input-token-facebook").children('.' + this.text.split(':')[0]);
                                            if (childCheck.text().indexOf(this.text.split(':')[1].replace(';', '')) < 0) {
                                                $(".boxme").prepend('<li class="token-input-token-facebook"><span style="background: url(\'images/' + listOfSearches[iii] + '.gif\') no-repeat center center;padding: 0px 11px;"></span><p class="' + listOfSearches[iii] + '">' + listOfSearches[iii + 1] + '</p><span class="token-input-delete-token-facebook remove">×</span></li>');
                                                $(".remove").live("click",
                                                    function () {
                                                        $(this).parents("li:first").remove();
                                                        $(".addition").focus();
                                                    });
                                            }
                                        }
                                    }
                                }
                            }
                        });
                });

                $(".sb_dropdown").html("");
                $(".sb_dropdown").append(e);
                $(".sb_dropdown").show();

                $(".sb_dropdown").children().first(".sb_filter.recent").attr("id", "keySelect");


            }
        });

    }

    function toggleDropDownUp() {
        $(".sb_dropdown").hide();
        a.find(".sb_up").addClass("sb_down").removeClass("sb_up");
    };

    $(".sb_down, .sb_up").toggle(toggleDropDown,
        toggleDropDownUp);

    $(".sb_search").click(function () {

        if (CurrentView != "") {
            $("." + CurrentView).click();

        }
        else {

            if ((!$('.addition').val().replace(/\s/g, '').length && $('.boxme').children('li').length <= 1) || (($('.boxme').children('li').length == 2) && $('.boxme').children('li')[0].innerHTML.indexOf('p class="end type"') > 0) || $('.addition').val() == "Search for an Item") {
                if (!($.browser.msie)) {

                    $('.boxme').stop().css("background-color", "#EE0000").animate({ backgroundColor: "#FFFFFF" }, 3000);
                }
                // $('.addition').removeAttr('disabled');
            } else {
                if ((a.find(".addition").val().length > 0) || $('.boxme').children('li').length > 1) {
                    $('.sb_clear').css({
                        display: 'inline'
                    });
                }

            pageNumber = 0;
            $('.content').css
            ({
                'opacity': 1.0
            });

            $(".grid").removeClass("active");
            $(".list").addClass("active");
            $("#ajaxBusy").css
            ({
                display: "block"
            });

            var n = buildQuery();
            retrieveFilters();
            runQuery(n, pageNumber, c, g);
            runFacet(n, pageNumber, meme, g);

                a.find(".sb_dropdown").hide();
                $(".navAlpha").html("");
                $(".slide-out-div").html("");
                $(".slide-out-div").prepend('<div id="ajaxBusyFacet"><p><img src="images/loading.gif"></p><p>Loading Facets...</p></div>');
                $("#ajaxBusyFacet").css({
                    margin: "0px auto",
                    width: "44px"
                });
            }
        }
    });


    $(".list").click(function () {
        CurrentView = "list";
        if ((!$('.addition').val().replace(/\s/g, '').length && $('.boxme').children('li').length <= 1) || (($('.boxme').children('li').length == 2) && $('.boxme').children('li')[0].innerHTML.indexOf('p class="end type"') > 0) || $('.addition').val() == "Search for an Item") {
            if (!($.browser.msie)) {

                $('.boxme').stop().css("background-color", "#EE0000").animate({ backgroundColor: "#FFFFFF" }, 3000);
            }
            // $('.addition').removeAttr('disabled');
        } else {
            if ((a.find(".addition").val().length > 0) || $('.boxme').children('li').length > 1) {
                $('.sb_clear').css({
                    display: 'inline'
                });
            }

            pageNumber = 0;
            $('.content').css({
                'opacity': 1.0
            });

            $(".grid").removeClass("active");
            $(".list").addClass("active");
            $("#ajaxBusy").css({
                display: "block"
            });

            var n = buildQuery();
            retrieveFilters();
            runQuery(n, pageNumber, c, g);
            runFacet(n, pageNumber, meme, g);

            a.find(".sb_dropdown").hide();
            $(".navAlpha").html("");
            $(".slide-out-div").html("");
            $(".slide-out-div").prepend('<div id="ajaxBusyFacet"><p><img src="images/loading.gif"></p><p>Loading Facets...</p></div>');
            $("#ajaxBusyFacet").css({
                margin: "0px auto",
                width: "44px"
            });
        }
    });

    $(".pageLink").live("click", function () {
        $(this).addClass("pageClickLoad");
        if ((!$('.addition').val().replace(/\s/g, '').length && $('.boxme').children('li').length <= 1) || (($('.boxme').children('li').length == 2) && $('.boxme').children('li')[0].innerHTML.indexOf('p class="end type"') > 0) || $('.addition').val() == "Search for an Item") {
            // $('.addition').removeAttr('disabled');
            if (!($.browser.msie)) {

                $('.boxme').stop().css("background-color", "#EE0000").animate({ backgroundColor: "#FFFFFF" }, 3000);
            }

            $(this).removeClass("pageClickLoad");
        }
        else {
            a.find(".sb_down").addClass("sb_up").removeClass("sb_down").andSelf().find(".sb_dropdown").hide();
            $('.content').css
            ({
                'opacity': 1.0
            });

            $("#ajaxBusy").css
            ({
                display: "block"
            });

            var p = buildQuery();
            retrieveFilters();

            if (CurrentView != "list" && CurrentView != "grid" && CurrentView != "") {
                pageNumber = $(this).attr("data-page");
                runQuery(p, pageNumber, h, g);
            }

            else if (CurrentView == "grid") {
                pageNumber = $(this).attr("data-page");
                runQuery(p, pageNumber, h, i);
                runFacet(p, pageNumber, meme, g);

                $(".navAlpha").html("");
                $(".slide-out-div").html("");
                $(".slide-out-div").prepend('<div id="ajaxBusyFacet"><p><img src="images/loading.gif"></p><p>Loading Facets...</p></div>');
                $("#ajaxBusyFacet").css({

                    margin: "0px auto",
                    width: "44px"
                });

            } else {
                pageNumber = $(this).attr("data-page");
                runQuery(p, pageNumber, c, g);
            }
        }

        $('html, body').animate({ scrollTop: 0 }, 'slow');



    });

    $(".grid").click(function () {
        CurrentView = "grid";
        if ((!$('.addition').val().replace(/\s/g, '').length && $('.boxme').children('li').length <= 1) || (($('.boxme').children('li').length == 2) && $('.boxme').children('li')[0].innerHTML.indexOf('p class="end type"') > 0) || $('.addition').val() == "Search for an Item") { //Add check for "Search for an Item"
            if (!($.browser.msie)) {

                $('.boxme').stop().css("background-color", "#EE0000").animate({ backgroundColor: "#FFFFFF" }, 3000);
            }
            // $('.addition').removeAttr('disabled');
        } else {
            a.find(".sb_down").addClass("sb_up").removeClass("sb_down").andSelf().find(".sb_dropdown").hide();
            pageNumber = 0;
            $(".list").removeClass("active");
            $(".grid").addClass("active");
            $('.content').css
            ({
                'opacity': 1.0
            });

            $("#ajaxBusy").css
            ({
                display: "block"
            });

            var n = buildQuery();
            retrieveFilters();
            runQuery(n, pageNumber, h, i);
            runFacet(n, pageNumber, meme, g);

            $(".navAlpha").html("");
            $(".slide-out-div").html("");
            $(".slide-out-div").prepend('<div id="ajaxBusyFacet"><p><img src="images/loading.gif"></p><p>Loading Facets...</p></div>');
            $("#ajaxBusyFacet").css({
                display: "none",
                margin: "0px auto",
                width: "44px"
            });
        }
    });

    establishViews();


    //Event Bindings

    a.find(".addition").bind("focus", function () {
        if ((a.find(".addition").val().length > 0) || $('.boxme').children('li').length > 1) {
            $('.sb_clear').css
            ({
                display: 'inline'
            });
        }

        $(".sb_dropdown").hide();
        a.find(".sb_up").addClass("sb_down").removeClass("sb_up");
    });

    a.find(".addition").live("keydown", function (b) {
        var c = b.keyCode || b.which;
        if (a.find(".addition").val().indexOf("tag:") > -1) {
            if (this.value.replace("tag:", "").length >= 2) {
                jQuery.ajax
                ({
                    type: "POST",
                    url: "/sitecore%20modules/Shell/Sitecore/ItemBuckets/ItemBucket.asmx/GetTag",
                    data: "{'tagChars' : '" + this.value.replace("tag:", "").substring(0, this.value.replace("tag:", "").length) + "'}",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (a) {
                        var b = a.d;
                        var c = new Array;
                        $.each(b, function () {
                            c.push("tag:" + this.DisplayText + "|tagid=" + this.DisplayValue);
                        });

                        $(".ui-corner-all").live("click", function () {
                            ConvertSearchQuery();
                        });

                        $(".addition").autocomplete({
                            source: c,
                            autoFocus: true
                        });
                    }
                });
            }
        }

        autoSuggestWithWait(this, "author", "GetAuthors", "{'tagChars' : '" + this.value.replace("author:", "").substring(0, this.value.replace("author:", "").length) + "'}", 1);
        autoSuggestWithWait(this, "custom", "GetFields", "{'tagChars' : '" + this.value.replace("custom:", "").substring(0, this.value.replace("custom:", "").length) + "'}", 1);
        autoSuggestWithWait(this, "extension", "GetFileType", "{'tagChars' : '" + this.value.replace("filetype:", "").substring(0, this.value.replace("filetype:", "").length) + "'}", 1);

        if ((a.find(".addition").val().length > 0) || $('.boxme').children('li').length > 1) {
            $('.sb_clear').css({
                display: 'inline'
            });
        }
        if (c == 88 && b.ctrlKey) {
            $('.sb_clear').css
            ({
                display: 'none'
            });

            $(".addition").val("");
            $(".addition").text("");
            $(".boxme").children(".token-input-token-facebook").remove();
        }

        if (c == 9) {
            retrieveFilters();
            b.preventDefault();
        }

        clearTimeout(typingTimer);



    });

    var typingTimer;                //timer identifier
    var doneTypingInterval = 3000;  //time in ms, 3 second for example
    function doneTyping() {
        autoSuggestText($(".addition"), "", "{'tagChars' : '" + $(".addition").val() + "'}", 3);
    }
    $("body").live("keyup", function (b) {

        typingTimer = setTimeout(doneTyping, doneTypingInterval);
       


        //Spacebar and Ctrl pressed.
        if (b.which == 32 && b.ctrlKey) {
            event.preventDefault();
            toggleDropDown();
        }
        //Escape pressed.
        if (b.which == 27) {

            toggleDropDownUp();
            if ($(".handle2").hasClass("toggled")) {
                $(".handle2").click();
            }
            if ($(".handle").hasClass("toggled")) {
                $(".handle").click();
            }
        }

        //"b" and Ctrl pressed.
        if (b.which == 66 && b.ctrlKey) {
            $(".addition").focus();
        }

        if (!$(".addition").is(":focus")) {
            if (b.which == 56 || b.which == 57 || b.which == 49 || b.which == 50 || b.which == 51 || b.which == 52 || b.which == 53 || b.which == 54 || b.which == 55) {

                if (b.shiftKey) {
                    switch (b.which) {
                        case 49:
                            CurrentView = $("#views").children().first().attr("id");
                            $("." + CurrentView).click();
                            break;
                        case 50:
                            CurrentView = $("#views").children().first().next().attr("id");
                            $("." + CurrentView).click();
                            break;
                        case 51:
                            CurrentView = $("#views").children().first().next().next().attr("id");
                            $("." + CurrentView).click();
                            break;
                        case 52:
                            CurrentView = $("#views").children().first().next().next().next().attr("id");
                            $("." + CurrentView).click();
                            break;
                        case 53:
                            CurrentView = $("#views").children().first().next().next().next().next().attr("id");
                            $("." + CurrentView).click();
                            break;

                        default:
                            CurrentView = $("#views").children().first().next().attr("id");
                            $("." + CurrentView).click();
                    }



                } else {
                    switch (b.which) {
                        case 49:
                            MoveToPage(1);
                            break;
                        case 50:
                            MoveToPage(2);
                            break;
                        case 51:
                            MoveToPage(3);
                            break;
                        case 52:
                            MoveToPage(4);
                            break;
                        case 53:
                            MoveToPage(5);
                            break;
                        case 54:
                            MoveToPage(6);
                            break;
                        case 55:
                            MoveToPage(7);
                            break;
                        case 56:
                            MoveToPage(8);
                            break;
                        case 57:
                            MoveToPage(9);
                            break;
                        default:
                    }
                }
            }
        }

    });


    a.find(".addition").live("keyup", function (b) {
        if ((a.find(".addition").val().length > 0) || $('.boxme').children('li').length > 1) {
            $('.sb_clear').css
            ({
                display: 'inline'
            });
        }
        else {
            $('.sb_clear').css
            ({
                display: 'none'
            });
        }

        if (b.which == 88 && b.ctrlKey) {
            $('.sb_clear').css
            ({
                display: 'none'
            });

            $(".addition").val("");
            $(".addition").text("");
            $(".boxme").children(".token-input-token-facebook").remove();
        }




        //Spacebar and Ctrl pressed.
        if (b.which == 32 && b.ctrlKey) {
            if ($(".addition").val() == " ") {
                $(".addition").val("");
            }
            Expanded = false;
            toggleDropDown();

        }
        //Escape pressed.
        if (b.which == 27) {

            toggleDropDownUp();
        }

        //"b" and Ctrl pressed.
        if (b.which == 66 && b.ctrlKey) {
            $(".addition").focus();
        }


        var $old = $('#keySelect');
        var $new;

        //"left" and Ctrl pressed.
        if (b.which == 37 && b.ctrlKey) {
            Expanded = false;
            $old.click();
            $new = $old.next("a");
            $old.removeAttr("id", 'keySelect');
            $new.attr("id", 'keySelect');
        }
        //"up" and Ctrl pressed.
        if (b.which == 38 && b.ctrlKey) {
            if (Expanded) {
                $new = $old.parent().prev("li").children("a")
            }
            else {
                $new = $old.prev().prev();
            }
            $(".sb_filter.recent").prev().focus();
            $old.removeAttr("id", 'keySelect');
            $new.attr("id", 'keySelect');
        }
        //"right" and Ctrl pressed.
        if (b.which == 39 && b.ctrlKey) {
            Expanded = true;
            $old.click();
            $new = $old.next("a");
            $old.removeAttr("id", 'keySelect');
            $new.attr("id", 'keySelect');
        }
        //"down" and Ctrl pressed.
        if (b.which == 40 && b.ctrlKey) {
            if (Expanded) {
                $new = $old.parent().next("li").children("a")
            }
            else {
                $new = $old.next().next();
            }
            $old.removeAttr("id", 'keySelect');
            $new.attr("id", 'keySelect');

        }

        //"down" and Ctrl pressed.
        if (b.which == 13 && b.ctrlKey) {
            $old.click();
        }



        if (b.which == 39 || b.which == 9 || b.keycode == 9) {

            //This allows for the NOT Search.
            if (a.find(".addition").val().indexOf("text:") > -1) {
                var d = a.find(".addition").val().split(":")[1];
                var e = a.find(".addition").val().replace("text:" + d, "");
                a.find(".addition").val(e);
                strike = d.replace(d.split(" NOT ")[1], '<span class="highlight" style="color:red;text-decoration:line-through">' + d.split(" NOT ")[1] + "</span>");

                if (d.indexOf("NOT") > -1) {
                    strike = d.replace(d.split(" NOT ")[1], '<span class="highlight" style="color:red;text-decoration:line-through">' + d.split(" NOT ")[1] + "</span>");
                    a.find(".boxme").prepend('<li class="token-input-token-facebook"><span style="background: url(\'images/text.gif\') no-repeat center center;padding: 0px 11px;" class="booleanOperation"></span><p class="text">' + strike + '</p><span class="token-input-delete-token-facebook remove">×</span></li>')
                } else {
                    a.find(".boxme").prepend('<li class="token-input-token-facebook"><span style="background: url(\'images/text.gif\') no-repeat center center;padding: 0px 11px;" class="booleanOperation"></span><p class="text">' + d + '</p><span class="token-input-delete-token-facebook remove">×</span></li>')
                }
                $(".remove").live("click", function () {
                    $(this).parents("li:first").remove();
                    a.find(".addition").focus();
                });
            }

            retrieveFilters();
        }

        if (b.which == 59 || b.which == 186 || b.keyCode == 186 || b.which == 58) {

            autoSuggestDate("start");
            autoSuggest("field", "GetFields", "");
            autoSuggest("location", "GetBuckets", "");
            autoSuggest("sort", "GetFields", "");
            autoSuggest("recent", "GetRecent", "");
            autoSuggest("language", "GetLanguages", "");
            autoSuggest("site", "GetSites", "");
            autoSuggest("template", "GetTemp", "");
            autoSuggestDate("end");
        }
    });

    function MoveToPage(a) {
        $(this).addClass("pageClickLoad");
        if ((!$('.addition').val().replace(/\s/g, '').length && $('.boxme').children('li').length <= 1) || (($('.boxme').children('li').length == 2) && $('.boxme').children('li')[0].innerHTML.indexOf('p class="end type"') > 0) || $('.addition').val() == "Search for an Item") {
            // $('.addition').removeAttr('disabled');
            if (!($.browser.msie)) {

                $('.boxme').stop().css("background-color", "#EE0000").animate({ backgroundColor: "#FFFFFF" }, 3000);
            }

            $(this).removeClass("pageClickLoad");
        } else {
            $(".sb_down").addClass("sb_up").removeClass("sb_down").andSelf().find(".sb_dropdown").hide();
            $('.content').css({
                'opacity': 1.0
            });

            $("#ajaxBusy").css({
                display: "block"
            });

            var p = buildQuery();
            retrieveFilters();

            if (CurrentView != "list" && CurrentView != "grid") {
                pageNumber = a;
                runQuery(p, pageNumber, h, g);
            }
            else if ($(".grid").hasClass("active")) {
                pageNumber = a;
                runQuery(p, pageNumber, h, i);
                runFacet(p, pageNumber, meme, g);

                $(".navAlpha").html("");
                $(".slide-out-div").html("");
                $(".slide-out-div").prepend('<div id="ajaxBusyFacet"><p><img src="images/loading.gif"></p><p>Loading Facets...</p></div>');
                $("#ajaxBusyFacet").css({
                    margin: "0px auto",
                    width: "44px"
                });

            } else {
                pageNumber = a;
                runQuery(p, pageNumber, c, g);
            }
        }

        $('html, body').animate({ scrollTop: 0 }, 'slow');

    }

    /* This will fade the dropdown menu away once you lose focus on it (blur) */
    $(".sb_dropdown").bind("mouseleave",
        function () {
            $(".sb_dropdown").fadeOut(2500,

                function () {
                    a.find(".sb_up").addClass("sb_down").removeClass("sb_up");
                }
            );
        }
    );

    /* This will stop the fading away of the drop down menu once you have lost focus on it */
    $(".sb_dropdown").live("mouseenter",
        function () {
            if ($(".sb_dropdown").is(':animated')) {
                $(".sb_dropdown").stop(true, true);
            }
            $(".sb_dropdown").show();
        }
    );

    a.find(".sb_dropdown").find('label[for="all"]').prev().bind("click",
        function () {
            $(this).parent().siblings().find(":checkbox").attr("checked", this.checked).attr("disabled", this.checked);
        }
    );

    /* This will hide the loading bar once an item has been loaded */
    $("#loadingSection").prepend('<div id="ajaxBusy"><p><img src="images/loading.gif"></p></div>');
    $("#ajaxBusy").css(
        {
            padding: "0px 122px 0px 0px",
            display: "none",
            margin: "0px auto",
            width: "24px"
        }
    );

    /* This will change the image and the sort direction of the sort filter */
    $('.sortDirection').live("click", function () {
        $(this).toggle
        (
            function () {
                $(this).css("background-image", "url(../ItemBuckets/images/sortdesc.gif)");
                $(this).addClass("desc");
                $(this).removeClass("asc");
            },
            function () {
                $(this).css("background-image", "url(../ItemBuckets/images/sort.gif)");
                $(this).addClass("asc");
                $(this).removeClass("desc");
            }
        );
    });

    $(".boxme").watch('width,height', function () {

        if ($.browser.msie) {
            $(".sb_clear").css("padding-bottom", parseFloat($(this).height() - 10));
            $(".sb_down").css("padding-bottom", parseFloat($(this).height() - 8));
            $(".sb_up").css("padding-bottom", parseFloat($(this).height() - 8));
            $(".sb_search").css("padding-bottom", parseFloat($(this).height() - 10));
        }
        else {
            if (parseFloat($(this).height()) >= 150) {

            $(".token-input-token-facebook").each(function () {
                var $this = $(this);
                $.data(this, 'css', { width: $this.css('width')
                });
            });


                $(".token-input-token-facebook").animate({ width: "28px" });
                $(".token-input-token-facebook").live("click", function () {
                    $(this).toggle(
                        function () {
                            var orig = $.data(this, 'css');
                            $(this).animate({ width: orig.width });
                        },
                        function () {
                            $(this).animate({ width: "28px" });
                        });
                });
            }
        }
    });

    /* This will change the image and the Boolean Operation of the text filter from SHOULD to NOT */
    $(".booleanOperation").live("click", function () {
        $(this).toggle
        (
            function () {
                $(this).css("background-image", "url(../ItemBuckets/images/not" + $(this).next()[0].className + ".gif)");
                $(this).next("p").text("-" + $(this).next("p").text());
                $(this).addClass("not");
                $(this).removeClass("must");
            },
            function () {
                $(this).css("background-image", "url(../ItemBuckets/images/" + $(this).next()[0].className + ".gif)");
                $(this).next("p").text($(this).next("p").text().replace("-", ""));
                $(this).addClass("must");
                $(this).removeClass("not");
            }
        );
    });

    function establishViews() {
        var a = $("#ui_element");
        var views; //= ["gallery"];
        var defaultViews = ["list", "grid"];

        jQuery.ajax({
            type: "POST",
            url: "/sitecore%20modules/Shell/Sitecore/ItemBuckets/ItemBucket.asmx/GetViews",
            data: "",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (b) {
                views = b.d;
                $.each(views, function (index, filter) {
                    $("#views").append("<a id=\"" + filter.ViewName + "\" class=\"" + filter.ViewName + "\" title=\"" + upperFirstLetter(filter.ViewName) + "\"></a>");
                });

                $.each(views, function (index, filter) {

                    $("." + filter.ViewName).click(function () {
                        CurrentView = filter.ViewName;

                        if ((!$('.addition').val().replace(/\s/g, '').length && $('.boxme').children('li').length <= 1) || (($('.boxme').children('li').length == 2) && $('.boxme').children('li')[0].innerHTML.indexOf('p class="end type"') > 0) || $('.addition').val() == "Search for an Item") { //Add check for "Search for an Item"
                            if (!($.browser.msie)) {

                                $('.boxme').stop().css("background-color", "#EE0000").animate({ backgroundColor: "#FFFFFF" }, 3000);
                            }
                            // $('.addition').removeAttr('disabled');
                        } else {
                            a.find(".sb_down").addClass("sb_up").removeClass("sb_down").andSelf().find(".sb_dropdown").hide();
                            pageNumber = 0;

                            $.each(defaultViews, function (subIndex, subfilter) {
                                $("." + subfilter).removeClass("active");
                            });
                            $("." + filter.ViewName).addClass("active");
                            $('.content').css
                    ({
                        'opacity': 1.0
                    });

                            $("#ajaxBusy").css
                    ({
                        display: "block"
                    });
                            var n = buildQuery();
                            retrieveFilters();
                            runQuery(n, pageNumber, h, i);
                            runFacet(n, pageNumber, meme, g);

                            $(".navAlpha").html("");
                            $(".slide-out-div").html("");
                            $(".slide-out-div").prepend('<div id="ajaxBusyFacet"><p><img src="images/loading.gif"></p><p>Loading Facets...</p></div>');
                            $("#ajaxBusyFacet").css({
                                display: "none",
                                margin: "0px auto",
                                width: "44px"
                            });
                        }
                    });
                });
            }
        });
    }
});

$.fn.watch = function (props, callback, timeout) {
    if (!timeout)
        timeout = 10;
    return this.each(function () {
        var el = $(this),
            func = function () { __check.call(this, el) },
            data = { props: props.split(","),
                func: callback,
                vals: []
            };
        $.each(data.props, function (i) { data.vals[i] = el.css(data.props[i]); });
        el.data(data);
        if (typeof (this.onpropertychange) == "object") {
            el.bind("propertychange", callback);
        } else if ($.browser.mozilla) {
            el.bind("DOMAttrModified", callback);
        } else {
            setInterval(func, timeout);
        }
    });
    function __check(el) {
        var data = el.data(),
            changed = false,
            temp = "";
        for (var i = 0; i < data.props.length; i++) {
            temp = el.css(data.props[i]);
            if (data.vals[i] != temp) {
                data.vals[i] = temp;
                changed = true;
                break;
            }
        }
        if (changed && data.func) {
            data.func.call(el, data);
        }
    }
}