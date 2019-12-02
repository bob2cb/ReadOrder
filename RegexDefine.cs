using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ReadWordForms
{
    class RegexDefine
    {
        public static Regex containsIntOrfloat = new Regex(@"\d+([.]\d+)?");
        public static Regex isIntOrfloat = new Regex(@"^\d+([.]\d+)?$");
        public static Regex isFloat = new Regex(@"^\d+[.]\d+$");
        public static Regex isCh = new Regex(@"[\u4e00 - \u9fa5]");
        public static Regex containsGe = new Regex(@"\d+(?=\u4e2a)");//*个
        public static Regex containsYuan = new Regex(@"\d+([.]\d+)?(?=\u5143)");//*元
        public static Regex containsPerGeOrTiao = new Regex(@"\d+([.]\d+)?(?=/[\u4e2a\u6761])");//*/个条
        public static Regex containsEqualInLeft = new Regex(@"(?<==)\d+([.]\d+)?");//=*
        public static Regex containsEqualInRight = new Regex(@"(\d+[.])?\d+(?==)");//*=
        public static Regex isDate = new Regex(@"(10|11|12|[0]?\d)[.]\s{0,2}[0-3]?\d");//12.30,1.05,1.5,01.5

        public static Regex containsWidth = new Regex(@"[\u5bbd]\d+");//宽*
        public static Regex containsHeight = new Regex(@"[\u9ad8]\d+");//高*
        public static Regex containsSide = new Regex(@"[\u4fa7]\d+");//侧*
        public static Regex containsWufangbu = new Regex(@"\d+[\u514b].*\u65e0\u7eba\u5e03");//*克***
        public static Regex containsBanfei = new Regex(@"(?<=\u7248\u8d39\s*)\d+");//*版费***
        
    }
}
