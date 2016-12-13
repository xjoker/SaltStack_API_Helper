﻿using SaltAPI;
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
            Salt_API_Function.loginRequestInfo = new LoginRequestType()
            {
                username = "salt",
                password = "salt@pass",
                eauth = "pam",
            };

            // 登陆
            if (Salt_API_Function.Login() != null)
            {
                Console.WriteLine("Login Success");
                Console.WriteLine("Token: "+ RequestType.xAuthToken);
            }
            else
            {
                Console.WriteLine("Login Failed");
            }


            var bnbbb = Salt_API_Function.SiteBindEdit("192.168.72.68", IISBindOperation.Create, "333.com", "adadgdfsvdfasdf.com", "*", "233");

            ////Salt_API_Function.GetAppPoolList("*");

            ////var cc = Salt_API_Function.SVNOperation(@"D:\Web\Repo120161012155749", SVNOperation.info, new List<string> { "192.168.72.68" }, st: SVNType.Order, fun: "xjoker_svn.info");
            //var cc =Salt_API_Function.WindowsServicesStatusGetByPowershell();
            Console.WriteLine();

        }
    }
}
