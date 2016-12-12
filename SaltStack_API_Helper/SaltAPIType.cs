using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaltAPI
{
    /// <summary>
    /// Windows 服务操作类型枚举
    /// </summary>
    public enum ServiceOperation
    {
        start,
        stop,
        enable,
        disable,
        delete,
        restart,
        update
    }

    /// <summary>
    /// SVN 操作类型
    /// </summary>
    public enum SVNOperation
    {
        checkout,
        update,
        commit,
        diff,
        export,
        info,
        remove,
        status,
        @switch
    }

    /// <summary>
    /// SVN 模块选择
    /// </summary>
    public enum SVNType
    {
        salt,
        Order
    }


    /// <summary>
    /// IIS 操作类型
    /// </summary>
    public enum IISOperation
    {
        StartSite,
        StopSite,
        RestartSite,
        RemoveSite,
        StartAppPool,
        StopAppPool,
        RestartAppPool,
        RemoveAppPool,
        update
    }

    /// <summary>
    /// 基本回显
    /// </summary>
    public class BaseType
    {
        public List<object> @return { get; set; }
    }
    /// <summary>
    /// 登陆信息返回类
    /// </summary>
    public class LoginResponseType
    {
        public List<string> perms { get; set; }
        public float start { get; set; }
        public string token { get; set; }
        public float expire { get; set; }
        public string user { get; set; }
        public string eauth { get; set; }
    }

    /// <summary>
    /// 登陆信息
    /// </summary>
    public class LoginRequestType
    {
        public string username { get; set; }
        public string password { get; set; }
        public string eauth { get; set; }
    }

    /// <summary>
    /// 标准请求头
    /// </summary>
    public static class RequestType
    {
        public static string accept = "application/json";
        public static string xAuthToken = "";
        public static string contentType = "application/json";

    }

    /// <summary>
    /// run 命令类型
    /// </summary>
    public class RunCmdType
    {
        public string fun { get; set; }
        public string expr_form { get; set; }
        public string client { get; set; }
        public dynamic tgt { get; set; }
        public List<string> arg { get; set; }
        public List<string> match { get; set; }
    }

    /// <summary>
    /// run 命令类型
    /// 不含arg和match
    /// </summary>
    public class RunCmdTypeNoArg
    {
        public string fun { get; set; }
        public string expr_form { get; set; }
        public string client { get; set; }
        public dynamic tgt { get; set; }

    }

    /// <summary>
    /// IIS 的站点详细类型
    /// </summary>
    public class IISSiteType
    {
        /// <summary>
        /// 站点ID
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// 站点名称
        /// </summary>
        public string siteName { get; set; }

        /// <summary>
        /// 站点当前状态
        /// </summary>
        public bool siteState { get; set; }

        /// <summary>
        /// 站点日志路径
        /// </summary>
        public string logFile { get; set; }
        /// <summary>
        /// 站点物理路径
        /// </summary>
        public string physicalPath { get; set; }

        /// <summary>
        /// 站点绑定信息
        /// </summary>
        public List<Dictionary<string, List<string>>> bindings { get; set; }
        /// <summary>
        /// 站点app信息以及物理路径
        /// </summary>
        public List<Dictionary<string, string>> application { get; set; }

    }

    /// <summary>
    /// IIS 的程序池详细类型
    /// </summary>
    public class AppPoolType
    {
        public string name { get; set; }
        public string autoStart { get; set; }
        public string pipelineMode { get; set; }
        public string runtimeVersion { get; set; }
        public string state { get; set; }
    }
}
