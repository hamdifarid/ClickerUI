using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickerUI
{
    internal class RectangleData
    {
        public int RectLeft { get; set; }
        public int RectRight { get; set; }
        public int RectTop { get; set; }
        public int RectBottom { get; set; }
        public int ClickDelay { get; set; }
        public int Tolerance { get; set; }  

        // Constructor
        public RectangleData(int rectLeft, int rectRight, int rectTop, int rectBottom, int clickDelay, int tolerance)
        {
            RectLeft = rectLeft;
            RectRight = rectRight;
            RectTop = rectTop;
            RectBottom = rectBottom;
            ClickDelay = clickDelay;
            Tolerance = tolerance;
        }
    }
}
