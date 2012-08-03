namespace Sitecore.ItemBucket.Kernel.Kernel.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Sitecore.Data.Validators;
    using Sitecore.Diagnostics;
    using Sitecore.ItemBucket.Kernel.Kernel.Util;

    internal class ValidBucketQuery : StandardValidator
    {
        // Methods
        public ValidBucketQuery()
        {
        }

        public ValidBucketQuery(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        private static bool ExtractSearchQuery(string searchQuery)
        {
            var searchStringModels = new List<SearchStringModel>();

            try
            {
                searchQuery = searchQuery.Replace("bucket:", string.Empty);
                searchQuery = searchQuery.Replace("text:;", string.Empty);
                var terms = searchQuery.Split(';');
                for (var i = 0; i < terms.Length; i++)
                {
                    if (i % 2 == 0)
                    {
                        searchStringModels.Add(new SearchStringModel
                                                   {
                                                       Type = terms[i].Split(':')[0],
                                                       Value = terms[i].Split(':')[1]
                                                   });
                    }

                    i++;
                }
            }
            catch (Exception exc)
            {
                Log.Error("Could not resolve search string", exc);
                return false;
            }

            return true;
        }

        protected override ValidatorResult Evaluate()
        {
            var str = this.ControlValidationValue;
            if (str.StartsWith("bucket:"))
            {
                return ExtractSearchQuery(str) ? ValidatorResult.Valid : this.GetFailedResult(ValidatorResult.CriticalError);
            }

            return ValidatorResult.Valid;
        }

        protected override ValidatorResult GetMaxValidatorResult()
        {
            return this.GetFailedResult(ValidatorResult.CriticalError);
        }

        /// <summary>
        /// Gets Name.
        /// </summary>
        public override string Name
        {
            get
            {
                return "Invalid Bucket Query Syntax";
            }
        }
    }
}