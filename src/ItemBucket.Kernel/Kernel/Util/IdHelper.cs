using System.ComponentModel;

namespace Sitecore.ItemBucket.Kernel.Util
{
    using System;
    using System.Linq;

    using Sitecore.Data;

    public class IdHelper
    {
        public static string[] ParseId(string value)
        {
            return value.Split(new[] { "|", " ", "," }, StringSplitOptions.RemoveEmptyEntries);
        }

        [DefaultValue(false)]
        public static bool ContainsMultiGuids(string value)
        {
            var ids = ParseId(value).Where(ID.IsID).ToArray();
            return ids.Length > 0;
        }

        /// <summary>
        /// This is a replacement for ID.IsID which is slower at detecting if string is a GUID
        /// </summary>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <returns>
        /// </returns>
        [DefaultValue(false)]
        public static bool IsGuid(string subject)
        {
            if (string.IsNullOrEmpty(subject)) return false;
            subject = subject.Trim();
            var result = '{' == subject[0] ? '}' == subject[37] && 38 == subject.Length : 36 == subject.Length;
            if (result)
            {
                var offset = '{' == subject[0] ? 1 : 0;
                result = '-' == subject[offset + 8]
                         && '-' == subject[offset + 13]
                         && '-' == subject[offset + 18]
                         && '-' == subject[offset + 23];
                if (result)
                {
                    var slen = subject.Length - offset;
                    for (var k = offset; k < slen; k++)
                    {
                        var suspect = subject[k];
                        result = ('A' <= suspect && 'F' >= suspect)
                                 || ('a' <= suspect && 'f' >= suspect)
                                 || ('0' <= suspect && '9' >= suspect)
                                 || '-' == suspect;
                        if (!result) break;
                    }
                }
            }

            return result;
        }

        public static string ProcessGuiDs(string value, bool lowercase)
        {
            if (ID.IsID(value))
            {
                return NormalizeGuid(value, lowercase);
            }

            return ContainsMultiGuids(value) ? string.Join(" ", ParseId(value).Select(NormalizeGuid).ToArray()) : value;
        }

        public static string ProcessGUIDs(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (ID.IsID(value))
            {
                return NormalizeGuid(value);
            }

            return ContainsMultiGuids(value) ? string.Join(" ", ParseId(value).Select(NormalizeGuid).ToArray()) : value;
        }

        public static string NormalizeGuid(string guid, bool lowercase)
        {
            if (!string.IsNullOrEmpty(guid))
            {
                var shortId = ShortID.Encode(guid);
                return lowercase ? shortId.ToLowerInvariant() : shortId;
            }

            return guid;
        }

        public static string NormalizeGuid(string guid)
        {
            if (!string.IsNullOrEmpty(guid) && guid.StartsWith("{"))
            {
                return ShortID.Encode(guid);
            }

            return guid;
        }

        public static string NormalizeGuid(ID guid)
        {
            return NormalizeGuid(guid.ToString());
        }
    }
}
