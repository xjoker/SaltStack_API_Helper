using Newtonsoft.Json;
using System.Collections.Generic;

namespace SaltAPI
{
    public partial class Salt_API_Function
    {
        /// <summary>
        /// 创建GoodSync任务
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, string> GoodSyncNewJob(
            List<string> minion,
            string jobname,
            string f1,
            string f2,
            int ReadOnlySource = 0,
            int Direction = 1,
            int CleanupOldGenerations = 0,
            int CopyCreateTime = 0,
            int WaitForLocks = 0,
            int WaitForLocksMinutes = 10,
            string exclude = "",
            string include = "",
            int LimitChangesPercent = 100,
            int OnFileChangeAction = 1,
            int OnTimerAction = 2,
            int TimerIntervalMinutes = 10,
            int AutoResolveConflicts = 1,
            int DetectMovesAndRenames = 0,
            int UberUnlockedUpload = 0,
            int Option = 0
            )
        {
            string minionList = "";
            foreach (var item in minion)
            {
                minionList = string.Join(",", "\"" + item + "\"");
            }

            string poJson = "{\"fun\":\"xjoker_goodsync.jobnew\",\"expr_form\":\"list\",\"client\":\"local\"," +
                    $"\"tgt\":[{minionList}]," +
                    $"\"arg\":[\"{ jobname }\",\"{ f1.Replace("\\", "\\\\") }\",\"{ f2.Replace("\\", "\\\\") }\",{ ReadOnlySource },{ Direction },{ CleanupOldGenerations },{ CopyCreateTime },{ WaitForLocks },{ WaitForLocksMinutes },\"{ exclude }\",\"{ include }\",{ LimitChangesPercent },{ OnFileChangeAction },{ OnTimerAction },{ TimerIntervalMinutes },{ AutoResolveConflicts },{ DetectMovesAndRenames },{ UberUnlockedUpload },{ Option }]}}";

            JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(poJson));

            poJson = $"{{\"fun\":\"xjoker_goodsync.jobsyncall\",\"expr_form\":\"list\",\"client\":\"local\",\"tgt\":[{minionList}],\"arg\":[]}}";

            var r = JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(poJson));
            return r;
        }


        /// <summary>
        /// 同步Goodsync任务
        /// </summary>
        /// <param name="RunAsUsername"></param>
        /// <param name="minion"></param>
        /// <param name="jobName"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GoodSyncSyncJob(List<string> minion, string jobName = "")
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = "list";
            rct.tgt = minion;
            if (string.IsNullOrWhiteSpace(jobName))
            {
                rct.fun = "xjoker_goodsync.jobanalyzeall";
                rct.arg = new List<string>() {  };
                CmdRunString(RunCmdTypeToString(rct));
                rct.fun = "xjoker_goodsync.jobsyncall";
                rct.arg = new List<string>() {  };
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
            }
            else
            {
                rct.fun = "xjoker_goodsync.jobanalyze";
                rct.arg = new List<string>() {  jobName };
                CmdRunString(RunCmdTypeToString(rct));
                rct.fun = "xjoker_goodsync.jobsync";
                rct.arg = new List<string>() {  jobName };
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
            }
        }

        /// <summary>
        /// 删除指定GoodSync 的 Job
        /// </summary>
        /// <param name="RunAsUsername"></param>
        /// <param name="minion"></param>
        /// <param name="jobName"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GoodSyncDeleteJob(List<string> minion, string jobName)
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = "list";
            rct.tgt = minion;
            rct.fun = "xjoker_goodsync.jobdelete";
            rct.arg = new List<string>() { jobName };
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
        }
    }
}
