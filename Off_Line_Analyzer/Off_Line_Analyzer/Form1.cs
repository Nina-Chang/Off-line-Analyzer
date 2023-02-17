using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Off_Line_Analyzer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            rawI = new double[m_a.Length];
            rawII = new double[m_a.Length];
            rawV1 = new double[m_a.Length];
        }
        StringBuilder res;
        int iStart, iEnd;
        List<byte> raw;
        List<int> Lecg, Lhr, Ltemp;
        Bitmap bmpI, bmpII, bmpV;
        int hr, temp;
        float s0, scaleH;
        Graphics gI, gII, gV;
        int ww, hh = 220, iS, iE, x1, y1, dH;

        private void btnClear_Click(object sender, EventArgs e)
        {
            gI.Clear(Color.White);
            gII.Clear(Color.White);
            gV.Clear(Color.White);
            label1.Text = "Heart Rate(From ECG):";
            label2.Text = "Temp:";
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            string[] strs = File.ReadAllLines(openFileDialog1.FileName);
            byte bt;
            foreach (string str in strs)
            {
                bt = byte.Parse(str);
                raw.Add(bt);
            }
            decode();
            displayG();
        }

        Pen pR, pB, myPen;
        SolidBrush sbW;
        int it, ihr, ecg;

        double[] m_a =
        {
            0.0024593392459372134, 0.0012694801871167231, -0.0029366492314269123,
           -0.0058732686125919061, -0.0023466957666592328, 0.0064945535372463231,
            0.011336261223662733, 0.0035568045681083839, -0.012655166954118943,
           -0.019960391743576569, -0.0047484627525475782, 0.023622153725863196,
            0.034690356202778876, 0.0057558397338354468, -0.046622431243507879,
           -0.067579936172768709, -0.0064297184128108377, 0.13227920610747901,
            0.27784275491046106, 0.34000000000000002, 0.27784275491046106,
            0.13227920610747901, -0.0064297184128108377, -0.067579936172768709,
           -0.046622431243507879, 0.0057558397338354468, 0.034690356202778876,
            0.023622153725863196, -0.0047484627525475782, -0.019960391743576569,
           -0.012655166954118943, 0.0035568045681083839, 0.011336261223662733,
            0.0064945535372463231,-0.0023466957666592328, -0.0058732686125919061,
           -0.0029366492314269123, 0.0012694801871167231, 0.0024593392459372134
        };
    double[] rawI, rawII, rawV1;
    int[] dtECG = new int[5];
    Color[] clr = new Color[] { Color.Crimson, Color.PaleVioletRed, Color.LightSlateGray };
        private void Form1_Load(object sender, EventArgs e)
        {
            btnLoad.BackColor = Color.FromArgb(139 , 157, 172);
            btnClear.BackColor = Color.FromArgb(204, 204, 204);
            raw = new List<byte>();
            res = new StringBuilder();
            dH = 25;
            pR = new Pen(Color.Red, 2);
            pB = new Pen(Color.Blue, 2);
            sbW = new SolidBrush(Color.White);
        }

        private double FIRfiltering(double[] raw)
        {
            int i;
            double filtered = 0.0;
            for (i = 0; i < m_a.Length; i++)
                filtered += m_a[i] * raw[i];
            return filtered;
        }

        private void decode()
        {
            iStart = 0;
            iEnd = raw.Count - 14;
            Lecg = new List<int>();
            Lhr = new List<int>();
            Ltemp = new List<int>();
            int iit = 0, ecgCNT = -1, iihr = 0;
            while (iStart < iEnd)
            {
                if(raw[iStart]==0xFF && raw[iStart + 1] == 0xE2)
                {
                    ecgCNT++;
                    ecg = ((raw[iStart + 2] & 0x7f) << 7) | (raw[iStart + 3] & 0x7f);
                    if (ecgCNT < m_a.Length - 1)
                        rawI[m_a.Length - 1 - ecgCNT] = ecg;
                    else
                    {
                        for (int ii = 0; ii < m_a.Length - 1; ii++)
                            rawI[m_a.Length - 1 - ii] = rawI[m_a.Length - 2 - ii];
                        rawI[0] = ecg;
                        ecg = (int)FIRfiltering(rawI);
                    }
                    Lecg.Add(ecg);
                    dtECG[0] = ecg;
                    //
                    ecg = ((raw[iStart + 4] & 0x7f) << 7) | (raw[iStart + 5] & 0x7f);
                    if (ecgCNT < m_a.Length - 1)
                        rawII[m_a.Length - 1 - ecgCNT] = ecg;
                    else
                    {
                        for (int ii = 0; ii < m_a.Length - 1; ii++)
                            rawII[m_a.Length - 1 - ii] = rawII[m_a.Length - 2 - ii];
                        rawII[0] = ecg;
                        ecg = (int)FIRfiltering(rawII);
                    }
                    
                    dtECG[1] = ecg;
                    Lecg.Add(ecg);
                    //

                    ecg = ((raw[iStart + 6] & 0x7f) << 7) | (raw[iStart + 7] & 0x7f);
                    if (ecgCNT < m_a.Length - 1)
                        rawV1[m_a.Length - 1 - ecgCNT] = ecg;
                    else
                    {
                        for (int ii = 0; ii < m_a.Length - 1; ii++)
                            rawV1[m_a.Length - 1 - ii] =rawV1[m_a.Length - 2 - ii];
                        rawV1[0] = ecg;
                        ecg = (int)FIRfiltering(rawV1);
                    }
      
                    dtECG[2] = ecg;
                    Lecg.Add(ecg);
                    //
                    hr = ((raw[iStart + 13] & 0x03 << 7) | (raw[iStart + 10] & 0x7F));
                    dtECG[3] = hr;
                    if (hr > 0)
                    {
                        iihr++;
                        if(iihr% 256 == 0)
                        {
                            ihr++;
                            ihr %= 3;
                            //label1.ForeColor = clr[ihr];
                        }
                        label1.Text = string.Format("Heart Rate (From ECG): {0} bpm", hr);
                    }
                    Lhr.Add(hr);
                    temp= ((raw[iStart + 13] & 0x0C << 5) | (raw[iStart + 11] & 0x7F));
                    if(temp>10)
                    {
                        label2.Text = string.Format("Temp1:{0:f1} °C", temp / 10.0);
                        Ltemp.Add(temp);
                        iit++;
                        if(iit % 60 == 0)
                        {
                            it++;
                            it %= 3;
                            label2.ForeColor = clr[it];

                        }
                    }
                    temp= ((raw[iStart + 13] & 0x30 << 3) | (raw[iStart + 12] & 0x7F));
                    if (temp > 10)
                    {
                        label2.Text = string.Format("Temp1:{0:f1} °C", temp / 10.0);
                        Ltemp.Add(temp);
                        iit++;
                        if (iit % 60 == 0)
                        {
                            it++;
                            it %= 3;
                            label2.ForeColor = clr[it];

                        }
                    }
                    iStart += 14;
                }
                iStart++;
            }
        }

        private void displayG()
        {
            iS = 0;
            iE = Lecg.Count / 3;
            ww = iE;
            bmpI = new Bitmap(iE, hh);
            bmpII = new Bitmap(iE, hh);
            bmpV = new Bitmap(iE, hh);
            gI = Graphics.FromImage(bmpI);
            gII = Graphics.FromImage(bmpII);
            gV = Graphics.FromImage(bmpV);
            myPen = pB;
            x1 = 0;
            int imx, imn, x00 = 0, y00 = 0, x01 = 0, x02 = 0, y01 = 0, y02 = 0;
            imx = Lecg.Max() + 10;
            imn = Lecg.Min() - 10;
            scaleH = (hh - 2.0f * dH) / (imx - imn);
            iE = Lecg.Count;
            while (iS < iE - 3)
            {
                s0 = Lecg[iS++];
                y1 = hh - dH - (int)((imx - s0) * scaleH);
                x1++;
                gI.DrawLine(myPen, x00, y00, x1, y1);
                x00 = x1;
                y00 = y1;
                s0 = Lecg[iS++];
                y1=hh-dH- (int)((imx - s0) * scaleH);
                //gII.DrawLine(myPen, x01*10, y01, x1*10, y1);//波長變長
                gII.DrawLine(myPen, x01, y01, x1, y1);
                x01 = x1;
                y01 = y1;
                s0 = Lecg[iS++];
                y1 = hh - dH - (int)((s0 - imn) * scaleH);
                gV.DrawLine(myPen, x02, y02, x1, y1);
                x02 = x1;
                y02 = y1;
            }
            pictureBox1.Image = bmpI;
            pictureBox2.Image = bmpII;
            pictureBox3.Image = bmpV;
        }
    }
}
