using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Utilities;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentTesting.ContentSearch;
using Sitecore.ContentTesting.ContentSearch.Models;
using Sitecore.ContentTesting.Helpers;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Web;

namespace AlexVanWolferen.SitecorePatches.ContentTesting
{
    public class TestingSearch : Sitecore.ContentTesting.ContentSearch.TestingSearch
    {
        public new IEnumerable<TestingSearchResultItem> GetRunningTests()
        {
            ISearchIndex testingSearchIndex = GetTestingSearchIndex();
            if (testingSearchIndex == null)
            {
                return Enumerable.Empty<Models.TestingSearchResultItem>();
            }
            using (IProviderSearchContext providerSearchContext = testingSearchIndex.CreateSearchContext(SearchSecurityOptions.DisableSecurityCheck))
            {
                Expression<Func<Models.TestingSearchResultItem, bool>> predicate = PredicateBuilder.True<Models.TestingSearchResultItem>();
                predicate = AddDatesSearchCriteria(predicate);
                predicate = AddDefaultSearchCriteria(predicate);

                // Querying our new field that is TriState compatible
                predicate = predicate.And((Models.TestingSearchResultItem x) => x.IsRunningString == "1");
                var results = providerSearchContext.GetQueryable<Models.TestingSearchResultItem>().Where(predicate).ToArray();
                return results;
            }
        }

