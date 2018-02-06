using System;
using Gurock.TestRail;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

namespace TestRailPlanReRun
{
    public class AccessTestRail
    {
        public static JArray GetRunsForMilestone(APIClient client, string milestoneID)
        {
            return (JArray)client.SendGet("get_runs/2&milestone_id=" + milestoneID);
        }

        public static JArray GetPlansForMilestone(APIClient client, string milestoneID)
        {
            return (JArray)client.SendGet("get_plans/2&milestone_id=" + milestoneID);
        }

        public static JObject GetPlan(APIClient client, string planID)
        {
            return (JObject)client.SendGet("get_plan/" + planID);
        }

        public static JArray GetTestsInRun(APIClient client, string runID)
        {
            return (JArray)client.SendGet("get_tests/" + runID);
        }

        public static JArray GetCasesInSuite(APIClient client, string projectID, string suiteID)
        {
            return (JArray)client.SendGet("get_cases/" + projectID + "&suite_id=" + suiteID);
        }

        public static JArray GetSuitesInProject(APIClient client, string projectID)
        {
            return (JArray)client.SendGet("get_suites/" + projectID);
        }

        public static JArray GetLatestResultsOfTest(APIClient client, string testID, string amountOfResultsToShow)
        {
            return (JArray)client.SendGet("get_results/" + testID + "&limit=" + amountOfResultsToShow);
        }

        public static JArray GetLatestResultsForCase(APIClient client, string runID, string caseID, string amountOfResultsToShow)
        {
            return (JArray)client.SendGet("get_results_for_case/" + runID + "/" + caseID + "&limit=" + amountOfResultsToShow);
        }

        public static JObject GetCase (APIClient client, string caseID)
        {
            return (JObject)client.SendGet($"get_case/" + caseID);
        }

        public static JObject GetSuite(APIClient client, string suiteID)
        {
            return (JObject)client.SendGet($"get_suite/" + suiteID);
        }
        public static JArray GetCaseTypes(APIClient client)
        {
            return (JArray)client.SendGet("get_case_types");
        }
        public static JArray GetStatuses(APIClient client)
        {
            return (JArray)client.SendGet("get_statuses");
        }
        public static JObject GetRun(APIClient client, string run_id)
        {
            return (JObject)client.SendGet("get_run/" + run_id);
        }


        public static JObject AddPlan(APIClient client, string projectID, object planObject)
        {
            return (JObject)client.SendPost("add_plan/" + projectID, planObject);
        }


        public static List<int> GetAllSuites(JArray arrayOfSuites)
        {
            List<int> listOfSuiteIds = new List<int>();
            for (int i = 0; i < arrayOfSuites.Count; i++)
            {
                JObject arrayObject = arrayOfSuites[i].ToObject<JObject>();
                int id = Int32.Parse(arrayObject.Property("id").Value.ToString());
                listOfSuiteIds.Add(id);
            }
            return listOfSuiteIds;
        }

        public static List<MainClass.Run> GetRunsInPlan(JObject singularPlanObject)
        {
            List<MainClass.Run> runs = new List<MainClass.Run>();
            //List<string> runInPlanIds = new List<string>();

            JProperty prop = singularPlanObject.Property("entries");
            if (prop != null && prop.Value != null)
            {
                JArray entries = (JArray)singularPlanObject.Property("entries").First;

                for (int k = 0; k < entries.Count; k++)
                {
                    JObject entriesObject = entries[k].ToObject<JObject>();


                    JArray runsArray = (JArray)entriesObject.Property("runs").First;

                    for (int j = 0; j < runsArray.Count; j++)
                    {
                        JObject runObject = runsArray[j].ToObject<JObject>();


                        string runInPlanId = runObject.Property("id").Value.ToString();

                        JToken[] configIDsToken = runObject.Property("config_ids").Value.ToArray();
                        string[] configIDs = configIDsToken.Cast<JToken>().Select(x => x.ToString()).ToArray();
                        string[] caseIDs = new string[1];

                        MainClass.Run run;
                        run.RunID = runInPlanId;
                        run.Config = runObject.Property("config").Value.ToString();
                        run.ConfigIDs = configIDs;
                        run.CaseIDs = caseIDs;
                        run.SuiteID = runObject.Property("suite_id").Value.ToString();
                        runs.Add(run);
                    }
                }
            }
            return runs;
        }

    }
}
