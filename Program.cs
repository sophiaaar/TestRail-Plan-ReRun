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
        public static List<int> suiteIDs = new List<int>();
        public static List<int> allConfigIDs = new List<int>();
        public static List<int> allCaseIDs = new List<int>();


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
            public int[] ConfigIDs;
            public int[] CaseIDs;
            public int SuiteID;
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

            Console.WriteLine("Enter ID of plan to be re-run");
            string planID = Console.ReadLine();
            Console.WriteLine("Enter project ID to put new plan");
            string projectID = Console.ReadLine();

            CreateRerunPlan(client, planID, projectID);
        }

        public static void CreateRerunPlan(APIClient client, string planId, string projectID)
        {
            JObject planObject =  AccessTestRail.GetPlan(client, planId);
            string planName = planObject.Property("name").Value.ToString();
            //var json = JsonConvert.SerializeObject(planObject);
            ////var test = JsonConvert.SerializeObject(json);
            //Console.Write(json);
            List<Run> runIDs = AccessTestRail.GetRunsInPlan(planObject);
            List<Dictionary<string, object>> planEntries = new List<Dictionary<string, object>>();

            List<Run> editedRuns = GetCasesInRun(client, runIDs);

            for (int j = 0; j < suiteIDs.Count; j++)
            {

                List<Dictionary<string, object>> runList = new List<Dictionary<string, object>>();
                List<Run> runsForSuite = editedRuns.FindAll(x => x.SuiteID == suiteIDs[j]);
                List<int> casesInRunsInSuite = new List<int>();
                List<int> configsInRunsInSuite = new List<int>();

                for (int i = 0; i < runsForSuite.Count; i++)
                {
                    Dictionary<string, object> runInPlanObject = StringManipulation.RunsInPlan(false, runsForSuite[i]);
                    runList.Add(runInPlanObject);

                    foreach (int caseID in runsForSuite[i].CaseIDs)
                    {
                        if (!casesInRunsInSuite.Contains(caseID))
                        {
                            casesInRunsInSuite.Add(caseID);
                        }
                    }

                    foreach (int configID in runsForSuite[i].ConfigIDs)
                    {
                        if (!configsInRunsInSuite.Contains(configID))
                        {
                            configsInRunsInSuite.Add(configID);
                        }
                    }

                }
                object runArray = runList.ToArray();
                //object configArray = allConfigIDs.ToArray();
                //object caseArray = allCaseIDs.ToArray(); // we don't want all case ids here
                object caseArray = casesInRunsInSuite.ToArray();
                object configArray = configsInRunsInSuite.ToArray();

                Dictionary<string, object> planEntry = StringManipulation.PlanEntry(suiteIDs[j], false, runArray, configArray, caseArray);
                //create list of plan entries, convert to array, put into new plan
                planEntries.Add(planEntry);

            }
            object entriesArray = planEntries.ToArray();
            Dictionary<string, object> newPlan = StringManipulation.NewPlan(planName + " re-run", entriesArray); //change name

            var json = JsonConvert.SerializeObject(newPlan);
            ////var test = JsonConvert.SerializeObject(json);
            Console.Write(json);
            //string json = newPlan.ToString();
            //Console.Write(json);
             AccessTestRail.AddPlan(client, projectID, newPlan);
        }

        public static List<Run> GetCasesInRun(APIClient client, List<Run> runs)
        {
            List<Run> fullRuns = new List<Run>();
            for (int i = 0; i < runs.Count; i++)
            {
                Run currentRun = runs[i];
                string runId = currentRun.RunID;
                int caseID = 0;
                string status = "";
                List<int> caseIdList = new List<int>();

                JArray testsArray = AccessTestRail.GetTestsInRun(client, runId);
                // need to get list of case ids
                for (int j = 0; j < testsArray.Count; j++)
                {
                    JObject testObject = testsArray[j].ToObject<JObject>();

                    status = StringManipulation.GetStatus(testObject.Property("status_id").Value.ToString());

                    if (testObject.Property("case_id").Value != null && !string.IsNullOrWhiteSpace(testObject.Property("case_id").Value.ToString()) && status != "Passed")
                    {
                        caseID = Int32.Parse(testObject.Property("case_id").Value.ToString());
                        caseIdList.Add(caseID); //here we add the case id before we even check if it has failed. why!!!!
                    }
                }

                if (status != "Passed") // we just want the case id of the test that didnt pass
                {
                    int[] caseIDs = caseIdList.ToArray();

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

                    foreach (int configID in newRun.ConfigIDs)
                    {
                        if (!allConfigIDs.Contains(configID))
                        {
                            allConfigIDs.Add(configID);
                        }
                    }

                    foreach (int caseIDloop in newRun.CaseIDs)
                    {
                        if (!allCaseIDs.Contains(caseIDloop))
                        {
                            allCaseIDs.Add(caseIDloop);
                        }
                    }
                }
            }
            return fullRuns;
        }
    }
}
