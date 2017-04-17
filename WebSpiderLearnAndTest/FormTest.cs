using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using OpenQA.Selenium;

namespace WebSpiderLearnAndTest
{
    public partial class FormTest : Form
    {
        public FormTest()
        {
            InitializeComponent();;
        }
        
        

        private void button1_Click(object sender, EventArgs e)
        {
            //SelectCities();
            //SelectHangzhou();
            AdvancedSelectHangzhou();
        }

        private void FormTest_Load(object sender, EventArgs e)
        {
            //textBox1.Text = "http://hotels.ctrip.com/citylist";
        }

        #region 获取城市列表数据


        private async void SelectCities()
        {
            textBox1.Text = "http://hotels.ctrip.com/citylist";
            WebSpider Spider = new WebSpider();
            Spider.OnStartEvent += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    textBox3.AppendText(e.Uri.ToString() + " 开始" + Environment.NewLine);
                }));
            };
            Spider.OnExceptionEvent += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    textBox3.AppendText(e.Uri.ToString() + " 异常：" + e.Exception.Message + Environment.NewLine);
                }));
            };
            Spider.OnCompletedEvent += (s, e) =>
            {
                string pattern = "<a href=\"/hotel/[a-z0-9]+\" title=\"[^\"]+\">[^<]+</a>";
                MatchCollection mc = Regex.Matches(e.PageSource, pattern, RegexOptions.IgnoreCase);
                StringBuilder sb = new StringBuilder();
                foreach (Match m in mc)
                {
                    sb.Append(Regex.Match(m.Value, ">[^<]+").Value.Substring(1) + Environment.NewLine);
                    //sb.Append(Regex.Match(m.Value, "/hotel/.+[^\\\"]").Value.Substring(1) + Environment.NewLine);
                    //sb.Append(m.Value + Environment.NewLine);
                }

                Invoke(new Action(() =>
                {
                    label1.Text = "共计数据：" + mc.Count;
                    textBox2.Text = sb.ToString();
                    textBox3.AppendText(e.Uri.ToString() + " 耗时：" + e.MilliSeconds + Environment.NewLine);
                }));
            };
            await Spider.Start(new Uri(textBox1.Text));
        }

        #endregion

        #region 获取杭州的酒店信息
        private async void SelectHangzhou()
        {
            /**************************************************************
             * 
             *    目前只能爬取到第一页的数据信息
             *     
             *    Only crawl the first page data
             * 
             **************************************************************/



            textBox1.Text = "http://hotels.ctrip.com/hotel/hangzhou17";
            WebSpider Spider = new WebSpider();
            Spider.OnStartEvent += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    textBox3.AppendText(e.Uri.ToString() + " 开始" + Environment.NewLine);
                }));
            };
            Spider.OnExceptionEvent += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    textBox3.AppendText(e.Uri.ToString() + " 异常：" + e.Exception.Message + Environment.NewLine);
                }));
            };
            Spider.OnCompletedEvent += (s, e) =>
            {
                //<span class="hotel_num">1</span>杭州马可波罗假日酒店</a>
                string pattern = "<span class=\"hotel_num\">[0-9]+</span>[^<]+";
                string addr = "<p class=\"searchresult_htladdress\">[\\S]+";
                MatchCollection mc = Regex.Matches(e.PageSource, pattern, RegexOptions.IgnoreCase);
                MatchCollection mcaddr = Regex.Matches(e.PageSource, addr, RegexOptions.IgnoreCase);
                StringBuilder sb = new StringBuilder();

                int index = 0;
                foreach (Match m in mc)
                {
                    sb.Append(m.Value.Substring(m.Value.LastIndexOf('>') + 1) +
                        "  地址["+mcaddr[index].Value.Substring(mcaddr[index].Value.LastIndexOf('>')+1) + "]"+ Environment.NewLine);
                    //sb.Append(Regex.Match(m.Value, "/hotel/.+[^\\\"]").Value.Substring(1) + Environment.NewLine);
                    //sb.Append(m.Value + Environment.NewLine);
                    index++;
                }

                Invoke(new Action(() =>
                {
                    label1.Text = "共计数据：" + mc.Count;
                    textBox2.Text = sb.ToString();
                    textBox3.AppendText(e.Uri.ToString() + " 耗时：" + e.MilliSeconds + Environment.NewLine);
                }));
            };
            await Spider.Start(new Uri(textBox1.Text));
        }


        #endregion

        #region 高级爬虫获取所有杭州的酒店信息

        private int CountTotle { get; set; } = 0;
        private async void AdvancedSelectHangzhou()
        {
            textBox1.Text = "http://hotels.ctrip.com/hotel/hangzhou17";//"http://hotels.ctrip.com/hotel/hangzhou17";
            AdvancedWebSpider Spider = new AdvancedWebSpider();
            Spider.OnStartEvent += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    textBox3.AppendText(e.Uri.ToString() + " 开始" + Environment.NewLine);
                }));
            };
            Spider.OnExceptionEvent += (s, e) =>
            {
                Invoke(new Action(() =>
                {
                    textBox3.AppendText(e.Uri.ToString() + " 异常：" + e.Exception.Message + Environment.NewLine);
                }));
            };

            var operation = new Operation
            {
                Action = (x) =>
                {
                    //通过Selenium驱动点击页面的“酒店评论”
                    //<li id="commentTab" class='current'><a href="http://hotels.ctrip.com/hotel/dianping/435383.html">酒店点评(21156)</a></li>
                    x.FindElement(By.XPath("//div[@id='page_info']/a[@id='downHerf']")).Click();
                },
                Condition = (x) =>
                {
                    return x.FindElement(By.XPath("//div[@id='hotel_list']")).Displayed && x.FindElement(By.XPath("//div[@id='page_info']")).Displayed && x.FindElement(By.XPath("//div[@id='page_info']/a[@id='downHerf']")).Displayed;
                },
                Timeout = 5000
            };
            Spider.OnCompletedEvent += (s, e) =>
            {
                //提取酒店的名称
                StringBuilder sb = new StringBuilder();

                //var hotelName = e.WebDriver.FindElements(By.XPath("//div[contains(@class,'searchresult_list2')]/ul/li[@class='searchresult_info_name']/h2/a"));
                //<span class="hotel_num">1</span>浙江西湖山庄<

                string pattern = "<span class=\"hotel_num\">[0-9]+</span>[^<]+";
                MatchCollection mc = Regex.Matches(e.PageSource, pattern);
                foreach(Match m in mc)
                {
                    sb.Append(m.Value.Substring(m.Value.LastIndexOf('>') + 1) + Environment.NewLine);
                }
                CountTotle += mc.Count;
                Invoke(new Action(() =>
                {
                    label1.Text = "共计数据：" + CountTotle;
                    //foreach(var m in hotelName)
                    //{
                    //    sb.Append(m.Text.Replace("\n","    ") + Environment.NewLine);
                    //}
                    textBox2.AppendText(sb.ToString());
                    textBox3.AppendText(e.Uri.ToString() + " 耗时：" + e.MilliSeconds + Environment.NewLine);
                }));
            };
            await Spider.StartPages(new Uri(textBox1.Text), null, operation, (m) =>
            {
                return m.FindElement(By.XPath("//div[@id='page_info']/div/a[@class='current']")).Text !=
                    m.FindElement(By.XPath("//div[@id='page_info']/div/a[last()]")).Text;
            });
        }

        #endregion


    }
}
