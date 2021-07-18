using S7Communication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScanTest
{
    public partial class Form1 : Form
    {

        /// <summary>
        /// Указатель на главный Simatic TCP сервер
        /// </summary>
        public SimaticServer server
        {
            get;
            set;
        }

        S7_Boolean GoodRead;
        S7_Boolean NoRead;
        S7_Boolean FS1;
        S7_CharsArray SCAN_DATA;
        string barcode;

        public Form1()
        {
            InitializeComponent();

            server = new SimaticServer();
            var device = new SimaticDevice("192.168.3.70", CpuType.S71200, 0, 1);
            server.AddDevice(device);
            var group = new SimaticGroup();
            device.AddGroup(group);

            GoodRead = new S7_Boolean("", "M8.0", group);
            NoRead = new S7_Boolean("", "M8.1", group);
            FS1 = new S7_Boolean("", "M8.2", group);
            SCAN_DATA = (S7_CharsArray)device.GetTagByAddress("DB9.DBD14-CHARS100");

            server.Start();


            rbGoodRead.CheckedChanged += (s, e) =>
            {
                var sender = (RadioButton)s;

                if (sender.Checked == false)
                {
                    return;
                }

                btnSend.Click -= btnSendNoRead;
                btnSend.Click += btnSendGoodRead;
            };

            rbNoRead.CheckedChanged += (s, e) =>
            {
                var sender = (RadioButton)s;

                if (sender.Checked == false)
                {
                    return;
                }

                btnSend.Click += btnSendNoRead;
                btnSend.Click -= btnSendGoodRead;
            };


            tbBarCode.TextChanged += (s, e) =>
            {
                barcode = tbBarCode.Text;
            };

            rbGoodRead.Checked = true;
            barcode = tbBarCode.Text;
        }



        private void btnSendGoodRead(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                FS1.Write(true);
                Thread.Sleep(1000);

                SCAN_DATA.Write(barcode);
                GoodRead.Write(true);
                
                Thread.Sleep(1000);

                FS1.Write(false);
                GoodRead.Write(false);
                
                
                
            });

            
        }

        private void btnSendNoRead(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                FS1.Write(true);
                Thread.Sleep(1000);

                NoRead.Write(true);

                Thread.Sleep(1000);

                FS1.Write(false);
                NoRead.Write(false);
            });
        }
    }
}
