using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SurfLogApp
{ 
    public partial class Form1 : Form
    {
        bool showSettings = false;
        bool active = false;

        public Form1()
        {
            InitializeComponent();
            
        }
        

        private async void button1_Click(object sender, EventArgs e)
        {
            int a = 0;
            int d = 0;
            int total = 0;
            bool aHolding = false;
            bool dHolding = true;
            double ticks = 0;
            double activeticks = 0;
            double unsyncedTicks = 0;
            bool synccalc = false;
            int a2 = 0;
            int d2 = 0;
            int total2 = 0;
            double activeticks2 = 0;
            double unsyncedticks2 = 0;
            int left = 0;
            int right = 0;
            int left2 = 0;
            int right2 = 0;
            int AOtotal = 0;
            int AOtotal2 = 0;
            bool leftholding = false;
            bool rightholding = true;
            bool deadzone = false;
            Color vel100 = Color.FromArgb(0, 0, 0);
            Color vel10 = Color.FromArgb(0, 0, 0);
            active = !active;
            if (active)
            {
                button1.BackColor = Color.FromArgb(210, 95, 95);
                button1.Text = "Stop";
                if(checkBox1.Checked)
                {
                    button1.Visible = false;
                }
            }
            else
            {
                button1.BackColor = Color.FromArgb(90, 200, 95);
                button1.Text = "Run";
            }
            if (StyleBx.Text == "Autohop")
            {
                AutohopPanel.Visible = true;
                AOnlyPanel.Visible = false;
                while (active == true)
                {//Keypress stats
                    ticks++;
                    //ticksdebugtxt.Text = Convert.ToString(ticks);
                    Bitmap px = getStrafePixels();
                    Color TL = px.GetPixel(0, 0); //Top left of the show keys UI ('A' key pressed)
                    Color TR = px.GetPixel(1, 0);//Top right of the show keys UI ('D' key pressed)
                    Color BL = px.GetPixel(0, 1);//Bottom left of the show keys UI (Mouse moving left)
                    Color BR = px.GetPixel(1, 1);//Bottom right of the show keys UI (Mouse moving right)
                    Color grey = Color.FromArgb(255, 45, 45, 45);
                    Color green = Color.FromArgb(255, 90, 200, 95);
                    Color red = Color.FromArgb(210, 95, 95);
                    if (TL == green || TL == red)
                    {
                        if (aHolding == false)
                        {
                            a++;
                            total++;
                            aPressesTxt.Text = Convert.ToString(a);
                            totalPressesTxt.Text = Convert.ToString(total);
                            aHolding = true;
                            dHolding = false;
                            aOverlay.BackColor = Color.FromArgb(90, 90, 90);
                            dOverlay.BackColor = Color.FromArgb(45, 45, 45);
                        }
                    }
                    else if (TR == green || TR == red)
                    {
                        if (dHolding == false)
                        {
                            d++;
                            total++;
                            dPressesTxt.Text = Convert.ToString(d);
                            totalPressesTxt.Text = Convert.ToString(total);
                            dHolding = true;
                            aHolding = false;
                            dOverlay.BackColor = Color.FromArgb(90, 90, 90);
                            aOverlay.BackColor = Color.FromArgb(45, 45, 45);
                        }
                    }
                    else
                    {
                        aHolding = false;
                        dHolding = false;
                        aOverlay.BackColor = Color.FromArgb(45, 45, 45);
                        dOverlay.BackColor = Color.FromArgb(45, 45, 45);
                    }//Detect when timer resets
                    if (ticks % 15 == 0)
                    {
                        Bitmap timerpx = getTimerPixels();
                        Color min = timerpx.GetPixel(0, 0); //This pixel is a different colour for each number 0-9, so can be used to get the number.
                        Color tensec = timerpx.GetPixel(17, 0);
                        Color sec = timerpx.GetPixel(28, 0);
                        Color tenth = timerpx.GetPixel(45, 0);
                        //Color hundredth = timerpx.GetPixel(67, 0);
                        //Color thousandth = timerpx.GetPixel(78, 0);
                        timerpx.Dispose();
                        Color c = Color.FromArgb(152, 152, 152); //Colour of the number 0
                        if (min == c && tensec == c && sec == c && tenth == c) //If timer is 0
                        {
                            a2 += a;
                            d2 += d;
                            total2 += total;
                            activeticks2 += activeticks;
                            unsyncedticks2 += unsyncedTicks;
                            aOverallTxt.Text = Convert.ToString(a2);
                            dOverallTxt.Text = Convert.ToString(d2);
                            strafesOverallTxt.Text = Convert.ToString(total2);
                            syncOverallTxt.Text = Convert.ToString(Math.Round(((1 - unsyncedticks2 / activeticks2) * 100), 2)) + "%";
                            a = 0;
                            d = 0;
                            total = 0;
                            activeticks = 0;
                            unsyncedTicks = 0;
                            ticks = 0;
                            aPressesTxt.Text = Convert.ToString(a);
                            dPressesTxt.Text = Convert.ToString(d);
                            totalPressesTxt.Text = Convert.ToString(total);
                            syncTxt.Text = "NaN %";
                            synccalc = false; //sync doesn't need to be calculated if the timer is stopped
                        }
                        else
                        {
                            synccalc = true;
                        }
                    }
                    if (synccalc)
                    {//Sync calculation
                        if (TL == green || TR == green || BL == green || BR == green) //If A/D is being pressed or the mouse is being moved
                        {
                            activeticks++;
                        }
                        if (((TL != grey) && BL == grey) || ((BL != grey) && TL == grey)) //If the 'A' key is being pressed but the mouse isn't moving or vice versa
                        {
                            unsyncedTicks++;
                        }
                        else if (((TR != grey) && BR == grey) || ((BR != grey) && TR == grey)) //If the 'D' key is being pressed but the mouse isn't moving or vice versa
                        {
                            unsyncedTicks++;
                        }
                        double sync = Math.Round((1 - (unsyncedTicks / activeticks)) * 100, 2);
                        syncTxt.Text = Convert.ToString(sync) + " %";
                    }
                    await Task.Delay(1);
                }
            }
            else if (StyleBx.Text == "A-Only")
            {
                AOnlyPanel.Visible = true;
                AutohopPanel.Visible = false;
                while (active == true)
                {//Keypress stats
                    ticks++;
                    //ticksdebugtxt.Text = Convert.ToString(ticks);
                    Bitmap px = getStrafePixels();
                    Color TL = px.GetPixel(0, 0);
                    Color TR = px.GetPixel(1, 0);
                    Color BL = px.GetPixel(0, 1);
                    Color BR = px.GetPixel(1, 1);
                    Color grey = Color.FromArgb(255, 45, 45, 45);
                    Color green = Color.FromArgb(255, 90, 200, 95);
                    Color red = Color.FromArgb(210, 95, 95);
                    if (BL == green || BL == red)
                    {
                        if (leftholding == false)
                        {
                            Bitmap Vpixel = getAOVelPixels();
                            vel100 = Vpixel.GetPixel(5, 0);
                            vel10 = Vpixel.GetPixel(0, 0);
                            Vpixel.Dispose();
                            leftholding = true;
                            rightholding = false;
                            leftHighlight.BackColor = Color.FromArgb(90, 45, 45);
                            rightHighlight.BackColor = Color.FromArgb(45, 45, 45);
                            deadzone = true;
                        }
                        else if (leftholding == true && deadzone == true)
                        {
                            Bitmap currentVpixel = getAOVelPixels();
                            Color currentvel100 = currentVpixel.GetPixel(5, 0);
                            Color currentvel10 = currentVpixel.GetPixel(0, 0);
                            if(currentvel100 == vel100 && currentvel10 == vel10)
                            { }
                            else
                            {
                                deadzone = false;
                                leftHighlight.BackColor = Color.FromArgb(90, 90, 90);
                                left++;
                                AOtotal++;
                                AOleftturns.Text = Convert.ToString(left);
                                AOstrafesthisrun.Text = Convert.ToString(AOtotal);
                            }
                        }
                    }
                    else if (BR == green || BR == red)
                    {
                        if (rightholding == false)
                        {
                            Bitmap Vpixel = getAOVelPixels();
                            vel100 = Vpixel.GetPixel(5, 0);
                            vel10 = Vpixel.GetPixel(0, 0);
                            Vpixel.Dispose();
                            rightholding = true;
                            leftholding = false;
                            leftHighlight.BackColor = Color.FromArgb(45, 45, 45);
                            rightHighlight.BackColor = Color.FromArgb(90, 45, 45);
                            deadzone = true;
                        }
                        else if (rightholding == true && deadzone == true)
                        {
                            Bitmap currentVpixel = getAOVelPixels();
                            Color currentvel100 = currentVpixel.GetPixel(5, 0);
                            Color currentvel10 = currentVpixel.GetPixel(0, 0);
                            if (currentvel100 == vel100 && currentvel10 == vel10)
                            {
                            }
                            else
                            {
                                deadzone = false;
                                rightHighlight.BackColor = Color.FromArgb(90, 90, 90);
                                right++;
                                AOtotal++;
                                AOrightturns.Text = Convert.ToString(right);
                                AOstrafesthisrun.Text = Convert.ToString(AOtotal);
                            }
                        }
                    }
                    else
                    {
                        leftholding = false;
                        rightholding = false;
                        deadzone = false;
                        leftHighlight.BackColor = Color.FromArgb(45, 45, 45);
                        rightHighlight.BackColor = Color.FromArgb(45, 45, 45);
                    }//Detect when timer resets
                    if (ticks % 15 == 0)
                    {
                        Bitmap timerpx = getTimerPixels();
                        Color min = timerpx.GetPixel(0, 0);
                        Color tensec = timerpx.GetPixel(17, 0);
                        Color sec = timerpx.GetPixel(28, 0);
                        Color tenth = timerpx.GetPixel(45, 0);
                        //Color hundredth = timerpx.GetPixel(67, 0);
                        //Color thousandth = timerpx.GetPixel(78, 0);
                        timerpx.Dispose();
                        Color c = Color.FromArgb(152, 152, 152);
                        if (min == c && tensec == c && sec == c && tenth == c)
                        {
                            left2 += left;
                            right2 += right;
                            AOtotal2 += AOtotal;
                            activeticks2 += activeticks;
                            unsyncedticks2 += unsyncedTicks;
                            AOtotalleftturns.Text = Convert.ToString(left2);
                            AOtotalrightturns.Text = Convert.ToString(right2);
                            AOstrafesoverall.Text = Convert.ToString(AOtotal2);
                            //syncOverallTxt.Text = Convert.ToString(Math.Round(((1 - unsyncedticks2 / activeticks2) * 100), 2)) + "%";
                            left = 0;
                            right = 0;
                            AOtotal = 0;
                            activeticks = 0;
                            unsyncedTicks = 0;
                            ticks = 0;
                            AOleftturns.Text = Convert.ToString(left);
                            AOrightturns.Text = Convert.ToString(right);
                            AOstrafesthisrun.Text = Convert.ToString(AOtotal);
                            AOSyncOverall.Text = "NaN %";
                            AOSync.Text = "NaN %";
                            synccalc = false;
                        }
                        else
                        {
                            synccalc = true;
                        }
                    }
                    synccalc = false;
                    if (synccalc)
                    {//Sync calculation
                        if (!deadzone)
                        {
                            activeticks++;
                        }
                        else
                        {
                            unsyncedTicks++;
                        }
                        double sync = Math.Round((1 - (unsyncedTicks / activeticks)) * 100, 2);
                        AOSync.Text = Convert.ToString(sync) + " %";
                    }
                    await Task.Delay(1);
                }
            }
            
        }
        private Bitmap getStrafePixels()
        {
            Graphics gfxScreenshot;
            Size selectionSize = new Size(2, 2);
            Bitmap bmpScreenshot = new Bitmap(2, 2, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(1717, 961, 0, 0, selectionSize, CopyPixelOperation.SourceCopy);
            return bmpScreenshot;
        }
        private Bitmap getTimerPixels()
        {
            Bitmap bmpScreenshot;
            Graphics gfxScreenshot;
            Size selectionSize = new Size(46, 1);
            bmpScreenshot = new Bitmap(46, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(84, 1011, 0, 0, selectionSize, CopyPixelOperation.SourceCopy);
            return bmpScreenshot;
        }
        private Bitmap getAOVelPixels()
        {
            Bitmap bmpScreenshot;
            Graphics gfxScreenshot;
            Size selectionSize = new Size(6, 1);
            bmpScreenshot = new Bitmap(6, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(115, 1048, 0, 0, selectionSize, CopyPixelOperation.SourceCopy);
            return bmpScreenshot;
        }
        private Bitmap getAOVelPixels2()
        {
            Bitmap bmpScreenshot;
            Graphics gfxScreenshot;
            Size selectionSize = new Size(1, 1);
            bmpScreenshot = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(115, 1048, 0, 0, selectionSize, CopyPixelOperation.SourceCopy);
            return bmpScreenshot;
        }
        private Bitmap getVelPixel()
        {
            Bitmap bmpScreenshot;
            Graphics gfxScreenshot;
            Size selectionSize = new Size(1, 1);
            bmpScreenshot = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            gfxScreenshot = Graphics.FromImage(bmpScreenshot);
            gfxScreenshot.CopyFromScreen(15, 1045, 0, 0, selectionSize, CopyPixelOperation.SourceCopy);
            return bmpScreenshot;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                Form1.ActiveForm.BackColor = Color.FromArgb(56, 56, 56);
                Form1.ActiveForm.FormBorderStyle = FormBorderStyle.None;
                if(button1.Text == "Stop")
                {
                    button1.Visible = false;
                }
            }
            else
            {
                Form1.ActiveForm.BackColor = Color.FromArgb(60, 60, 60);
                Form1.ActiveForm.FormBorderStyle = FormBorderStyle.Sizable;
                button1.Visible = true;
            }
        }


        private void settingsBtn_Click(object sender, EventArgs e)
        {
            showSettings = !showSettings;
            settingsPanel.Visible = showSettings;
            settingsPanel2.Visible = showSettings;
        }

        private void StyleBx_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (active)
            {
            button1_Click(sender, e);
            }
            else if(StyleBx.Text == "Autohop")
            {
                AutohopPanel.Visible = true;
                AOnlyPanel.Visible = false;
            }
            else if (StyleBx.Text == "A-Only")
            {
                AutohopPanel.Visible = false;
                AOnlyPanel.Visible = true;
            }
        }

    }
}
