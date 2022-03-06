using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Text.RegularExpressions;

namespace USB_window_form
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public static UsbDevice MyUsbDevice;
        public static UsbDeviceFinder MyUsbFinder = new UsbDeviceFinder(1155, 22352); // VID ve PID leri buraya yazıyoruz...
        UsbEndpointReader reader;
        UsbEndpointWriter writer;

        private void OnRxEndPointData(object sender, EndpointDataEventArgs e)
        {
            Action<string> Action = addToTextBox;
            this.BeginInvoke(Action, (Encoding.Default.GetString(e.Buffer, 0, e.Count)));
        }
        private void addToTextBox(string input)
        {
            textBox2.Text += input.Substring(1);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text.Length > 64)
            {
                MessageBox.Show("Max. 64 Characters");
            }
            try
            {
                int bytesWritten;
                writer.Write(Encoding.Default.GetBytes("\x02" + textBox1.Text), 1000, out bytesWritten);
            }
            catch (Exception err)
            {
                MessageBox.Show("Can Not Send Data To USB Device\nDetails: " + err);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "Connect") //kết nối
            {
                try
                {
                    MyUsbDevice = UsbDevice.OpenUsbDevice(MyUsbFinder);
                    if (MyUsbDevice == null) throw new Exception("Device Not Found.");
                    IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                    if (!ReferenceEquals(wholeUsbDevice, null))
                    {
                        wholeUsbDevice.SetConfiguration(1);
                        wholeUsbDevice.ClaimInterface(0);
                    }
                    reader = MyUsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                    writer = MyUsbDevice.OpenEndpointWriter(WriteEndpointID.Ep01);
                    reader.DataReceived += (OnRxEndPointData);
                    reader.DataReceivedEnabled = true;
                    button2.Text = "Disconnect";
                }
                catch
                {
                    MessageBox.Show("error");
                }
            }
            else // ngắt kết nối
            {
                reader.DataReceivedEnabled = false;
                reader.DataReceived -= (OnRxEndPointData);
                reader.Dispose();
                writer.Dispose();
                if (MyUsbDevice != null)
                {
                    if (MyUsbDevice.IsOpen)
                    {
                        IUsbDevice wholeUsbDevice = MyUsbDevice as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            wholeUsbDevice.ReleaseInterface(0);
                        }
                        MyUsbDevice.Close();

                    }
                    MyUsbDevice = null;
                    UsbDevice.Exit();
                }
                button2.Text = "Connect";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
