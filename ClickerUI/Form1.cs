using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using static System.Net.Mime.MediaTypeNames;
using Formatting = Newtonsoft.Json.Formatting;

namespace ClickerUI
{
    public partial class Form1 : Form
    {
        private Thread monitoringThread;
        private Thread indiscriminateThread;
        private Thread monitorColorThread;
        private bool isRunning = false;
        private OverlayForm overlayForm;
        private int rectLeft;
        private int rectTop;
        private int rectRight;
        private int rectBottom;
        private int clickDelay;

        private GlobalKeyboardHook keyboardHook;

        public Form1()
        {
            InitializeComponent();
            keyboardHook = new GlobalKeyboardHook();
            keyboardHook.KeyPressed += KeyboardHook_KeyPressed;
            fillComboColorBox();
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            pictureBox1.BackColor = Color.White;



        }
        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Color selectedColor = (Color)comboBox1.SelectedItem;

            pictureBox1.BackColor = selectedColor;
        }

        private void fillComboColorBox()
        {
            comboBox1.Items.Clear();
            List<Color> colors = new List<Color>();

            foreach (KnownColor knownColor in Enum.GetValues(typeof(KnownColor)))
            {
                Color color = Color.FromKnownColor(knownColor);
                colors.Add(color);
            }

            // Add colors to the combo box
            foreach (var color in colors)
            {
                comboBox1.Items.Add(color);
            }

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }





        // Start button
        private void button1_Click(object sender, EventArgs e)
        {
            startMethod();
        }
        private void KeyboardHook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            // Check for the desired hotkey combination
            if (e.KeyPressed == Keys.G && Control.ModifierKeys == Keys.Shift)
            {
                if (!isRunning)
                {
                    startMethod();
                }
                else
                {
                    return;
                }
            }

            if (e.KeyPressed == Keys.Y && Control.ModifierKeys == Keys.Shift)
            {
                if (isRunning)
                {
                    stopMethod();
                }
                else
                {
                    return;
                }
            }

            if (e.KeyPressed == Keys.D && Control.ModifierKeys == Keys.Shift)
            {
                if (!isRunning)
                {
                    startColorClicker();
                }
                else
                {
                    return;
                }
            }

