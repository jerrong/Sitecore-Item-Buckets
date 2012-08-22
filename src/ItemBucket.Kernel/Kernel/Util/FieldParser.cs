using System.Collections.Generic;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace Sitecore.ItemBucket.Kernel.Kernel.Util
{
    public static class FieldParser
    {
        public static MediaItem ParseIdsForImage(Item itm, string fieldName)
        {
            var idsForImage = itm.Fields[fieldName];
            if (idsForImage.IsNotNull())
            {
                var items = ((MultilistField)itm.Fields[fieldName]).GetItems();
                if (items.Any())
                {
                    if (items.First().Paths.IsMediaItem)
                    {
                        switch (idsForImage.Type)
                        {
                            case "Multilist":
                                return new MediaItem(((MultilistField) itm.Fields[fieldName]).GetItems().First());
                            case "thumbnail":
                                return ((ThumbnailField) itm.Fields[fieldName]).MediaItem;
                        }
                    }
                }
            }
            return null;
        }

        public static string ParseIdsForFieldFriendlyValue(Item itm, string fieldName)
        {
            var idsForImage = itm.Fields[fieldName];
            if (idsForImage.IsNotNull())
            {
                switch (idsForImage.Type)
                {
                    case "Multilist":
                        return ((MultilistField) itm.Fields[fieldName]).GetItems().Aggregate(default(string), (current, item) => current + item.Name + "<br>");
                    case "Checkbox":
                        return (((CheckboxField)itm.Fields[fieldName]).Checked ? "True" : "False");
            

                }
            }
            return "";
        }
    }
}
