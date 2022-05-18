# LocalServer
## 使用web链接打开本地应用 （含在线抓取网页生成pdf）
 
[<a href="http://www.laddyq.com/tools/vn/pdf.html">官网地址</a>]

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
 注意:有动态生成二维码的页面 需要用到QRCodeEncoder 由服务端生成图片流
 ---
 简单介绍其他方式生成pdf
 jspdf 
 ```
  <script src="/js/jqueryplus/htmlcanvas/html2canvas.min.js"></script>
  <script src="/js/jqueryplus/htmlcanvas/jspdf.debug.js"></script>
     $("#" + divID).css("background-color", "white");
                var canvas_new = document.createElement("canvas");

                var scale = 3;

                var w = parseInt(window.getComputedStyle(document.querySelector("#" + divID)).width);

                var h = parseInt(window.getComputedStyle(document.querySelector("#" + divID)).height);
                canvas_new.width = w * scale;

                canvas_new.height = h * scale;

                canvas_new.getContext("2d").scale(scale, scale);
                var options = {
                    canvas: canvas_new
                };

                html2canvas(document.querySelector("#" + divID), options).then(function (canvas) {

                    var contentWidth = canvas.width;

                    var contentHeight = canvas.height;



                    //一页pdf显示html页面生成的canvas高度;

                    var pageHeight = contentWidth / 592.28 * 841.89;

                    //未生成pdf的html页面高度

                    var leftHeight = contentHeight;

                    //页面偏移

                    var position = 0;

                    //a4纸的尺寸[595.28,841.89]，html页面生成的canvas在pdf中图片的宽高

                    var imgWidth = 595.28;

                    var imgHeight = 592.28 / contentWidth * contentHeight + 45;



                    var pageData = canvas.toDataURL('image/jpeg', 1.0);

                    var pdf = new jsPDF('', 'pt', 'a4');

                    //有两个高度需要区分，一个是html页面的实际高度，和生成pdf的页面高度(841.89)

                    //当内容未超过pdf一页显示的范围，无需分页

                    if (leftHeight < pageHeight) {

                        pdf.addImage(pageData, 'JPEG', 0, 0, imgWidth, imgHeight);

                    } else {

                        while (leftHeight > 0) {

                            pdf.addImage(pageData, 'JPEG', 0, position, imgWidth, imgHeight)

                            leftHeight -= pageHeight;

                            position -= 841.89;

                            //避免添加空白页

                            if (leftHeight > 0) {

                                pdf.addPage();

                            }

                        }

                    }

                    pdf.save('我的.pdf');
                });
    ```
这种方式生成pdf 当页面小的时候 没有问题  当页面特别大有5，6页的时候就会有问题

  
