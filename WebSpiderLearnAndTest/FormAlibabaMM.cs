using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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

        private async void GetMM()
        {
            textBox1.Text = "https://www.taobao.com/markets/mm/mmku?spm=5679.126488.640763.1.KmoNZE";
            AdvancedWebSpider Spider = new AdvancedWebSpider();

            Spider.OnCompletedEvent += (s, e) =>
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

                 */
                



                Invoke(new Action(() =>
                {
                    textBox2.Text = e.PageSource;
                }));
            };
            await Spider.Start(new Uri(textBox1.Text), null, new Operation());
        }
    }
}
