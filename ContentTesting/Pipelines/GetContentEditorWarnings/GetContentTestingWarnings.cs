using Sitecore.ContentTesting;
using Sitecore.ContentTesting.Configuration;
using Sitecore.ContentTesting.Data;
using Sitecore.ContentTesting.Services;
using Sitecore.Globalization;
using Sitecore.Pipelines.GetContentEditorWarnings;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AlexVanWolferen.SitecorePatches.ContentTesting.Pipelines.GetContentEditorWarnings
{
    public class GetContentTestingWarnings : Sitecore.ContentTesting.Pipelines.GetContentEditorWarnings.GetContentTestingWarnings
    {
        private readonly ITestCandidateInspectionInitiator testCandidateInitiator;

        public GetContentTestingWarnings() : this(null, null)
        {
        }

        public GetContentTestingWarnings(IContentTestStore contentTestStore, ITestCandidateInspectionInitiator startTestOptions) : base(contentTestStore, startTestOptions)
        {
            testCandidateInitiator = (startTestOptions ?? ContentTestingFactory.Instance.TestCandidateInspectionInitiator);
        }

        public new void Process(GetContentEditorWarningsArgs args)
        {
            if (Settings.IsAutomaticContentTestingEnabled && args.Item != null && !AddSuspendedTestWarning(args) && !AddActiveTestWarning(args) && !AddPartOfActiveTestWarning(args) && testCandidateInitiator.GetTestInitiator(args.Item) == TestCandidatesInitiatorsEnum.Notification)
            {
                AddContentEditorTestCandidateNotification(args);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:Mark members as static", Justification = "This will introduce breaking changes.")]
        public new bool AddPartOfActiveTestWarning(GetContentEditorWarningsArgs args)
        {
            if (new TestingSearch().GetRunningTestsWithDataSourceItem(args.Item).Any())
            {
                args.Add(Translate.Text("This component is part of an active test."), Translate.Text("If you edit the content it could have a negative impact on statistical significance of the test."));
                return true;
            }
            return false;
        }
    }
}