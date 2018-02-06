using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace TestRailPlanReRun
{
    public class StringManipulation
    {
        public static string HasSteps(JObject arrayObject)
        {
            if (arrayObject.Property("custom_steps") != null && !string.IsNullOrWhiteSpace(arrayObject.Property("custom_steps").Value.ToString()))
            {
                return "Yes";
            }
            else
            {
                return "No";
            }
        }

        public static string HasStepsSeparated(JObject arrayObject)
        {
            if (arrayObject.Property("custom_steps_separated") != null && !string.IsNullOrWhiteSpace(arrayObject.Property("custom_steps_separated").Value.ToString()))
            {
                return "Yes";
            }
            else
            {
                return "No";
            }
        }

        public static string IsInvalid(JObject arrayObject)
        {
            if (HasSteps(arrayObject) == "No" && HasStepsSeparated(arrayObject) == "No")
            {
                return "Invalid";
            }
            else
            {
                return "Valid";
            }
        }

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

        private Dictionary<string, object> CreateResultData(string status_id)
        {
            //objectdata needs to be a json i think
            //http://docs.gurock.com/testrail-api2/reference-results#add_result

            var resultObject = new Dictionary<string, object>
            {
                {"status_id", status_id}
            };
            return resultObject;
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

        public static Dictionary<string, object> PlanEntry(string suite_id, bool include_all, object runs)
        {
            var planObject = new Dictionary<string, object>
            {
                {"suite_id", suite_id},
                {"include_all", include_all = false},
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
