using System.Collections.Generic;

namespace SaltAPI
{
    public partial class Salt_API_Function
    {
        /// <summary>
        ///  SVN 操作方法
        /// </summary>
        /// <param name="filePath">SVN路径</param>
        /// <param name="so">SVN命令模式</param>
        /// <param name="svnUsername">SVN 用户名</param>
        /// <param name="svnPassword"> SVN 密码</param>
        /// <param name="minionName">Minion 机器名</param>
        /// <param name="version">版本</param>
        /// <param name="remote">源</param>
        /// <param name="st">使用自带模块还是私有模块</param>
        /// <param name="fun">使用私有模块的时候需要提供模块名称</param>
        /// <returns></returns>
        public static string SVNOperation(string filePath, SVNOperation so, List<string> minionName, string svnUsername = "", string svnPassword = "", string remote = "", int? version = null, SVNType st = SVNType.salt, string fun = "")
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
                            rct.arg = new List<string> { "username=" + svnUsername, "password=" + svnPassword, "remote=" + remote, "cwd=" + filePath, "certCheck=False", "revision=" + rversion };
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
                    rct.arg = new List<string> { filePath };
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
    }
}
