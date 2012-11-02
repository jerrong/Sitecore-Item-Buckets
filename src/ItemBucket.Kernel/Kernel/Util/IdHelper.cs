using System.Collections.Generic;
using System.ComponentModel;

namespace Sitecore.ItemBucket.Kernel.Util
{
    using System;
    using System.Linq;

    using Sitecore.Data;

    public class IdHelper
    {
        public static IEnumerable<Guid> ParseId(string value)
        {
            return 
                value.Split(new[] {"|", " ", ","}, StringSplitOptions.RemoveEmptyEntries)
                .Where(IsGuid)
                .Select(s => new Guid(s));
        }

        [DefaultValue(false)]
        public static bool ContainsMultiGuids(string value)
        {
            return value.Contains('|') || value.Contains(' ') || value.Contains(',');
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
            if (subject.Length == 0) return false;
            var result = '{' == subject[0] 
                    ? 38 == subject.Length && '}' == subject[37]
                    : 36 == subject.Length;
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
                        if (!result) return false;
                    }
                }
            }
            return result;
        }

        public static string ProcessGUIDs(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (IsGuid(value))
            {
                return NormalizeGuid(value);
            }

            return ContainsMultiGuids(value) ? string.Join(" ", ParseId(value).Select(NormalizeGuid).ToArray()) : value;
        }
        public static string NormalizeGuid(Guid id)
        {
            return NormalizeGuid(id, true);
        }
        public static string NormalizeGuid(Guid id, bool lowercase)
        {
            return NormalizeGuid(new ID(id), lowercase);
        }
        public static string NormalizeGuid(ID id)
        {
            return NormalizeGuid(id, true);
        }
        public static string NormalizeGuid(ID id, bool lowercase)
        {
            return lowercase ? id.ToShortID().ToString().ToLowerInvariant() : id.ToShortID().ToString();
        }
        public static string NormalizeGuid(string guid)
        {
            return NormalizeGuid(guid, true);
        }
        public static string NormalizeGuid(string guid, bool lowercase)
        {
            if (!string.IsNullOrEmpty(guid) && IsGuid(guid))
            {
                var shortId = new ShortID(guid);
                return lowercase ? shortId.ToString().ToLowerInvariant() : shortId.ToString();
            }

            return guid;
        }
    }
}
