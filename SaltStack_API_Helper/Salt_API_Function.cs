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
    public class Salt_API_Function
    {
        /// <summary>
        /// API 所需信息
        /// </summary>
        public static string APIurl = "";

        /// <summary>
        /// 登陆API的账户信息
        /// </summary>
        public static LoginRequestType LoginRequestInfo = null;


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
        /// </summary>
        /// <param name="lrt">登陆信息</param>
        /// <param name="url">API登陆的Url</param>
        /// <returns></returns>
        public static LoginResponseType Login()
        {
            if (LoginRequestInfo != null)
            {
                var json = JsonConvert.SerializeObject(LoginRequestInfo);
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
            if (resp.Length==3)
            {

                return new BaseType() { @return=new List<object> { "ERROR",resp } };
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
        /// 获取所有服务的状态
        /// 只可使用与Windows
        /// </summary>
        /// <param name="rct"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, bool>> WindowsServicesStatusGet(RunCmdTypeNoArg rct = null)
        {
            try
            {
                // minion名称 <服务名称,状态>
                Dictionary<string, Dictionary<string, bool>> list = new Dictionary<string, Dictionary<string, bool>>();
                if (rct == null)
                {
                    rct = new RunCmdTypeNoArg();
                }
                rct.tgt = "os:Windows";
                rct.expr_form = "grain";
                rct.client = "local";
                rct.fun = "service.get_enabled";
                var enableServiceList = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(CmdRunString(RunCmdTypeNoArgToString(rct)));
                rct.fun = "service.get_disabled";
                var disableServiceList = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(CmdRunString(RunCmdTypeNoArgToString(rct)));

                foreach (var minionName in enableServiceList)
                {

                    Dictionary<string, bool> temp = new Dictionary<string, bool>();
                    // 处于启用状态的服务
                    foreach (var item in minionName.Value)
                    {
                        temp.Add(item, true);
                    }
                    // 处于禁用状态的服务
                    foreach (var item in disableServiceList[minionName.Key])
                    {
                        temp.Add(item, false);
                    }

                    list.Add(minionName.Key, temp);
                }
                return list;
            }
            catch
            {
                return null;
            }

        }


        /// <summary>
        /// Windows 创建服务
        /// </summary>
        /// <param name="minionName">Minion名称</param>
        /// <param name="serviceName">服务名称</param>
        /// <param name="servicePath">服务可执行文件路径</param>
        /// <param name="serviceDisplayName">服务显示名称</param>
        /// <returns></returns>
        public static Dictionary<string, bool> WindowsServiceCreate(string minionName, string serviceName, string servicePath, string serviceDisplayName)
        {
            try
            {
                List<string> args = new List<string>();
                args.Add(serviceName);
                args.Add(servicePath);
                args.Add(serviceDisplayName);

                RunCmdType rct = new RunCmdType();
                rct.client = "local";
                rct.expr_form = "glob";
                rct.tgt = new List<string> { minionName };
                rct.fun = "service.create";
                rct.arg = args;

                var r = JsonConvert.DeserializeObject<Dictionary<string, bool>>(CmdRunString(RunCmdTypeToString(rct)));
                return r;
            }
            catch
            {
                return null;
            }

        }

        /// <summary>
        /// 获取 Windows 服务真实状态
        /// </summary>
        /// <param name="minionName">Minion 名称</param>
        /// <param name="serviceName">服务名称(非显示名)</param>
        /// <returns></returns>
        public static bool WindowsGetServiceRealStatus(string minionName, string serviceName)
        {
            try
            {
                RunCmdType rct = new RunCmdType();
                rct.tgt = new List<string> { minionName };
                rct.fun = "service.status";
                rct.expr_form = "glob";
                rct.client = "local";
                rct.arg = new List<string>() { serviceName };

                var r = JsonConvert.DeserializeObject<Dictionary<string, bool>>(CmdRunString(RunCmdTypeToString(rct)));
                return r[minionName];
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 自定义方法获取Windows 服务准确的当前状态
        /// 通过 Powershell 的 get-service 方法
        /// </summary>
        /// <param name="rct"></param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, List<string>>> WindowsServicesStatusGetByPowershell(RunCmdType rct = null)
        {
            try
            {
                var list = new Dictionary<string, Dictionary<string, List<string>>>();

                if (rct==null)
                {
                    rct = new RunCmdType();
                    rct.client = "local";
                    rct.tgt = "os:Windows";
                    rct.expr_form = "grain";
                    rct.fun = "xjoker_win_service.get_service_status";
                    rct.arg =new List<string> { };
                }
                
                var r=JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
                foreach (var minion in r.Keys)
                {
                    if (!r[minion].Contains("is not available."))
                    {
                        var service = r[minion].Trim().Replace("\r\n\r\n", ",").Split(',');
                        var serviceListTemp = new Dictionary<string, List<string>>();
                        foreach (var i in service)
                        {
                            /*
                                0 = 服务名称
                                1 = 服务状态
                                2 = 服务显示名称
                            */
                            var z =i.Replace("\r\n", ",").Split(',');
                            serviceListTemp.Add(
                                z[0].Replace("Name","").Replace(":","").Trim(), 
                                new List<string>() {
                                    z[1].Replace("Status","").Replace(":","").Trim(),
                                    z[2].Replace("DisplayName","").Replace(":","").Trim()
                                });
                        }
                        list.Add(minion, serviceListTemp);
                    }
                    
                }
                return list;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Windows 服务操作方法
        /// </summary>
        /// <param name="minionName">Minion名称</param>
        /// <param name="so">操作模式</param>
        /// <param name="serviceName">服务名称</param>
        /// <param name="svnUsername">SVN 用户名</param>
        /// <param name="svnPassword">SVN 密码</param>
        /// <param name="svnVersion">SVN 版本</param>
        /// <returns></returns>
        public static Dictionary<string, dynamic> WindowsServiceOperation(List<string> minionName, ServiceOperation so, string serviceName, string svnUsername = "", string svnPassword = "", int? svnVersion = null)
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = "list";
            rct.tgt = minionName;
            rct.arg = new List<string>() { serviceName };

            switch (so)
            {
                case ServiceOperation.start:
                    rct.fun = "service.start";
                    break;
                case ServiceOperation.stop:
                    rct.fun = "service.stop";
                    break;
                case ServiceOperation.enable:
                    rct.fun = "service.enable";
                    break;
                case ServiceOperation.disable:
                    rct.fun = "service.disable";
                    break;
                case ServiceOperation.delete:
                    rct.fun = "service.delete";
                    break;
                case ServiceOperation.restart:
                    rct.fun = "service.restart";
                    break;
                case ServiceOperation.update:
                    var comm = string.Format("reg query \"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\{0}\" | find /i  \"ImagePath\"", serviceName);
                    rct.fun = "cmd.run";
                    rct.arg = new List<string> { comm };
                    var resp = JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
                    Dictionary<string, dynamic> temp = new Dictionary<string, dynamic>();
                    foreach (var item in minionName)
                    {
                        var bbb = resp[item].Trim().Replace("    ", ",").Split(',');
                        if (bbb.Length == 3)
                        {
                            string p = Path.GetDirectoryName(bbb[2].Replace("\"", ""));
                            if (svnVersion != null)
                            {
                                temp.Add(item, SVNOperation(p, SaltAPI.SVNOperation.update,  minionName, svnUsername, svnPassword, svnVersion.ToString()));
                            }
                            else
                            {
                                temp.Add(item, SVNOperation(p, SaltAPI.SVNOperation.update, minionName, svnUsername, svnPassword));
                            }
                        }
                    }
                    return temp;
                default:
                    return null;
            }

            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(CmdRunString(RunCmdTypeToString(rct)));
        }



        /// <summary>
        ///  SVN 操作方法
        /// </summary>
        /// <param name="filePath">SVN路径</param>
        /// <param name="so">SVN命令模式</param>
        /// <param name="svnUsername">SVN 用户名</param>
        /// <param name="svnPassword"> SVN 密码</param>
        /// <param name="minionName">Minion 机器名</param>
        /// <param name="version">版本</param>
        /// <param name="source">源</param>
        /// <param name="st">使用自带模块还是私有模块</param>
        /// <param name="fun">使用私有模块的时候需要提供模块名称</param>
        /// <returns></returns>
        public static string SVNOperation(string filePath, SVNOperation so, List<string> minionName, string svnUsername="", string svnPassword="", string remote="",int? version = null, string source = "", SVNType st = SVNType.salt, string fun = "")
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = "list";
            rct.tgt = minionName;

            string opts = "--trust-server-cert";
            string rversion = "";
            if (version != null)
            {
                opts = string.Format("-r {0} --trust-server-cert", version);
                rversion = version.ToString();
            }


            switch (so)
            {
                case SaltAPI.SVNOperation.checkout:
                    switch (st)
                    {
                        case SVNType.salt:
                            rct.fun = "svn.checkout";
                            rct.arg = new List<string> { "username=" + svnUsername, "password=" + svnPassword, filePath, remote, opts };
                            return CmdRunString(RunCmdTypeToString(rct));
                        case SVNType.Order:
                            rct.fun = fun;
                            rct.arg = new List<string> { "username=" + svnUsername, "password=" + svnPassword, remote,filePath, "certCheck=False", "revision=" + rversion };
                            return CmdRunString(RunCmdTypeToString(rct));
                        default:
                            return null;
                    }
                case SaltAPI.SVNOperation.update:
                    switch (st)
                    {
                        case SVNType.salt:
                            rct.fun = "svn.update";
                            rct.arg = new List<string> { "username=" + svnUsername, "password=" + svnPassword, filePath, opts };
                            return CmdRunString(RunCmdTypeToString(rct));
                        case SVNType.Order:
                            rct.fun = fun;
                            rct.arg = new List<string> { "username=" + svnUsername, "password=" + svnPassword, "cwd=" + filePath, "certCheck=False", "revision=" + rversion };
                            return CmdRunString(RunCmdTypeToString(rct));
                        default:
                            return null;
                    }
                case SaltAPI.SVNOperation.commit:
                    break;
                case SaltAPI.SVNOperation.diff:
                    break;
                case SaltAPI.SVNOperation.export:
                    break;
                case SaltAPI.SVNOperation.info:
                    rct.arg = new List<string> {  filePath };
                    switch (st)
                    {
                        case SVNType.salt:
                            rct.fun = "svn.info";
                            return CmdRunString(RunCmdTypeToString(rct));
                        case SVNType.Order:
                            rct.fun = fun;
                            return CmdRunString(RunCmdTypeToString(rct));
                        default:
                            return null;
                    }
                case SaltAPI.SVNOperation.remove:
                    break;
                case SaltAPI.SVNOperation.status:
                    break;
                case SaltAPI.SVNOperation.@switch:
                    break;
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        /// 获取所有 Job
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, object>> Jobs()
        {
            var r = HttpUtilities.APIWebHelper(APIUrlSelect(APIType.JOBS), HttpUtilities.HttpRequestMethod.GET);
            var baseType = JsonConvert.DeserializeObject<BaseType>(r).@return[0].ToString();
            return JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(baseType);

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

        /// <summary>
        /// IIS 操作模块
        /// </summary>
        /// <param name="minionName">minion名称</param>
        /// <param name="io">操作方式</param>
        /// <param name="Name">站点或程序池名称</param>
        /// <param name="svnUsername">仅用于"update"模式</param>
        /// <param name="svnPassword">仅用于"update"模式</param>
        /// <param name="physicalPath">仅用于"update"模式</param>
        /// <param name="version">仅用于"update"模式</param>
        /// <returns></returns>
        public static Dictionary<string, string> IISOperation(List<string> minionName, IISOperation io, List<string> Name, string svnUsername = "", string svnPassword = "", string physicalPath = "", int? version = null)
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = "list";
            rct.tgt = minionName;
            rct.arg = Name;

            switch (io)
            {
                case SaltAPI.IISOperation.StartSite:
                    rct.fun = "xjoker_win_iis.start_site";
                    break;
                case SaltAPI.IISOperation.StopSite:
                    rct.fun = "xjoker_win_iis.stop_site";
                    break;
                case SaltAPI.IISOperation.RestartSite:
                    rct.fun = "xjoker_win_iis.restart_site";
                    break;
                case SaltAPI.IISOperation.RemoveSite:
                    rct.fun = "xjoker_win_iis.remove_site";
                    break;
                case SaltAPI.IISOperation.StartAppPool:
                    rct.fun = "xjoker_win_iis.start_apppool";
                    break;
                case SaltAPI.IISOperation.StopAppPool:
                    rct.fun = "xjoker_win_iis.stop_apppool";
                    break;
                case SaltAPI.IISOperation.RestartAppPool:
                    rct.fun = "xjoker_win_iis.restart_apppool";
                    break;
                case SaltAPI.IISOperation.RemoveAppPool:
                    rct.fun = "xjoker_win_iis.remove_apppool";
                    break;
                case SaltAPI.IISOperation.update:
                    return new Dictionary<string, string> {
                        { "update",SVNOperation(physicalPath, SaltAPI.SVNOperation.update, minionName,svnUsername, svnPassword, version.ToString(),st:SVNType.Order) }
                    };
                default:
                    return null;
            }


            return JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
        }

        /// <summary>
        /// 获取所有站点列表
        /// </summary>
        /// <param name="minionName">minion名称，如果为通配查询需要同时修改expr_form属性</param>
        /// <param name="expr_form"></param>
        /// <returns></returns>
        public static Dictionary<string, List<IISSiteType>> GetSiteList(dynamic minionName, string expr_form = "glob")
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = expr_form;
            rct.tgt = minionName;
            rct.fun = "xjoker_win_iis.list_sites_xml";
            rct.arg = new List<string> { };

            var r = JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
            if (r != null)
            {
                // 所有机器站点的List
                Dictionary<string, List<IISSiteType>> siteList = new Dictionary<string, List<IISSiteType>>();
                foreach (var item in r)
                {
                    if (item.Value.StartsWith("<") && item.Value.EndsWith(">"))
                    {
                        // 本台机器内所有站点List
                        List<IISSiteType> site = new List<IISSiteType>();
                        XmlDocument xml = new XmlDocument();
                        xml.LoadXml(item.Value);
                        XmlNodeList root = xml.SelectNodes("appcmd/SITE");
                        foreach (XmlNode i in root)
                        {

                            // 逐行处理bindings
                            List<Dictionary<string, List<string>>> bindDict = new List<Dictionary<string, List<string>>>();
                            for (int j = 0; j < i.SelectSingleNode("site/bindings").ChildNodes.Count; j++)
                            {
                                List<string> bindTemp = new List<string>();
                                var bindOneRow = i.SelectSingleNode("site/bindings").ChildNodes[j];
                                var bindInfo = bindOneRow.Attributes["bindingInformation"].Value.Split(':');
                                bindTemp.Add(bindInfo[0]);// IP 
                                bindTemp.Add(bindInfo[1]);// port
                                bindTemp.Add(bindInfo[2]);// domain

                                bindDict.Add(
                                    new Dictionary<string, List<string>>()
                                    {
                                        { bindOneRow.Attributes["protocol"].Value ,bindTemp}
                                    }
                                );
                            }

                            // 逐行处理虚拟目录
                            List<Dictionary<string, string>> app = new List<Dictionary<string, string>>();
                            for (int j = 0; j < i.SelectSingleNode("site/application").ChildNodes.Count; j++)
                            {
                                if (i.SelectSingleNode("site/application").ChildNodes[j].Name != "virtualDirectoryDefaults")
                                {
                                    var applicationOneRow = i.SelectSingleNode("site/application").ChildNodes[j];
                                    var path = applicationOneRow.Attributes["path"].Value;
                                    var physicalPath = applicationOneRow.Attributes["physicalPath"].Value;

                                    app.Add(
                                        new Dictionary<string, string>()
                                        {
                                            { path,physicalPath }
                                        }
                                    );
                                }

                            }
                            // 日志存放路径
                            // 如果没有特别指定则为空
                            string logFile;
                            if (i.SelectSingleNode("site/logFile").Attributes["directory"] == null)
                            {
                                logFile = null;
                            }
                            else
                            {
                                logFile = i.SelectSingleNode("site/logFile").Attributes["directory"].Value;
                            }

                            site.Add(new IISSiteType()
                            {
                                id = Convert.ToInt32(i.SelectSingleNode("site").Attributes["id"].Value),
                                siteName = i.SelectSingleNode("site").Attributes["name"].Value,
                                logFile = logFile,
                                physicalPath = i.SelectSingleNode("site/application/virtualDirectory").Attributes["physicalPath"].Value,
                                bindings = bindDict,
                                application = app
                            });

                        }
                        siteList.Add(item.Key, site);
                    }
                }
                return siteList;
            }

            return null;
        }


        /// <summary>
        /// 获取所有AppPool列表
        /// </summary>
        /// <param name="minionName">minion名称，如果为通配查询需要同时修改expr_form属性</param>
        /// <param name="expr_form"></param>
        /// <returns></returns>
        public static Dictionary<string, List<AppPoolType>> GetAppPoolList(dynamic minionName, string expr_form = "glob")
        {
            RunCmdType rct = new RunCmdType();
            rct.client = "local";
            rct.expr_form = expr_form;
            rct.tgt = minionName;
            rct.fun = "xjoker_win_iis.list_apppools_xml";
            rct.arg = new List<string> { };

            var r = JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));

            if (r != null)
            {
                Dictionary<string, List<AppPoolType>> siteList = new Dictionary<string, List<AppPoolType>>();
                foreach (var item in r)
                {
                    if (item.Value.StartsWith("<") && item.Value.EndsWith(">"))
                    {
                        List<AppPoolType> site = new List<AppPoolType>();
                        XmlDocument xml = new XmlDocument();
                        xml.LoadXml(item.Value);
                        XmlNodeList root = xml.SelectNodes("appcmd/APPPOOL");
                        foreach (XmlNode i in root)
                        {
                            string autoStart;
                            if (i.SelectSingleNode("add").Attributes["autoStart"] == null)
                            {
                                autoStart = null;
                            }
                            else
                            {
                                autoStart = i.SelectSingleNode("add").Attributes["autoStart"].Value;
                            }

                            site.Add(
                                new AppPoolType()
                                {
                                    name = i.Attributes["APPPOOL.NAME"].Value,
                                    pipelineMode = i.Attributes["PipelineMode"].Value,
                                    autoStart = autoStart,
                                    runtimeVersion = i.Attributes["RuntimeVersion"].Value,
                                    state = i.Attributes["state"].Value
                                }
                            );
                        }
                        siteList.Add(item.Key,site);
                    }
                }
                return siteList;
            }
            return null;
        }


    }
}
