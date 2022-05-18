using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Bc.LocalServer
{
    public class HttpListenerHelper
    {
        public static HttpListener httpobj;
        public static void Start()
        {
            var url = "http://127.0.0.1:15080/";
            ToolUtils.Print($"启动地址:{url}");

            //提供一个简单的、可通过编程方式控制的 HTTP 协议侦听器。此类不能被继承。
            httpobj = new HttpListener();
            //定义url及端口号，通常设置为配置文件
            httpobj.Prefixes.Add(url);
            //启动监听器
            httpobj.Start();
            //异步监听客户端请求，当客户端的网络请求到来时会自动执行Result委托
            //该委托没有返回值，有一个IAsyncResult接口的参数，可通过该参数获取context对象
            httpobj.BeginGetContext(Result, null);
            ToolUtils.Print($"服务端初始化完毕，正在等待客户端请求,时间：{DateTime.Now.ToString()}");

        }
        public static HttpListenerContext Context
        {
            get { return httpobj.GetContext(); }
        }

        private static void Result(IAsyncResult ar)
        {
            //当接收到请求后程序流会走到这里

            //继续异步监听
            httpobj.BeginGetContext(Result, null);
            var guid = Guid.NewGuid().ToString();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"接到新的请求:{guid},时间：{DateTime.Now.ToString()}");
            //获得context对象
            var context = httpobj.EndGetContext(ar);
            var request = context.Request;
            var response = context.Response;
            ////如果是js的ajax请求，还可以设置跨域的ip地址与参数
            //context.Response.AppendHeader("Access-Control-Allow-Origin", "*");//后台跨域请求，通常设置为配置文件
            //context.Response.AppendHeader("Access-Control-Allow-Headers", "ID,PW");//后台跨域参数设置，通常设置为配置文件
            //context.Response.AppendHeader("Access-Control-Allow-Method", "post");//后台跨域请求设置，通常设置为配置文件
            context.Response.ContentType = "text/plain;charset=UTF-8";//告诉客户端返回的ContentType类型为纯文本格式，编码为UTF-8
            context.Response.AddHeader("Content-type", "application/json");//添加响应头信息
            context.Response.ContentEncoding = Encoding.UTF8;
            string returnObj = null;//定义返回客户端的信息
            if (request.HttpMethod == "POST" && request.InputStream != null)
            {
                //处理客户端发送的请求并返回处理信息
                returnObj = HandleRequest(request, response);

            }
            else
            {
                returnObj = $"不是post请求或者传过来的数据为空";
            }
            var returnByteArr = Encoding.UTF8.GetBytes(returnObj);//设置客户端返回信息的编码
            try
            {
                using (var stream = response.OutputStream)
                {
                    //把处理信息返回到客户端
                    stream.Write(returnByteArr, 0, returnByteArr.Length);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"网络蹦了：{ex.ToString()}");
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"请求处理完成：{guid},时间：{ DateTime.Now.ToString()}\r\n");
        }

        private static string HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string data = null;
            try
            {
                var byteList = new List<byte>();
                var byteArr = new byte[2048];
                int readLen = 0;
                int len = 0;
                //接收客户端传过来的数据并转成字符串类型
                do
                {
                    readLen = request.InputStream.Read(byteArr, 0, byteArr.Length);
                    len += readLen;
                    byteList.AddRange(byteArr);
                } while (readLen != 0);
                data = Encoding.UTF8.GetString(byteList.ToArray(), 0, len);

                var info = JsonHelper.Deserialize<PdfInfoVm>(data);
                new PdfHelper().GentrtaPdf(info);
                return "";
                //获取得到数据data可以进行其他操作
            }
            catch (Exception ex)
            {
                response.StatusDescription = "404";
                response.StatusCode = 404;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"在接收数据时发生错误:{ex.ToString()}");
                return $"在接收数据时发生错误:{ex.ToString()}";//把服务端错误信息直接返回可能会导致信息不安全，此处仅供参考
            }

            response.StatusDescription = "200";//获取或设置返回给客户端的 HTTP 状态代码的文本说明。
            response.StatusCode = 200;// 获取或设置返回给客户端的 HTTP 状态代码。
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"接收数据完成:{data.Trim()},时间：{DateTime.Now.ToString()}");
            return $"接收数据完成";
        }
        /// <summary>
        /// 输出结果(返回结果)
        /// </summary>
        /// <param name="content">结果</param>
        /// <param name="rsp">httprespon对象</param>
        /// <returns></returns>
        public static string ResponData(string content, HttpListenerResponse rsp)
        {
            try
            {
                using (var stream = rsp.OutputStream)
                {
                    // 获取类名和方法名 格式： class.method
                    //string class_name = actionDict.ContainsKey(route) ? actionDict[route] : "";
                    //  /传入类名，利用反射创建对象并返回的数据，要返回给接口的
                    //string content = respObj.GetDataMain(class_name, data);

                    // response的outputStream输出数据的问题
                    //方法一：程序以什么码表输出，一定要控制浏览器以什么码表打开
                    //若"text/html;charset=UTF-8"写错，浏览器会提示下载
                    rsp.StatusCode = 200;
                    rsp.ContentType = "text/html;charset=UTF-8";//告诉客户端返回的ContentType类型为纯文本格式，编码为UTF-8
                    rsp.AddHeader("Content-type", "application/json");//添加响应头信息
                    rsp.ContentEncoding = Encoding.UTF8;
                    rsp.AppendHeader("Access-Control-Allow-Origin", "*");//允许跨域
                    rsp.AppendHeader("Access-Control-Allow-Credentials", "true");

                    //后台跨域请求;//允许跨域
                    //后台跨域请求，必须配置
                    rsp.AppendHeader("Access-Control-Allow-Headers", "Authorization,Content-Type,Accept,Origin,User-Agent,DNT,Cache-Control,X-Mx-ReqToken,X-Requested-With");
                    rsp.AppendHeader("Access-Control-Max-Age", "86400");

                    byte[] dataByte = Encoding.UTF8.GetBytes(content);
                    stream.Write(dataByte, 0, dataByte.Length);


                    stream.Close();
                }
            }
            catch (Exception e)
            {
                rsp.Close();
                return e.Message;

            }
            rsp.Close();
            return "";
        }

        /// <summary>
        /// 输出结果(返回结果)
        /// </summary>
        /// <param name="content">结果</param>
        /// <param name="rsp">httprespon对象</param>
        /// <returns></returns>
        public static string ResponData(byte[] dataByte, HttpListenerResponse rsp)
        {
            try
            {
                using (var stream = rsp.OutputStream)
                {
                    // 获取类名和方法名 格式： class.method
                    //string class_name = actionDict.ContainsKey(route) ? actionDict[route] : "";
                    //  /传入类名，利用反射创建对象并返回的数据，要返回给接口的
                    //string content = respObj.GetDataMain(class_name, data);

                    // response的outputStream输出数据的问题
                    //方法一：程序以什么码表输出，一定要控制浏览器以什么码表打开
                    //若"text/html;charset=UTF-8"写错，浏览器会提示下载
                    rsp.StatusCode = 200;
                    rsp.ContentType = "text/html;charset=UTF-8";//告诉客户端返回的ContentType类型为纯文本格式，编码为UTF-8
                    rsp.AddHeader("Content-type", "application/json");//添加响应头信息
                    rsp.ContentEncoding = Encoding.UTF8;
                    rsp.AppendHeader("Access-Control-Allow-Origin", "*");//允许跨域
                    rsp.AppendHeader("Access-Control-Allow-Credentials", "true");

                    //后台跨域请求;//允许跨域
                    //后台跨域请求，必须配置
                    rsp.AppendHeader("Access-Control-Allow-Headers", "Authorization,Content-Type,Accept,Origin,User-Agent,DNT,Cache-Control,X-Mx-ReqToken,X-Requested-With");
                    rsp.AppendHeader("Access-Control-Max-Age", "86400");
                    stream.Write(dataByte, 0, dataByte.Length);
                    stream.Close();
                }
            }
            catch (Exception e)
            {
                rsp.Close();
                return e.Message;

            }
            rsp.Close();
            return "";
        }
    }
}
