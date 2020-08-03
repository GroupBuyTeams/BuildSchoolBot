using System;
using System.Collections.Generic;

namespace BuildSchoolBot.Service{
    
    public class WeekdaysEnum{
        [Flags]
        private enum weekdays{
            SUN = 1, MON = 2, TUE = 4, WED = 8, THU = 16, FRI = 32, SAT = 64
        }

        public string GetWeekDays(int num){
            string res = string.Empty;
            var list = Enum.GetNames(typeof(weekdays));
            int i = 0;
            while(num != 0){
                if((num & 1) == 1){
                    res += $"{list[i]},";
                }
                i++;
                num >>= 1;
            }
            return res.TrimEnd(',');
        }
    }
}