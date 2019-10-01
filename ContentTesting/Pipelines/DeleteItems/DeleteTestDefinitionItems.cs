using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.ContentTesting;
using Sitecore.ContentTesting.ContentSearch.Models;
using Sitecore.ContentTesting.Data;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.SecurityModel;
using Sitecore.StringExtensions;
using Sitecore.Web.UI.Sheer;

namespace AlexVanWolferen.SitecorePatches.ContentTesting.Pipelines.DeleteItems
{
    public class DeleteTestDefinitionItems : Sitecore.ContentTesting.Pipelines.DeleteItems.DeleteTestDefinitionItems
    {
        private readonly IContentTestStore contentTestStore;

        public DeleteTestDefinitionItems()
            : this(null)
        {
        }

        public DeleteTestDefinitionItems(IContentTestStore contentTestStore) : base(contentTestStore)
        {
            this.contentTestStore = (contentTestStore ?? ContentTestingFactory.Instance.ContentTestStore);
        }

        public override void CheckActiveTests(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Item[] contentItems = GetContentItems(args);
            if (!contentItems.Any())
            {
                return;
            }
            if (args.Result.IsNullOrEmpty())
            {
                string confirmMessage = GetConfirmMessage(contentItems.ToArray());
                if (!string.IsNullOrEmpty(confirmMessage))
                {
                    Context.ClientPage.ClientResponse.Confirm(confirmMessage);
                    args.WaitForPostBack();
                }
            }
            else if (args.Result == "yes")
            {
                args.Result = string.Empty;
                Database database = Factory.GetDatabase(args.Parameters["database"]);
                DeleteActiveTests(contentItems, database);
            }
            else if (args.Result == "no")
            {
                args.AbortPipeline();
            }
        }

        public override string GetConfirmMessage(Item[] contentItems)
        {
            bool flag = false;
            foreach (Item hostItem in contentItems)
            {
                if (contentTestStore.GetActiveTestsInAllLanguages(hostItem).Any())
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                return string.Empty;
            }
            if (contentItems.Count() <= 1)
            {
                return Translate.Text("The item has Active tests. Are you sure you want to remove the item and its test definitions?");
            }
            return Translate.Text("Some items have Active tests. Are you sure you want to remove all the items and their test definitions?");
        }

        public override void DeleteActiveTests(Item[] contentItems, Database database)
        {
            foreach (Item hostItem in contentItems)
            {
                IEnumerable<TestingSearchResultItem> activeTestsInAllLanguages = contentTestStore.GetActiveTestsInAllLanguages(hostItem);
                using (new SecurityDisabler())
                {
                    foreach (TestingSearchResultItem item in activeTestsInAllLanguages)
                    {
                        database.GetItem(item.Uri.ToDataUri())?.Recycle();
                    }
                }
            }
        }

        protected override Item[] GetContentItems(ClientPipelineArgs args)
        {
            List<Item> list = new List<Item>();
            Database database = Factory.GetDatabase(args.Parameters["database"]);
            List<ID> list2 = args.Parameters["items"].Split('|').Select(ID.Parse).ToList();
            if (!list2.Any())
            {
                return list.ToArray();
            }
            foreach (ID item2 in list2)
            {
                Item item = database.GetItem(item2);
                if (item == null)
                {
                    Log.Error($"Item {item2} to delete doesn't exist", this);
                }
                else
                {
                    list.AddRange(GetContentItemsDescendants(item));
                    if (IsContentItem(item))
                    {
                        list.Add(item);
                    }
                }
            }
            return list.ToArray();
        }

        private static Item[] GetContentItemsDescendants(Item item)
        {
            List<Item> list = new List<Item>();
            Item[] descendants = item.Axes.GetDescendants();
            foreach (Item item2 in descendants)
            {
                if (item2 == null || IsContentItem(item2))
                {
                    list.Add(item2);
                }
            }
            return list.ToArray();
        }

        private static bool IsContentItem(Item item)
        {
            Item item2 = item.Database.GetItem(ItemIDs.ContentRoot);
            return item.Axes.IsDescendantOf(item2);
        }
    }
}