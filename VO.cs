﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadWordForms
{
    public class Config
    {
        public string senderName;
        public string receiveName;
        public string imageName;
        public string zizhi;
        public string waixie;
        public string[] yinshua;
        public string[] gongyi;
        public Dictionary<string,string> type;
    }


    public class OrderData
    {
        public string text;
        public string img;
        public string orderDate;
        public string customer;
        public string product;
        public int number;
        public float danjia;
        public float zongjia;
        public int wx_number;
        public float wx_danjia;
        public float wx_zongjia;
        public string waixie;

        public string deliveryDate;
        public string size;
        public string type;
        public string buliao;
        public string yinshua;
        public string gongyi;
    }
}
