using System;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace WorkAssignmentTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var endPoint = tbxEndPoint.Text;
            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(endPoint);
            httpWebRequest.ContentType = "applicaion/json";
            httpWebRequest.Method = "POST";

            using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = rtbJSONContent.Text;
                sw.Write(json);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var sr = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = sr.ReadToEnd();
                MessageBox.Show($"Получен ответ: {result}");
            }
        }
    }
}
