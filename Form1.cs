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
            string rawData = "11.30";
            Regex rg1 = new Regex(@"^((10|11|12|[0]?\d)[.][0-3]?\d)$");
            //Regex rg2 = new Regex("[0-9]+([.]{1}[0-9]+){0,1}$");
            //Regex rg2 = new Regex(@"\d+[.]\d+");
            Console.WriteLine(rg1.IsMatch(rawData));
            if (rg1.IsMatch(rawData))
            {
                var matches = rg1.Matches(rawData);
                Console.WriteLine(matches[0].ToString());
            }
            //else if (rg2.IsMatch(rawData))
            //{
            //    var matches = rg2.Matches(rawData);
            //    Console.WriteLine(Regex.Replace(matches[0].ToString(), @"[^0-9]+", ""));
            //}
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
                orderData.date = date;
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
                    var wx_datas = splitRawDatas.Skip(waixieIndex + 1).ToList();
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
            for (int i = 0; i < rawDatas.Count; i++)
            {
                var matches = RegexDefine.isDate.Matches(rawDatas[i]);
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

        #region PriceAndNumber
        void GetPriceAndNumber(List<string> rawDatas, out int number, out float danjia, out float zongjia)
        {
            number = 0;
            danjia = 0;
            zongjia = 0;
            MatchCollection matches = null;
            string tempStr = string.Empty;
            foreach (var rawData in rawDatas)
            {
                if (IsChWithoutGeOrYuna(rawData))
                    continue;

                if (IsPerfectEquation(rawData))
                {
                    GetPriceAndNumberByOneString(rawData, ref number, ref danjia, ref zongjia);
                    return;
                }

                if (number == 0 && RegexDefine.containsGe.IsMatch(rawData))
                {
                    matches = RegexDefine.containsGe.Matches(rawData);
                    tempStr = GetIntOrFloatString(matches[0].ToString());
                    int.TryParse(tempStr, out number);
                }
                if (danjia == 0 && RegexDefine.containsPerge.IsMatch(rawData))
                {
                    matches = RegexDefine.containsPerge.Matches(rawData);
                    tempStr = GetIntOrFloatString(matches[0].ToString());
                    float.TryParse(tempStr, out danjia);
                }
                if (zongjia == 0 && RegexDefine.containsEqualInLeft.IsMatch(rawData))
                {
                    matches = RegexDefine.containsEqualInLeft.Matches(rawData);
                    tempStr = GetIntOrFloatString(matches[0].ToString());
                    float.TryParse(tempStr, out zongjia);
                }
                if (HasValueWithNumberAndPrice(number, danjia, zongjia))
                    return;
                if ((danjia == 0 || number == 0) && RegexDefine.containsEqualInRight.IsMatch(rawData))
                {
                    matches = RegexDefine.containsEqualInRight.Matches(rawData);
                    SetValueToDanjiaOrNumber(matches[0].ToString(),ref danjia, ref number);
                }
                if (HasValueWithNumberAndPrice(number, danjia, zongjia))
                    return;
                if ((danjia == 0 || number == 0) && rawData.Contains('*'))
                {
                    var splitedStr = rawData.Split('*');
                    foreach (var str in splitedStr)
                        SetValueToDanjiaOrNumber(str, ref danjia, ref number);
                }
                if (HasValueWithNumberAndPrice(number, danjia, zongjia))
                    return;
                if ((danjia == 0 || zongjia == 0) && RegexDefine.containsYuan.IsMatch(rawData))
                {
                    matches = RegexDefine.containsYuan.Matches(rawData);
                    SetValueToDanjiaOrZongjia(matches[0].ToString(), ref danjia, ref zongjia);
                }
                if (HasValueWithNumberAndPrice(number, danjia, zongjia))
                    return;
                //if ((danjia == 0 || zongjia == 0) && RegexDefine.containsMulit.IsMatch(rawData))
                //{
                //    matches = RegexDefine.containsMulit.Matches(rawData);
                //    SetValueToDanjiaOrZongjia(matches[0].ToString(), ref danjia, ref zongjia);
                //}
                //if (HasValueWithNumberAndPrice(number, danjia, zongjia))
                //    return;
                if ((zongjia == 0) && RegexDefine.isIntOrfloat.IsMatch(rawData) && !RegexDefine.isDate.IsMatch(rawData))
                {
                    matches = RegexDefine.isIntOrfloat.Matches(rawData);
                    tempStr = GetIntOrFloatString(matches[0].ToString());
                    float.TryParse(tempStr, out zongjia);
                }
            }
        }
        bool HasValueWithNumberAndPrice(int number, float danjia, float zongjia)
        {
            return number > 0 && danjia > 0 && zongjia > 0;
        }

        bool IsChWithoutGeOrYuna(string str)
        {
            return RegexDefine.isCh.IsMatch(str) && !RegexDefine.containsGe.IsMatch(str) && !RegexDefine.containsYuan.IsMatch(str);
        }
        bool IsPerfectEquation(string str)
        {
            List<string> results = new List<string>();
            var splitedArrWithEqual = str.Split('=');
            foreach (var splitedWithEqual in splitedArrWithEqual)
            {
                var splitedArrWithX = splitedWithEqual.Split('*');
                foreach (var splitedWithX in splitedArrWithX)
                {
                    if (!string.IsNullOrEmpty(splitedWithX))
                        results.Add(splitedWithX);
                }
            }
            return results.Count == 3;
        }
        void GetPriceAndNumberByOneString(string rawData, ref int number, ref float danjia, ref float zongjia)
        {
            var matches = RegexDefine.containsEqualInLeft.Matches(rawData);
            string equalRightStr = matches[0].ToString();
            var tempStr = GetIntOrFloatString(equalRightStr);
            float.TryParse(tempStr, out zongjia);

            tempStr = rawData.Substring(0, rawData.IndexOf(equalRightStr));
            var danjiaAndNumber = tempStr.Split('*');
            var shuziStr1 = GetIntOrFloatString(danjiaAndNumber[0]);
            var shuziStr2 = GetIntOrFloatString(danjiaAndNumber[1]);
            float shuzi1 = 0;
            float shuzi2 = 0;
            float.TryParse(shuziStr1, out shuzi1);
            float.TryParse(shuziStr2, out shuzi2);
            if (shuzi1 > 10)
            {
                number = (int)shuzi1;
                danjia = shuzi2;
            }
            else
            {
                number = (int)shuzi2;
                danjia = shuzi1;
            }
        }
        string GetIntOrFloatString(string rawData)
        {
            var matches = RegexDefine.containsIntOrfloat.Matches(rawData);
            return matches.Count > 0 ? matches[0].ToString() : string.Empty;
        }

        void SetValueToDanjiaOrNumber(string rawData, ref float danjia, ref int number)
        {
            float tempFloat = 0;
            var tempStr = GetIntOrFloatString(rawData);
            float.TryParse(tempStr, out tempFloat);
            if (RegexDefine.isFloat.IsMatch(rawData) || tempFloat < 10)
                danjia = tempFloat;
            else
                number = (int)tempFloat;
        }
        void SetValueToDanjiaOrZongjia(string rawData, ref float danjia, ref float zongjia)
        {
            float tempFloat = 0;
            var tempStr = GetIntOrFloatString(rawData);
            //var tempStr = Regex.Replace(rawData, @"[^0-9]+", "");
            float.TryParse(tempStr, out tempFloat);
            if (tempFloat < 10)
                danjia = tempFloat;
            else
                zongjia = tempFloat;
        }
        #endregion
    }
}
