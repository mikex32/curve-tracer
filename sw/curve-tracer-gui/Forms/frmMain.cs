using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Design;

namespace CurveTracerGui
{
    public partial class frmMain : Form
    {        
        public frmMain()
        {
            InitializeComponent();
        }

        UInt16 id = 0;
        double[] voltage = new double[4096];
        double[] current = new double[4096];
        double[] m_current = new double[4096];
        decimal set_max_voltage = 0;
        string max_voltage;
        int send_max = 0;
        byte[] buffer = new byte[2];
        string send;



        private UInt16 ReadU16(System.IO.Stream stm)
        {
            byte[] buffer = new byte[2];
            stm.Read(buffer, 0, 2);
            return (UInt16)(buffer[0] + buffer[1] * 256);
        }

        private void port_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (port.IsOpen)
            {
                progressBar1.Invoke(new MethodInvoker(delegate { progressBar1.Minimum = 0; }));
                progressBar1.Invoke(new MethodInvoker(delegate { progressBar1.Maximum = send_max; }));
                label5.Invoke(new MethodInvoker(delegate { label5.Text = "Recieving data..."; }));
                button2.Invoke(new MethodInvoker(delegate { button2.Enabled = false; }));
                while (port.BytesToRead > 1)
                {
                    m_current[id] = ReadU16(port.BaseStream);
                    progressBar1.Invoke(new MethodInvoker(delegate { progressBar1.Increment(1); }));
                    if (id >= send_max)
                    {            
                        id = 0;
                        port.Close();
                        label5.Invoke(new MethodInvoker(delegate { label5.Text = "Data recieved!"; }));
                        button2.Invoke(new MethodInvoker(delegate { button2.Enabled = true; }));
                        break;
                    }
                    id++;
                }
            }
        
        }
       
        private void frmMain_Load(object sender, EventArgs e)
        {
            set_max_voltage = numericUpDown2.Value;
            send_max = (int)(((float)set_max_voltage - 0.017) / 0.000654);
            send = send_max.ToString();
            max_voltage = numericUpDown2.Value.ToString();
            label1.Text = max_voltage;
            chart1.Series[0].Points.Clear();
            chart1.Series[0].Points.AddXY(0, 0);
            if (send_max >= 4095)
                send_max = 4095;
        }

        private void button1_Click_1(object sender, EventArgs e)
        { 
            try
            {
                if (!port.IsOpen)
                    port.Open();
                if (port.IsOpen)
                    port.Write(send);
            }
            catch (IOException)
            {
                MessageBox.Show("Device not connected! Make sure you are using proper COM port.","Error");
            }


            progressBar1.Invoke(new MethodInvoker(delegate { progressBar1.Value = 0; }));
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            set_max_voltage = numericUpDown2.Value;
            send_max = (int)(((float)set_max_voltage - 0.0173) / 0.000654);
            send = send_max.ToString();
            max_voltage = numericUpDown2.Value.ToString();
            label1.Text = max_voltage;
            if (send_max >= 4095)
                send_max = 4095;


        }

        private void button2_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < send_max + 1; i++)
            {
                if (i == 0)
                {
                    voltage[i] = 0;
                    current[i] = 0;
                }
                else
                {
                    voltage[i] = ((i / 1.530) + 17.3) / 1000;
                    current[i] = ((m_current[i] / 1.53) + 17.3) / 1000;
                }
            }
            chart1.ChartAreas[0].AxisY.Maximum = current[send_max] + current[send_max] / 5;
            chart1.ChartAreas[0].AxisX.Maximum = voltage[send_max] + voltage[send_max] / 5;

            for (int i = 0; i < send_max + 1; i++)
            {
                chart1.Series[0].Points.AddXY(voltage[i], current[i]);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            chart1.Series[0].Points.Clear();
            chart1.Series[0].Points.AddXY(0, 0);
            chart1.ChartAreas[0].AxisX.Maximum = 3;
            chart1.ChartAreas[0].AxisY.Maximum = 3;

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            port.PortName = comboBox1.Text;
        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are u sure you want to Exit?", "Exit", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {

                System.Windows.Forms.Application.Exit(); 

            }
        }

        private void saveChartAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string saved_img = " ";

            saveFD.InitialDirectory = "C:";
            saveFD.Title = "Save chart image";
            saveFD.FileName = " ";
            saveFD.Filter = "Image files|*.jpg|All Files|*.*";

            if (saveFD.ShowDialog() != DialogResult.Cancel)
            {
                saved_img = saveFD.FileName;
                chart1.SaveImage(saved_img, ImageFormat.Jpeg);
                MessageBox.Show("Successfully saved", "Chart");
            }
        }

        private void saveDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int i;
            string saved_data = " ";
            string[] data = new string[4097];
            int help = 0;

            saveFD.InitialDirectory = "C:";
            saveFD.Title = "Save chart image";
            saveFD.FileName = " ";
            saveFD.Filter = "Text files|*.txt|All Files|*.*";

            if (saveFD.ShowDialog() != DialogResult.Cancel)
            {
                saved_data = saveFD.FileName;
                data[0] = " U [V]     I [mA]";
                for (i = 1; i < send_max; i++)
                {
                    data[i] = voltage[i - 1].ToString("N5") +"   "+ current[i - 1].ToString("N5");
                }

                File.WriteAllLines(saved_data,data,System.Text.Encoding.Default);
                MessageBox.Show("Successfully saved", "Data");
            }

        }


        
    }
}