        public new IEnumerable<TestingSearchResultItem> GetStoppedTests()
        {
            ISearchIndex testingSearchIndex = GetTestingSearchIndex();
            if (testingSearchIndex == null)
            {
                return Enumerable.Empty<TestingSearchResultItem>();
            }
            using (IProviderSearchContext providerSearchContext = testingSearchIndex.CreateSearchContext(SearchSecurityOptions.DisableSecurityCheck))
            {
                Expression<Func<Models.TestingSearchResultItem, bool>> predicate = PredicateBuilder.True<Models.TestingSearchResultItem>();
                predicate = AddDatesSearchCriteria(predicate);
                predicate = AddDefaultSearchCriteria(predicate);

                // Querying our new field that is TriState compatible
                predicate = predicate.And((Models.TestingSearchResultItem x) => x.IsRunningString != "1");
                return from x in providerSearchContext.GetQueryable<Models.TestingSearchResultItem>().Where(predicate).ToArray()
                       orderby x.UpdatedDate descending
                       select x;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:Mark members as static", Justification = "This will introduce breaking changes.")]
        public new IEnumerable<TestingSearchResultItem> GetRunningTestsInAllLanguages(Item hostItem)
        {
            Assert.ArgumentNotNull(hostItem, "hostItem");
            ISearchIndex testingSearchIndex = GetTestingSearchIndex();
            if (testingSearchIndex == null)
            {
                return Enumerable.Empty<Models.TestingSearchResultItem>();
            }
            List<Models.TestingSearchResultItem> list = new List<Models.TestingSearchResultItem>();
            using (IProviderSearchContext providerSearchContext = testingSearchIndex.CreateSearchContext(SearchSecurityOptions.DisableSecurityCheck))
            {
                Expression<Func<Models.TestingSearchResultItem, bool>> first = PredicateBuilder.True<Models.TestingSearchResultItem>();
                Language[] languages = hostItem.Languages;
                foreach (Language language in languages)
                {
                    Item itemInLanguage = hostItem.Database.GetItem(hostItem.ID, language);

                    // This is de wrong implementation because it only focusses on if the current item is the initiator/host of the test.
                    first = first.Or((Models.TestingSearchResultItem x) => x.HostItemPartial == FieldFormattingHelper.FormatUriForSearch(itemInLanguage));

                    // It should focus on if the current item is subject of the actual test.
                    string dataSourceId = itemInLanguage.ID.ToShortID().ToString().Replace("-", string.Empty);
                    first = first.Or((Models.TestingSearchResultItem x) => x.DataSourceItemsTidy.Contains(dataSourceId));
                }

                // Querying our new field that is TriState compatible
                first = first.And((Models.TestingSearchResultItem x) => x.IsRunningString == "1");
                list.AddRange(providerSearchContext.GetQueryable<Models.TestingSearchResultItem>().Where(first).ToArray());
                return list;
            }
        }

        public new IEnumerable<TestingSearchResultItem> GetRunningTestsWithDataSourceItem(Item dataSourceItem)
        {
            ISearchIndex testingSearchIndex = GetTestingSearchIndex();
            if (testingSearchIndex == null)
            {
                return Enumerable.Empty<Models.TestingSearchResultItem>();
            }
            using (IProviderSearchContext providerSearchContext = testingSearchIndex.CreateSearchContext(SearchSecurityOptions.DisableSecurityCheck))
            {
                Expression<Func<Models.TestingSearchResultItem, bool>> predicate = PredicateBuilder.True<Models.TestingSearchResultItem>();
                predicate = AddDatesSearchCriteria(predicate);
                predicate = AddDefaultSearchCriteria(predicate);
                
                // Querying our new field that is TriState compatible
                predicate = predicate.And((Models.TestingSearchResultItem x) => x.IsRunningString == "1");

                // https://doc.sitecore.com/developers/90/platform-administration-and-architecture/en/support-reference-for-azure-search.html
                // Quote: You must not apply queries such as StartsWith, Contains to fields with of type EDM.String that contain paths (such as /sitecore/content/home) or collections of GUIDs.
                //        This is because Azure Search matches regular expression searches to single words.
                //
                // Alright then we must fix this. DataSourceItems contains path like content...
                // "datasourceitems_s": "sitecore://{B765C5DD-E0AD-4D91-BA63-B082656ED909}?lang=en&ver=1|sitecore://{C547CF9A-D9CC-42D7-A1D5-B14AB9BBD870}?lang=en&ver=1"

                // Therefore I introduced a new field with a propper value in it that won't need any special escape characters
                string dataSourceId = dataSourceItem.ID.ToShortID().ToString().Replace("-",string.Empty);
                predicate = predicate.And((Models.TestingSearchResultItem x) => x.DataSourceItemsTidy.Contains(dataSourceId));
                var result = providerSearchContext.GetQueryable<Models.TestingSearchResultItem>().Where(predicate).ToArray();
                return result;
            }
        }

        protected Expression<Func<Models.TestingSearchResultItem, bool>> AddDatesSearchCriteria(Expression<Func<Models.TestingSearchResultItem, bool>> predicate)
        {
            Expression<Func<Models.TestingSearchResultItem, bool>> expression = predicate;
            if (Start != DefaultStartDate)
            {
                expression = expression.And((Models.TestingSearchResultItem x) => x.UpdatedDate >= Start);
            }
            if (End != DefaultEndDate)
            {
                expression = expression.And((Models.TestingSearchResultItem x) => x.UpdatedDate < End);
            }
            return expression;
        }

        protected Expression<Func<Models.TestingSearchResultItem, bool>> AddDefaultSearchCriteria(Expression<Func<Models.TestingSearchResultItem, bool>> predicate)
        {
            Expression<Func<Models.TestingSearchResultItem, bool>> expression = predicate;
            if (!string.IsNullOrEmpty(Owner))
            {
                expression = expression.And((Models.TestingSearchResultItem x) => x.Owner == FormattingHelper.GetCompactUserName(Owner));
            }
            if (HostItem != null)
            {
                expression = expression.And((Models.TestingSearchResultItem x) => x.HostItemPartial == FieldFormattingHelper.FormatUriForSearch(HostItem));
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                expression = expression.And((Models.TestingSearchResultItem x) => x.SearchText.Contains(SearchText));
            }
            if (TestUri != null)
            {
                expression = expression.And((Models.TestingSearchResultItem x) => x.Uri == TestUri);
            }
            if (!ID.IsNullOrEmpty(DeviceId))
            {
                expression = expression.And((Models.TestingSearchResultItem x) => x.DeviceId == DeviceId);
            }
            return expression;
        }
    }
}