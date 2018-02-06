using System;
using System.Collections.Generic;
using Gurock.TestRail;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace TestRailPlanReRun
{
    public static class MainClass
    {
        private static readonly IConfigReader _configReader = new ConfigReader();

        //public static List<Run> runs = new List<Run>();
        public static List<string> suiteIDs = new List<string>();


        public struct Test
        {
            public string SuiteID;
            public string SuiteName;
            public int RunID;
            public string TestID;
            public int CaseID;
            public string Title;
            public string Status;
            public string Defects;
            public string Comment;
            public string Config;
            public string EditorVersion;
        }

        public struct Case
        {
            public string SuiteID;
            public string SuiteName;
            public int CaseID;
            public string CaseName;
            public string Status;
            public string Type;
        }

        public struct Run
        {
            public string RunID;
            public string Config;
            public string[] ConfigIDs;
            public string[] CaseIDs;
            public string SuiteID;
        }

        public struct Suite
        {
            public string SuiteID;
            public string SuiteName;
        }

        public static APIClient ConnectToTestrail()
        {
            APIClient client = new APIClient("http://qatestrail.hq.unity3d.com");
            client.User = _configReader.TestRailUser;
            client.Password = _configReader.TestRailPass;
            return client;
        }


        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            APIClient client = ConnectToTestrail();
            // user input name of plan (orget it from previous
            // get plan to be rerun
            // AccessTestrail.AddPlan
            Console.WriteLine("Enter ID of plan to be re-run");
            string planID = Console.ReadLine();
            Console.WriteLine("Enter project ID to put new plan");
            string projectID = Console.ReadLine();

            CreateRerunPlan(client, planID, projectID);
        }

        public static void CreateRerunPlan(APIClient client, string planId, string projectID)
        {
            JObject planObject =  AccessTestRail.GetPlan(client, planId);
            List<Run> runIDs = AccessTestRail.GetRunsInPlan(planObject);
            List<Dictionary<string, object>> planEntries = new List<Dictionary<string, object>>();
            List<Dictionary<string, object>> runList = new List<Dictionary<string, object>>();
            List<Run> editedRuns = GetCasesInRun(client, runIDs);

            for (int j = 0; j < suiteIDs.Count; j++)
            {
                //for (int i = 0; i < editedRuns.Count; i++)
                //{
                //if (!string.IsNullOrEmpty(editedRuns[i].CaseIDs[0]))

                //}
                List<Run> runsForSuite = editedRuns.FindAll(x => x.SuiteID == suiteIDs[j]);

                for (int i = 0; i < runsForSuite.Count; i++)
                {
                    Dictionary<string, object> runInPlanObject = StringManipulation.RunsInPlan(false, runsForSuite[i]);
                    runList.Add(runInPlanObject);
                    //object runArray = runList.ToArray();
                    //Dictionary<string, object> planEntry = StringManipulation.PlanEntry(runsForSuite[i].SuiteID, false, runArray);
                    ////create list of plan entries, convert to array, put into new plan
                    //planEntries.Add(planEntry);
                }
                object runArray = runList.ToArray();
                Dictionary<string, object> planEntry = StringManipulation.PlanEntry(suiteIDs[j], false, runArray);
                //create list of plan entries, convert to array, put into new plan
                planEntries.Add(planEntry);

            }
            object entriesArray = planEntries.ToArray();
            Dictionary<string, object> newPlan = StringManipulation.NewPlan("Re-run plan", entriesArray); //change name

            var json = JsonConvert.SerializeObject(newPlan);
            Console.Write(json);
            AccessTestRail.AddPlan(client, projectID, json);
        }

        public static List<Run> GetCasesInRun(APIClient client, List<Run> runs)
        {
            List<Run> fullRuns = new List<Run>();
            for (int i = 0; i < runs.Count; i++)
            {
                Run currentRun = runs[i];
                string runId = currentRun.RunID;
                string caseID = "";
                string status = "";
                List<string> caseIdList = new List<string>();

                JArray testsArray = AccessTestRail.GetTestsInRun(client, runId);
                // need to get list of case ids
                for (int j = 0; j < testsArray.Count; j++)
                {
                    JObject testObject = testsArray[j].ToObject<JObject>();

                    status = StringManipulation.GetStatus(testObject.Property("status_id").Value.ToString());

                    if (status == "Passed")
                    {
                        break;
                    }

                    if (testObject.Property("case_id").Value != null && !string.IsNullOrWhiteSpace(testObject.Property("case_id").Value.ToString()))
                    {
                        caseID = testObject.Property("case_id").Value.ToString();
                        caseIdList.Add(caseID);
                    }
                }

                if (status != "Passed")
                {
                    string[] caseIDs = caseIdList.ToArray();
                    //currentRun.CaseIDs = caseIDs;

                    Run newRun;
                    newRun.CaseIDs = caseIDs;
                    newRun.Config = currentRun.Config;
                    newRun.ConfigIDs = currentRun.ConfigIDs;
                    newRun.RunID = currentRun.RunID;
                    newRun.SuiteID = currentRun.SuiteID;
                    fullRuns.Add(newRun);

                    if (!suiteIDs.Contains(currentRun.SuiteID))
                    {
                        suiteIDs.Add(currentRun.SuiteID);
                    }
                }

                // then input info into StringManipulation.RunsInPlan ???
                // i hate this stupid api
            }
            return fullRuns;
        }
    }
}
