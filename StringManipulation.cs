using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TestRailPlanReRun
{
    public class StringManipulation
    {
        /// <summary>
        /// Converts the status number to a string
        /// </summary>
        /// <returns>The status.</returns>
        /// <param name="rawValue">Raw value.</param>
        public static string GetStatus(string rawValue)
        {
            switch (rawValue)
            {
                case "1":
                    return "Passed";
                case "2":
                    return "Blocked";
                case "3":
                    return "Untested";
                case "4":
                    return "Retest";
                case "5":
                    return "Failed";
                case "6":
                    return "Pending";
                default:
                    return "Other";
            }
        }

        public static Dictionary<string, object> NewPlan(string name, object entries)
        {
            var planObject = new Dictionary<string, object>
            {
                {"name", name},
                {"entries", entries}
            };
            return planObject;
        }

        public static Dictionary<string, object> PlanEntry(int suite_id, bool include_all, object runs, object allConfigIDs, object allCaseIDs)
        {
            var planObject = new Dictionary<string, object>
            {
                {"suite_id", suite_id},
                {"assignedto_id", 1},
                {"include_all", include_all = false},
                {"config_ids", allConfigIDs},
                {"case_ids", allCaseIDs},
                {"runs", runs}
            };
            return planObject;
        }

        public static Dictionary<string, object> RunsInPlan(bool include_all, MainClass.Run run)
        {
            var planObject = new Dictionary<string, object>
            {
                {"include_all", include_all = false},
                {"case_ids", run.CaseIDs},
                {"config_ids", run.ConfigIDs}
                //{"suite_id", run.SuiteID}
            };
            return planObject;
        }

    }
}
