using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace SaltAPI
{
    public partial class Salt_API_Function
    {
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

                if (rct == null)
                {
                    rct = new RunCmdType();
                    rct.client = "local";
                    rct.tgt = "os:Windows";
                    rct.expr_form = "grain";
                    rct.fun = "xjoker_win_service.get_service_status";
                    rct.arg = new List<string> { };
                }

                var r = JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
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
                            var z = i.Replace("\r\n", ",").Split(',');
                            serviceListTemp.Add(
                                z[0].Replace("Name", "").Replace(":", "").Trim(),
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
                                temp.Add(item, SVNOperation(p, SaltAPI.SVNOperation.update, minionName, svnUsername, svnPassword, svnVersion.ToString()));
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
    }
}
