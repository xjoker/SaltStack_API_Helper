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



}
