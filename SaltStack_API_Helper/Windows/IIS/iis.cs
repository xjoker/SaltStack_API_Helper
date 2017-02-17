
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace SaltAPI
{
    public partial class Salt_API_Function
    {
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
                        { "update",SVNOperation(physicalPath, SaltAPI.SVNOperation.update, minionName,svnUsername, svnPassword, version:version,st:SVNType.Order,fun:"xjoker_svn.update") }
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
                                if (bindInfo.Length == 2)
                                {
                                    bindTemp.Add(bindInfo[1]);// IP 
                                    bindTemp.Add(bindInfo[0]);// port
                                    bindTemp.Add("");// domain
                                }
                                else if (bindInfo.Length == 3)
                                {
                                    bindTemp.Add(bindInfo[0]);// IP 
                                    bindTemp.Add(bindInfo[1]);// port
                                    bindTemp.Add(bindInfo[2]);// domain
                                }


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


                            // 站点状态判断
                            var siteState = i.Attributes["state"].Value;
                            bool state = false;
                            if (siteState == "Started")
                            {
                                state = true;
                            }

                            site.Add(new IISSiteType()
                            {
                                id = Convert.ToInt32(i.SelectSingleNode("site").Attributes["id"].Value),
                                siteName = i.SelectSingleNode("site").Attributes["name"].Value,
                                logFile = logFile,
                                physicalPath = i.SelectSingleNode("site/application/virtualDirectory").Attributes["physicalPath"].Value,
                                bindings = bindDict,
                                application = app,
                                siteState = state
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
                        siteList.Add(item.Key, site);
                    }
                }
                return siteList;
            }
            return null;
        }



        /// <summary>
        ///  编辑 IIS 站点绑定
        /// </summary>
        /// <param name="minionName">机器名称</param>
        /// <param name="domain">域名</param>
        /// <param name="ip">IP</param>
        /// <param name="port">端口</param>
        /// <returns></returns>
        public static bool SiteBindEdit(string minionName, IISBindOperation ibo, string siteName, string domain, string ip, string port)
        {
            if (!string.IsNullOrWhiteSpace(siteName) && !string.IsNullOrWhiteSpace(domain) && !string.IsNullOrWhiteSpace(ip) && !string.IsNullOrWhiteSpace(port))
            {
                string fun = "xjoker_win_iis.create_binding";
                if (ibo == IISBindOperation.Delete)
                {
                    fun = "xjoker_win_iis.remove_binding";
                }

                RunCmdType rct = new RunCmdType();
                rct.client = "local";
                rct.expr_form = "glob";
                rct.tgt = minionName;
                rct.fun = fun;
                rct.arg = new List<string> { siteName, domain, ip, port };

                var r = JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
                if (r != null && r.Keys.Contains(minionName))
                {
                    if (Convert.ToBoolean(r[minionName]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// IIS 站点创建
        /// </summary>
        /// <param name="minions">选定的minion</param>
        /// <param name="apppoolName">程序池名称</param>
        /// <param name="apppoolVersion">程序池版本</param>
        /// <param name="apppoolEnable32bit">是否启用32位模式</param>
        /// <param name="apppoolModel">程序池模式</param>
        /// <param name="siteName">站点名称</param>
        /// <param name="siteProtocol">站点协议</param>
        /// <param name="siteDomain">站点域名</param>
        /// <param name="sitePath">站点路径</param>
        /// <param name="siteLogPath">站点日志路径</param>
        /// <param name="siteIP">站点IP</param>
        /// <param name="sitePort">站点端口</param>
        /// <param name="siteRunasUsername">站点运行用户</param>
        /// <param name="siteRunasPassword">站点运行用户密码</param>
        /// <param name="svnUrl">用于克隆站点文件的SVN地址</param>
        /// <param name="svnUsername">用于克隆站点文件的SVN账户名</param>
        /// <param name="svnPassword">用于克隆站点文件的SVN密码</param>
        /// <param name="siteSourceGoodsync">Goodsync站点同步源地址</param>
        /// <param name="siteSourceGoodsyncExclude">Goodsync站点同步排除列表</param>
        /// <param name="siteSourceGoodsyncInclude">Goodsync站点同步包含列表</param>
        /// <returns></returns>
        public static Dictionary<string, bool> CreateIISSite(
            List<string> minions,
            string apppoolName = "",
            string apppoolVersion = "",
            bool apppoolEnable32bit = false,
            string apppoolModel = "",
            string siteName = "",
            string siteProtocol = "",
            string siteDomain = "",
            string sitePath = "",
            string siteLogPath = "",
            string siteIP = "",
            string sitePort = "",
            string siteRunasUsername = "",
            string siteRunasPassword = "",
            string svnUrl = "",
            string svnUsername = "",
            string svnPassword = "",
            string siteSourceGoodsync = "",
            string siteSourceGoodsyncExclude = "",
            string siteSourceGoodsyncInclude = "")
        {

            if (minions.Count > 0)
            {
                // 初始化返回信息
                Dictionary<string, bool> r = new Dictionary<string, bool>();
                foreach (var item in minions)
                {
                    r.Add(item, true);
                }

                RunCmdType rct = new RunCmdType();
                string tgt = string.Join(",", minions);// minion目标
                string expr_from = "list"; // 默认的expr_from

                // 如果站点IP为空，则设定为"*"
                if (string.IsNullOrWhiteSpace(siteIP))
                {
                    siteIP = "*";
                }

                // 如果站点端口为空，则设定为"80"
                if (string.IsNullOrWhiteSpace(sitePort))
                {
                    sitePort = "80";
                }

                // 如果站点对应的程序池名称未提供为空，则设定为同站点名
                if (string.IsNullOrWhiteSpace(apppoolName))
                {
                    apppoolName = siteName;
                }


                // 从SVN拉取数据
                if (!string.IsNullOrWhiteSpace(svnUrl))
                {
                    var svnPath = sitePath.Substring(0, sitePath.LastIndexOf("\\"));

                    var bb = JsonConvert.DeserializeObject<Dictionary<string, string>>(SVNOperation(svnPath, SaltAPI.SVNOperation.checkout, minions, svnUsername, svnPassword, svnUrl));
                    foreach (var i in minions)
                    {
                        if (bb.Keys.Contains(i))
                        {
                            // 如果SVN返回的结果含有error 则判断为更新失败
                            // 不太准确
                            if (bb[i].Contains("ERROR"))
                            {
                                r[i] = false;
                            }
                        }
                    }
                }

                // 创建程序池

                rct.fun = "xjoker_win_iis.create_apppool";
                rct.client = "local";
                rct.expr_form = expr_from;
                rct.tgt = minions;
                rct.arg = new List<string> { apppoolName };
                var b = JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
                foreach (var i in minions)
                {
                    if (b.Keys.Contains(i))
                    {
                        // 如果SVN返回的结果含有error 则判断为更新失败
                        // 不太准确
                        if (b[i].Contains("ERROR"))
                        {
                            r[i] = false;
                        }
                    }
                }

                // 设定程序池
                rct.fun = "xjoker_win_iis.apppool_setting";
                rct.client = "local";
                rct.expr_form = expr_from;
                rct.tgt = minions;
                rct.arg = new List<string> { "name=" + apppoolName, "runtime_version=" + apppoolVersion, "pipeline_mode=" + apppoolModel, "bit_setting=" + apppoolEnable32bit.ToString() };
                var bbb = JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
                foreach (var i in minions)
                {
                    if (bbb.Keys.Contains(i))
                    {
                        // 如果SVN返回的结果含有error 则判断为更新失败
                        // 不太准确
                        if (bbb[i].Contains("ERROR"))
                        {
                            r[i] = false;
                        }
                    }
                }

                // 创建站点
                rct = new RunCmdType();
                rct.fun = "xjoker_win_iis.create_site";
                rct.client = "local";
                rct.expr_form = expr_from;
                rct.tgt = minions;
                rct.arg = new List<string> { "name=" + siteName,
                                    "protocol=" + siteProtocol,
                                    "sourcepath=" + sitePath,
                                    "port=" + sitePort,
                                    "apppool=" + apppoolName,
                                    "hostheader=" + siteDomain,
                                    "ipaddress="+siteIP };
                var c = JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
                foreach (var i in minions)
                {
                    if (c.Keys.Contains(i))
                    {
                        // 如果SVN返回的结果含有error 则判断为更新失败
                        // 不太准确
                        if (c[i].Contains("ERROR"))
                        {
                            r[i] = false;
                        }
                    }
                }


                //设定站点Runas
                if (!string.IsNullOrWhiteSpace(siteRunasUsername) && !string.IsNullOrWhiteSpace(siteRunasPassword))
                {
                    rct = new RunCmdType();
                    rct.fun = "xjoker_win_iis.site_run_as";
                    rct.client = "local";
                    rct.expr_form = expr_from;
                    rct.tgt = minions;
                    rct.arg = new List<string> { siteName, siteRunasUsername, siteRunasPassword };
                    var d = JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
                    foreach (var i in minions)
                    {
                        if (d.Keys.Contains(i))
                        {
                            // 如果SVN返回的结果含有error 则判断为更新失败
                            // 不太准确
                            if (d[i] != null)
                            {
                                if (d[i].Contains("ERROR"))
                                {
                                    r[i] = false;
                                }

                            }
                        }
                    }
                }


                //设置站点日志
                if (!string.IsNullOrWhiteSpace(siteLogPath))
                {
                    rct = new RunCmdType();
                    rct.fun = "xjoker_win_iis.site_log_path";
                    rct.client = "local";
                    rct.expr_form = expr_from;
                    rct.tgt = minions;
                    rct.arg = new List<string> { siteName, siteLogPath };
                    var e = JsonConvert.DeserializeObject<Dictionary<string, string>>(CmdRunString(RunCmdTypeToString(rct)));
                    foreach (var i in minions)
                    {
                        if (e.Keys.Contains(i))
                        {
                            // 如果SVN返回的结果含有error 则判断为更新失败
                            // 不太准确
                            if (e[i] != null)
                            {
                                if (e[i].Contains("ERROR"))
                                {
                                    r[i] = false;
                                }

                            }
                        }
                    }
                }

                // 设定goodsync
                if (!string.IsNullOrWhiteSpace(siteSourceGoodsync))
                {
                    var f = GoodSyncNewJob(minions,
                                            siteName,
                                            siteSourceGoodsync,
                                            sitePath,
                                            exclude: siteSourceGoodsyncExclude,
                                            include: siteSourceGoodsyncInclude);
                }
                return r;

            }
            else
            {
                return null;
            }
        }
    }
}
