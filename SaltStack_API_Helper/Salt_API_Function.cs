using Codeplex.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SaltAPI
{
    public partial class Salt_API_Function
    {
        /// <summary>
        /// API 所需信息
        /// </summary>
        public static string APIurl = "";

        /// <summary>
        /// 登陆API的账户信息
        /// </summary>
        public static LoginRequestType loginRequestInfo = null;


        /// <summary>
        /// API URL 类型枚举
        /// </summary>
        public enum APIType
        {
            ROOT,
            LOGIN,
            LOGOUT,
            MINIONS,
            JOBS,
            RUN,
            EVENTS,
            WS,
            HOOK,
            STATS
        }

        /// <summary>
        ///  URL 拼接
        /// </summary>
        /// <param name="baseUri"></param>
        /// <param name="relativeOrAbsoluteUri"></param>
        /// <returns></returns>
        public static string CombineUriToString(string baseUri, string relativeOrAbsoluteUri)
        {
            return new Uri(new Uri(baseUri), relativeOrAbsoluteUri).ToString();
        }

        /// <summary>
        /// API url 拼接
        /// </summary>
        /// <param name="at"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        public static string APIUrlSelect(APIType at, string arg = "")
        {
            switch (at)
            {
                case APIType.ROOT:
                    return APIurl;
                case APIType.LOGIN:
                    return CombineUriToString(APIurl, "login");
                case APIType.LOGOUT:
                    return CombineUriToString(APIurl, "logout");
                case APIType.MINIONS:
                    if (!string.IsNullOrEmpty(arg))
                    {
                        return CombineUriToString(APIurl, "minions/" + arg);
                    }
                    else
                    {
                        return CombineUriToString(APIurl, "minions");
                    }
                case APIType.JOBS:
                    if (!string.IsNullOrEmpty(arg))
                    {
                        return CombineUriToString(APIurl, "jobs/" + arg);
                    }
                    else
                    {
                        return CombineUriToString(APIurl, "jobs");
                    }
                case APIType.RUN:
                    return CombineUriToString(APIurl, "run");
                case APIType.EVENTS:
                    return CombineUriToString(APIurl, "events");
                case APIType.WS:
                    if (!string.IsNullOrEmpty(arg))
                    {
                        return CombineUriToString(APIurl, "ws/" + arg);
                    }
                    else
                    {
                        return CombineUriToString(APIurl, "ws");
                    }
                case APIType.HOOK:
                    return CombineUriToString(APIurl, "hook");
                case APIType.STATS:
                    return CombineUriToString(APIurl, "stats");
                default:
                    return "";
            }
        }

        /// <summary>
        /// 命令转为JSON
        /// </summary>
        /// <param name="rct"></param>
        /// <returns></returns>
        public static string RunCmdTypeToString(RunCmdType rct)
        {
            return CmdToString(rct);
        }

        /// <summary>
        /// 命令转为JSON
        /// </summary>
        /// <param name="rctna"></param>
        /// <returns></returns>
        public static string RunCmdTypeNoArgToString(RunCmdTypeNoArg rctna)
        {
            return CmdToString(null, rctna);
        }

        /// <summary>
        /// 命令转为JSON
        /// </summary>
        /// <param name="rct"></param>
        /// <param name="rctna"></param>
        /// <returns></returns>
        public static string CmdToString(RunCmdType rct = null, RunCmdTypeNoArg rctna = null)
        {
            if (rct != null)
            {
                return JsonConvert.SerializeObject(rct);
            }
            else if (rctna != null)
            {
                return JsonConvert.SerializeObject(rctna);
            }
            return "";
        }


        /// <summary>
        /// API登陆方法
        /// Token有过期时间，需要偶尔调用一次
        /// </summary>
        /// <param name="lrt">登陆信息</param>
        /// <param name="url">API登陆的Url</param>
        /// <returns></returns>
        public static LoginResponseType Login()
        {
            if (loginRequestInfo != null)
            {
                var json = JsonConvert.SerializeObject(loginRequestInfo);
                var resp = JsonConvert.DeserializeObject<BaseType>(HttpUtilities.APIWebHelper(APIUrlSelect(APIType.LOGIN), HttpUtilities.HttpRequestMethod.POST, json));
                var loginInfo = JsonConvert.DeserializeObject<LoginResponseType>(resp.@return[0].ToString());
                RequestType.xAuthToken = loginInfo.token;
                return loginInfo;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 用于直接返回命令 return 内的第一个信息
        /// 获得的值直接转为String
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static string CmdRunString(string json)
        {
            return CmdRun(json).@return[0].ToString();
        }

        /// <summary>
        /// 命令执行
        /// </summary>
        /// <param name="rct">命令参数</param>
        /// <param name="rctna">命令参数(不含arg和match)</param>
        /// <returns></returns>
        public static BaseType CmdRun(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            var resp = HttpUtilities.APIWebHelper(APIurl, HttpUtilities.HttpRequestMethod.POST, json);
            if (resp.Length == 3)
            {

                return new BaseType() { @return = new List<object> { "ERROR", resp } };
            }
            return JsonConvert.DeserializeObject<BaseType>(resp);
        }

        /// <summary>
        /// minion 信息查询
        /// </summary>
        /// <returns>value 需要使用dynamic类型才可调用</returns>
        public static Dictionary<string, DynamicJson> minions()
        {
            var resp = HttpUtilities.APIWebHelper(APIUrlSelect(APIType.MINIONS), HttpUtilities.HttpRequestMethod.GET);
            var minion = JsonConvert.DeserializeObject<BaseType>(resp);
            Dictionary<string, DynamicJson> minionDictionary = new Dictionary<string, DynamicJson>();
            var allMinion = JsonConvert.DeserializeObject<Dictionary<string, object>>(minion.@return[0].ToString());

            foreach (var m in allMinion.Keys)
            {
                var aa = DynamicJson.Parse(allMinion[m].ToString());
                minionDictionary.Add(m, aa);
            }
            return minionDictionary;
        }

        /// <summary>
        /// 获取所有Minion状态
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, List<string>> MinionsStatus()
        {
            RunCmdType rct = new RunCmdType();
            rct.fun = "manage.status";
            rct.client = "runner";
            var b = CmdRunString(RunCmdTypeToString(rct));
            return JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(b);
        }

        /// <summary>
        /// 获取所有minion的Key
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, List<string>> GetAllMinionsKey()
        {
            RunCmdType rct = new RunCmdType();
            rct.fun = "key.list_all";
            rct.client = "wheel";
            var c = DynamicJson.Parse(CmdRunString(RunCmdTypeToString(rct)));
            Dictionary<string, List<string>> l = new Dictionary<string, List<string>>();
            string aaa = c.data.@return.minions.ToString().Replace("[", "").Replace("]", "").Replace("\"", "");
            l.Add("minions", aaa.Split(',').ToList());
            aaa = c.data.@return.minions_denied.ToString().Replace("[", "").Replace("]", "").Replace("\"", "");
            l.Add("minions_denied", aaa.Split(',').ToList());
            aaa = c.data.@return.minions_pre.ToString().Replace("[", "").Replace("]", "").Replace("\"", "");
            l.Add("minions_pre", aaa.Split(',').ToList());
            aaa = c.data.@return.minions_rejected.ToString().Replace("[", "").Replace("]", "").Replace("\"", "");
            l.Add("minions_rejected", aaa.Split(',').ToList());
            return l;
        }

        /// <summary>
        ///   fun 参数有这几种
        //      accept 接受
        //      delete 删除
        //      finge 返回minions的指纹
        //      key_str 返回key的内容
        //      list_all 列出所有key
        //      reject 拒绝
        //    minions 多台主机用list传入
        //      like this  minions=["SSS", "BBB"]
        /// </summary>
        /// <param name="minionName"></param>
        /// <param name="fun"></param>
        /// <returns></returns>
        public static bool ManageMinions(List<string> minionName, string fun)
        {
            try
            {
                RunCmdType rct = new RunCmdType();
                rct.client = "wheel";
                rct.fun = "key." + fun;
                rct.match = minionName;
                var r = DynamicJson.Parse(CmdRunString(RunCmdTypeToString(rct)));
                bool isSuccess = r.data.success;
                return isSuccess;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取 minion 的详细信息
        /// </summary>
        /// <param name="minionName"> minion 名称</param>
        /// <returns></returns>
        public static dynamic MinionDetails(string minionName)
        {
            var r = JsonConvert.DeserializeObject<BaseType>(HttpUtilities.APIWebHelper(APIUrlSelect(APIType.MINIONS, minionName), HttpUtilities.HttpRequestMethod.GET));
            return DynamicJson.Parse(r.@return[0].ToString());
        }

        /// <summary>
        /// 获取所有 Job
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, object>> Jobs()
        {
            try
            {
                var r = HttpUtilities.APIWebHelper(APIUrlSelect(APIType.JOBS), HttpUtilities.HttpRequestMethod.GET);
                var baseType = JsonConvert.DeserializeObject<BaseType>(r).@return[0].ToString();
                return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(baseType);
            }
            catch
            {
                return null;
            }


        }

        /// <summary>
        /// 详细查询特定 Job
        /// </summary>
        /// <param name="jid"></param>
        /// <returns></returns>
        public static Dictionary<string, List<Dictionary<string, dynamic>>> job(string jid)
        {
            var r = HttpUtilities.APIWebHelper(APIUrlSelect(APIType.JOBS, jid), HttpUtilities.HttpRequestMethod.GET);
            return JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, dynamic>>>>(r);
        }

    }
}
