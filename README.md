# Fixing the Sitecore Content Testing Index
Fixing the Sitecore Content Testing Index, Experience Optimization, Active Tests and Historical Tests for real.
Applicable to Sitecore 9.0.171219

Read the blog here <a href="https://blog.alexvanwolferen.nl/fixing-the-sitecore-content-testing-index,-experience-optimization,-active-tests-and-historical-tests-for-real/" target="_blank">https://blog.alexvanwolferen.nl/fixing-the-sitecore-content-testing-index,-experience-optimization,-active-tests-and-historical-tests-for-real/</a>

**Summary**

When you are working with Sitecore 9.0.1 (9.0.171219) in Azure with Azure Search you might be familiar with or experience the following symptoms.

- In your Content Editor you might have seen XHR requests about Optimization.ActiveTests.Count and Optimization.HistoricalTests.Count that have a value 0.
- Experience Optimization is not showing Active Tests, but you are 100% sure you just created one.
- Experience Optimization is not showing any Historical Tests and you are 100% sure you had them.
- Experience Optimization is showing Draft Tests but after starting they don’t show up in the Active Tests and again you are 100% sure about these as well.
- Someone deleted a content item that was part of an active test.
- Someone deleted a content item that was subjected to a test.
- In your Traces you find weird entries telling you: AzureSearch Query [sitecore_testing_index]: &search=This_Is_Equal_ConstNode_Return_Nothing

This patch fixes all of these problems.