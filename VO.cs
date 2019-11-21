using System;
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
    }


    public interface IData { }

    public class TextData : IData
    {
        public string text;
        public string date;
        public string customer;
        public string product;
        public int number;
        public int danjia;
        public int zongjia;
        public string type;
        public string guige;
        public string buliao;
        public string yinshua;
        public string gongyi;


    }

    public class ImageData : IData
    {
        public string text;
    }
}
