using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebSpiderLearnAndTest
{
    public partial class FormAlibabaMM : Form
    {
        public FormAlibabaMM()
        {
            InitializeComponent();
        }

        private void FormAlibabaMM_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetMM();

        }

        int NumberCount { get; set; } = 0;

        private async void GetMM()
        {
            textBox1.Text = "https://www.taobao.com/markets/mm/mmku?spm=5679.126488.640763.1.KmoNZE";
            textBox2.Clear();
            textBox3.Text = @"D:\图片3";
            NumberCount = 0;
            if (!System.IO.Directory.Exists(textBox3.Text))
            {
                System.IO.Directory.CreateDirectory(textBox3.Text);
            }


            AdvancedWebSpider Spider = new AdvancedWebSpider();
            Spider.SleepTimeWait = 500;
            Spider.OnExceptionEvent += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    textBox2.Text = e.Exception.Message + Environment.NewLine + e.Exception.StackTrace;
                }));
            };

            Spider.OnTotleCompleted += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    textBox2.AppendText("完成！");
                }));
            };

            Spider.OnCompletedEvent += DealWithData;
            Operation operater = new Operation()
            {
                Action = (x) => x.FindElement(By.XPath("//div[@class='paginations']/a[contains(@class,'next')]")).Click(),
                Timeout = 5000,
                Condition = (x) =>
                {
                    return x.FindElement(By.XPath("//div[@id='fn_page']")).Displayed &&
                    x.FindElement(By.XPath("//div[@class='fn_listing']")).Displayed &&
                    x.FindElement(By.XPath("//div[@class='paginations']")).Displayed &&
                    x.FindElement(By.XPath("//div[@class='paginations']/a[contains(@class,'next')]")).Displayed;
                }
            };
            //await Spider.Start(new Uri(textBox1.Text), null, new Operation());
            await Spider.StartPages(new Uri(textBox1.Text), null, operater, (x) =>
             {
                 return x.FindElement(By.XPath("//div[@class='paginations']/span[contains(@class,'current')]")).Text !=
                     x.FindElement(By.XPath("//div[@class='paginations']/span[@class='skip-wrap']/em")).Text;
             });
        }

        private void DealWithData(object sender, OnCompletedEventArgs e)
        {
            /*
            <div class="cons_li">
                <a href="//da.taobao.com/n/author/homepage?userId=728310618" target="_blank" class="item_url">
                    <div class="item_img">
                        <img alt=" " src="//img.alicdn.com/imgextra/i3/728310618/TB2s7glc3NlpuFjy0FfXXX3CpXa_!!728310618-2-beehive-scenes.png_468x468q75.jpg">
                    </div>
                    <div class="item_name">
                        <p>楚菲楚然twins</p>
                    </div>
                </a>
                <a href="javascript:void(0);" class="ewm_hover clearfix">
                    <img class="ewm_icon" alt=" " src="//gw.alicdn.com/tps/TB12apyPXXXXXaaXXXXXXXXXXXX-16-16.png">
                    <div class="zz"></div>
                    <div class="ewm_img" url="https://h5.m.taobao.com/daren/home.html?userId=728310618" id="list_img0" title="https://h5.m.taobao.com/daren/home.html?userId=728310618">
                    <canvas width="110" height="110"></canvas><img style="display: none;"></div>
                </a>
            </div>


            <div class="cons_li">
                <a href="//da.taobao.com/n/author/homepage?userId=69226163" target="_blank" class="item_url">
                    <div class="item_img">
                        <img data-ks-lazyload="http://gw1.alicdn.com/tfscom/tuitui/TB1td7MKXXXXXXrXXXXXXXXXXXX_468x468q75.jpg" alt=" ">
                    </div>
                    <div class="item_name">
                        <p>薇娅viya</p>
                    </div>
                </a>
                <a href="javascript:void(0);" class="ewm_hover clearfix">
                    <img class="ewm_icon" data-ks-lazyload="//gw.alicdn.com/tps/TB12apyPXXXXXaaXXXXXXXXXXXX-16-16.png" alt=" ">
                    <div class="zz"></div>
                    <div class="ewm_img" url="https://h5.m.taobao.com/daren/home.html?userId=69226163" id="list_img4" title="https://h5.m.taobao.com/daren/home.html?userId=69226163">
                    <canvas width="110" height="110"></canvas><img style="display: none;"></div>
                </a>
            </div>
            <div id="fn_page" class="fn_page">
                <div class="paginations">
                    <a href="#" class="item J_Item prev disabled" aria-label="上一页">&lt;</a>
                    <span class="item current">1</span>
                    <a href="#" class="item J_Item" data-page="2">2</a>
                    <a href="#" class="item J_Item" data-page="3">3</a>
                    <a href="#" class="item J_Item" data-page="4">4</a>
                    <span class="item dot">...</span>
                    <a href="#" class="item J_Item" data-page="67">67</a>
                    <a href="#" class="item J_Item next" aria-label="下一页">下一页&gt;</a>
                    <span class="skip-wrap">
                        共 <em>67</em> 页
                        到第 <input type="text" class="input J_SkipInput" value="1" aria-label="页码输入框"> 页
                        <button class="J_Item skip" aria-label="确定跳转">确定</button>
                    </span>
                </div>
            </div>

             */

            var items = e.WebDriver.FindElements(OpenQA.Selenium.By.XPath("//div[@class='cons_li']"));
            foreach (var m in items)
            {
                ////div[@class='item_img']/img\"
                try
                {
                    string src = m.FindElement(OpenQA.Selenium.By.XPath(".//div[@class='item_img']/img")).GetAttribute("src");
                    string name = m.FindElement(OpenQA.Selenium.By.XPath(".//div[@class='item_name']/p")).Text;
                    if (string.IsNullOrEmpty(src))
                    {
                        src = m.FindElement(OpenQA.Selenium.By.XPath(".//div[@class='item_img']/img")).GetAttribute("data-ks-lazyload");
                        if (!src.StartsWith("http")) src = "http:" + src;
                    }
                    NumberCount++;
                    string filename = textBox3.Text + "\\" + name + src.Substring(src.LastIndexOf('.'));
                    if (!System.IO.File.Exists(filename))
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile(src, textBox3.Text + "\\" + name + src.Substring(src.LastIndexOf('.')));
                        webClient.Dispose();
                    }

                    Invoke(new Action(() =>
                    {
                        textBox2.AppendText(NumberCount + " : " + name + Environment.NewLine + src + Environment.NewLine + Environment.NewLine);
                    }));
                }
                catch (Exception ex)
                {
                    Exception e1 = ex;
                    Invoke(new Action(() =>
                    {
                        textBox4.AppendText(m.ToString() + Environment.NewLine);
                    }));
                }
            }
        }
    }
}
