using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Linq;
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
        #region Variables
        private Thread monitoringThread;
        private Thread indiscriminateThread;
        private Thread monitorColorThread;
        private bool isRunning = false;
        private OverlayForm overlayForm;
        private int rectLeft;
        private int rectTop;
        private int rectRight;
        private int rectBottom;
        private int tolerance;
        private int clickDelay;
        private int comboBoxCounter = 1;
        private int comboBoxLocationY = 260;
        private List<ComboBox> comboBoxList = new List<ComboBox>();
        private List<PictureBox> pictureBoxList = new List<PictureBox>();
        private GlobalKeyboardHook keyboardHook;
        #endregion


        public Form1()
        {
            InitializeComponent();
            keyboardHook = new GlobalKeyboardHook();
            keyboardHook.KeyPressed += KeyboardHook_KeyPressed;

        }
        private void ComboBox_selectionIndexChanged(ComboBox comboBox, PictureBox pictureBox)
        {
            changePictureBoxColor(comboBox, pictureBox);
        }

        private void changePictureBoxColor(ComboBox comboBox, PictureBox pictureBox)
        {
            Color selectedColor = (Color)comboBox.SelectedItem;

            pictureBox.BackColor = selectedColor;
        }

        private void fillComboColorBox(ComboBox comboBox)
        {
            comboBox.Items.Clear();
            List<Color> colors = new List<Color>();

            foreach (KnownColor knownColor in Enum.GetValues(typeof(KnownColor)))
            {
                Color color = Color.FromKnownColor(knownColor);
                colors.Add(color);
            }

            // Add colors to the combo box
            foreach (var color in colors)
            {
                comboBox.Items.Add(color);
            }

            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }

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
            clickDelay = textBox5.Text.Length == 0 ? rectData.ClickDelay : Convert.ToInt32(textBox5.Text); //500
            tolerance = textBox6.Text.Length == 0 ? rectData.Tolerance : Convert.ToInt32(textBox6.Text);
            textBox1.Text = rectLeft.ToString();
            textBox2.Text = rectRight.ToString();
            textBox3.Text = rectTop.ToString();
            textBox4.Text = rectBottom.ToString();
            textBox5.Text = clickDelay.ToString();
            textBox6.Text = tolerance.ToString();
            storeInJson(rectLeft, rectRight, rectTop, rectBottom, clickDelay, tolerance);

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
            RectangleData storeData = storeInJson(798, 1081, 700, 768, 500, 140);
            return storeData;
        }

        private RectangleData storeInJson(int rectLeft, int rectRight, int rectTop, int rectBottom, int clickDelay, int tolerance)
        {
            RectangleData data = new RectangleData(rectLeft, rectRight, rectTop, rectBottom, clickDelay, tolerance);

            string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);

            File.WriteAllText("rectangleData.json", jsonString);
            return data;
        }

        private void setEnabledAll(bool value)
        {
            button1.Enabled = value;
            button3.Enabled = value;
            button4.Enabled = value;
            button5.Enabled = value;
            button6.Enabled= value;
            button7.Enabled= value;
            textBox1.Enabled = value;
            textBox2.Enabled = value;
            textBox3.Enabled = value;
            textBox4.Enabled = value;
            textBox5.Enabled = value;
            textBox6.Enabled = value;

            foreach (var comboBox in comboBoxList)
            {
                comboBox.Enabled = value;
            }

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

        private Color GetAveragePixelColor(int centerX, int centerY, int radius)
        {
            // Define the area around the cursor to sample
            int startX = centerX - radius;
            int startY = centerY - radius;
            int width = 3 * radius + 1;
            int height = 3 * radius + 1;

            // Create a bitmap to store sampled pixels
            Bitmap screenPixels = new Bitmap(width, height);

            // Use Graphics to copy pixels from the screen to the bitmap
            using (Graphics g = Graphics.FromImage(screenPixels))
            {
                g.CopyFromScreen(startX, startY, 0, 0, new Size(width, height));
            }

            // Draw the sampling area circle
            //using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) // Graphics object for the entire screen
            //using (Pen pen = new Pen(Color.Red, 1))
            //{
            //    g.DrawEllipse(pen, centerX - radius, centerY - radius, 2 * radius, 2 * radius);
            //}

            // Calculate the total values for each color component
            int totalR = 0, totalG = 0, totalB = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixelColor = screenPixels.GetPixel(x, y);
                    totalR += pixelColor.R;
                    totalG += pixelColor.G;
                    totalB += pixelColor.B;
                }
            }

            // Calculate the average color
            int totalPixels = width * height;
            int avgR = totalR / totalPixels;
            int avgG = totalG / totalPixels;
            int avgB = totalB / totalPixels;

            // Dispose the bitmap
            screenPixels.Dispose();

            // Return the average color
            return Color.FromArgb(avgR, avgG, avgB);
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
            float scalingFactor = WindowScale.getScalingFactor();
            float scale = 1f;

            switch (scalingFactor)
            {
                case 1.25f:
                    scale = 0.8f;
                    break;
                case 1.5f:
                    scale = 0.67f;
                    break;
                case 1.75f:
                    scale = 0.65f;
                    break;
            }
            clickDelay = 1;
            while (isRunning)
            {
                Point cursorPos = Cursor.Position;

                int scaledX = (int)(cursorPos.X / scale);
                int scaledY = (int)(cursorPos.Y / scale);

                Color avgColor = GetAveragePixelColor(scaledX, scaledY, 2);

                List<Color> selectedColors = new List<Color>();

                foreach (var comboBox in comboBoxList)
                {
                    comboBox.Invoke(new Action(() =>
                    {
                        selectedColors.Add((Color)comboBox.SelectedItem);
                    }));
                }

                foreach (var comboBox in comboBoxList)
                {
                    string pictureBoxName = "PictureBox" + comboBox.Name.Substring(comboBox.Name.Length - 1);
                    PictureBox associatedPictureBox = this.Controls.Find(pictureBoxName, true).FirstOrDefault() as PictureBox;

                    if (associatedPictureBox != null)
                    {
                        associatedPictureBox.Invoke(new Action(() =>
                        {
                            label13.Text = avgColor.ToString();
                            label13.ForeColor = avgColor;

                            bool isSimilar = IsColorSimilar(avgColor, selectedColors, tolerance);

                            if (isSimilar)
                            {
                                UpdateLabel("Target Color Detected", Color.Green);

                                Clicknow();
                            }
                            else
                            {
                                UpdateLabel("Target Color Not Detected", Color.Red);
                            }
                        }));
                    }
                }

                Thread.Sleep(1);
            }
        }

        private bool IsColorSimilar(Color targetColor, List<Color> selectedColors, int tolerance)
        {
            foreach (Color selectedColor in selectedColors)
            {
                int deltaR = Math.Abs(targetColor.R - selectedColor.R);
                int deltaG = Math.Abs(targetColor.G - selectedColor.G);
                int deltaB = Math.Abs(targetColor.B - selectedColor.B);


                if (deltaR <= tolerance && deltaG <= tolerance && deltaB <= tolerance)
                {
                    return true;
                }
            }
            return false;
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

        private void button6_Click(object sender, EventArgs e)
        {
            ComboBox newComboBox = new ComboBox();
            PictureBox newPictureBox = new PictureBox();
            newComboBox.FormattingEnabled = true;
            newComboBox.Name = "ColorBox" + comboBoxCounter;
            newComboBox.Location = new Point(613, comboBoxLocationY);
            newPictureBox.Name = "PictureBox" + comboBoxCounter;
            newPictureBox.Size = new Size(31, 21);
            newPictureBox.Location = new Point(576, comboBoxLocationY);

            newComboBox.SelectedIndexChanged += (s, ev) => ComboBox_selectionIndexChanged(newComboBox, newPictureBox);

            fillComboColorBox(newComboBox);
            comboBoxCounter++;
            comboBoxLocationY += 25;

            this.Controls.Add(newComboBox);
            this.Controls.Add(newPictureBox);

            comboBoxList.Add(newComboBox);
            pictureBoxList.Add(newPictureBox);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (comboBoxList.Count > 0 && pictureBoxList.Count > 0)
            {
                ComboBox lastComboBox = comboBoxList[comboBoxList.Count - 1];
                PictureBox lastPictureBox = pictureBoxList[pictureBoxList.Count - 1];

                this.Controls.Remove(lastComboBox);
                this.Controls.Remove(lastPictureBox);

                comboBoxList.RemoveAt(comboBoxList.Count - 1);
                pictureBoxList.RemoveAt(pictureBoxList.Count - 1);

                comboBoxCounter--;
                comboBoxLocationY -= 20;
            }
            else
            {
                MessageBox.Show("No ComboBox or PictureBox to delete.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

    }
}
