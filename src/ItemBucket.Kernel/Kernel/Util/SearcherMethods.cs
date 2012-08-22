namespace Sitecore.ItemBucket.Kernel.Kernel.Util
{
    using System;
    using System.Linq;

    using Lucene.Net.Index;
    using Lucene.Net.QueryParsers;
    using Lucene.Net.Search;

    using Sitecore.Collections;
    using Sitecore.ItemBucket.Kernel.Search;
    using Sitecore.ItemBucket.Kernel.Util;
    using Sitecore.Search;

    internal static class SearcherMethods
    {
        #region Clause Construction Helpers

        internal static void ApplyDateRangeSearchParam(BooleanQuery query, DateRangeSearchParam param, BooleanClause.Occur innerOccurance)
        {
            if (param.Ranges == null)
            {
                return;
            }

            if (param.Ranges.Count <= 0)
            {
                return;
            }

            param.Ranges.ForEach(dateParam => AddDateRangeQuery(query, dateParam, innerOccurance));
        }
        internal static void ApplyLatestVersion(CombinedQuery globalQuery)
        {
            var fieldQuery = new FieldQuery(BuiltinFields.LatestVersion, "1");
            globalQuery.Add(fieldQuery, QueryOccurance.Must);
        }

        internal static void AddDateRangeQuery(BooleanQuery query, DateRangeSearchParam.DateRangeField dateRangeField, BooleanClause.Occur occurance)
        {
            var startDateTime = dateRangeField.StartDate;
            if (dateRangeField.InclusiveStart)
            {
                if (startDateTime != DateTime.MinValue)
                {
                    startDateTime.ChangeTime(0, 0, 0, 0);
                }
            }

            var endDateTime = dateRangeField.EndDate;
            if (dateRangeField.InclusiveStart)
            {
                if (endDateTime != DateTime.MaxValue)
                {
                    endDateTime = endDateTime.ChangeTime(23, 59, 59, 59);
                }
            }

            BooleanQuery.SetMaxClauseCount(int.MaxValue);

            if (!(dateRangeField.StartDate == DateTime.MinValue && dateRangeField.EndDate == DateTime.MaxValue))
            {
                query.Add(new RangeQuery(new Term(SearchFieldIDs.CreatedDate, startDateTime.ToString("yyyyMMdd")), new Term(SearchFieldIDs.CreatedDate, endDateTime.ToString("yyyyMMdd")), true), occurance);
            }
        }

        internal static void ApplyRefinements(CombinedQuery query, SafeDictionary<string> refinements, QueryOccurance occurance)
        {
            if (refinements.Count <= 0)
            {
                return;
            }


            var combinedQuery = new CombinedQuery();
            refinements.ForEach(refinement => AddFieldValueClause(combinedQuery, refinement.Key.ToLowerInvariant(), refinement.Key == "_tags" ? refinement.Value.Replace("_tags ", "") : refinement.Value, occurance));
            query.Add(combinedQuery, QueryOccurance.Must);
        }

        internal static void AddFieldValueClause(CombinedQuery query, string fieldName, string fieldValue, QueryOccurance occurance)
        {
            if (fieldName.IsNullOrEmpty() || fieldValue.IsNullOrEmpty())
            {
                return;
            }

            fieldValue = IdHelper.ProcessGUIDs(fieldValue);
            query.Add(new FieldQuery(fieldName, fieldValue), occurance);
        }

        internal static void AddFieldValueClause(BooleanQuery query, string fieldName, string fieldValue, QueryOccurance occurance)
        {
            if (fieldName.IsNullOrEmpty() || fieldValue.IsNullOrEmpty())
            {
                return;
            }
            var globalBooleanQuery = new BooleanQuery();
            var qp = new QueryParser("__workflow state", ItemBucket.Kernel.Util.IndexSearcher.Index.Analyzer);
            qp.SetAllowLeadingWildcard(true);
            globalBooleanQuery.Add(qp.Parse(fieldValue), BooleanClause.Occur.MUST);

            query.Add(globalBooleanQuery, BooleanClause.Occur.MUST);
        }

        internal static void AddPartialFieldValueClause(BooleanQuery query, string fieldName, string fieldValue)
        {
            if (fieldValue.IsNullOrEmpty())
            {
                return;
            }

            fieldValue = IdHelper.ProcessGUIDs(fieldValue);
            query.Add(new QueryParser(fieldName, ItemBucket.Kernel.Util.IndexSearcher.Index.Analyzer).Parse(fieldValue), BooleanClause.Occur.MUST);
        }

        internal static void ApplyLanguageClause(CombinedQuery query, string language)
        {
            if (language.IsNullOrEmpty())
            {
                return;
            }

            query.Add(new FieldQuery(BuiltinFields.Language, language.ToLowerInvariant()), QueryOccurance.Must);
        }

        internal static void ApplyFullTextClause(CombinedQuery query, string searchText)
        {
            ApplyFullTextClause(query, searchText, string.Empty);
        }

        internal static void ApplyFullTextClause(BooleanQuery query, string searchText)
        {
            SearcherMethods.ApplyFullTextClause(query, searchText, string.Empty);
        }

        internal static void ApplyFullTextClause(CombinedQuery query, string searchText, string indexName)
        {
            if (searchText.IsNullOrEmpty())
            {
                return;
            }

            var combinedQuery = new CombinedQuery();
            combinedQuery.Add(new FullTextQuery(searchText), QueryOccurance.Should);
            combinedQuery.Add(new FieldQuery(BuiltinFields.Name, searchText), QueryOccurance.Should);
            if (!indexName.IsNullOrEmpty())
            {
                AppendIndexSpecificTerms(combinedQuery, indexName, searchText);
            }


            query.Add(combinedQuery, QueryOccurance.Must);
        }

        private static void AppendIndexSpecificTerms(CombinedQuery combinedQuery, string indexName, string searchText)
        {
            switch (indexName)
            {
                case "itembuckets_medialibrary":
                    combinedQuery.Add(new FieldQuery("artist", searchText), QueryOccurance.Should);
                    combinedQuery.Add(new FieldQuery("copyright", searchText), QueryOccurance.Should);
                    combinedQuery.Add(new FieldQuery("imagedescription", searchText), QueryOccurance.Should);
                    combinedQuery.Add(new FieldQuery("title", searchText), QueryOccurance.Should);
                    break;
                case "itembuckets_sitecore":
                    combinedQuery.Add(new FieldQuery("artist", searchText), QueryOccurance.Should);
                    combinedQuery.Add(new FieldQuery("copyright", searchText), QueryOccurance.Should);
                    combinedQuery.Add(new FieldQuery("imagedescription", searchText), QueryOccurance.Should);
                    combinedQuery.Add(new FieldQuery("title", searchText), QueryOccurance.Should);
                    break;
                case "itembuckets_systemfolder":
                    break;
                case "itembuckets_templates":
                    break;
            }
        }

        internal static void ApplyFullTextClause(BooleanQuery query, string searchText, string indexName)
        {
            if (searchText.IsNullOrEmpty())
            {
                return;
            }

            var globalBooleanQuery = new BooleanQuery();
            var qp = new QueryParser("_content", ItemBucket.Kernel.Util.IndexSearcher.Index.Analyzer);
            qp.SetAllowLeadingWildcard(true);
            globalBooleanQuery.Add(qp.Parse(searchText), BooleanClause.Occur.SHOULD);

            var qp1 = new QueryParser("_name", ItemBucket.Kernel.Util.IndexSearcher.Index.Analyzer);
            qp1.SetAllowLeadingWildcard(true);
            globalBooleanQuery.Add(qp1.Parse(searchText), BooleanClause.Occur.SHOULD);
            query.Add(globalBooleanQuery, BooleanClause.Occur.MUST);
        }

        internal static void ApplyAuthor(BooleanQuery query, string searchText)
        {
            if (searchText.IsNullOrEmpty())
            {
                return;
            }

            query.Add(new TermQuery(new Term("__created by", searchText)), BooleanClause.Occur.MUST);
        }

        internal static void ApplyIdFilter(CombinedQuery query, string fieldName, string filter)
        {
            if (fieldName.IsNullOrEmpty() || filter.IsNullOrEmpty())
            {
                return;
            }

            var filterQuery = new CombinedQuery();
            IdHelper.ParseId(filter).Where(IdHelper.IsGuid).ForEach(value => AddFieldValueClause(filterQuery, fieldName, value, QueryOccurance.Should));
            query.Add(filterQuery, QueryOccurance.Must);
        }

        internal static void ApplyTemplateFilter(CombinedQuery query, string templateIds)
        {
            if (templateIds.IsNullOrEmpty())
            {
                return;
            }

            var templateList = templateIds.Split('|');
            if (templateList.Length > 1)
            {
                var filterQuery = new CombinedQuery();
                foreach (var templateId in templateList)
                {
                    templateIds = IdHelper.NormalizeGuid(templateId);
                    filterQuery.Add(new FieldQuery(BuiltinFields.Template, templateIds), QueryOccurance.Should);
                }

                query.Add(filterQuery, QueryOccurance.Must);
            }
            else
            {
                templateIds = IdHelper.NormalizeGuid(templateIds);
                query.Add(new FieldQuery(BuiltinFields.Template, templateIds), QueryOccurance.Must);
            }
        }

        internal static void ApplyTemplateNotFilter(CombinedQuery query)
        {
            query.Add(new FieldQuery(BuiltinFields.Template, IdHelper.NormalizeGuid(Config.ContainerTemplateId)), QueryOccurance.MustNot);
        }

        internal static void ApplyIDFilter(CombinedQuery query, string Id)
        {
            if (Id.IsNullOrEmpty())
            {
                return;
            }

            Id = IdHelper.NormalizeGuid(Id);
            query.Add(new FieldQuery(BuiltinFields.ID, Id), QueryOccurance.Must);
        }

        internal static void ApplyContextItemRemoval(CombinedQuery query, string Id)
        {
            if (Id.IsNullOrEmpty())
            {
                return;
            }

            Id = IdHelper.NormalizeGuid(Id);
            query.Add(new FieldQuery(BuiltinFields.ID, Id), QueryOccurance.MustNot);
        }

        internal static void ApplyNameFilter(CombinedQuery query, string Name)
        {
            if (Name.IsNullOrEmpty())
            {
                return;
            }

            query.Add(new FieldQuery(BuiltinFields.Name, Name), QueryOccurance.Should);
        }

      
        internal static void ApplyLocationFilter(CombinedQuery query, string locationIds)
        {
            ApplyIdFilter(query, BuiltinFields.Path, locationIds);
        }

        internal static void ApplyCombinedLocationFilter(CombinedQuery query, string locationIds)
        {
            ApplyIdFilter(query, BuiltinFields.Path, locationIds);
        }


        internal static void ApplyRelationFilter(CombinedQuery query, string ids)
        {
            ApplyIdFilter(query, BuiltinFields.Links, ids);
        }

        #endregion
    }
}
