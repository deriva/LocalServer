# LocalServer
## 使用web链接打开本地应用 
 内含html在线生成pdf功能


c# html生成pdf,C#编写 HTML生成PDF的方式有几种
这里介绍一种:C#使用wkhtmltopdf，把HTML生成PDF（包含分页）
架构设计:

本地搭建一个控制台应用程序（指定端口：15080）--->
   
  web系统 利用ajax访问本地程序：http://127.0.0.1:15080
  
---
  本地搭建一个控制台应用程序过程:主要用到HttpListener 监听  核心代码如下
//服务对象
```
public static MyHttpServer httpServer;
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
```
web页面调用本地服务方法其实很简单:
检测web服务是否 开始:
 //检测本地服务是否开启
 ```
   $.ajax({
     url: 'http://127.0.0.1:15080/Pdf/index',
     type: 'get',
     complete: function (r) {
         if (r.status == 200) {
            //开启 todo
         } else {
             layer.alert("本地服务未开启，请先开启再来导出");
         }
     },
     error: function (XMLHttpRequest, textStatus, errorThrown) {
         layer.alert("本地服务未开启，请先开启再来导出");
     }
   });
 ```
 --------------------------------------------------------------------------------------------------------------------------
 接下来的我们可以在自己的页面调用本地服务了 ，可以通过传要抓取的页面http地址，和下载的pdf文件名 2个参数  
 html代码如下:
 ```
 <input type="button" class="layui-btn btnexport" value="本地导出" onclick="PP.ExportPdf(1)" />
<script type="text/javascript">
    var PP = {
        Export2: () => {//通过workhtml来转pdf
           var url = "http://www.baidu.com";//要抓取的页面 这里可以自己切换更改
            var data = { };
            data.NewPdfFileName = url;
            data.SourceUrl ="自己指定的pdf文件名.pdf";
            $.get("http://127.0.0.1:15080/Pdf/index", data, function (r) {
                layer.closeAll();
                if (top.location != self.location) {
                    parent.layer.alert(r.message, function () {
                        $.get("http://127.0.0.1:15080/Pdf/operdir", data);
                        parent.layer.closeAll();
                    })
                } else {
                    layer.alert(r.message, function () { layer.closeAll(); })
                }
       
            });
        }
    }

</script>


 ```
 

  
