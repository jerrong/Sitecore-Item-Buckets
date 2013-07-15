// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BucketList.cs" company="Sitecore A/S">
//   Copyright (C) 2013 by Sitecore A/S
// </copyright>
// <summary>
//   Bucket List Control
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Sitecore.ItemBucket.Kernel.FieldTypes
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Collections.Specialized;
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
  using Sitecore.Web.UI.Sheer;

  /// <summary>
  ///   Bucket List Control
  /// </summary>
  public class BucketList : MultilistEx
  {
    /// <summary>
    /// The page number
    /// </summary>
    private int pageNumber = 1;

    /// <summary>
    /// The filter
    /// </summary>
    private string filter = string.Empty;

    /// <summary>
    /// Outputs the string.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>The item's display name, template's name and parent bucket item's name.</returns>
    public virtual string OutputString(Item item)
    {
      return item.DisplayName + " (" + item.TemplateName + " - " + item.GetParentBucketItemOrParent().Name + ")";
    }

    /// <summary>
    /// Gets the output string with java script.
    /// </summary>
    /// <param name="clientId">The client id.</param>
    /// <returns>The java script.</returns>
    public virtual string JavaScriptOutputString(string clientId)
    {
      return "responseParsed" + clientId + @".items[i].Name + ' (' + responseParsed" + clientId
             + @".items[i].TemplateName + ' - ' + responseParsed" + clientId + @".items[i].Bucket + ')'";
    }

    /// <summary>
    /// Handle Client Side Events
    /// </summary>
    /// <param name="message">The message.</param>
    [Obsolete]
    public override void HandleMessage(Message message)
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

    #region Protected Methods

    /// <summary>
    ///   Get Items to be loaded when the Control is loaded on the item
    /// </summary>
    /// <param name="current">
    ///   The current.
    /// </param>
    /// <returns>
    ///   Array of Item
    /// </returns>
    protected override Item[] GetItems(Item current)
    {
      Assert.ArgumentNotNull(current, "current");
      NameValueCollection values = StringUtil.GetNameValues(this.Source, '=', '&');
      var refinements = new SafeDictionary<string>();
      if (values["FieldsFilter"] != null)
      {
        NameValueCollection splittedFields = StringUtil.GetNameValues(values["FieldsFilter"], ':', ',');
        foreach (string key in splittedFields.Keys)
        {
          refinements.Add(key, splittedFields[key]);
        }
      }

      string locationFilter = values["StartSearchLocation"];
      locationFilter = this.MakeFilterQuerable(locationFilter);

      string templateFilter = values["TemplateFilter"];
      templateFilter = this.MakeTemplateFilterQuerable(templateFilter);

      string pageSize = values["PageSize"];
      var searchParam = new DateRangeSearchParam
                          {
                            Refinements = refinements,
                            LocationIds =
                              locationFilter.IsNullOrEmpty()
                                ? Sitecore.Context.ContentDatabase.GetItem(this.ItemID)
                                          .GetParentBucketItemOrRootOrSelf()
                                          .ID.ToString()
                                : locationFilter,
                            TemplateIds = templateFilter,
                            FullTextQuery = values["FullTextQuery"],
                            Language = values["Language"],
                            PageSize = pageSize.IsEmpty() ? 10 : int.Parse(pageSize),
                            PageNumber = this.pageNumber,
                            SortByField = values["SortField"],
                            SortDirection = values["SortDirection"]
                          };

      this.filter = "&location="
                    + (locationFilter.IsNullOrEmpty()
                         ? Sitecore.Context.ContentDatabase.GetItem(this.ItemID)
                                   .GetParentBucketItemOrRootOrSelf()
                                   .ID.ToString()
                         : locationFilter) + "&filterText=" + values["FullTextQuery"] + "&language="
                    + values["Language"] + "&pageSize=" + (pageSize.IsEmpty() ? 10 : int.Parse(pageSize)) + "&sort="
                    + values["SortField"];

      if (values["TemplateFilter"].IsNotNull())
      {
        this.filter += "&template=" + templateFilter;
      }

      using (var searcher = new IndexSearcher(Constants.Index.Name))
      {
        KeyValuePair<int, List<SitecoreItem>> keyValuePair = searcher.GetItems(searchParam);
        List<SitecoreItem> items = keyValuePair.Value;
        this.pageNumber = keyValuePair.Key / searchParam.PageSize;
        if (this.pageNumber <= 0)
        {
          this.pageNumber = 1;
        }

        return items.Select(sitecoreItem => sitecoreItem.GetItem()).Where(i => i != null).ToArray();
      }
    }

    /// <summary>
    /// Render Script to Page
    /// </summary>
    /// <param name="output">The output.</param>
    protected override void DoRender(HtmlTextWriter output)
    {
      IDictionary dictionary;
      ArrayList list;
      Item current = Sitecore.Context.ContentDatabase.GetItem(this.ItemID);
      Item[] items = this.GetItems(current);
      this.GetSelectedItems(items, out list, out dictionary);

      var sb = new StringBuilder();
      foreach (Item item in (from DictionaryEntry entry in dictionary select entry.Value).OfType<Item>())
      {
        sb.Append(item.DisplayName + ",");
        sb.Append(this.GetItemValue(item) + ",");
      }

      output.Write(
        "<input type=\"hidden\" width=\"100%\" id=\"multilistValues" + this.ClientID + "\" value=\"" + sb
        + "\" style=\"width: 200px;margin-left:3px;\">");

      this.ServerProperties["ID"] = this.ID;
      string str = string.Empty;
      if (this.ReadOnly)
      {
        str = " disabled=\"disabled\"";
      }

      output.Write(
        "<input id=\"" + this.ID + "_Value\" type=\"hidden\" value=\"" + StringUtil.EscapeQuote(this.Value) + "\" />");
      output.Write("<table" + this.GetControlAttributes() + ">");
      output.Write("<tr>");
      output.Write("<td class=\"scContentControlMultilistCaption\" width=\"50%\">" + Translate.Text("All") + "</td>");
      output.Write("<td width=\"20\">" + Images.GetSpacer(20, 1) + "</td>");
      output.Write(
        "<td class=\"scContentControlMultilistCaption\" width=\"50%\">" + Translate.Text("Selected") + "</td>");
      output.Write("<td width=\"20\">" + Images.GetSpacer(20, 1) + "</td>");
      output.Write("</tr>");
      output.Write("<tr>");
      output.Write("<td valign=\"top\" height=\"100%\">");
      var treeViewThing = new TreeList { ID = "DropTreeForBucket" };
      treeViewThing.RenderControl(output);
      output.Write(
        "<input type=\"text\" width=\"100%\" class=\"scIgnoreModified\" style=\"color:gray\" value=\"Type here to search\" id=\"filterBox"
        + this.ClientID + "\" style=\"width:100%\" " + (Sitecore.Context.Item.Access.CanWrite() ? string.Empty : "disabled") + ">");
      output.Write(
        @"<span id='prev" + this.ClientID
        + @"' class='hovertext' style='cursor:pointer;' onMouseOver=""this.style.color='#666'"" onMouseOut=""this.style.color='#000'"">"
        + " <img width=\"10\" height=\"10\" src=\"/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/right.png\" style=\"margin-top: 1px;\"> prev |"
        + "</span>");
      output.Write(
        @"<span id='next" + this.ClientID
        + @"' class='hovertext' style='cursor:pointer;' onMouseOver=""this.style.color='#666'"" onMouseOut=""this.style.color='#000'""> "
        + " next <img width=\"10\" height=\"10\" src=\"/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/left.png\" style=\"margin-top: 1px;\">  "
        + "</span>");
      output.Write(
        @"<span id='refresh" + this.ClientID
        + @"' class='hovertext' style='cursor:pointer;display: none;' onMouseOver=""this.style.color='#666'"" onMouseOut=""this.style.color='#000'""> "
        + " refresh <img width=\"10\" height=\"10\" src=\"/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/refresh.png\" style=\"margin-top: 1px; \">  "
        + "</span>");
      output.Write(
        @"<span style='padding-left:34px;'><strong>Page Number: </strong></span><span id='pageNumber" + this.ClientID
        + @"'></span>");
      output.Write(
        "<select id=\"" + this.ID
        + "_unselected\" class=\"scContentControlMultilistBox\" multiple=\"multiple\" size=\"10\"" + str + " >");
      foreach (DictionaryEntry entry in dictionary)
      {
        var item = entry.Value as Item;
        if (item != null)
        {
          string outputString = this.OutputString(item);
          output.Write("<option value=\"" + this.GetItemValue(item) + "\">" + outputString + "</option>");
        }
      }

      output.Write("</select>");
      output.Write("</td>");
      output.Write("<td valign=\"top\">");
      output.Write(
        "<img class=\"\" height=\"16\" width=\"16\" border=\"0\" alt=\"\" style=\"margin: 2px;\" src=\"/sitecore/shell/themes/standard/Images/blank.png\"/>");
      output.Write("<br />");
      this.RenderButton(output, "Core/16x16/arrow_blue_right.png", string.Empty);
      output.Write("<br />");
      this.RenderButton(output, "Core/16x16/arrow_blue_left.png", string.Empty);
      output.Write("</td>");
      output.Write("<td valign=\"top\" height=\"100%\">");
      output.Write(
        "<select style=\"margin-top:28px\" id=\"" + this.ID
        + "_selected\" class=\"scContentControlMultilistBox\" multiple=\"multiple\" size=\"10\"" + str + ">");
      for (int i = 0; i < list.Count; i++)
      {
        var item3 = list[i] as Item;
        if (item3 != null)
        {
          string outputString = this.OutputString(item3);
          output.Write("<option value=\"" + this.GetItemValue(item3) + "\">" + outputString + "</option>");
        }
        else
        {
          var path = list[i] as string;
          if (path != null)
          {
            string str3;
            Item item4 = Sitecore.Context.ContentDatabase.GetItem(path);
            if (item4 != null)
            {
              string outputString = this.OutputString(item4);
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
      output.Write(
        "<img class=\"\" height=\"16\" width=\"16\" border=\"0\" alt=\"\" style=\"margin: 2px;\" src=\"/sitecore/shell/themes/standard/Images/blank.png\"/>");
      output.Write("<br />");
      this.RenderButton(
        output, "Core/16x16/arrow_blue_up.png", "javascript:scContent.multilistMoveUp('" + this.ID + "')");
      output.Write("<br />");
      this.RenderButton(
        output, "Core/16x16/arrow_blue_down.png", "javascript:scContent.multilistMoveDown('" + this.ID + "')");
      output.Write("</td>");
      output.Write("</tr>");
      output.Write(
        "<div style=\"border:1px solid #999999;font:8pt tahoma;display:none;padding:2px;margin:4px 0px 4px 0px;height:14px\" id=\""
        + this.ID + "_all_help\"></div>");
      output.Write(
        "<div style=\"border:1px solid #999999;font:8pt tahoma;display:none;padding:2px;margin:4px 0px 4px 0px;height:14px\" id=\""
        + this.ID + "_selected_help\"></div>");
      output.Write("</table>");
      this.RenderScript(output);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Makes the template filter queryable.
    /// </summary>
    /// <param name="templateFilter">The template filter.</param>
    /// <returns>The queryable template filter.</returns>
    private string MakeTemplateFilterQuerable(string templateFilter)
    {
      if (templateFilter.IsNotNull())
      {
        if (templateFilter.StartsWith("query:"))
        {
          templateFilter = templateFilter.Replace("->", "=");
          string query = templateFilter.Substring(6);
          bool flag = query.StartsWith("fast:");
          
          if (!flag)
          {
            QueryParser.Parse(query);
          }

          Item[] itemArray;
          if (flag)
          {
            itemArray = Sitecore.Context.ContentDatabase.GetItem(this.ItemID).Database.SelectItems(query);
          }
          else
          {
            itemArray = Sitecore.Context.ContentDatabase.GetItem(this.ItemID).Axes.SelectItems(query);
          }

          templateFilter = itemArray.Aggregate(templateFilter, (current1, item) => current1 + item.ID.ToString());
        }
      }

      return templateFilter;
    }

    /// <summary>
    /// Makes the filter queryable.
    /// </summary>
    /// <param name="locationFilter">The location filter.</param>
    /// <returns>The queryable template filter.</returns>
    private string MakeFilterQuerable(string locationFilter)
    {
      if (locationFilter.IsNotNull())
      {
        if (locationFilter.StartsWith("query:"))
        {
          locationFilter = locationFilter.Replace("->", "=");
          string query = locationFilter.Substring(6);
          bool flag = query.StartsWith("fast:");

          if (!flag)
          {
            QueryParser.Parse(query);
          }

          Item itemArray;
          if (flag)
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

    /// <summary>
    /// Renders the button.
    /// </summary>
    /// <param name="output">The output.</param>
    /// <param name="icon">The icon.</param>
    /// <param name="click">The click.</param>
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
                        ID =
                          icon.Contains("right") ? "btnRight" + this.ClientID : "btnLeft" + this.ClientID
                      };

      if (!this.ReadOnly)
      {
        builder.OnClick = click;
      }

      output.Write(builder.ToString());
    }

    /// <summary>
    /// Renders the supporting java script
    /// </summary>
    /// <param name="output">The writer.</param>
    private void RenderScript(HtmlTextWriter output)
    {
      string script = @" var pageNumberCount" + this.ClientID + @" = 1;
                            $('pageNumber" + this.ClientID + @"').innerHTML = pageNumberCount" + this.ClientID
                      + @" + ' of ' + '" + this.pageNumber + @"';
                            function applyMultilistFilter" + this.ClientID + @"(id) {
                                 pageNumberCount" + this.ClientID + @" = 1;    
                                $('pageNumber" + this.ClientID + @"').innerHTML = pageNumberCount" + this.ClientID
                      + @" + ' of ' + '" + this.pageNumber + @"';

                                    var multilist" + this.ClientID + @" = document.getElementById(id + '_unselected');
                                    var filterBox" + this.ClientID + @" = document.getElementById('filterBox' + id);
                                    var multilistValues" + this.ClientID
                      + @" = document.getElementById('multilistValues' + id).value.split(',');
                                    var savedStr" + this.ClientID + @" = filterBox" + this.ClientID + @".value;
                           multilist" + this.ClientID
                      + @".style.backgroundImage = ""url('/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/load.gif')"";
                    multilist" + this.ClientID + @".style.backgroundPosition = ""50%"";
multilist" + this.ClientID + @".style.backgroundRepeat = ""no-repeat"";
                                        function ajaxRequest" + this.ClientID + @"(){
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

                                            var mygetrequest" + this.ClientID + @"=new ajaxRequest" + this.ClientID
                      + @"()
                                            mygetrequest" + this.ClientID + @".onreadystatechange=function(){
                                            if (mygetrequest" + this.ClientID + @".readyState==4){
                                            if (mygetrequest" + this.ClientID
                      + @".status==200 || window.location.href.indexOf(""http"")==-1){
                                            var responseParsed" + this.ClientID + @" = eval('(' + mygetrequest"
                      + this.ClientID + @".responseText + ')');
                                            multilist" + this.ClientID + @".options.length = 0;
  multilist" + this.ClientID + @".style.background = ""window"";
                                            for (i = 0; i < responseParsed" + this.ClientID + @".items.length; i++) {
                                            multilist" + this.ClientID + @".options[multilist" + this.ClientID
                      + @".options.length] = new Option(" + this.JavaScriptOutputString(this.ClientID)
                      + ", responseParsed" + this.ClientID + @".items[i].ItemId);
                                                }

$('pageNumber" + this.ClientID + @"').innerHTML = pageNumberCount" + this.ClientID + @" + ' of ' +   responseParsed"
                      + this.ClientID + @".PageNumbers;
                                            }
                                            else{
                                            
                                            }
                                            }
                                        }

                                            mygetrequest" + this.ClientID
                      + @".open(""GET"", ""/sitecore%20modules/Shell/Sitecore/ItemBuckets/Services/Search.ashx?fromBucketListField="" + savedStr"
                      + this.ClientID + @" + '" + this.filter + @"', true)
                                                        mygetrequest" + this.ClientID + @".send(null);
                                            }
                                            function onFilterFocus" + this.ClientID + @"(filterBox) {
                                                if (filterBox.value == 'Type here to search') {
                                                    filterBox.value = '';
                                                    filterBox.style.color = 'black';
                                                }
                                                else {
                                                    filterBox.select();
                                                }
                                            }

                                            function onFilterBlur" + this.ClientID + @"(filterBox) {
                                                if (filterBox.value == '') {
                                                    filterBox.value = 'Type here to search';
                                                    filterBox.style.color = 'gray';
                                                }

                                            }

                                            function multilistValuesMoveRight" + this.ClientID + @"(id, allOptions) {
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

                                            function multilistValuesMoveLeft" + this.ClientID + @"(id, allOptions) {
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
                                            $('filterBox" + this.ClientID
                      + @"').observe('focus', function() { onFilterFocus" + this.ClientID + @"($('filterBox"
                      + this.ClientID + @"')) } );
                                            $('filterBox" + this.ClientID
                      + @"').observe('blur', function() { onFilterBlur" + this.ClientID + @"($('filterBox"
                      + this.ClientID + @"')) } );
                

                                                    //setup before functions
                                                    var typingTimer" + this.ClientID
                      + @";                //timer identifier
                                                    var doneTypingInterval" + this.ClientID
                      + @" = 2000;  //time in ms, 5 second for example

                                                    $('filterBox" + this.ClientID + @"').observe('keyup', function() {  
                                                    typingTimer" + this.ClientID + @" = setTimeout(doneTyping"
                      + this.ClientID + @", doneTypingInterval" + this.ClientID + @"); 
                                                    } );

                                                    $('filterBox" + this.ClientID
                      + @"').observe('keydown', function() {  
                                                    clearTimeout(typingTimer" + this.ClientID + @");
                                                    } );



                                                    function doneTyping" + this.ClientID + @" () {
                                                      applyMultilistFilter" + this.ClientID + @"('" + this.ClientID
                      + @"')
                                                    }









                                                      $('next" + this.ClientID + @"').observe('click', function() {



                                                     pageNumberCount" + this.ClientID + @" = pageNumberCount"
                      + this.ClientID + @" + 1;    
                                                    var filterBox" + this.ClientID
                      + @" = document.getElementById('filterBox' + '" + this.ClientID + @"');
                                                                               if (filterBox" + this.ClientID
                      + @".value == 'Type here to search') {
                                                    filterBox" + this.ClientID + @".value =  '*All*';
                                                    }
                                                                                var savedStr" + this.ClientID
                      + @" = filterBox" + this.ClientID + @".value;


                                                      var multilist" + this.ClientID + @" = document.getElementById('"
                      + this.ClientID + @"' + '_unselected');

                             multilist" + this.ClientID
                      + @".style.backgroundImage = ""url('/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/load.gif')"";
                    multilist" + this.ClientID + @".style.backgroundPosition = ""50%"";
multilist" + this.ClientID + @".style.backgroundRepeat = ""no-repeat"";
                                                                                var multilistValues" + this.ClientID
                      + @" = document.getElementById('multilistValues' + '" + this.ClientID + @"').value.split(',');
                         

                                                    function ajaxRequest" + this.ClientID + @"(){
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

                                                            var mygetrequest" + this.ClientID + @"=new ajaxRequest"
                      + this.ClientID + @"()
                                                    mygetrequest" + this.ClientID + @".onreadystatechange=function(){
                                                     if (mygetrequest" + this.ClientID + @".readyState==4){
                                                      if (mygetrequest" + this.ClientID
                      + @".status==200 || window.location.href.indexOf(""http"")==-1){
                                                      var responseParsed" + this.ClientID
                      + @" = eval('(' + mygetrequest" + this.ClientID + @".responseText + ')');
                                                     multilist" + this.ClientID + @".options.length = 0;
  multilist" + this.ClientID + @".style.background = ""window"";
                                                     for (i = 0; i < responseParsed" + this.ClientID
                      + @".items.length; i++) {

                                                        multilist" + this.ClientID + @".options[multilist"
                      + this.ClientID + @".options.length] = new Option(" + this.JavaScriptOutputString(this.ClientID)
                      + ", responseParsed" + this.ClientID + @".items[i].ItemId);
                                                    }
$('pageNumber" + this.ClientID + @"').innerHTML = pageNumberCount" + this.ClientID + @" + ' of ' +   responseParsed"
                      + this.ClientID + @".PageNumbers;

                                                      }
                                                      else{
                                                 
                                                      }
                                                     }
                                                    }

  
                                                $('pageNumber" + this.ClientID + @"').innerHTML = pageNumberCount"
                      + this.ClientID + @";
                                                mygetrequest" + this.ClientID
                      + @".open(""GET"", ""/sitecore%20modules/Shell/Sitecore/ItemBuckets/Services/Search.ashx?fromBucketListField="" + savedStr"
                      + this.ClientID + @" + '" + this.filter + @"&pageNumber=' + pageNumberCount" + this.ClientID
                      + @", true)
                                                            mygetrequest" + this.ClientID + @".send(null);
  

                                                } );




$('refresh" + this.ClientID + @"').observe('click', function() {



                                                     pageNumberCount" + this.ClientID + @" = pageNumberCount"
                      + this.ClientID + @";    
                                                    var filterBox" + this.ClientID
                      + @" = document.getElementById('filterBox' + '" + this.ClientID + @"');
                                                                               if (filterBox" + this.ClientID
                      + @".value == 'Type here to search') {
                                                    filterBox" + this.ClientID + @".value =  '*All*';
                                                    }
                                                                                var savedStr" + this.ClientID
                      + @" = filterBox" + this.ClientID + @".value;


                                                      var multilist" + this.ClientID + @" = document.getElementById('"
                      + this.ClientID + @"' + '_unselected');

                             multilist" + this.ClientID
                      + @".style.backgroundImage = ""url('/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/load.gif')"";
                    multilist" + this.ClientID + @".style.backgroundPosition = ""50%"";
multilist" + this.ClientID + @".style.backgroundRepeat = ""no-repeat"";
                                                                                var multilistValues" + this.ClientID
                      + @" = document.getElementById('multilistValues' + '" + this.ClientID + @"').value.split(',');
                         

                                                    function ajaxRequest" + this.ClientID + @"(){
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

                                                            var mygetrequest" + this.ClientID + @"=new ajaxRequest"
                      + this.ClientID + @"()
                                                    mygetrequest" + this.ClientID + @".onreadystatechange=function(){
                                                     if (mygetrequest" + this.ClientID + @".readyState==4){
                                                      if (mygetrequest" + this.ClientID
                      + @".status==200 || window.location.href.indexOf(""http"")==-1){
                                                      var responseParsed" + this.ClientID
                      + @" = eval('(' + mygetrequest" + this.ClientID + @".responseText + ')');
                                                     multilist" + this.ClientID + @".options.length = 0;
  multilist" + this.ClientID + @".style.background = ""window"";
                                                     for (i = 0; i < responseParsed" + this.ClientID
                      + @".items.length; i++) {

                                                        multilist" + this.ClientID + @".options[multilist"
                      + this.ClientID + @".options.length] = new Option(" + this.JavaScriptOutputString(this.ClientID)
                      + ", responseParsed" + this.ClientID + @".items[i].ItemId);
                                                    }
$('pageNumber" + this.ClientID + @"').innerHTML = pageNumberCount" + this.ClientID + @" + ' of ' +   responseParsed"
                      + this.ClientID + @".PageNumbers;

                                                      }
                                                      else{
                                                 
                                                      }
                                                     }
                                                    }

  
                                                $('pageNumber" + this.ClientID + @"').innerHTML = pageNumberCount"
                      + this.ClientID + @";
                                                mygetrequest" + this.ClientID
                      + @".open(""GET"", ""/sitecore%20modules/Shell/Sitecore/ItemBuckets/Services/Search.ashx?fromBucketListField="" + savedStr"
                      + this.ClientID + @" + '" + this.filter + @"&pageNumber=1'" + @", true)
                                                            mygetrequest" + this.ClientID + @".send(null);
  

                                                } );

                                                $('prev" + this.ClientID + @"').observe('click', function() {

                                                if (pageNumberCount" + this.ClientID + @" > 1) {
                                                pageNumberCount" + this.ClientID + @" = pageNumberCount" + this.ClientID
                      + @" - 1; 
                                                }
                                                var filterBox" + this.ClientID
                      + @" = document.getElementById('filterBox' + '" + this.ClientID + @"');
                           
                                                                           if (filterBox" + this.ClientID
                      + @".value == 'Type here to search') {
                                                filterBox" + this.ClientID + @".value =  '*All*';
                                                }

                                                                            var savedStr" + this.ClientID
                      + @" = filterBox" + this.ClientID + @".value;


                                                  var multilist" + this.ClientID + @" = document.getElementById('"
                      + this.ClientID + @"' + '_unselected');
                    
                                                                            var multilistValues" + this.ClientID
                      + @" = document.getElementById('multilistValues' + '" + this.ClientID + @"').value.split(',');
                          multilist" + this.ClientID
                      + @".style.backgroundImage = ""url('/sitecore%20modules/Shell/Sitecore/ItemBuckets/images/load.gif')"";
multilist" + this.ClientID + @".style.backgroundPosition = ""50%"";
multilist" + this.ClientID + @".style.backgroundRepeat = ""no-repeat"";


                                                function ajaxRequest" + this.ClientID + @"(){
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

                                                        var mygetrequest" + this.ClientID + @"=new ajaxRequest"
                      + this.ClientID + @"()
                                                mygetrequest" + this.ClientID + @".onreadystatechange=function(){
                                                 if (mygetrequest" + this.ClientID + @".readyState==4){
                                                  if (mygetrequest" + this.ClientID
                      + @".status==200 || window.location.href.indexOf(""http"")==-1){
                                                  var responseParsed" + this.ClientID + @" = eval('(' + mygetrequest"
                      + this.ClientID + @".responseText + ')');
                                                 multilist" + this.ClientID + @".options.length = 0;
  multilist" + this.ClientID + @".style.background = ""window"";
                                                 for (i = 0; i < responseParsed" + this.ClientID
                      + @".items.length; i++) {

                                                    multilist" + this.ClientID + @".options[multilist" + this.ClientID
                      + @".options.length] = new Option(" + this.JavaScriptOutputString(this.ClientID)
                      + ", responseParsed" + this.ClientID + @".items[i].ItemId);
                                                }
$('pageNumber" + this.ClientID + @"').innerHTML = pageNumberCount" + this.ClientID + @" + ' of ' +  responseParsed"
                      + this.ClientID + @".PageNumbers;

                                                  }
                                                  else{
                                                  
                                                  }
                                                 }
                                                }
      
                                                $('pageNumber" + this.ClientID + @"').innerHTML = pageNumberCount"
                      + this.ClientID + @";

                                                mygetrequest" + this.ClientID
                      + @".open(""GET"", ""/sitecore%20modules/Shell/Sitecore/ItemBuckets/Services/Search.ashx?fromBucketListField="" + savedStr"
                      + this.ClientID + @" + '" + this.filter + @"&pageNumber=' + pageNumberCount" + this.ClientID
                      + @", true)
                                                            mygetrequest" + this.ClientID + @".send(null);

                                                } );
                                              
                                                $('" + this.ID
                      + "_unselected').observe('dblclick', function() { multilistValuesMoveRight" + this.ClientID
                      + @"('" + this.ID + @"'); javascript:scContent.multilistMoveRight('" + this.ID + @"') } );
                                                                $('" + this.ID
                      + "_selected').observe('dblclick', function() { multilistValuesMoveLeft" + this.ClientID + @"('"
                      + this.ID + @"'); javascript:scContent.multilistMoveLeft('" + this.ID + @"') } );

                                                                $('btnRight" + this.ID
                      + "').observe('click', function() { multilistValuesMoveRight" + this.ClientID + @"('" + this.ID
                      + @"'); javascript:scContent.multilistMoveRight('" + this.ID + @"') } );
                                                                $('btnLeft" + this.ID
                      + "').observe('click', function() { multilistValuesMoveLeft" + this.ClientID + @"('" + this.ID
                      + @"') ; javascript:scContent.multilistMoveLeft('" + this.ID + @"') } );";

      script = "<script type='text/javascript' language='javascript'>" + script + "</script>";

      output.Write(script);
    }
    #endregion
  }
}