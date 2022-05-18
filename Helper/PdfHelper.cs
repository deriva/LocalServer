using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;

namespace Bc.LocalServer
{
    public class PdfHelper
    {

        public string Do(Dictionary<string, string> dic)
        {
            var infp = JsonHelper.Assign<PdfInfoVm>(dic);
            infp.SourceUrl = HttpUtility.UrlDecode(infp.SourceUrl);
            if(string.IsNullOrWhiteSpace(infp.SourceUrl))
                return JsonHelper.ToObjectStr(false, "地址不存在"); 

            infp.NewPdfFileName = HttpUtility.UrlDecode(infp.NewPdfFileName);
            GentrtaPdf(infp);
            return JsonHelper.ToObjectStr(true,"抓取结束,请请往:"+infp.ReportPdfRoot, infp.ReportPdfRoot); ;

        }

        /// <summary>
        /// 打开目录
        /// </summary>
        /// <param name="folderPath">目录路径（比如：C:\Users\Administrator\）</param>
        public  void OpenFolder(Dictionary<string, string> dic)
        {
            var infp = JsonHelper.Assign<PdfInfoVm>(dic);
            if (string.IsNullOrEmpty(infp.ReportPdfRoot)) return;

            Process process = new Process();
            ProcessStartInfo psi = new ProcessStartInfo("Explorer.exe");
            psi.Arguments = infp.ReportPdfRoot;
            process.StartInfo = psi;

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                process?.Close();

            }

        }
  
        public bool OutFile(PdfInfoVm pvm, ref string msg)
        {
            var ct = Bc.LocalServer.Program.httpServer.listener.GetContext();
            var request = ct.Request;
            var response = ct.Response;
            try
            {
                string path = pvm.PdfFullPath; string outfilename = pvm.OutFileName;
                if (System.IO.File.Exists(path))
                {

                    FileStream fs = new FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, FileShare.ReadWrite);
                    byte[] bytes = new byte[(int)fs.Length];
                    fs.Read(bytes, 0, bytes.Length);
                    fs.Close();
                    if (request.UserAgent != null)
                    {
                        string userAgent = request.UserAgent.ToUpper();
                        if (userAgent.IndexOf("FIREFOX", StringComparison.Ordinal) <= 0)
                        {
                            response.AddHeader("Content-Disposition",
                                          "attachment;  filename=" + HttpUtility.UrlEncode(outfilename, Encoding.UTF8));
                        }
                        else
                        {
                            response.AddHeader("Content-Disposition", "attachment;  filename=" + outfilename);
                        }
                    }
                    response.ContentEncoding = Encoding.UTF8;
                    response.ContentType = "application/octet-stream";
                    //通知浏览器下载文件而不是打开
                    using (var stream = response.OutputStream)
                    {
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Close();
                    }
                    Bc.LocalServer.Program.httpServer.ResponData(bytes,response);
                    //HttpListenerHelper.ResponData(bytes, response);
                    fs.Close();
                    return true;
                }
                else
                {

                    var message = "文件未找到,可能已经被删除";
                    msg = message;
                    ToolUtils.Print(message);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;

                ToolUtils.Print(ex.Message);
            }
            return false;
        }

        /// <summary>
        /// 生成pdf
        /// </summary>
        /// <param name="pvm"></param>
        public void GentrtaPdf(PdfInfoVm pvm)
        {
            string url = pvm.SourceUrl; string pdfName = pvm.PdfFullPath.Replace(@"/",@"\");
            string str = AppDomain.CurrentDomain.BaseDirectory + ("wkhtmltopdf.exe");
            ToolUtils.Print("【解析开始】"+pvm.NewPdfFileName);
            Process p = System.Diagnostics.Process.Start(str, url + " --margin-left 0 --margin-right 0 --margin-top 0 " + pdfName);
            p.WaitForExit(3000);
            ToolUtils.Print("【解析结束】" + pvm.NewPdfFileName);
            Thread.Sleep(1000);
         
        }
         

    }


    public class PdfInfoVm
    {
        /// <summary>
        /// html网址
        /// </summary>
        public string SourceUrl { get; set; }

        public string ReportPdfRoot
        {
            get
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + "PDF\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        private string newPdfFileName = "";
        /// <summary>
        ///pdf文件名
        /// </summary>
        public string NewPdfFileName
        {
            get
            {

                return newPdfFileName;
            }
            set { newPdfFileName = value; }
        }
        /// <summary>
        ///pdf完整路径名
        /// </summary>
        public string PdfFullPath
        {
            get
            {

                return ReportPdfRoot + newPdfFileName;
            }
        }


        /// <summary>
        /// pdf相对路径（返回前台页面要调用的相对路径）
        /// </summary>
        public string RelativePdfPath
        {

            get
            {
                return "/PDF/" + NewPdfFileName;
            }
        }

        /// <summary>
        /// 压缩文件存储目录
        /// </summary>
        private string ZipFileNameRoot
        {
            get
            {
                var path = AppDomain.CurrentDomain.BaseDirectory + "PDF/TempZip/";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }


        private string zipPdfFileName = "";

        /// <summary>
        /// 压缩文件（完整文件路径名）
        /// </summary>
        public string ZipPdfFileName { get { return ZipFileNameRoot + zipPdfFileName; } set { zipPdfFileName = value; } }


        /// <summary>
        /// 待输出的压缩文件名
        /// </summary>
        public string OutFileName { get; set; }
    }
}
