using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Maintenance;
using Sitecore.ContentTesting.ContentSearch.Models;
using Sitecore.ContentTesting.Model.Data.Items;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AlexVanWolferen.SitecorePatches.ContentTesting
{
    /// <summary>
    /// Altered implementation of the SitecoreContentTestStore
    /// Resolves issues with activetests, historicaltests
    /// </summary>
    public class SitecoreContentTestStore : Sitecore.ContentTesting.Data.SitecoreContentTestStore
    {
        public override IEnumerable<TestingSearchResultItem> GetActiveTests(DataUri hostItemDataUri = null, string searchText = null, ID deviceId = null)
        {
            TestingSearch testingSearch = new TestingSearch();
            ISearchIndex testingSearchIndex = Sitecore.ContentTesting.ContentSearch.TestingSearch.GetTestingSearchIndex();
            if (testingSearchIndex == null)
            {
                return Enumerable.Empty<TestingSearchResultItem>();
            }
            int num = 0;
            while (IndexCustodian.IsRebuilding(testingSearchIndex) && num < 10)
            {
                Thread.Sleep(200);
                num++;
            }
            if (hostItemDataUri != null)
            {
                testingSearch.HostItem = hostItemDataUri;
            }
            if (!string.IsNullOrEmpty(searchText))
            {
                testingSearch.SearchText = searchText;
            }
            if (deviceId != (ID)null)
            {
                testingSearch.DeviceId = deviceId;
            }
            return testingSearch.GetRunningTests();
        }

        public override IEnumerable<TestingSearchResultItem> GetHistoricalTests(DataUri hostItemDataUri = null, string searchText = null)
        {
            TestingSearch testingSearch = new TestingSearch();
            if (hostItemDataUri != null)
            {
                testingSearch.HostItem = hostItemDataUri;
            }
            if (!string.IsNullOrEmpty(searchText))
            {
                testingSearch.SearchText = searchText;
            }
            IEnumerable<TestingSearchResultItem> stoppedTests = testingSearch.GetStoppedTests();
            List<TestingSearchResultItem> list = new List<TestingSearchResultItem>();
            foreach (TestingSearchResultItem item2 in stoppedTests)
            {
                if (item2.Uri != null)
                {
                    Item item = Database.GetItem(item2.Uri);
                    if (item != null)
                    {
                        TestDefinitionItem testDefinitionItem = TestDefinitionItem.Create(item);
                        if (testDefinitionItem != null && testDefinitionItem.IsFinished)
                        {
                            list.Add(item2);
                        }
                    }
                }
            }
            return list;
        }

        public override IEnumerable<TestingSearchResultItem> GetActiveTestsInAllLanguages(Item hostItem)
        {
            Assert.ArgumentNotNull(hostItem, "hostItem");
            return new TestingSearch().GetRunningTestsInAllLanguages(hostItem);
        }
    }
}