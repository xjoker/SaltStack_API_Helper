using SaltAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //初始化
            Salt_API_Function.APIurl = "https://192.168.72.60:1559/";
            Salt_API_Function.LoginRequestInfo = new LoginRequestType()
            {
                username = "salt",
                password = "salt@pass",
                eauth = "pam",
            };

            // 登陆
            var loginInfo = Salt_API_Function.Login();
            Console.WriteLine(loginInfo.token);

            //Salt_API_Function.IISOperation(new List<string> { "192.168.72.68" }, IISOperation.StopSite, new List<string> { "333.com" });

            //Salt_API_Function.GetAppPoolList("*");

            var cc = Salt_API_Function.SVNOperation(@"D:\Web\Repo120161012155749", SVNOperation.info, new List<string> { "192.168.72.68" },st:SVNType.Order,fun: "xjoker_svn.info");
            Console.WriteLine();

        }
    }
}
