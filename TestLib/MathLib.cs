using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLib
{
    public class MathLib
    {
        //V1 Code
        //public int Div(int i, int j)
        //{
        //    return i / j;
        //}

        //V2 Code
        public int Div(int i, int j)
        {
            if (j == 0) throw new Exception("The second Argument can't be 0.");
            return i / j;
        }

        public int Add(int i, int j)
        {
            return i + j;
        }

    }
}
