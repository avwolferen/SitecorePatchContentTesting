using Sitecore;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.ComputedFields;
using Sitecore.Data.Items;
using System.Collections.Generic;
using System.Linq;

namespace AlexVanWolferen.SitecorePatches.ContentTesting.ContentSearch.ComputedIndexFields
{
    [UsedImplicitly]
    public class TestDataSources : AbstractComputedIndexField
    {
        public override object ComputeFieldValue(IIndexable indexable)
        {
            Item item = indexable as SitecoreIndexableItem;
            if (item == null)
            {
                return null;
            }
            IEnumerable<Item> source = from x in item.Axes.GetDescendants()
                                       where x.Fields["Datasource"] != null && !string.IsNullOrEmpty(x.Fields["Datasource"].Value)
                                       select x;
            var result = string.Join("|", from x in source
                                    select x.Fields["Datasource"].Value);

            // Could do this with a slick Regular Expression, but didn't want to invest that much effort in it.
            result = result.Replace("{", string.Empty).Replace("}", string.Empty).Replace("-", string.Empty);

            return result;
        }
    }
}