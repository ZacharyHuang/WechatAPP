using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WerewolfBackend.Utils
{
    public class RandomUtil
    {
        public static int[] GenRandomArray(int size)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            int[] res = new int[size];
            for (int i = 0; i < size; ++i)
            {
                res[i] = i;
            }
            for (int i = 0; i < size - 2; ++i)
            {
                int j = size - 1 - i;
                int r = rand.Next(j + 1);
                if (r != j)
                {
                    int tmp = res[r];
                    res[r] = res[j];
                    res[j] = tmp;
                }
            }
            return res;
        }

        public static int GenRandomInt(int maxValue)
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            return rand.Next(maxValue);
        }
    }
}