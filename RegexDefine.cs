using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ReadWordForms
{
    class RegexDefine
    {
        public static Regex containsIntOrfloat = new Regex(@"\d+([.]{1}\d+){0,1}");
        public static Regex isIntOrfloat = new Regex(@"^\d+([.]{1}\d+){0,1}$");
        public static Regex isFloat = new Regex(@"\d+[.]\d+");
        public static Regex isCh = new Regex(@"[\u4e00 - \u9fa5]");
        public static Regex containsGe = new Regex(@"\d+[\u4e2a]");//*个
        public static Regex containsYuan = new Regex(@"\d+[\u5143]");//*元
        public static Regex containsPerge = new Regex(@"\d+/[\u4e2a]");//*/个
        public static Regex containsMulit = new Regex(@"[*]");//*
        public static Regex containsEqualInLeft = new Regex(@"=\d+[.]?\d+?");//=*
        public static Regex containsEqualInRight = new Regex(@"\d+?[.]?\d+=");//*=
        public static Regex isDate = new Regex(@"^((10|11|12|[0]?\d)[.][0-3]?\d)$");//12.30,1.05,1.5,01.5
    }
}
