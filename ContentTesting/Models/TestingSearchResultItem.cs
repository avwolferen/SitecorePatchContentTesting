using Sitecore.ContentSearch;

namespace AlexVanWolferen.SitecorePatches.ContentTesting.Models
{
    public class TestingSearchResultItem : Sitecore.ContentTesting.ContentSearch.Models.TestingSearchResultItem
    {
        [IndexField("__is_running_tristate")]
        public string IsRunningString
        {
            get;
            set;
        }

        [IndexField("datasourceitems_tidy")]
        public string DataSourceItemsTidy
        {
            get;
            set;
        }
    }
}