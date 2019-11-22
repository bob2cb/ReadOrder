using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LitJson;

namespace ReadWordForms
{
    public partial class Form1 : Form
    {
        private Config config;
        private string fullPath;
        private List<OrderData> orderDatas;

        public Form1()
        {
            //InitializeComponent();
            //Test();
            FastRun();
        }

        private void button_up_Click(object sender, EventArgs e)
        {
            //OpenFileDialog fileDialog = new OpenFileDialog();
            //fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); 
            //fileDialog.Filter = "文本文件|*.txt";
            //fileDialog.RestoreDirectory = false;    //若为false，则打开对话框后为上次的目录。若为true，则为初始目录
            //if (fileDialog.ShowDialog() == DialogResult.OK)
            //{
            //    string str = File.ReadAllText(@"config\config.txt");
            //    this.config = JsonMapper.ToObject<Config>(str);
            //    this.TextBoxFile.Text = Path.GetFileName(fileDialog.FileName);
            //    this.fullPath = Path.GetFullPath(fileDialog.FileName);
            //    Console.WriteLine(this.rawData);
            //}
        }

        private void button_bottom_Click(object sender, EventArgs e)
        {

        }

        void Test()
        {
            string rawData = "2940";
            Regex rg1 = new Regex(@"=\d+[\u5143]?");
            Regex rg2 = new Regex(@"=?\d+[\u5143]");
            if (rg1.IsMatch(rawData))
            {
                var matches = rg1.Matches(rawData);
                Console.WriteLine(Regex.Replace(matches[0].ToString(), @"[^0-9]+", ""));
            }
            else if (rg2.IsMatch(rawData))
            {
                var matches = rg2.Matches(rawData);
                Console.WriteLine(Regex.Replace(matches[0].ToString(), @"[^0-9]+", ""));
            }
        }



        void FastRun()
        {
            string str = File.ReadAllText(@"config\config.txt");
            this.config = JsonMapper.ToObject<Config>(str);
            ParseRawData();
        }

        void ParseRawData()
        {
            this.orderDatas = new List<OrderData>();
            var rawData = File.ReadAllText("data.txt");
            var textMsgArray = GetTextMsgs(rawData);

            for (int i = 0; i < textMsgArray.Count; i++)
            {
                var orderData = new OrderData();
                orderData.text = textMsgArray[i];
                var splitRawDatas = GetSplitedRawData(orderData.text);
                //name
                string customerName = string.Empty;
                string productName = string.Empty;
                GetCustomerAndProductName(splitRawDatas, out customerName, out productName);
                orderData.customer = customerName;
                orderData.product = productName;
                splitRawDatas.RemoveAt(0);
                //date
                string date = string.Empty;
                int index = GetDate(splitRawDatas, out date);
                if (index != -1)
                    splitRawDatas.RemoveAt(index);
                //wx
                string waixie = string.Empty;
                int waixieIndex = GetWaiXie(splitRawDatas, out waixie);
                orderData.waixie = waixie;
                //wx-price
                if (waixieIndex > -1)
                {
                    int wx_number = 0;
                    float wx_danjia = 0;
                    float wx_zongjia = 0;
                    var wx_datas = splitRawDatas.Skip(waixieIndex).ToList();
                    GetPriceAndNumber(wx_datas, out wx_number, out wx_danjia, out wx_zongjia);
                    orderData.wx_number = wx_number;
                    orderData.wx_zongjia = wx_zongjia;
                    orderData.wx_danjia = wx_danjia;
                }
                //zz-price
                int zz_number = 0;
                float zz_danjia = 0;
                float zz_zongjia = 0;
                var zz_datas = waixieIndex == -1 ? splitRawDatas : splitRawDatas.Take(waixieIndex).ToList();
                GetPriceAndNumber(zz_datas, out zz_number, out zz_danjia, out zz_zongjia);
                orderData.number = zz_number;
                orderData.zongjia = zz_zongjia;
                orderData.danjia = zz_danjia;

                this.orderDatas.Add(orderData);
            }
        }


        List<string> GetTextMsgs(string rawData)
        {
            List<string> textMsgs = new List<string>();
            var splitMsgs = Regex.Split(rawData, this.config.senderName);
            foreach (var msg in splitMsgs)
            {
                if (string.IsNullOrEmpty(msg))
                    continue;
                if (msg.Contains(this.config.imageName))
                    continue;
                textMsgs.Add(msg);
            }
            return textMsgs;
        }

        List<string> GetSplitedRawData(string rawData)
        {
            List<string> result = new List<string>();
            var splitedArrayByEnter = Regex.Split(rawData, "\r\n");
            foreach (var splitedByEnter in splitedArrayByEnter)
            {
                var splitedArrayBySpace = Regex.Split(splitedByEnter, @"\s{1,}");
                foreach (var splitedBySpace in splitedArrayBySpace)
                {
                    if (string.IsNullOrEmpty(splitedBySpace))
                        continue;
                    result.Add(splitedBySpace);
                }
            }
            return result;
        }

        void GetCustomerAndProductName(List<string> rawDatas, out string customerName, out string productName)
        {
            customerName = string.Empty;
            productName = string.Empty;
            string[] datas = rawDatas[0].Split('-');
            if (datas.Length == 1)
            {
                productName = datas[0];
            }
            else
            {
                customerName = datas[0];
                productName = datas[1];
            }
        }