            if (e.KeyPressed == Keys.H && Control.ModifierKeys == Keys.Shift)
            {
                if (!isRunning)
                {
                    startIndiscriminateClicker();
                }
                else
                {
                    return;
                }
            }
        }
        private void stopMethod()
        {
            label1.Text = "Stopped";
            isRunning = false;
            setEnabledAll(true);
            if (monitoringThread != null && monitoringThread.IsAlive)
            {
                monitoringThread.Abort();
            }
            if (indiscriminateThread != null && indiscriminateThread.IsAlive)
            {
                indiscriminateThread.Abort();
            }
            if (monitorColorThread != null && monitorColorThread.IsAlive)
            {
                monitorColorThread.Abort();
            }
            if (overlayForm != null)
            {
                overlayForm.Close();
            }
        }

        private void startMethod()
        {
            label1.Text = "Started";
            setEnabledAll(false);
            setRect();
            bool isValidated = validate();
            if (!isRunning && isValidated)
            {
                this.WindowState = FormWindowState.Minimized;
                isRunning = true;
                overlayForm = new OverlayForm(rectLeft, rectRight, rectTop, rectBottom);
                overlayForm.Show();
                monitoringThread = new Thread(MonitorCursor);
                monitoringThread.Start();
            }
            else if (!isValidated)
            {
                label1.Text = "Please take care of the rules \n\n X1 > X2 and Y1 > Y2 \n\n or click speed should be positive\n or Click Zone is already " + isRunning.ToString();
                stopMethod();
            }
        }
        private void setRect()
        {
            RectangleData rectData = readJson();
            rectLeft = textBox1.Text.Length == 0 ? rectData.RectLeft : Convert.ToInt32(textBox1.Text); // 798
            rectRight = textBox2.Text.Length == 0 ? rectData.RectRight : Convert.ToInt32(textBox2.Text); //1081
            rectTop = textBox3.Text.Length == 0 ? rectData.RectTop : Convert.ToInt32(textBox3.Text); // 700
            rectBottom = textBox4.Text.Length == 0 ? rectData.RectBottom : Convert.ToInt32(textBox4.Text); // 768
            textBox1.Text = rectLeft.ToString();
            textBox2.Text = rectRight.ToString();
            textBox3.Text = rectTop.ToString();
            textBox4.Text = rectBottom.ToString();
            clickDelay = textBox5.Text.Length == 0 ? rectData.ClickDelay : Convert.ToInt32(textBox5.Text); //500
            textBox5.Text = clickDelay.ToString();
            storeInJson(rectLeft, rectRight, rectTop, rectBottom, clickDelay);

        }

        private RectangleData readJson()
        {
            bool fileExists = File.Exists("rectangleData.json");
            if (fileExists)
            {
                string jsonString = File.ReadAllText("rectangleData.json");
                RectangleData data = JsonConvert.DeserializeObject<RectangleData>(jsonString);
                return data;
            }
            RectangleData storeData = storeInJson(798, 1081, 700, 768, 500);
            return storeData;
        }


        private RectangleData storeInJson(int rectLeft, int rectRight, int rectTop, int rectBottom, int clickDelay)
        {
            RectangleData data = new RectangleData(rectLeft, rectRight, rectTop, rectBottom, clickDelay);

            string jsonString = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText("rectangleData.json", jsonString);
            return data;
        }

        private void setEnabledAll(bool value)
        {
            button1.Enabled = value;
            button3.Enabled = value;
            button4.Enabled = value;
            button5.Enabled = value;
            textBox1.Enabled = value;
            textBox2.Enabled = value;
            textBox3.Enabled = value;
            textBox4.Enabled = value;
            textBox5.Enabled = value;
            comboBox1.Enabled = value;

        }



        private bool validate()
        {
            if (rectRight < rectLeft || rectTop < rectBottom && clickDelay > 0)
            {
                UpdateLabel("Please set Click delay greater than 0", color: Color.Red);
                return true;
            }
            if (clickDelay <= 0)
            {
                UpdateLabel("Please set Click delay greater than 0", color: Color.Red);
                return false;
            }
            return false;
        }

        // Stop button
        private void button2_Click(object sender, EventArgs e)
        {
            stopMethod();
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll")]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        const int MOUSEEVENTF_LEFTUP = 0x0004;

        private void MonitorCursor()
        {
            while (isRunning)
            {
                Point defPnt = new Point();
                GetCursorPos(ref defPnt);

                //// Define the rectangle coordinates and dimensions
                //int rectLeft = 798, rectRight = 1081, rectTop = 700, rectBottom = 768;
                int nearThreshold = 50; // Distance in pixels considered as 'near'

                if (defPnt.X > rectLeft && defPnt.X < rectRight && defPnt.Y > rectTop && defPnt.Y < rectBottom)
                {
                    UpdateLabel("Inside Click Zone", Color.Green);
                    Clicknow();
                }
                else if (IsNear(defPnt, rectLeft, rectRight, rectTop, rectBottom, nearThreshold))
                {
                    UpdateLabel("Near Click Zone", Color.OrangeRed);
                }
                else
                {
                    UpdateLabel("Far from Click Zone", Color.Red);
                }
                Thread.Sleep(clickDelay);
            }
        }

        private Color GetPixelColor(int x, int y)
        {
            Bitmap screenPixel = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(screenPixel);
            g.CopyFromScreen(x, y, 0, 0, new Size(1, 1));
            Color pixelColor = screenPixel.GetPixel(0, 0);
            screenPixel.Dispose();
            return pixelColor;
        }

        private bool IsNear(Point p, int left, int right, int top, int bottom, int threshold)
        {
            return (p.X > left - threshold && p.X < right + threshold && p.Y > top - threshold && p.Y < bottom + threshold);
        }

        private void UpdateLabel(string text, Color color)
        {
            if (label1.InvokeRequired)
            {
                label1.Invoke(new Action(() =>
                {
                    label1.Text = text;
                    label1.ForeColor = color;
                }));
            }
            else
            {
                label1.Text = text;
                label1.ForeColor = color;
            }
        }




        private void Clicknow()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RectangleData rectData = readJson();
            textBox1.Text = rectData.RectLeft.ToString();
            textBox2.Text = rectData.RectRight.ToString();
            textBox3.Text = rectData.RectTop.ToString();
            textBox4.Text = rectData.RectBottom.ToString();
            textBox5.Text = rectData.ClickDelay.ToString();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            startIndiscriminateClicker();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            startColorClicker();
        }
        private void startColorClicker()
        {
            setEnabledAll(false);
            //colorClicker();
            setRect();

            bool isValidated = validate();
            if (isValidated)
            {
                isRunning = true;
                label1.Text = "Started";
                monitorColorThread = new Thread(colorClicker);
                monitorColorThread.Start();
            }
            else
            {
                stopMethod();
            }

        }
            
        private void colorClicker()
        {
            Thread.Sleep(3000);
            while (isRunning)
            {
                Point cursorPos = Cursor.Position;
                Color pixelColor = GetPixelColor(cursorPos.X, cursorPos.Y);

                comboBox1.Invoke(new Action(() =>
                {
                    Color targetColor = (Color)comboBox1.SelectedItem;
                    label13.Invoke(new Action(() =>
                    {
                        label13.Text = pixelColor.ToString();
                        label13.ForeColor = pixelColor;

                        if (IsColorSimilar(pixelColor, targetColor, tolerance: 100)) // Adjust tolerance as needed
                        {
                            UpdateLabel("Target Color Detected", Color.Green);
                            Clicknow();
                        }
                        else
                        {
                            UpdateLabel("Target Color Not Detected", Color.Red);
                        }
                    }));
                }));

                Thread.Sleep(clickDelay);
            }
        }

        private bool IsColorSimilar(Color color1, Color color2, int tolerance)
        {
            // Calculate differences for each color component
            int deltaR = Math.Abs(color1.R - color2.R);
            int deltaG = Math.Abs(color1.G - color2.G);
            int deltaB = Math.Abs(color1.B - color2.B);

            // Check if the differences fall within the tolerance for each color component
            return deltaR <= tolerance && deltaG <= tolerance && deltaB <= tolerance;
        }


        private void startIndiscriminateClicker()
        {
            setEnabledAll(false);
            setRect();
            bool isValidated = validate();
            if (isValidated)
            {
                this.WindowState = FormWindowState.Minimized;
                isRunning = true;
                label1.Text = "Started";

                indiscriminateThread = new Thread(indiscriminateClicker);
                indiscriminateThread.Start();
            }
            else
            {
                stopMethod();
            }

        }

        private void indiscriminateClicker()
        {
            Thread.Sleep(1000);
            while (isRunning)
            {
                Clicknow();
                Thread.Sleep(clickDelay);
            }
        }

    }
}
