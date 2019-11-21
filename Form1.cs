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
        private List<IData> datas;

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
            this.datas = new List<IData>();
            var rawData = File.ReadAllText("data.txt");
            string[] msgArray = Regex.Split(rawData, this.config.senderName);
            foreach (var msg in msgArray)
            {
                if (string.IsNullOrEmpty(msg))
                    continue;

                if (msg.Contains(this.config.imageName))
                {
                    //this.datas.Add(new ImageData());
                }
                else
                {
                    var splitRawDatas = GetSplitedRawData(msg);
                    var textData = new TextData();
                    textData.text = msg;
                    textData.date = GetDate(splitRawDatas);
                    textData.waixiedanwei = GetWaiXieDanWei(splitRawDatas);

                    string customerName = string.Empty;
                    string productName = string.Empty;
                    GetCustomerAndProductName(splitRawDatas, out customerName, out productName);
                    textData.customer = customerName;
                    textData.product = productName;

                    int number = 0;
                    float danjia = 0;
                    float zongjia = 0;
                    GetPriceAndNumber(splitRawDatas, out number, out danjia, out zongjia);
                    textData.number = number;
                    textData.zongjia = zongjia;
                    textData.danjia = danjia;


                    this.datas.Add(textData);
                }
            }
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
            foreach (var rawData in rawDatas)
            {
                string[] strs = rawData.Split('-');
                if (strs.Length == 2)
                {
                    customerName = strs[0];
                    productName = strs[1];
                    return;
                }
            }
        }

        string GetDate(List<string> rawDatas)
        {
            string regularExpression = @"^((10|11|12|[0]?\d).[0-3]?\d)$";//12.30,1.05,1.5,01.5
            Regex rg = new Regex(regularExpression);
            foreach (var rawData in rawDatas)
            {
                var matches = rg.Matches(rawData);
                if (matches.Count > 0)
                    return matches[0].ToString();
            }
            return string.Empty;
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
