using System;
using System.Collections.Generic;
using System.Text;

namespace AcfunTools.CommentXuMing.Crawler
{
    public static class Util
    {
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <param name="dateEnd"></param>
        /// <returns></returns>
        public static int ToTimeStamp(this DateTime dateEnd, bool isMillisecond = true)
        {
            DateTime dateBegin = new DateTime(1970, 1, 1, 8, 0, 0);
            int timeStamp = Convert.ToInt32(isMillisecond ?
                (dateEnd - dateBegin).TotalSeconds :
                (dateEnd - dateBegin).TotalMilliseconds);
            return timeStamp;
        }

        /// <summary>
        /// 根据时间戳转换
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long timestamp, bool isMillisecond = true)
        {
            DateTime dtStart = new DateTime(1970, 1, 1).ToLocalTime();
            long lTime = timestamp * (isMillisecond ? 10000 : 10000_000);
            TimeSpan toNow = new TimeSpan(lTime);
            DateTime targetDt = dtStart.Add(toNow);
            return targetDt;
        }
    }
}
