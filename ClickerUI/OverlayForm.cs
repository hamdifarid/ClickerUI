using System;
using System.Drawing;
using System.Windows.Forms;

namespace ClickerUI
{
    public partial class OverlayForm : Form
    {
        private int rectLeft;
        private int rectTop;
        private int rectBottom;
        private int rectRight;
        private Color color;
        public OverlayForm(int rectLeft, int rectRight, int rectTop, int rectBottom)
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.Lime;
            this.TransparencyKey = Color.Lime;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.Height = Screen.PrimaryScreen.Bounds.Height;
            this.Paint += new PaintEventHandler(OverlayForm_Paint);
            this.rectLeft = rectLeft;
            this.rectTop = rectTop;
            this.rectBottom = rectBottom;
            this.rectRight = rectRight;

        }

   

        private void OverlayForm_Paint(object sender, PaintEventArgs e)
        {
            
            Rectangle rect = new Rectangle(rectLeft, rectTop, rectRight - rectLeft, rectBottom - rectTop);
            using (Pen pen = new Pen(Color.Red, 3))
            {
                e.Graphics.DrawRectangle(pen, rect);
            }
        }
    }
}