        int GetDate(List<string> rawDatas,out string date)
        {
            date = string.Empty;
            string regularExpression = @"^((10|11|12|[0]?\d).[0-3]?\d)$";//12.30,1.05,1.5,01.5
            Regex rg = new Regex(regularExpression);
            for (int i = 0; i < rawDatas.Count; i++)
            {
                var matches = rg.Matches(rawDatas[i]);
                if (matches.Count == 0)
                    continue;
                date = matches[0].ToString();
                return i;
            }
            return -1;
        }


        int GetWaiXie(List<string> rawDatas,out string waixie)
        {
            waixie = this.config.zizhi;
            for (int i = 0; i < rawDatas.Count; i++)
            {
                if (!rawDatas[i].Contains(this.config.waixie))
                    continue;
                waixie = rawDatas[i].Replace(this.config.waixie, "");
                return i;
            }
            return -1;
        }

        string GetWaiXieDanWei(List<string> rawDatas)
        {
            foreach (var rawData in rawDatas)
            {
                if (rawData.Contains(this.config.zizhi))
                    return this.config.zizhi;
                else if (rawData.Contains(this.config.waixie))
                    return rawData.Replace(this.config.waixie, "");
            }
            return string.Empty;
        }

        void GetPriceAndNumber(List<string> rawDatas, out int number, out float danjia, out float zongjia)
        {
            number = 0;
            danjia = 0;
            zongjia = 0;

            Regex rg_float = new Regex(@"\d+.\d+");
            Regex rg_num = new Regex(@"\d+[\u4e2a]");//*个
            //string regularExpressionX = "[*]";
            Regex rg_zj = new Regex(@"=\d+");
            Regex rg_djOrNum = new Regex(@"\d+=");
            //Regex rg_x = new Regex(regularExpressionX);
            MatchCollection matches = null;
            string tempStr = string.Empty;
            foreach (var rawData in rawDatas)
            {
                if (rg_num.IsMatch(rawData))
                {
                    matches = rg_num.Matches(rawData);
                    tempStr = Regex.Replace(matches[0].ToString(), @"[^0-9]+", "");
                    int.TryParse(tempStr, out number);
                }
                if (rg_zj.IsMatch(rawData))
                {
                    matches = rg_zj.Matches(rawData);
                    tempStr = Regex.Replace(matches[0].ToString(), @"[^0-9]+", "");
                    float.TryParse(tempStr, out zongjia);
                }
                if (rg_djOrNum.IsMatch(rawData))
                {
                    matches = rg_djOrNum.Matches(rawData);
                    tempStr = matches[0].ToString();
                    var splitedStr = tempStr.Split('*');
                    if (splitedStr.Length == 1) //只有1个数字
                    {
                        if (rg_float.IsMatch(splitedStr[0]))
                        {
                            matches = rg_float.Matches(rawData);
                            float.TryParse(matches[0].ToString(), out danjia);
                        }
                        else
                        {
                            tempStr = Regex.Replace(tempStr, @"[^0-9]+", "");
                            float tempFloat = 0;
                            float.TryParse(tempStr, out tempFloat);
                            if (tempFloat > 10)
                                number = (int)tempFloat;
                            else
                                danjia = tempFloat;
                        }
                    }
                }
            }
        }


        int GetNumber(List<string> rawDatas)
        {
            int result_int = 0;
            string regularExpression = @"\d+[\u4e2a]";
            Regex rg = new Regex(regularExpression);
            foreach (var rawData in rawDatas)
            {
                if (rg.IsMatch(rawData))
                {
                    string numberStr = Regex.Replace(rawData, @"[^0-9]+", "");
                    return int.TryParse(numberStr, out result_int) ? result_int : 0;
                }
            }
            return result_int;
        }

        int GetZongjia(List<string> rawDatas)
        {
            int result_int = 0;
            MatchCollection matches = null;
            string regularExpression1 = @"=?\d+[\u5143]";//*元
            string regularExpression2 = @"=\d+[\u5143]?";//=*
            Regex rg1 = new Regex(regularExpression1);
            Regex rg2 = new Regex(regularExpression2);
            foreach (var rawData in rawDatas)
            {
                if (rg1.IsMatch(rawData))
                {
                    matches = rg1.Matches(rawData);
                }
                else if (rg2.IsMatch(rawData))
                {
                    matches = rg2.Matches(rawData);
                }
                if (matches == null || matches.Count == 0)
                    continue;
                string result_str = Regex.Replace(matches[0].ToString(), @"[^0-9]+", "");
                return int.TryParse(result_str, out result_int) ? result_int : 0;
            }
            return result_int;
        }

        int GetDanjia(List<string> rawDatas)
        {
            int result_int = 0;
            MatchCollection matches = null;
            string regularExpression1 = @"=?\d+[\u5143]";//*元
            string regularExpression2 = @"=\d+[\u5143]?";//=*
            Regex rg1 = new Regex(regularExpression1);
            Regex rg2 = new Regex(regularExpression2);
            foreach (var rawData in rawDatas)
            {
                if (rg1.IsMatch(rawData))
                {
                    matches = rg1.Matches(rawData);
                }
                else if (rg2.IsMatch(rawData))
                {
                    matches = rg2.Matches(rawData);
                }
                if (matches == null || matches.Count == 0)
                    continue;
                string result_str = Regex.Replace(matches[0].ToString(), @"[^0-9]+", "");
                return int.TryParse(result_str, out result_int) ? result_int : 0;
            }
            return result_int;
        }
    }
}
