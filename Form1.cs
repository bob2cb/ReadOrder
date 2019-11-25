using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LitJson;
using NPOI;
using NPOI.SS.UserModel;

namespace ReadWordForms
{
    public partial class Form1 : Form
    {
        private Config config;
        private string textData;
        private string imgData;
        private List<OrderData> orderDatas;

        public Form1()
        {
            //InitializeComponent();
            //Test();
            FastRun();
        }

        void Test()
        {
            string rawData = "11. 30";
            Regex rg1 = new Regex(@"(10|11|12|[0]?\d)[.]\s{0,2}[0-3]?\d");
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

        private void button_text_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            fileDialog.Filter = "文本文件|*.txt";
            fileDialog.RestoreDirectory = false;    //若为false，则打开对话框后为上次的目录。若为true，则为初始目录
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox_text.Text = Path.GetFileName(fileDialog.FileName);
                this.textData = File.ReadAllText(Path.GetFullPath(fileDialog.FileName));
            }
        }

        private void button_img_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            fileDialog.Filter = "文本文件|*.txt";
            fileDialog.RestoreDirectory = false;    //若为false，则打开对话框后为上次的目录。若为true，则为初始目录
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox_img.Text = Path.GetFileName(fileDialog.FileName);
                this.imgData = File.ReadAllText(Path.GetFullPath(fileDialog.FileName));
            }
        }

        private void button_execute_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.imgData))
            {
                MessageBox.Show($"错误！！没有图片数据");
                return;
            }
            if (string.IsNullOrEmpty(this.textData))
            {
                MessageBox.Show($"错误！！没有文字数据");
                return;
            }
            ParseConfigData();
            ParseRawData();
        }

        void FastRun()
        {
            this.textData = File.ReadAllText("data.txt");
            this.imgData = File.ReadAllText("data_img.txt");
            ParseConfigData();
            ParseRawData();
            ExportExcelData();
        }

        void ParseConfigData()
        {
            string str = File.ReadAllText(@"config\config.txt");
            this.config = JsonMapper.ToObject<Config>(str);
        }

        void ParseRawData()
        {
            this.orderDatas = new List<OrderData>();
            var textMsgArray = GetTextMsgs(textData);
            var imgMsgArray = GetImageMsgs(imgData);
            if (textMsgArray.Count != imgMsgArray.Count)
            {
                MessageBox.Show($"错误！！文字{textMsgArray.Count}条,图片{imgMsgArray.Count}张");
                return;
            }

            for (int i = 0; i < textMsgArray.Count; i++)
            {
                var orderData = new OrderData();
                orderData.text = textMsgArray[i];
                orderData.img = imgMsgArray[i];
                var splitTextDatas = GetSplitedDataByEnterAndSpace(orderData.text);
                //name
                string customerName = string.Empty;
                string productName = string.Empty;
                GetCustomerAndProductName(splitTextDatas, out customerName, out productName);
                orderData.customer = customerName;
                orderData.product = productName;
                splitTextDatas.RemoveAt(0);
                //orderDate
                string orderDate = string.Empty;
                int orderDateIndex = GetOrderDate(splitTextDatas, out orderDate);
                orderData.orderDate = orderDate;
                if (orderDateIndex != -1)
                    splitTextDatas.RemoveAt(orderDateIndex);
                //wx
                string waixie = string.Empty;
                int waixieIndex = GetWaiXie(splitTextDatas, out waixie);
                orderData.waixie = waixie;
                //wx-price
                if (waixieIndex > -1)
                {
                    int wx_number = 0;
                    float wx_danjia = 0;
                    float wx_zongjia = 0;
                    var wx_datas = splitTextDatas.Skip(waixieIndex + 1).ToList();
                    GetPriceAndNumber(wx_datas, out wx_number, out wx_danjia, out wx_zongjia);
                    orderData.wx_number = wx_number;
                    orderData.wx_zongjia = wx_zongjia;
                    orderData.wx_danjia = wx_danjia;
                }
                //zz-price
                int zz_number = 0;
                float zz_danjia = 0;
                float zz_zongjia = 0;
                var zz_datas = waixieIndex == -1 ? splitTextDatas : splitTextDatas.Take(waixieIndex).ToList();
                GetPriceAndNumber(zz_datas, out zz_number, out zz_danjia, out zz_zongjia);
                orderData.number = zz_number;
                orderData.zongjia = zz_zongjia;
                orderData.danjia = zz_danjia;

                var splitImgDatas = GetSplitedDataByEnterAndSpace(orderData.img);
                //deliveryDate
                orderData.deliveryDate = GetDeliveryDate(orderData.img);
                //size
                string size = string.Empty;
                int sizeIndex = GetSize(splitImgDatas, out size);
                orderData.size = size;
                if (sizeIndex != -1)
                    splitImgDatas.RemoveAt(sizeIndex);
                //buliao
                string buliao = string.Empty;
                int buliaoIndex = GetBuliao(splitImgDatas, out buliao);
                orderData.buliao = buliao;
                if (buliaoIndex != -1)
                    splitImgDatas.RemoveAt(buliaoIndex);
                //type
                orderData.type = GetType(orderData.img);
                //gongyi
                orderData.gongyi = GetGongyi(orderData.img);
                //yinshua
                orderData.yinshua = GetYinshua(splitImgDatas, orderData.gongyi);
                this.orderDatas.Add(orderData);
            }
            //MessageBox.Show($"成功解析{this.orderDatas.Count}条数据！！");
        }

        #region Text
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

        List<string> GetSplitedDataByEnterAndSpace(string rawData)
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

        int GetOrderDate(List<string> rawDatas,out string date)
        {
            date = string.Empty;
            for (int i = 0; i < rawDatas.Count; i++)
            {
                var matches = RegexDefine.isDate.Matches(rawDatas[i]);
                if (matches.Count == 0)
                    continue;
                date = matches[0].ToString().Replace(" ", "");
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

        #region Image
        List<string> GetImageMsgs(string rawData)
        {
            List<string> imageMsgs = new List<string>();
            var splitedArrayByEnter = Regex.Split(rawData, "\r\n");
            foreach (var line in splitedArrayByEnter)
            {
                if (RegexDefine.containsWidth.IsMatch(line))
                    imageMsgs.Add(line);
                else
                    imageMsgs[imageMsgs.Count - 1] += " " + line;
            }
            return imageMsgs;
        }
        int GetSize(List<string> rawDatas, out string size)
        {
            size = string.Empty;
            MatchCollection matches = null;
            for (int i = 0; i < rawDatas.Count; i++)
            {           
                string rawData = rawDatas[i];
                if (!RegexDefine.containsWidth.IsMatch(rawData))
                    continue;
                if (!RegexDefine.containsHeight.IsMatch(rawData))
                    continue;
                if (!RegexDefine.containsSide.IsMatch(rawData))
                    continue;
                matches = RegexDefine.containsWidth.Matches(rawData);
                string sizeWidth = matches[0].ToString();
                matches = RegexDefine.containsHeight.Matches(rawData);
                string sizeHeight = matches[0].ToString();
                matches = RegexDefine.containsSide.Matches(rawData);
                string sizeSize = matches[0].ToString();
                size = $"{sizeWidth}*{sizeHeight}*{sizeSize}";
                return i;
            }
            return -1;
        }
        string GetDeliveryDate(string rawData)
        {
            var matches = RegexDefine.isDate.Matches(rawData);
            return matches.Count != 0 ? matches[0].ToString().Replace(" ", "") : string.Empty;
        }
        string GetType(string rawData)
        {
            foreach (var type in this.config.type)
            {
                if (rawData.Contains(type.Key))
                    return type.Value;
            }
            return string.Empty;
        }
        string GetGongyi(string rawData)
        {
            foreach (var gongyi in this.config.gongyi)
            {
                if (rawData.Contains(gongyi))
                    return gongyi;
            }
            return string.Empty;
        }
        int GetBuliao(List<string> rawDatas, out string buliao)
        {
            buliao = string.Empty;
            for (int i = 0; i < rawDatas.Count; i++)
            {
                string rawData = rawDatas[i];
                if (!RegexDefine.containsG.IsMatch(rawData))
                    continue;
                buliao = rawData;
                return i;
            }
            return -1;
        }

        string GetYinshua(List<string> rawDatas, string gongyi)
        {
            foreach (var rawData in rawDatas)
            {
                foreach (var yinshua in this.config.yinshua)
                {
                    if (!rawData.Contains(yinshua))
                        continue;
                    if (string.IsNullOrEmpty(gongyi))
                    {
                        return rawData;
                    }
                    else
                    {
                        var datas = Regex.Split(rawData, gongyi);
                        foreach (var data in datas)
                        {
                            if (data.Contains(yinshua))
                                return data;
                        }
                    }
                }
            }
            return string.Empty;
        }
        #endregion

        #region Excel
        void ExportExcelData()
        {
            string importExcelPath = @"config\order.xlsx";
            string exportExcelPath = @"order_result.xlsx";
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            ISheet sheet = workbook.GetSheetAt(0);//获取第一个工作薄
            for (int i = 0; i < this.orderDatas.Count; i++)
            {
                int propertiesIndex = -3;
                var orderData = this.orderDatas[i];
                Type t = orderData.GetType();
                IRow row = (IRow)sheet.GetRow(i + 1);//获取第i+1行
                foreach (PropertyInfo pi in t.GetProperties())
                {
                    propertiesIndex++;
                    if (propertiesIndex >= 0)
                        SetCellValue(row, propertiesIndex, pi.GetValue(orderData, null));
                }
                //    SetCellValue(row, 0, orderData.orderDate);
                //SetCellValue(row, 1, orderData.customer);
                //SetCellValue(row, 2, orderData.product);
                //SetCellValue(row, 3, orderData.type);
                //SetCellValue(row, 4, orderData.size);
                //SetCellValue(row, 5, orderData.buliao);
                //SetCellValue(row, 6, orderData.yinshua);
                //SetCellValue(row, 7, orderData.gongyi);
                //SetCellValue(row, 8, orderData.number);
                //SetCellValue(row, 9, orderData.danjia);
                //SetCellValue(row, 10, orderData.zongjia);
                //row.CreateCell(0).SetCellValue("test");
            }
            //导出excel
            FileStream fs = new FileStream(exportExcelPath, FileMode.Create, FileAccess.ReadWrite);
            workbook.Write(fs);
            fs.Close();
        }
        void SetCellValue(IRow row, int cell, object vaule)
        {
            if (vaule is string)
            {
                var str = vaule.ToString();
                if (!string.IsNullOrEmpty(str))
                    row.CreateCell(cell).SetCellValue(str);
            }else if (vaule is float)
            {
                var f = (float)vaule;
                if (f > 0)
                    row.CreateCell(cell).SetCellValue(f);
            }
        }
        #endregion
    }
}


