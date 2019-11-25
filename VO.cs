using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadWordForms
{
    public class Config
    {
        public string[] senderName;
        public string receiveName;
        public string imageName;
        public string zizhi;
        public string waixie;
        public string[] yinshua;
        public string[] gongyi;
        public string wufangbudai;
        public string year;
        public Dictionary<string,string> type;
    }


    public class OrderData
    {
        public string text { get; set; }
        public string img { get; set; }

        public string orderDate { get; set; }
        public string customer { get; set; }
        public string product { get; set; }
        public string type { get; set; }
        public string size { get; set; }
        public string buliao { get; set; }
        public string yinshua { get; set; }
        public string gongyi { get; set; }
        public float number { get; set; }
        public float danjia { get; set; }
        public float zongjia { get; set; }
        public float dingjin { get; set; }//*
        public float weikuan { get; set; }//*
        public string deliveryDate { get; set; }
        public string waixie { get; set; }
        public float wx_number { get; set; }
        public float wx_danjia { get; set; }
        public float wx_zongjia { get; set; }
        public float banfei { get; set; }//*
        public string buliao2 { get; set; }//*
        public float profit { get; set; }//*
        public float yunfei { get; set; }//*
        public float fr_number { get; set; }//*
        public float fr_danjia { get; set; }//*
        public float fr_zongjia { get; set; }//*
    }
}
