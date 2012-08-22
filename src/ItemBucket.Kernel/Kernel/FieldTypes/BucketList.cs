namespace Sitecore.ItemBucket.Kernel.FieldTypes
{
    using System.Collections;
    using System.Linq;
    using System.Text;
    using System.Web.UI;

    using Sitecore.Collections;
    using Sitecore.Data.Items;
    using Sitecore.Data.Query;
    using Sitecore.Diagnostics;
    using Sitecore.Globalization;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;
    using Sitecore.ItemBucket.Kernel.Managers;
    using Sitecore.ItemBucket.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Resources;
    using Sitecore.Shell.Applications.ContentEditor;
    using System;

    /// <summary>
    /// Bucket List Control
    /// </summary>
    public class BucketList : MultilistEx
    {
        private int pageNumber = 1;

        private string filter = string.Empty;

        /// <summary>
        /// Get Items to be loaded when the Control is loaded on the item
        /// </summary>
        /// <param name="current">
        /// The current.
        /// </param>
        /// <returns>
        /// Array of Item
        /// </returns>
        protected override Item[] GetItems(Item current)
        {
            Assert.ArgumentNotNull(current, "current");
            var values = StringUtil.GetNameValues(Source, '=', '&');
            var refinements = new SafeDictionary<string>();
            if (values["FieldsFilter"] != null)
            {
                var splittedFields = StringUtil.GetNameValues(values["FieldsFilter"], ':', ',');
                foreach (string key in splittedFields.Keys)
                {
                    refinements.Add(key, splittedFields[key]);
                }
            }

            var locationFilter = values["StartSearchLocation"];
            locationFilter = MakeFilterQuerable(locationFilter);

            var templateFilter = values["TemplateFilter"];
            templateFilter = MakeTemplateFilterQuerable(templateFilter);

            var pageSize = values["PageSize"];
            var searchParam = new DateRangeSearchParam
            {
                Refinements = refinements,
                LocationIds = locationFilter.IsNullOrEmpty() ? Sitecore.Context.ContentDatabase.GetItem(this.ItemID).GetParentBucketItemOrRootOrSelf().ID.ToString() : locationFilter,
                TemplateIds = templateFilter,
                FullTextQuery = values["FullTextQuery"],
                Language = values["Language"],
                PageSize = pageSize.IsEmpty() ? 10 : int.Parse(pageSize),
                PageNumber = this.pageNumber,
                SortByField = values["SortField"],
                SortDirection = values["SortDirection"]
            };

            this.filter = "&location=" + (locationFilter.IsNullOrEmpty() ? Sitecore.Context.ContentDatabase.GetItem(this.ItemID).GetParentBucketItemOrRootOrSelf().ID.ToString() : locationFilter) +
                     "&filterText=" + values["FullTextQuery"] +
                     "&language=" + values["Language"] +
                     "&pageSize=" + (pageSize.IsEmpty() ? 10 : int.Parse(pageSize)) +
                     "&sort=" + values["SortField"];

            if (values["TemplateFilter"].IsNotNull())
            {
                this.filter += "&template=" + templateFilter;
            }

            using (var searcher = new IndexSearcher(Constants.Index.Name))
            {
                var keyValuePair = searcher.GetItems(searchParam);
                var items = keyValuePair.Value;
                this.pageNumber = keyValuePair.Key / searchParam.PageSize;
                if (this.pageNumber <= 0)
                {
                    this.pageNumber = 1;
                }
                return items.Select(sitecoreItem => sitecoreItem.GetItem()).Where(i => i != null).ToArray();
            }
        }

        private string MakeTemplateFilterQuerable(string templateFilter)
        {
            if (templateFilter.IsNotNull())
            {
                if (templateFilter.StartsWith("query:"))
                {
                    templateFilter = templateFilter.Replace("->", "=");
                    Item[] itemArray;
                    var query = templateFilter.Substring(6);
                    var flag = query.StartsWith("fast:");
                    Opcode opcode = null;
                    if (!flag)
                    {
                        QueryParser.Parse(query);
                    }

                    if (flag || (opcode is Root))
                    {
                        itemArray = Sitecore.Context.ContentDatabase.GetItem(this.ItemID).Database.SelectItems(query);
                    }
                    else
                    {
                        itemArray = Sitecore.Context.ContentDatabase.GetItem(this.ItemID).Axes.SelectItems(query);
                    }

                    templateFilter = string.Empty;
                    templateFilter = itemArray.Aggregate(templateFilter, (current1, item) => current1 + item.ID.ToString());
                }
            }
            return templateFilter;
        }

        private string MakeFilterQuerable(string locationFilter)
        {
            if (locationFilter.IsNotNull())
            {
                if (locationFilter.StartsWith("query:"))
                {
                    locationFilter = locationFilter.Replace("->", "=");
                    Item itemArray;
                    var query = locationFilter.Substring(6);
                    var flag = query.StartsWith("fast:");
                    Opcode opcode = null;
                    if (!flag)
                    {
                        QueryParser.Parse(query);
                    }

                    if (flag || (opcode is Root))
                    {
                        itemArray = Sitecore.Context.ContentDatabase.GetItem(this.ItemID).Database.SelectSingleItem(query);
                    }
                    else
                    {
                        itemArray = Sitecore.Context.ContentDatabase.GetItem(this.ItemID).Axes.SelectSingleItem(query);
                    }

                    locationFilter = itemArray.ID.ToString();
                }
            }
            return locationFilter;
        }

        #region Protected Methods



        #endregion

        #region Private methods

        /// <summary>
        /// Render Buttons
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        /// <param name="icon">
        /// The icon.
        /// </param>
        /// <param name="click">
        /// The click.
        /// </param>
        private void RenderButton(HtmlTextWriter output, string icon, string click)
        {
            Assert.ArgumentNotNull(output, "output");
            Assert.ArgumentNotNull(icon, "icon");
            Assert.ArgumentNotNull(click, "click");
            var builder = new ImageBuilder
                              {
                                  Src = icon,
                                  Width = 0x10,
                                  Height = 0x10,
                                  Margin = "2px",
                                  ID = icon.Contains("right") ? "btnRight" + ClientID : "btnLeft" + ClientID
                              };

            if (!this.ReadOnly)
            {
                builder.OnClick = click;
            }

            output.Write(builder.ToString());
        }

        /// <summary>
        /// Handle Client Side Events
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        [Obsolete]
        public override void HandleMessage(Web.UI.Sheer.Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            base.HandleMessage(message);
            if (message["id"] == this.ID)
            {
                switch (message.Name)
                {
                    case "contentmultilist:nextpage":
                        this.pageNumber++;
                        return;

                    case "contentmultilist:previouspage":
                        this.pageNumber--;
                        return;
                }
            }
        }

        /// <summary>
        /// Renders the supporting javascript
        /// </summary>
        /// <param name="output">The writer.</param>
        private void RenderScript(HtmlTextWriter output)
        {
            var script = @" var pageNumberCount" + ClientID + @" = 1;
                            $('pageNumber" + ClientID + @"').innerHTML = pageNumberCount" + ClientID + @" + ' of ' + '" + this.pageNumber + @"';
                            function applyMultilistFilter" + ClientID + @"(id) {
                                 pageNumberCount" + ClientID + @" = 1;    
                                $('pageNumber" + ClientID + @"').innerHTML = pageNumberCount" + ClientID + @" + ' of ' + '" + this.pageNumber + @"';

                                    var multilist" + ClientID + @" = document.getElementById(id + '_unselected');
                                    var filterBox" + ClientID + @" = document.getElementById('filterBox' + id);
                                    var multilistValues" + ClientID + @" = document.getElementById('multilistValues' + id).value.split(',');
                                    var savedStr" + ClientID + @" = filterBox" + ClientID + @".value;
                           multilist" + ClientID + @".style.backgroundImage = ""url('/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/load.gif')"";
                    multilist" + ClientID + @".style.backgroundPosition = ""50%"";
multilist" + ClientID + @".style.backgroundRepeat = ""no-repeat"";
                                        function ajaxRequest" + ClientID + @"(){
                                            var activexmodes=[""Msxml2.XMLHTTP"", ""Microsoft.XMLHTTP""] //activeX versions to check for in IE
                                            if (window.ActiveXObject){ //Test for support for ActiveXObject in IE first (as XMLHttpRequest in IE7 is broken)
                                            for (var i=0; i<activexmodes.length; i++){
                                            try{
                                            return new ActiveXObject(activexmodes[i])
                                            }
                                            catch(e){
                                            //suppress error
                                            }
                                            }
                                            }
                                            else if (window.XMLHttpRequest) // if Mozilla, Safari etc
                                            return new XMLHttpRequest()
                                            else
                                            return false
                                        }

                                            var mygetrequest" + ClientID + @"=new ajaxRequest" + ClientID + @"()
                                            mygetrequest" + ClientID + @".onreadystatechange=function(){
                                            if (mygetrequest" + ClientID + @".readyState==4){
                                            if (mygetrequest" + ClientID + @".status==200 || window.location.href.indexOf(""http"")==-1){
                                            var responseParsed" + ClientID + @" = eval('(' + mygetrequest" + ClientID + @".responseText + ')');
                                            multilist" + ClientID + @".options.length = 0;
  multilist" + ClientID + @".style.background = ""window"";
                                            for (i = 0; i < responseParsed" + ClientID + @".items.length; i++) {
                                            multilist" + ClientID + @".options[multilist" + ClientID + @".options.length] = new Option(" + JavaScriptOutputString(ClientID) + ", responseParsed" + ClientID + @".items[i].ItemId);
                                                }

$('pageNumber" + ClientID + @"').innerHTML = pageNumberCount" + ClientID + @" + ' of ' +   responseParsed" + ClientID + @".PageNumbers;
                                            }
                                            else{
                                            
                                            }
                                            }
                                        }

                                            mygetrequest" + ClientID + @".open(""GET"", ""/sitecore%20modules/Shell/Sitecore/ItemBuckets/Services/Search.ashx?fromBucketListField="" + savedStr" + ClientID + @" + '" + this.filter + @"', true)
                                                        mygetrequest" + ClientID + @".send(null);
                                            }
                                            function onFilterFocus" + ClientID + @"(filterBox) {
                                                if (filterBox.value == 'Type here to search') {
                                                    filterBox.value = '';
                                                    filterBox.style.color = 'black';
                                                }
                                                else {
                                                    filterBox.select();
                                                }
                                            }

                                            function onFilterBlur" + ClientID + @"(filterBox) {
                                                if (filterBox.value == '') {
                                                    filterBox.value = 'Type here to search';
                                                    filterBox.style.color = 'gray';
                                                }

                                            }

                                            function multilistValuesMoveRight" + ClientID + @"(id, allOptions) {
                                                          var all = scForm.browser.getControl(id + '_unselected');
                                                          var selected = scForm.browser.getControl(id + '_selected');
                                                          var multilistValues = document.getElementById('multilistValues'+id);
                                                          for(var n = 0; n < all.options.length; n++) {
                                                            var option = all.options[n];
                                                            if (option.selected || allOptions == true) {
                                                              var opt = option.innerHTML + ',' + option.value + ',';
                                                              multilistValues.value = multilistValues.value.replace(opt, '')
                                                            }
                                                          }
                                                        }

                                            function multilistValuesMoveLeft" + ClientID + @"(id, allOptions) {
                                                          var all = scForm.browser.getControl(id + '_unselected');
                                                          var selected = scForm.browser.getControl(id + '_selected');
                                                          var multilistValues = document.getElementById('multilistValues'+id);
                                                          for(var n = 0; n < selected.options.length; n++) {
                                                            var option = selected.options[n];
                                                            if (option.selected || allOptions == true) {
                                                              var opt = option.innerHTML + ',' + option.value + ',';
                                                              multilistValues.value += opt;
                                                            }
                                                          }
                                                        }
                                            $('filterBox" +
                                            ClientID + @"').observe('focus', function() { onFilterFocus" + ClientID + @"($('filterBox" + ClientID +
                                            @"')) } );
                                            $('filterBox" + ClientID +
                                            @"').observe('blur', function() { onFilterBlur" + ClientID + @"($('filterBox" + ClientID +
                                            @"')) } );
                

                                                    //setup before functions
                                                    var typingTimer" + ClientID +
                                                                    @";                //timer identifier
                                                    var doneTypingInterval" + ClientID +
                                                                    @" = 2000;  //time in ms, 5 second for example

                                                    $('filterBox" + ClientID +
                                                                    @"').observe('keyup', function() {  
                                                    typingTimer" + ClientID +
                                                                    @" = setTimeout(doneTyping" + ClientID +
                                                                    @", doneTypingInterval" + ClientID +
                                                                    @"); 
                                                    } );

                                                    $('filterBox" + ClientID +
                                                                    @"').observe('keydown', function() {  
                                                    clearTimeout(typingTimer" + ClientID +
                                                                    @");
                                                    } );



                                                    function doneTyping" + ClientID +
                                                                    @" () {
                                                      applyMultilistFilter" + ClientID + @"('" + ClientID +
                                                                    @"')
                                                    }









                                                      $('next" + ClientID + @"').observe('click', function() {



                                                     pageNumberCount" + ClientID + @" = pageNumberCount" + ClientID + @" + 1;    
                                                    var filterBox" + ClientID + @" = document.getElementById('filterBox' + '" + ClientID + @"');
                                                                               if (filterBox" + ClientID + @".value == 'Type here to search') {
                                                    filterBox" + ClientID + @".value =  '*All*';
                                                    }
                                                                                var savedStr" + ClientID + @" = filterBox" + ClientID + @".value;


                                                      var multilist" + ClientID + @" = document.getElementById('" + ClientID + @"' + '_unselected');

                             multilist" + ClientID + @".style.backgroundImage = ""url('/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/load.gif')"";
                    multilist" + ClientID + @".style.backgroundPosition = ""50%"";
multilist" + ClientID + @".style.backgroundRepeat = ""no-repeat"";
                                                                                var multilistValues" + ClientID + @" = document.getElementById('multilistValues' + '" + ClientID + @"').value.split(',');
                         

                                                    function ajaxRequest" + ClientID + @"(){
                                                     var activexmodes=[""Msxml2.XMLHTTP"", ""Microsoft.XMLHTTP""] //activeX versions to check for in IE
                                                     if (window.ActiveXObject){ //Test for support for ActiveXObject in IE first (as XMLHttpRequest in IE7 is broken)
                                                      for (var i=0; i<activexmodes.length; i++){
                                                       try{
                                                        return new ActiveXObject(activexmodes[i])
                                                       }
                                                       catch(e){
                                                        //suppress error
                                                       }
                                                      }
                                                     }
                                                     else if (window.XMLHttpRequest) // if Mozilla, Safari etc
                                                      return new XMLHttpRequest()
                                                     else
                                                      return false
                                                    }

                                                            var mygetrequest" + ClientID + @"=new ajaxRequest" + ClientID + @"()
                                                    mygetrequest" + ClientID + @".onreadystatechange=function(){
                                                     if (mygetrequest" + ClientID + @".readyState==4){
                                                      if (mygetrequest" + ClientID + @".status==200 || window.location.href.indexOf(""http"")==-1){
                                                      var responseParsed" + ClientID + @" = eval('(' + mygetrequest" + ClientID + @".responseText + ')');
                                                     multilist" + ClientID + @".options.length = 0;
  multilist" + ClientID + @".style.background = ""window"";
                                                     for (i = 0; i < responseParsed" + ClientID + @".items.length; i++) {

                                                        multilist" + ClientID + @".options[multilist" + ClientID + @".options.length] = new Option(" + JavaScriptOutputString(ClientID) + ", responseParsed" + ClientID + @".items[i].ItemId);
                                                    }
$('pageNumber" + ClientID + @"').innerHTML = pageNumberCount" + ClientID + @" + ' of ' +   responseParsed" + ClientID + @".PageNumbers;

                                                      }
                                                      else{
                                                 
                                                      }
                                                     }
                                                    }

  
                                                $('pageNumber" + ClientID + @"').innerHTML = pageNumberCount" + ClientID + @";
                                                mygetrequest" + ClientID + @".open(""GET"", ""/sitecore%20modules/Shell/Sitecore/ItemBuckets/Services/Search.ashx?fromBucketListField="" + savedStr" + ClientID + @" + '" + this.filter + @"&pageNumber=' + pageNumberCount" + ClientID + @", true)
                                                            mygetrequest" + ClientID + @".send(null);
  

                                                } );




$('refresh" + ClientID + @"').observe('click', function() {



                                                     pageNumberCount" + ClientID + @" = pageNumberCount" + ClientID + @";    
                                                    var filterBox" + ClientID + @" = document.getElementById('filterBox' + '" + ClientID + @"');
                                                                               if (filterBox" + ClientID + @".value == 'Type here to search') {
                                                    filterBox" + ClientID + @".value =  '*All*';
                                                    }
                                                                                var savedStr" + ClientID + @" = filterBox" + ClientID + @".value;


                                                      var multilist" + ClientID + @" = document.getElementById('" + ClientID + @"' + '_unselected');

                             multilist" + ClientID + @".style.backgroundImage = ""url('/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/load.gif')"";
                    multilist" + ClientID + @".style.backgroundPosition = ""50%"";
multilist" + ClientID + @".style.backgroundRepeat = ""no-repeat"";
                                                                                var multilistValues" + ClientID + @" = document.getElementById('multilistValues' + '" + ClientID + @"').value.split(',');
                         

                                                    function ajaxRequest" + ClientID + @"(){
                                                     var activexmodes=[""Msxml2.XMLHTTP"", ""Microsoft.XMLHTTP""] //activeX versions to check for in IE
                                                     if (window.ActiveXObject){ //Test for support for ActiveXObject in IE first (as XMLHttpRequest in IE7 is broken)
                                                      for (var i=0; i<activexmodes.length; i++){
                                                       try{
                                                        return new ActiveXObject(activexmodes[i])
                                                       }
                                                       catch(e){
                                                        //suppress error
                                                       }
                                                      }
                                                     }
                                                     else if (window.XMLHttpRequest) // if Mozilla, Safari etc
                                                      return new XMLHttpRequest()
                                                     else
                                                      return false
                                                    }

                                                            var mygetrequest" + ClientID + @"=new ajaxRequest" + ClientID + @"()
                                                    mygetrequest" + ClientID + @".onreadystatechange=function(){
                                                     if (mygetrequest" + ClientID + @".readyState==4){
                                                      if (mygetrequest" + ClientID + @".status==200 || window.location.href.indexOf(""http"")==-1){
                                                      var responseParsed" + ClientID + @" = eval('(' + mygetrequest" + ClientID + @".responseText + ')');
                                                     multilist" + ClientID + @".options.length = 0;
  multilist" + ClientID + @".style.background = ""window"";
                                                     for (i = 0; i < responseParsed" + ClientID + @".items.length; i++) {

                                                        multilist" + ClientID + @".options[multilist" + ClientID + @".options.length] = new Option(" + JavaScriptOutputString(ClientID) + ", responseParsed" + ClientID + @".items[i].ItemId);
                                                    }
$('pageNumber" + ClientID + @"').innerHTML = pageNumberCount" + ClientID + @" + ' of ' +   responseParsed" + ClientID + @".PageNumbers;

                                                      }
                                                      else{
                                                 
                                                      }
                                                     }
                                                    }

  
                                                $('pageNumber" + ClientID + @"').innerHTML = pageNumberCount" + ClientID + @";
                                                mygetrequest" + ClientID + @".open(""GET"", ""/sitecore%20modules/Shell/Sitecore/ItemBuckets/Services/Search.ashx?fromBucketListField="" + savedStr" + ClientID + @" + '" + this.filter + @"&pageNumber=1'" + @", true)
                                                            mygetrequest" + ClientID + @".send(null);
  

                                                } );

                                                $('prev" + ClientID + @"').observe('click', function() {

                                                if (pageNumberCount" + ClientID + @" > 1) {
                                                pageNumberCount" + ClientID + @" = pageNumberCount" + ClientID + @" - 1; 
                                                }
                                                var filterBox" + ClientID + @" = document.getElementById('filterBox' + '" + ClientID + @"');
                           
                                                                           if (filterBox" + ClientID + @".value == 'Type here to search') {
                                                filterBox" + ClientID + @".value =  '*All*';
                                                }

                                                                            var savedStr" + ClientID + @" = filterBox" + ClientID + @".value;


                                                  var multilist" + ClientID + @" = document.getElementById('" + ClientID + @"' + '_unselected');
                    
                                                                            var multilistValues" + ClientID + @" = document.getElementById('multilistValues' + '" + ClientID + @"').value.split(',');
                          multilist" + ClientID + @".style.backgroundImage = ""url('/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/load.gif')"";
multilist" + ClientID + @".style.backgroundPosition = ""50%"";
multilist" + ClientID + @".style.backgroundRepeat = ""no-repeat"";


                                                function ajaxRequest" + ClientID + @"(){
                                                 var activexmodes=[""Msxml2.XMLHTTP"", ""Microsoft.XMLHTTP""] //activeX versions to check for in IE
                                                 if (window.ActiveXObject){ //Test for support for ActiveXObject in IE first (as XMLHttpRequest in IE7 is broken)
                                                  for (var i=0; i<activexmodes.length; i++){
                                                   try{
                                                    return new ActiveXObject(activexmodes[i])
                                                   }
                                                   catch(e){
                                                    //suppress error
                                                   }
                                                  }
                                                 }
                                                 else if (window.XMLHttpRequest) // if Mozilla, Safari etc
                                                  return new XMLHttpRequest()
                                                 else
                                                  return false
                                                }

                                                        var mygetrequest" + ClientID + @"=new ajaxRequest" + ClientID + @"()
                                                mygetrequest" + ClientID + @".onreadystatechange=function(){
                                                 if (mygetrequest" + ClientID + @".readyState==4){
                                                  if (mygetrequest" + ClientID + @".status==200 || window.location.href.indexOf(""http"")==-1){
                                                  var responseParsed" + ClientID + @" = eval('(' + mygetrequest" + ClientID + @".responseText + ')');
                                                 multilist" + ClientID + @".options.length = 0;
  multilist" + ClientID + @".style.background = ""window"";
                                                 for (i = 0; i < responseParsed" + ClientID + @".items.length; i++) {

                                                    multilist" + ClientID + @".options[multilist" + ClientID + @".options.length] = new Option(" + JavaScriptOutputString(ClientID) + ", responseParsed" + ClientID + @".items[i].ItemId);
                                                }
$('pageNumber" + ClientID + @"').innerHTML = pageNumberCount" + ClientID + @" + ' of ' +  responseParsed" + ClientID + @".PageNumbers;

                                                  }
                                                  else{
                                                  
                                                  }
                                                 }
                                                }
      
                                                $('pageNumber" + ClientID + @"').innerHTML = pageNumberCount" + ClientID + @";

                                                mygetrequest" + ClientID + @".open(""GET"", ""/sitecore%20modules/Shell/Sitecore/ItemBuckets/Services/Search.ashx?fromBucketListField="" + savedStr" + ClientID + @" + '" + this.filter + @"&pageNumber=' + pageNumberCount" + ClientID + @", true)
                                                            mygetrequest" + ClientID + @".send(null);

                                                } );
                                              
                                                $('" + ID +
                                                                "_unselected').observe('dblclick', function() { multilistValuesMoveRight" + ClientID + @"('" + ID +
                                                                @"'); javascript:scContent.multilistMoveRight('" + ID + @"') } );
                                                                $('" + ID +
                                                                "_selected').observe('dblclick', function() { multilistValuesMoveLeft" + ClientID + @"('" + ID +
                                                                @"'); javascript:scContent.multilistMoveLeft('" + ID + @"') } );

                                                                $('btnRight" +
                                                                ID + "').observe('click', function() { multilistValuesMoveRight" + ClientID + @"('" + ID +
                                                                @"'); javascript:scContent.multilistMoveRight('" + ID + @"') } );
                                                                $('btnLeft" +
                                                                ID + "').observe('click', function() { multilistValuesMoveLeft" + ClientID + @"('" + ID +
                                                                @"') ; javascript:scContent.multilistMoveLeft('" + ID + @"') } );";


            script = "<script type='text/javascript' language='javascript'>" + script + "</script>";

            output.Write(script);
        }

        /// <summary>
        /// Render Script to Page
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        protected override void DoRender(HtmlTextWriter output)
        {
            IDictionary dictionary;
            ArrayList list;
            var current = Sitecore.Context.ContentDatabase.GetItem(this.ItemID);
            var items = this.GetItems(current);
            this.GetSelectedItems(items, out list, out dictionary);

            #region Rendering filter box

            var sb = new StringBuilder();
            foreach (var item in (from DictionaryEntry entry in dictionary select entry.Value).OfType<Item>())
            {
                sb.Append(item.DisplayName + ",");
                sb.Append(GetItemValue(item) + ",");
            }

            output.Write("<input type=\"hidden\" width=\"100%\" id=\"multilistValues" + ClientID + "\" value=\"" + sb + "\" style=\"width: 200px;margin-left:3px;\">");

            #endregion

            this.ServerProperties["ID"] = this.ID;
            var str = string.Empty;
            if (this.ReadOnly)
            {
                str = " disabled=\"disabled\"";
            }

            output.Write("<input id=\"" + this.ID + "_Value\" type=\"hidden\" value=\"" + StringUtil.EscapeQuote(this.Value) + "\" />");
            output.Write("<table" + this.GetControlAttributes() + ">");
            output.Write("<tr>");
            output.Write("<td class=\"scContentControlMultilistCaption\" width=\"50%\">" + Translate.Text("All") + "</td>");
            output.Write("<td width=\"20\">" + Images.GetSpacer(20, 1) + "</td>");
            output.Write("<td class=\"scContentControlMultilistCaption\" width=\"50%\">" + Translate.Text("Selected") + "</td>");
            output.Write("<td width=\"20\">" + Images.GetSpacer(20, 1) + "</td>");
            output.Write("</tr>");
            output.Write("<tr>");
            output.Write("<td valign=\"top\" height=\"100%\">");
            var treeViewThing = new TreeList { ID = "DropTreeForBucket" };
            treeViewThing.RenderControl(output);
            output.Write("<input type=\"text\" width=\"100%\" class=\"scIgnoreModified\" style=\"color:gray\" value=\"Type here to search\" id=\"filterBox" + this.ClientID + "\" style=\"width:100%\" " +  (Sitecore.Context.Item.Access.CanWrite() ? "" : "disabled") +">");
            output.Write(@"<span id='prev" + this.ClientID + @"' class='hovertext' style='cursor:pointer;' onMouseOver=""this.style.color='#666'"" onMouseOut=""this.style.color='#000'"">" + " <img width=\"10\" height=\"10\" src=\"/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/right.png\" style=\"margin-top: 1px;\"> prev |" + "</span>");
            output.Write(@"<span id='next" + this.ClientID + @"' class='hovertext' style='cursor:pointer;' onMouseOver=""this.style.color='#666'"" onMouseOut=""this.style.color='#000'""> " + " next <img width=\"10\" height=\"10\" src=\"/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/left.png\" style=\"margin-top: 1px;\">  " + "</span>");
            output.Write(@"<span id='refresh" + this.ClientID + @"' class='hovertext' style='cursor:pointer;' onMouseOver=""this.style.color='#666'"" onMouseOut=""this.style.color='#000'""> " + " refresh <img width=\"10\" height=\"10\" src=\"/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/refresh.png\" style=\"margin-top: 1px;\">  " + "</span>");
            output.Write(@"<span style='padding-left:34px;'><strong>Page Number: </strong></span><span id='pageNumber" + this.ClientID + @"'></span>");
            output.Write("<select id=\"" + this.ID + "_unselected\" class=\"scContentControlMultilistBox\" multiple=\"multiple\" size=\"10\"" + str + " >");
            foreach (DictionaryEntry entry in dictionary)
            {
                var item = entry.Value as Item;
                if (item != null)
                {
                    var outputString = OutputString(item);
                    output.Write("<option value=\"" + this.GetItemValue(item) + "\">" + outputString + "</option>");
                }
            }

            output.Write("</select>");
            output.Write("</td>");
            output.Write("<td valign=\"top\">");
            output.Write("<img class=\"\" height=\"16\" width=\"16\" border=\"0\" alt=\"\" style=\"margin: 2px;\" src=\"/sitecore/shell/themes/standard/Images/blank.png\"/>");
            output.Write("<br />");
            this.RenderButton(output, "Core/16x16/arrow_blue_right.png", string.Empty);
            output.Write("<br />");
            this.RenderButton(output, "Core/16x16/arrow_blue_left.png", string.Empty);
            output.Write("</td>");
            output.Write("<td valign=\"top\" height=\"100%\">");
            output.Write("<select style=\"margin-top:28px\" id=\"" + this.ID + "_selected\" class=\"scContentControlMultilistBox\" multiple=\"multiple\" size=\"10\"" + str + ">");
            for (int i = 0; i < list.Count; i++)
            {
                var item3 = list[i] as Item;
                if (item3 != null)
                {
                    var outputString = OutputString(item3);
                    output.Write("<option value=\"" + this.GetItemValue(item3) + "\">" + outputString + "</option>");
                }
                else
                {
                    var path = list[i] as string;
                    if (path != null)
                    {
                        string str3;
                        var item4 = Sitecore.Context.ContentDatabase.GetItem(path);
                        if (item4 != null)
                        {
                            var outputString = OutputString(item4);
                            str3 = outputString;
                        }
                        else
                        {
                            str3 = path + ' ' + Translate.Text("[Item not found]");
                        }

                        output.Write("<option value=\"" + path + "\">" + str3 + "</option>");
                    }
                }
            }

            output.Write("</select>");
            output.Write("</td>");
            output.Write("<td valign=\"top\">");
            output.Write("<img class=\"\" height=\"16\" width=\"16\" border=\"0\" alt=\"\" style=\"margin: 2px;\" src=\"/sitecore/shell/themes/standard/Images/blank.png\"/>");
            output.Write("<br />");
            this.RenderButton(output, "Core/16x16/arrow_blue_up.png", "javascript:scContent.multilistMoveUp('" + this.ID + "')");
            output.Write("<br />");
            this.RenderButton(output, "Core/16x16/arrow_blue_down.png", "javascript:scContent.multilistMoveDown('" + this.ID + "')");
            output.Write("</td>");
            output.Write("</tr>");
            output.Write("<div style=\"border:1px solid #999999;font:8pt tahoma;display:none;padding:2px;margin:4px 0px 4px 0px;height:14px\" id=\"" + this.ID + "_all_help\"></div>");
            output.Write("<div style=\"border:1px solid #999999;font:8pt tahoma;display:none;padding:2px;margin:4px 0px 4px 0px;height:14px\" id=\"" + this.ID + "_selected_help\"></div>");
            output.Write("</table>");
            this.RenderScript(output);
        }

        public virtual string OutputString(Item item4)
        {
            return item4.DisplayName + " (" + item4.TemplateName + " - " + item4.GetParentBucketItemOrParent().Name + ")";
        }

        public virtual string JavaScriptOutputString(string clientId)
        {
            return "responseParsed" + clientId + @".items[i].Name + ' (' + responseParsed" + clientId + @".items[i].TemplateName + ' - ' + responseParsed" + clientId + @".items[i].Bucket + ')'";
        }

        #endregion
    }
}
