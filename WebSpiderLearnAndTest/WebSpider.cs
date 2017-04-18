using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace WebSpiderLearnAndTest
{

    /***********************************************************************************
     * 
     *    本爬虫部分代码来自，感谢作者的无私分享
     *    https://github.com/coldicelion/Simple-Web-Crawler
     *    https://github.com/coldicelion/Strong-Web-Crawler
     * 
     * 
     * 
     * 
     ***********************************************************************************/


    /// <summary>
    /// 爬虫启动的事件
    /// </summary>
    public class OnStartEventArgs
    {
        /// <summary>
        /// 爬虫的地址
        /// </summary>
        public Uri Uri { get; set; }
        /// <summary>
        /// 启动爬虫的事件
        /// </summary>
        /// <param name="uri"></param>
        public OnStartEventArgs(Uri uri)
        {
            Uri = uri;
        }
    }
    public class OnExcetionEventArgs
    {
        public Uri Uri { get; set; }

        public Exception Exception { get; set; }

        public OnExcetionEventArgs(Uri uri, Exception exception)
        {
            this.Uri = uri;
            this.Exception = exception;
        }
    }

    /// <summary>
    /// 爬虫完成的事件
    /// </summary>
    public class OnCompletedEventArgs
    {
        /// <summary>
        /// 爬虫的地址
        /// </summary>
        public Uri Uri { get;private set; }
        /// <summary>
        /// 任务现场ID
        /// </summary>
        public int ThreadId { get;private set; }
        /// <summary>
        /// 页面源代码
        /// </summary>
        public string PageSource { get;private set; }
        /// <summary>
        /// 爬虫请求的时间
        /// </summary>
        public long MilliSeconds { get; private set; }
        /// <summary>
        /// 通信进程
        /// </summary>
        public IWebDriver WebDriver { get; private set; }

        public OnCompletedEventArgs(Uri uri,int threadId,string pageSource,long milliSeconds)
        {
            Uri = uri;
            ThreadId = threadId;
            PageSource = pageSource;
            MilliSeconds = milliSeconds;
        }
        public OnCompletedEventArgs(Uri uri, int threadId, string pageSource, long milliseconds, IWebDriver driver)
        {
            this.Uri = uri;
            this.ThreadId = threadId;
            this.MilliSeconds = milliseconds;
            this.PageSource = pageSource;
            this.WebDriver = driver;
        }
    }



    /// <summary>
    /// 实际的爬虫类
    /// </summary>
    public class WebSpider
    {
        /// <summary>
        /// 爬虫的启动事件
        /// </summary>
        public event EventHandler<OnStartEventArgs> OnStartEvent;

        /// <summary>
        /// 触发爬虫开始事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Onstart(object sender,OnStartEventArgs e)
        {
            OnStartEvent?.Invoke(sender, e);
        }

        /// <summary>
        /// 爬虫的结束事件
        /// </summary>
        public event EventHandler<OnCompletedEventArgs> OnCompletedEvent;
        /// <summary>
        /// 触发爬虫完成的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCompleted(object sender,OnCompletedEventArgs e)
        {
            OnCompletedEvent?.Invoke(sender, e);
        }

        /// <summary>
        /// 爬虫出错的事件
        /// </summary>
        public event EventHandler<OnExcetionEventArgs> OnExceptionEvent;
        /// <summary>
        /// 触发爬虫失败的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnException(object sender,OnExcetionEventArgs e)
        {
            OnExceptionEvent?.Invoke(sender, e);
        }


        /// <summary>
        /// cookie容器
        /// </summary>
        public CookieContainer CookiesContainer { get; set; }

        public WebSpider()
        {

        }



        public async Task<string> Start(Uri uri, string proxy = null)
        {
            return await Task.Run(() =>
            {
                var pageSource = string.Empty;
                try
                {
                    Onstart(this, new OnStartEventArgs(uri));//触发启动的事件
                    var watch = new Stopwatch();
                    watch.Start();
                    var request = WebRequest.CreateHttp(uri);
                    request.Accept = "*/*";
                    request.ServicePoint.Expect100Continue = false;//加快载入速度
                    request.ServicePoint.UseNagleAlgorithm = false;//禁止Nagle算法加快载入速度
                    request.AllowWriteStreamBuffering = false;//禁止缓冲加快载入速度
                    request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");//定义gzip压缩页面支持
                    request.ContentType = "text/html";//"application/x-www-form-urlencoded";//定义文档类型及编码
                    request.AllowAutoRedirect = false;//禁止自动跳转
                    //设置User-Agent，伪装成Google Chrome浏览器
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
                    request.Timeout = 5000;//定义请求超时时间为5秒
                    request.KeepAlive = true;//启用长连接
                    request.Method = "GET";//定义请求方式为GET              
                    if (proxy != null) request.Proxy = new WebProxy(proxy);//设置代理服务器IP，伪装请求地址
                    request.CookieContainer = this.CookiesContainer;//附加Cookie容器
                    request.ServicePoint.ConnectionLimit = int.MaxValue;//定义最大连接数

                    using (var response = (HttpWebResponse)request.GetResponse())
                    {
                        //获取请求响应

                        foreach (System.Net.Cookie cookie in response.Cookies) this.CookiesContainer.Add(cookie);//将Cookie加入容器，保存登录状态

                        if (response.ContentEncoding.ToLower().Contains("gzip"))//解压
                        {
                            using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    pageSource = reader.ReadToEnd();
                                }
                            }
                        }
                        else if (response.ContentEncoding.ToLower().Contains("deflate"))//解压
                        {
                            using (DeflateStream stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    pageSource = reader.ReadToEnd();
                                }

                            }
                        }
                        else
                        {
                            using (Stream stream = response.GetResponseStream())//原始
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {

                                    pageSource = reader.ReadToEnd();
                                }
                            }
                        }
                    }
                    request.Abort();
                    watch.Stop();
                    var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;//获取当前任务线程ID
                    var milliseconds = watch.ElapsedMilliseconds;//获取请求执行时间
                    OnCompleted(this, new OnCompletedEventArgs(uri, threadId, pageSource, milliseconds));
                }
                catch(Exception ex)
                {
                    OnException(this, new OnExcetionEventArgs(uri, ex));
                }
                return pageSource;
            });
        }
    }

    public class Script
    {
        public string Code { get; set; }

        public object[] Args { get; set; }
    }
    public class Operation
    {
        public int Timeout { get; set; }

        public Action<IWebDriver> Action { get; set; }

        public Func<IWebDriver, bool> Condition { get; set; }

    }

    /// <summary>
    /// 一个高级的爬虫类，实现了页面的自动渲染
    /// </summary>
    public class AdvancedWebSpider : WebSpider
    {
        public int SleepTimeWait { get; set; } = 0;
        /// <summary>
        /// 内核对象，用于生成网页
        /// </summary>
        private PhantomJSOptions _options;
        /// <summary>
        /// 定义通信参数
        /// </summary>
        private PhantomJSDriverService _service;

        public event EventHandler OnTotleCompleted = null;


        public AdvancedWebSpider(string proxy = null)
        {
            this._options = new PhantomJSOptions();//定义PhantomJS的参数配置对象
            this._service = PhantomJSDriverService.CreateDefaultService(Environment.CurrentDirectory);//初始化Selenium配置，传入存放phantomjs.exe文件的目录
            _service.IgnoreSslErrors = true;//忽略证书错误
            _service.WebSecurity = false;//禁用网页安全
            _service.HideCommandPromptWindow = true;//隐藏弹出窗口
            _service.LoadImages = false;//禁止加载图片
            _service.LocalToRemoteUrlAccess = true;//允许使用本地资源响应远程 URL
            _options.AddAdditionalCapability(@"phantomjs.page.settings.userAgent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36");
            if (proxy != null)
            {
                _service.ProxyType = "HTTP";//使用HTTP代理
                _service.Proxy = proxy;//代理IP及端口
            }
            else
            {
                _service.ProxyType = "none";//不使用代理
            }
        }

        public async Task Start(Uri uri, Script script, Operation operation)
        {
            await Task.Run(() =>
            {
                Onstart(this, new OnStartEventArgs(uri));//触发启动的事件
                var driver = new PhantomJSDriver(_service, _options);//实例化PhantomJS的WebDriver
                try
                {
                    var watch = DateTime.Now;
                    driver.Navigate().GoToUrl(uri.ToString());//请求URL地址
                    if (script != null) driver.ExecuteScript(script.Code, script.Args);//执行Javascript代码

                    if (operation.Action != null) operation.Action.Invoke(driver);
                    var driverWait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(operation.Timeout));//设置超时时间为x毫秒
                    if (operation.Condition != null) driverWait.Until(operation.Condition);
                    var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;//获取当前任务线程ID
                    var milliseconds = DateTime.Now.Subtract(watch).Milliseconds;//获取请求执行时间;
                    var pageSource = driver.PageSource;//获取网页Dom结构
                    OnCompleted(this, new OnCompletedEventArgs(uri, threadId, pageSource, milliseconds, driver));
                }
                catch (Exception ex)
                {
                    OnException(this, new OnExcetionEventArgs(uri, ex));
                }
                finally
                {
                    driver.Close();
                    driver.Quit();
                }
            });
        }

        public async Task StartPages(Uri uri, Script script, Operation operation,Func<IWebDriver,bool> nextPage)
        {
            await Task.Run(() =>
            {
                Onstart(this, new OnStartEventArgs(uri));//触发启动的事件
                var driver = new PhantomJSDriver(_service, _options);//实例化PhantomJS的WebDriver
                try
                {
                    var watch = DateTime.Now;
                    driver.Navigate().GoToUrl(uri.ToString());//请求URL地址
                    if (script != null) driver.ExecuteScript(script.Code, script.Args);//执行Javascript代码

                    var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;//获取当前任务线程ID

                    P1:
                    var milliseconds = (int)DateTime.Now.Subtract(watch).TotalMilliseconds;//获取请求执行时间;
                    var pageSource = driver.PageSource;//获取网页Dom结构
                    OnCompleted(this, new OnCompletedEventArgs(uri, threadId, pageSource, milliseconds, driver));

                    if (nextPage.Invoke(driver))
                    {
                        watch = DateTime.Now;
                        if (operation.Action != null) operation.Action.Invoke(driver);
                        var driverWait = new WebDriverWait(driver, TimeSpan.FromMilliseconds(operation.Timeout));//设置超时时间为x毫秒
                        if(SleepTimeWait>10) System.Threading.Thread.Sleep(SleepTimeWait);
                        if (operation.Condition != null) driverWait.Until(operation.Condition);
                        goto P1;
                    }

                    OnTotleCompleted(this, new EventArgs());
                }
                catch (Exception ex)
                {
                    OnException(this, new OnExcetionEventArgs(uri, ex));
                }
                finally
                {
                    driver.Close();
                    driver.Quit();
                }
            });
        }
    }
}
