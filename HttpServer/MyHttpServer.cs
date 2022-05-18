
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Bc.LocalServer.HttpServer
{
    public class MyHttpServer
    {
        //创建委托。用于通知
        public delegate void respNoticeDelegate(Dictionary<string, string> data, HttpListenerResponse resp, string route, string request_type = "get");
        public event respNoticeDelegate respNotice;

        public HttpListener listener = new HttpListener();
        private Dictionary<string, string> actionDict = new Dictionary<string, string>();
        private ReturnDataBase respObj;//返回的数据
        public string curr_path = "";
        //接收到的数据
        public Dictionary<string, string> data_rec = new Dictionary<string, string>();

        public MyHttpServer(Dictionary<string, string> routes = null)
        {

            if (routes != null)
            {
                //遍历字典路由
                foreach (KeyValuePair<string, string> kvp in routes)
                {
                    AddPrefixes(kvp.Key, kvp.Value);

                }
            }

        }

        public void AddPrefixes(string url, string action)
        {
            actionDict.Add(url, action);
        }

        public void close()
        {
            listener.Stop();//停止监听
            //listener.Close();//释放资源
        }

        //开始监听
        public void Start(int port, Dictionary<string, string> routes = null)
        {

            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("无法在当前系统上运行服务(Windows XP SP2 or Server 2003)。" + DateTime.Now.ToString());
                return;
            }

            if (actionDict.Count <= 0)
            {
                System.Console.WriteLine("没有监听端口");
            }

            //监听url
            foreach (var item in actionDict)
            {
                var url = string.Format("http://127.0.0.1:{0}/{1}", port, item.Key+"/" );
                listener.Prefixes.Add(url);  //监听的是以item.Key + "/"+XXX接口
            }

            listener.Start();
            listener.BeginGetContext(Result, null);
            respObj = new ReturnDataBase();


            ToolUtils.Print("开始监听");
            System.Console.Read();
        }


        private void Result(IAsyncResult asy)
        {
            if (!listener.IsListening) return;//如果已经停止监听了就直接返回

            listener.BeginGetContext(Result, null);
            var context = listener.EndGetContext(asy);
            var req = context.Request;
            var rsp = context.Response;


            //对接口url处理，解析出curr_path也就是当前路由
            string route = HandlerReq(req.RawUrl);//获取当前路由

            //对接口所传数据处理
            Dictionary<string, string> data = new Dictionary<string, string>();
            data = HandleHttpMethod(context, rsp, route);

            //将解析后的结果 通知
            dataNoticeEvent(data, rsp, route, context.Request.HttpMethod);
        }

        /// <summary>
        /// 输出结果(返回结果)
        /// </summary>
        /// <param name="content">结果</param>
        /// <param name="rsp">httprespon对象</param>
        /// <returns></returns>
        public string responData(string content, HttpListenerResponse rsp)
        {
            try
            {
                using (var stream = rsp.OutputStream)
                {
                    //获取类名和方法名 格式： class.method
                    //string class_name = actionDict.ContainsKey(route) ? actionDict[route] : "";
                    // /传入类名，利用反射创建对象并返回的数据，要返回给接口的
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
                  //  rsp.AppendHeader("Access-Control-Allow-Headers", "Authorization,Content-Type,Accept,Origin,User-Agent,DNT,Cache-Control,X-Mx-ReqToken,X-Requested-With");
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
        public string ResponData(byte[] dataByte, HttpListenerResponse rsp)
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
                    rsp.AppendHeader("Access-Control-Allow-Headers", "*,Authorization,Content-Type,Accept,Origin,User-Agent,DNT,Cache-Control,X-Mx-ReqToken,X-Requested-With");
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
        /// <summary>
        /// 对客户端来的url处理
        /// </summary>
        /// <param name="url"></param>
        private string HandlerReq(string url)
        {
            try
            {
                //url : /test?a=1
                System.Console.WriteLine("url : " + url);

                string[] arr_str = url.Split('?');

                if (arr_str.Length > 0)
                {
                    return curr_path = arr_str[0];//  如下：/api/test
                }

                return "";
            }
            catch (Exception e)
            {
                return "";
            }
        }
        //处理接口所传数据 Post和Get 获取数据
        private Dictionary<string, string> HandleHttpMethod(HttpListenerContext context, HttpListenerResponse resp, string route)
        {
            Dictionary<string, string> return_data = new Dictionary<string, string>();
            data_rec.Clear();//先清空上一次http接收的数据
            string contentType = context.Request.ContentType == null ? "" : context.Request.ContentType;
            //1. ContentType = multipart/form-data
            if (contentType.Contains("multipart/form-data"))
            {
                //新建解析类（解析post数据）
                HttpListenerPostParaHelper parse = new HttpListenerPostParaHelper(context);
                List<HttpListenerPostValue> list = parse.GetHttpListenerPostValue();
                ;
                foreach (HttpListenerPostValue item in list)
                {
                    string k = item.name;
                    string value = "";
                    if (item.type == 0)
                    {
                        //文本解析
                        value = Encoding.UTF8.GetString(item.datas).Replace("\r\n", "");
                    }
                    else
                    {
                        //byte数组转文件
                        File.WriteAllBytes(@"D:\test.png", item.datas);
                        value = @"D:\test.png";
                    }
                    dataRecAdd(k, value);
                }
                return_data = data_rec;

                return return_data;
            }


            //2.ContentType=application/json
            if (contentType.Contains("application/json") || contentType.Contains("application/x-www-form-urlencoded"))
            {
                try
                {
                    Stream stream = context.Request.InputStream;
                    StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                    string json = reader.ReadToEnd();
                    Dictionary<string, string> DicContent = new Dictionary<string, string>();
                    if (string.IsNullOrEmpty(json)) return return_data;
                    if (json == "[]" || json == "") return return_data;
                    data_rec = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    return_data = data_rec;//返回的数据

                }
                catch (Exception ex)
                {
                    return return_data;
                }
                return return_data;
            }
            if (contentType.Contains("application/x-www-form-urlencoded"))
            {

            }



            //3. ContentType = text/html等类型 就使用别的解析算法
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    var data = context.Request.QueryString;
                    // 没解决乱码问题，用url进行解析正常
                    string url = context.Request.Url.ToString();
                    string[] pars = url.Split('?');
                    string content = "";
                    if (pars.Length == 0)
                    {
                        return return_data;
                    }
                    if (pars.Length <= 1) return return_data;
                    string canshus = pars[1];
                    if (canshus.Length > 0)
                    {
                        string[] canshu = canshus.Split('&');

                        foreach (string i in canshu)
                        {
                            string[] messages = i.Split('=');
                            dataRecAdd(messages[0], messages[1]);
                            //content += "参数为：" + messages[0] + " 值为：" + messages[1];
                        }
                        return_data = data_rec;
                    }


                    return return_data;
                    break;
            }


            return return_data;

        }

        /// <summary>
        /// 将解析后的数据通知事件
        /// </summary>
        /// <param name="data">解析后的字典数据</param>
        /// <param name="rsp">respon对象</param>
        /// <param name="route">路由</param>
        /// <param name="method">方法</param>
        public void dataNoticeEvent(Dictionary<string, string> data, HttpListenerResponse rsp, string route, string method = "unkonwn")
        {
            //通知
            //if (data_rec.Count > 1)
            //{
            //    respNotice?.Invoke(data_rec, rsp,route,method);
            //}

            respNotice?.Invoke(data, rsp, route, method);

        }

        public void dataRecAdd(string k, string v)
        {
            if (data_rec.ContainsKey(k))
            {
                data_rec[k] = v;
            }
            else
            {
                data_rec.Add(k, v);
            }
        }

    }

    class ReturnDataBase
    {
        public string GetDataMain(string class_method, Dictionary<string, string> rec_data)
        {
            string[] class_arr = class_method.Split('.');//分割类名跟方法

            string class_name, method;
            if (class_arr.Length == 1) class_name = class_arr[0];
            if (class_arr.Length == 2) method = class_arr[1];
            if (class_arr.Length == 0) return "";

            return "cesh";
        }
    }




}

