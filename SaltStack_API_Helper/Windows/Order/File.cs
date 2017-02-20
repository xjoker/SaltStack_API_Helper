using Newtonsoft.Json;
using System.Collections.Generic;

namespace SaltAPI
{
    public partial class Salt_API_Function
    {

        /// <summary>
        /// Windows 下的文件/文件夹移动
        /// </summary>
        /// <param name="minionName"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Win_FileMove(List<string> minionName, string src, string dst)
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = "list";
            rct.tgt = minionName;
            rct.fun = "xjoker_win.move";
            rct.arg = new List<string>() { src, dst };
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
        }


        /// <summary>
        /// Windows 下的文件/文件夹复制
        /// </summary>
        /// <param name="minionName"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Win_FileCopy(List<string> minionName, string src, string dst)
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = "list";
            rct.tgt = minionName;
            rct.fun = "xjoker_win.copy";
            rct.arg = new List<string>() { src, dst };
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
        }



    }
}
