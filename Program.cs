
using Bc.LocalServer.HttpServer;
using System;
using System.Collections.Generic;
using System.Net;

namespace Bc.LocalServer
{
    public class Program
    {

        //服务对象
        public static MyHttpServer httpServer;
        //http服务路由
        static Dictionary<string, string> routes = new Dictionary<string, string>
            {    {"pdf","index"},{ "home","index"} };
        static void Main(string[] args)
        {

            Console.Title = ("本地服务器");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            ToolUtils.Print("-------------------------------------------------------------------------", 0);
            ToolUtils.Print("-----------------------------    Hello World ----------------------------", 0);
            ToolUtils.Print("-------------------------------------------------------------------------", 0);
           //启动http服务
            httpServer = new MyHttpServer(routes);//初始化，传入路由
            httpServer.respNotice += dataHandle;//回调方法，接收到http请求时触发
            httpServer.Start(15080);//端口  
            Console.ReadKey();
        }
        /// <summary>
        /// 收到请求的回调函数
        /// </summary>
        /// <param name="data">客户端请求的数据</param>
        /// <param name="resp">respon对象</param>
        /// <param name="route">网址路径,如/api/test</param>
        /// <param name="request_type">请求类型，get或者post</param>
        public static void dataHandle(Dictionary<string, string> data, HttpListenerResponse resp, string route = "", string request_type = "get")
        {
            string view = string.Empty;
            var ss = route.Split('/');
            string controller = ss[1].ToLower();
            if (ss.Length >= 3)
                view = route.Split('/')[2];
            //预定义返回的json数据

            //根据路由key的val匹配相应的算法,以下是自己的逻辑
            switch (controller)
            {
                case "pdf":
                    if (view == "index")
                    {
                        var resp_data = new PdfHelper().Do(data);
                        httpServer.responData(resp_data, resp);
                    }
                    if (view == "operdir")
                    {
                        new PdfHelper().OpenFolder(data);
                        httpServer.responData("", resp);
                    }

                    break;
                default:
                    httpServer.responData("404", resp);
                    break;
            }


        }


    }
}
