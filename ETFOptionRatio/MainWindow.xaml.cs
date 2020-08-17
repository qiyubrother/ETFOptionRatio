using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using mshtml;

namespace ETFOptionRatio
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> lstRenGou = new List<string>();
        List<string> lstRenGu = new List<string>();
        Thread workThread = null;
        int taskNo = 1;
        public MainWindow()
        {
            InitializeComponent();

            web.Navigate("http://quote.eastmoney.com/center/gridlist.html#options_sh50etf_call");

        }
        /// <summary>
        /// 开始提取数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetData_Click(object sender, RoutedEventArgs e)
        {
            var doc = web.Document as HTMLDocument;
            if (doc == null) return;

            IHTMLTable table = (IHTMLTable)doc.getElementById("table_wrapper-table");
            var rowIndex = 0;

            #region 提取数据
            foreach (IHTMLElement row in table.rows)
            {
                if (rowIndex == 0)
                {
                    rowIndex++;
                    continue;
                }
                var cjl = row.children[8].innerText.ToString(); // 持仓量

                if (taskNo == 1) 
                {
                    lstRenGou.Add(cjl);
                }
                else
                {
                    lstRenGu.Add(cjl);
                }
                Console.WriteLine(cjl);
            }
            #endregion
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var doc = web.Document as HTMLDocument;
            if (doc == null) return;
            IHTMLTable table = (IHTMLTable)doc.getElementById("table_wrapper-table");

            #region 点击下一页
            foreach (IHTMLElement item in doc.getElementsByTagName("a"))
            {
                var text = item.innerHTML.ToString();
                if (text == "下一页")
                {
                    string className = item.getAttribute("className").ToString();
                    if (className.Contains("disabled"))
                    {
                        //MessageBox.Show("提取完成！");
                        workThread?.Abort();

                        if (taskNo == 1)
                        {
                            taskNo++;
                            RenGuQiQuan_Click(null, null); // Task 2
                            AutoStart2_Click(null, null);
                        }
                        else
                        {
                            Compute_Click(null, null);
                        }
                        break;
                    }
                    item.click();
                    break;
                }
            }
            #endregion
        }
        /// <summary>
        /// 运行自动提取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoStart_Click(object sender, RoutedEventArgs e)
        {
            AutoStart.IsEnabled = false;
            lstRenGou.Clear();
            lstRenGu.Clear();
            web.Navigate("http://quote.eastmoney.com/center/gridlist.html#options_sh50etf_call");

            if (taskNo >= 2)
            {
                taskNo = 1;
                lblRenGou.Content = string.Empty;
                lblRenGu.Content = string.Empty;
                lblRate.Content = string.Empty;

            }
            workThread = new Thread(new ThreadStart(new Action(()=> {
                Thread.Sleep(2000);
                while (taskNo == 1)
                {
                    Dispatcher.Invoke(new Action(()=> GetData_Click(null, null)));
                    Thread.Sleep(1800);
                    Dispatcher.Invoke(new Action(() => Next_Click(null, null)));
                }
            })));

            workThread.Start();
        }
        /// <summary>
        /// 认沽期权
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenGuQiQuan_Click(object sender, RoutedEventArgs e)
        {
            web.Navigate("http://quote.eastmoney.com/center/gridlist.html#options_sh50etf_put");
        }

        /// <summary>
        /// 运行自动提取
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AutoStart2_Click(object sender, RoutedEventArgs e)
        {
            workThread = new Thread(new ThreadStart(new Action(() => {
                Thread.Sleep(3000);
                while (taskNo == 2)
                {
                    Dispatcher.Invoke(new Action(() => GetData_Click(null, null)));
                    Thread.Sleep(1800);
                    Dispatcher.Invoke(new Action(() => Next_Click(null, null)));
                }
            })));

            workThread.Start();
        }

        private void Compute_Click(object sender, RoutedEventArgs e)
        {
            var SumRenGou = 0;
            foreach(var item in lstRenGou)
            {
                if (item.Contains("万"))
                {
                    SumRenGou += (int)(Convert.ToSingle(item.Replace("万", string.Empty).Trim()) * 10000);
                }
                else
                {
                    SumRenGou += Convert.ToInt32(item);
                }
            }
            var SumRenGu = 0;
            foreach (var item in lstRenGu)
            {
                if (item.Contains("万"))
                {
                    SumRenGu += (int)(Convert.ToSingle(item.Replace("万", string.Empty).Trim()) * 10000);
                }
                else
                {
                    SumRenGu += Convert.ToInt32(item);
                }
            }
            var rate = Math.Round(1.0f * SumRenGu / SumRenGou, 2);

            lblRenGou.Content = SumRenGou.ToString();
            lblRenGu.Content = SumRenGu.ToString();
            lblRate.Content = rate.ToString();
            AutoStart.IsEnabled = true;

            //var s = $"认购：{SumRenGou}，认沽：{SumRenGu}，比值：{rate}";

            //MessageBox.Show(s);


        }
    }
}
