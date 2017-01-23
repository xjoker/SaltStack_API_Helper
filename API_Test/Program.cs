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

            var a = "{\"Data\":\"执行完成!\"}";
            Console.WriteLine(a.Length.ToString());
            //初始化
            Salt_API_Function.APIurl = "https://192.168.2.20:1559/";
            Salt_API_Function.loginRequestInfo = new LoginRequestType()
            {
                username = "saltapi",
                password = "salt@pass",
                eauth = "pam",
            };

            // 登陆
            if (Salt_API_Function.Login() != null)
            {
                Console.WriteLine("Login Success");
                Console.WriteLine("Token: " + RequestType.xAuthToken);
            }
            else
            {
                Console.WriteLine("Login Failed");
            }




            var r = Salt_API_Function.GoodSyncNewJob(new List<string>() { "web" }, "Kevinkern", "www.12aqqq.com", @"\\site.fzcyjh.com\www.12aqqq.com", @"D:\Web\www.12aqqq.com");
            //var c = Salt_API_Function.GoodSyncSyncJob("Administrator", new List<string>() { "192.168.79.111" }, "fdfd111fd111");
            //var c = Salt_API_Function.GoodSyncDeleteJob("Administrator", new List<string>() { "192.168.79.111" }, "fdfd111fd111");
            //var bb = Salt_API_Function.SVNOperation("D:\\Web", SVNOperation.checkout,new List<string> { "192.168.72.68","192.168.72.69"},"salt","saltsalt", "https://salt-minion-2008-2:8443/svn/salt/");
            // var ccc = Salt_API_Function.CreateIISSite(new List<string> { "192.168.72.68", "192.168.72.69" }, siteName: "salt", svnUrl: "https://salt-minion-2008-2:8443/svn/salt/", svnUsername: "salt", svnPassword: "saltsalt",sitePath: "D:\\web\\salt",apppoolVersion:"v4.0",apppoolModel:"1",apppoolEnable32bit:false, siteProtocol:"http");
            /////var bnbbb = Salt_API_Function.IISOperation(new List<string> { "192.168.72.68", "192.168.72.69" }, IISOperation.StopSite, new List<string> { "333.com" });

            //var ccc = Salt_API_Function.GetSiteList("192.168.72.69");

            //////Salt_API_Function.GetAppPoolList("*");

            ////var cc = Salt_API_Function.SVNOperation(@"D:\Web\Repo120161012155749", SVNOperation.info, new List<string> { "192.168.72.68" }, st: SVNType.Order, fun: "xjoker_svn.info");
            //var cc =Salt_API_Function.WindowsServicesStatusGetByPowershell();
            Console.WriteLine();

        }
    }
}
