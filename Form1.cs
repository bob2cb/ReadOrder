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
using System.Threading;
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
        private string dataPath = string.Empty;

        private string lastExcelFullPath
        {
            set { this.textBox_last.Text = value; }
            get { return this.textBox_last.Text; }
        }
        private string exportExcelName
        {
            get { return string.IsNullOrEmpty(this.textBox_name.Text) ? DateTime.Now.ToString("d") : this.textBox_name.Text; }
        }
        private const string DEFAULT_EXCEL_FULLPATH = @"config\order.xlsx";
        private const string DEFAULT_TEXTBOX_TEXT = "输入导出文件名";

        public Form1()
        {
            InitializeComponent();
            //Test();
            //FastRun();
            bw.DoWork += bw_DoWork;
            bw.ProgressChanged += bw_ProgressChanged;
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
                this.dataPath = Path.GetDirectoryName(fileDialog.FileName);
            }
        }

        private void button_img_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = string.IsNullOrEmpty(this.dataPath)?Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory): this.dataPath;
            fileDialog.Filter = "文本文件|*.txt";
            fileDialog.RestoreDirectory = false;    //若为false，则打开对话框后为上次的目录。若为true，则为初始目录
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                this.textBox_img.Text = Path.GetFileName(fileDialog.FileName);
                this.imgData = File.ReadAllText(Path.GetFullPath(fileDialog.FileName));
            }
        }

        private void button_last_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = string.IsNullOrEmpty(this.dataPath) ? Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) : this.dataPath;
            fileDialog.Filter = "Excel文件|*.xlsx;*.xls";
            fileDialog.RestoreDirectory = false;    //若为false，则打开对话框后为上次的目录。若为true，则为初始目录
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                lastExcelFullPath = Path.GetFullPath(fileDialog.FileName);
            }
        }

        private void textBox_name_GotFocus(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == DEFAULT_TEXTBOX_TEXT)
                textBox.Text = "";
        }

        private void textBox_name_LostFocus(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrEmpty(textBox.Text))
                textBox.Text = DEFAULT_TEXTBOX_TEXT;
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
            DisableAllControls();
            Execute();
            EnableAllControls();
        }

        void FastRun()
        {
            this.textData = File.ReadAllText("data.txt");
            this.imgData = File.ReadAllText("data_img.txt");
            Execute();
        }

        void Execute()
        {
            ClearConsole();
            WriteToConsole(System.DateTime.Now.ToString("F"));      
            bw.RunWorkerAsync();
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            bw.ReportProgress(0, "开始解析配置文件...");
            ParseConfigData();
            bw.ReportProgress(10, "开始解析订单数据...");
            ParseRawData();
            bw.ReportProgress(90, string.IsNullOrEmpty(lastExcelFullPath) ? "开始导出新的Excel..." : $"继续上一次Excel：{lastExcelFullPath} 导出");
            ExportExcelData();
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            WriteToConsole(e.UserState.ToString());
        }

        void ParseConfigData()
        {
            string str = File.ReadAllText(@"config\config.txt");
            if (string.IsNullOrEmpty(str))
            {
                MessageBox.Show("配置文件没有数据");
                return;
            }
            this.config = JsonMapper.ToObject<Config>(str);
        }

        void ParseRawData()
        {      

            var textMsgArray = GetTextMsgs(textData);
            var imgMsgArray = GetImageMsgs(imgData);
            this.orderDatas = new List<OrderData>();
            if (textMsgArray.Count != imgMsgArray.Count)
            {
                MessageBox.Show($"数据不匹配！！文字{textMsgArray.Count}条,图片{imgMsgArray.Count}张");
                return;
            }

            for (int i = 0; i < textMsgArray.Count; i++)
            {
                //WriteToConsole($"正在解析{this.orderDatas.Count}/{textMsgArray.Count}数据！！");
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
                //type
                orderData.type = GetType(orderData.img);
                //buliao
                orderData.buliao = GetBuliao(splitImgDatas, orderData.type);
                //gongyi
                orderData.gongyi = GetGongyi(orderData.img);
                //yinshua
                orderData.yinshua = GetYinshua(splitImgDatas, orderData.buliao, orderData.gongyi);
                //tishou
                orderData.buliao = AddTishouToBuliao(splitImgDatas, orderData.buliao);
                this.orderDatas.Add(orderData);
                int percent = 10 + (int)(this.orderDatas.Count / (float)textMsgArray.Count * 80);
                bw.ReportProgress(percent, $"解析 {this.orderDatas.Count}/{textMsgArray.Count} 数据");
                Thread.Sleep(500);
            }
        }

        #region Text
        List<string> GetTextMsgs(string rawData)
        {
            string senderName = GetSenderName(rawData);
            List<string> textMsgs = new List<string>();
            var splitMsgs = Regex.Split(rawData, senderName);
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
        string GetSenderName(string rawData)
        {
            foreach (var sendName in this.config.senderName)
            {
                if (rawData.Contains(sendName))
                    return sendName;
            }
            return rawData;
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
                date = $"{this.config.year}.{date}";
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
                rawData = CorrectSizeData(rawData);
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
        string CorrectSizeData(string rawData)
        {
            foreach (var sizeCorrect in this.config.sizeCorrect)
                rawData = rawData.Replace(sizeCorrect.Key, sizeCorrect.Value);
            return rawData;
        }
        string GetDeliveryDate(string rawData)
        {
            string date = string.Empty;
            var matches = RegexDefine.isDate.Matches(rawData);
            if (matches.Count > 0)
                date = $"{this.config.year}.{matches[0].ToString().Replace(" ", "")}";
            return date;
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
        string GetBuliao(List<string> rawDatas,string type)
        {
            if (type == this.config.wufangbudai)
            {
                foreach (var rawData in rawDatas)
                {
                    var matches = RegexDefine.containsWufangbu.Matches(rawData);
                    if (matches.Count > 0)
                        return matches[0].ToString();
                }
            }
            return string.Empty;
        }
        string AddTishouToBuliao(List<string> rawDatas, string buliao)
        {
            foreach (var rawData in rawDatas)
            {
                var tishouIndex = rawData.IndexOf(this.config.tishou);
                if (tishouIndex > -1)
                    return $"{buliao} {rawData.Substring(tishouIndex, rawData.Length - tishouIndex)}";
            }
            return buliao;
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

        string GetYinshua(List<string> rawDatas, string buliao, string gongyi)
        {
            foreach (var rawData in rawDatas)
            {
                foreach (var yinshua in this.config.yinshua)
                {
                    if (!rawData.Contains(yinshua)) 
                        continue;
                    var datas = SplitByBuliaoAndGongyi(rawData, buliao, gongyi);
                    foreach (var data in datas)
                    {
                        if (data.Contains(yinshua))
                            return data;
                    }
                }
            }
            return string.Empty;
        }

        List<string> SplitByBuliaoAndGongyi(string rawData, string buliao, string gongyi)
        {
            var result = new List<string>();
            var datas = Regex.Split(rawData, buliao);
            foreach (var data in datas)
                result.AddRange(Regex.Split(data, gongyi));
            return result;
        }

        #endregion

        #region Excel
        void ExportExcelData()
        {       
            string importExcelPath = string.IsNullOrEmpty(lastExcelFullPath)? DEFAULT_EXCEL_FULLPATH: lastExcelFullPath;
            string exportExcelPath = $"{this.dataPath}/{exportExcelName}.xlsx";
            IWorkbook workbook = WorkbookFactory.Create(importExcelPath);
            ISheet sheet = workbook.GetSheetAt(0);//获取第一个工作薄
            int startRow = GetStartRow(sheet);
            for (int i = 0; i < this.orderDatas.Count; i++)
            {
                int propertiesIndex = -3;
                var orderData = this.orderDatas[i];
                Type t = orderData.GetType();
                IRow row = (IRow)sheet.GetRow(i + startRow);
                foreach (PropertyInfo pi in t.GetProperties())
                {
                    propertiesIndex++;
                    if (propertiesIndex >= 0)
                        SetCellValue(row, propertiesIndex, pi.GetValue(orderData, null));
                }
            }
            //导出excel
            FileStream fs = new FileStream(exportExcelPath, FileMode.Create, FileAccess.ReadWrite);
            workbook.Write(fs);
            fs.Close();
            bw.ReportProgress(100, $"导出完成：{exportExcelPath}");
        }
        int GetStartRow(ISheet sheet)
        {
            int startRow = -1;
            while (true)
            {
                startRow++;
                IRow row = (IRow)sheet.GetRow(startRow);
                var cell = row.GetCell(0);
                if (cell == null || string.IsNullOrEmpty(cell.ToString()))
                    return startRow;

            }
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

        #region Components

        void WriteToConsole(string str)
        {
            if (this.textBox_console != null)
                this.textBox_console.Text +=  str + "\r\n";
            else
                Console.WriteLine(str);
        }
        void ClearConsole()
        {
            if (this.textBox_console != null)
                this.textBox_console.Text = "";
        }
        void DisableAllControls()
        {
            foreach (Control ctl in Controls)
            {
                ctl.Enabled = false;
            }
        }

        void EnableAllControls()
        {
            foreach (Control ctl in Controls)
            {
                ctl.Enabled = true;
            }
        }
        #endregion
    }
}


