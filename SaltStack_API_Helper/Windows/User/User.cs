using Newtonsoft.Json;
using System.Collections.Generic;

namespace SaltAPI
{
    public partial class Salt_API_Function
    {
        /// <summary>
        /// 创建 windows 用户
        /// </summary>
        /// <param name="minion"></param>
        /// <param name="name">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="fullname">全名(默认会与用户名相同)</param>
        /// <param name="description">描述</param>
        /// <param name="groups">隶属组</param>
        /// <returns></returns>
        public static Dictionary<string, bool> Win_UserAdd(List<string> minion, string name, string password = null, string fullname = null, string description = null, string groups = null)
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = "list";
            rct.tgt = minion;
            rct.fun = "user.add";
            rct.arg = new List<string>() { name, password, fullname, description, groups };
            return JsonConvert.DeserializeObject<Dictionary<string, bool>>(CmdRunString(RunCmdTypeToString(rct)));
        }

        /// <summary>
        /// 将用户添加至用户组
        /// </summary>
        /// <param name="minion"></param>
        /// <param name="name">用户名</param>
        /// <param name="groups">组名</param>
        /// <returns></returns>
        public static Dictionary<string, bool> Win_AddUserToGroup(List<string> minion, string name, string groups)
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = "list";
            rct.tgt = minion;
            rct.fun = "user.addgroup";
            rct.arg = new List<string>() { name, groups };
            return JsonConvert.DeserializeObject<Dictionary<string, bool>>(CmdRunString(RunCmdTypeToString(rct)));
        }

        /// <summary>
        /// 获得用户名的SID
        /// </summary>
        /// <param name="minion"></param>
        /// <param name="name">用户名</param>
        /// <returns></returns>
        public static Dictionary<string, string> Win_GetUserSID(List<string> minion, string name)
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = "list";
            rct.tgt = minion;
            rct.fun = "user.getUserSid";
            rct.arg = new List<string>() { name };
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
        }


        /// <summary>
        /// 获取指定 Windows 类型 minion 中所有用户的简单信息
        /// </summary>
        /// <param name="minion"></param>
        /// <returns></returns>
        public static Dictionary<string, List<WindowsAllUserInfo>> Win_Getent(List<string> minion)
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = "list";
            rct.tgt = minion;
            rct.fun = "user.getent";
            rct.arg = new List<string>() { };
            return JsonConvert.DeserializeObject<Dictionary<string, List<WindowsAllUserInfo>>>(CmdRunString(RunCmdTypeToString(rct)));
        }

        /// <summary>
        /// 获取指定 Windows 类型 minion 中指定用户的详细信息
        /// </summary>
        /// <param name="minion"></param>
        /// <param name="name">用户名</param>
        /// <returns></returns>
        public static Dictionary<string, WindowsUserInfo> Win_UserInfo(List<string> minion, string name)
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = "list";
            rct.tgt = minion;
            rct.fun = "user.info";
            rct.arg = new List<string>() { name };
            var cc = CmdRunString(RunCmdTypeToString(rct));
            return JsonConvert.DeserializeObject<Dictionary<string, WindowsUserInfo>>(CmdRunString(RunCmdTypeToString(rct)));
        }

    }
}
