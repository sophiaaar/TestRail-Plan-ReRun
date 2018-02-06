using System;
using System.Collections.Generic;
using Gurock.TestRail;
using Newtonsoft.Json.Linq;

namespace TestRailPlanReRun
{
    public static class MainClass
    {
        private static readonly IConfigReader _configReader = new ConfigReader();

        public static List<Run> runs = new List<Run>();


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
        }

        public static void GetPlanInfo(APIClient client, string planId)
        {
            JObject planObject =  AccessTestRail.GetPlan(client, planId);
            List<string> runIDs = AccessTestRail.GetRunsInPlan(planObject, runs);
        }

        private static void GetAllTests(APIClient client, int previousResults, List<string> runIDs)
        {
            List<Case> listOfCases = new List<Case>();
            List<Test> listOfTests = new List<Test>();
            List<Suite> listOfSuites = new List<Suite>();

            JArray suitesArray = AccessTestRail.GetSuitesInProject(client, "2");

            for (int i = 0; i < suitesArray.Count; i++)
            {
                JObject arrayObject = suitesArray[i].ToObject<JObject>();
                string id = arrayObject.Property("id").Value.ToString();
                string suiteName = arrayObject.Property("name").Value.ToString(); //create list of suiteNames to use later

                Suite newSuite;
                newSuite.SuiteID = id;
                newSuite.SuiteName = suiteName;
                listOfSuites.Add(newSuite);


                //JArray casesArray = AccessTestRail.GetCasesInSuite(client, "2", id);
                //listOfCases = CreateListOfCases(casesArray, listOfCases, id, suiteName);
            }


            for (int i = 0; i < runIDs.Count; i++)
            {
                JArray testsArray = AccessTestRail.GetTestsInRun(client, runIDs[i]);

                string testID = "";
                int caseID = 0;
                string title = "";
                string status = "";

                for (int j = 0; j < testsArray.Count; j++)
                {
                    JObject testObject = testsArray[j].ToObject<JObject>();

                    testID = testObject.Property("id").Value.ToString();

                    if (testObject.Property("case_id").Value != null && !string.IsNullOrWhiteSpace(testObject.Property("case_id").Value.ToString()))
                    {
                        caseID = Int32.Parse(testObject.Property("case_id").Value.ToString());
                    }
                    //caseIDsInMilestone.Add(caseID);

                    title = testObject.Property("title").Value.ToString();
                    status = StringManipulation.GetStatus(testObject.Property("status_id").Value.ToString());

                    if (status == "Passed")
                    {
                        numberPassed++;
                    }
                    else if (status == "Failed")
                    {
                        numberFailed++;
                        Case failedCase;
                        failedCase.SuiteID
                    }
                    else if (status == "Blocked")
                    {
                        numberBlocked++;
                    }

                    string suiteName = "";

                    // Some suites have been deleted, but the tests and runs remain
                    if (suiteInPlanIDs[i] != "0")
                    {
                        Suite currentSuite = listOfSuites.Find(x => x.SuiteID == suiteInPlanIDs[i]);
                        suiteName = currentSuite.SuiteName;
                    }
                    else
                    {
                        suiteName = "deleted";
                    }

                    // Get the most recent defects/bugs and comments on the test
                    string defects = "";
                    string comment = "";
                    string editorVersion = "";

                    JArray resultsOfLatestTest = AccessTestRail.GetLatestResultsOfTest(client, testID, "1");

                    for (int k = 0; k < resultsOfLatestTest.Count; k++)
                    {
                        JObject resultObject = resultsOfLatestTest[k].ToObject<JObject>();

                        defects = resultObject.Property("defects").Value.ToString();
                        comment = resultObject.Property("comment").Value.ToString();
                        editorVersion = resultObject.Property("custom_editorversion").Value.ToString();
                    }

                    // Find config for runID
                    Run currentRun = runs.Find(o => o.RunID == runIDs[i]);
                    string config = currentRun.Config;
                    string runID = currentRun.RunID;
                }

            }
        }
    }
}
