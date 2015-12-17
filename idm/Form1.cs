////////////////////Arash Kayhani 9023013
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO;


namespace idm
{
    public partial class Form1 : Form
    {

        int segmentnum = 4;


        List<file> queue = new List<file>();
        long downdize = 0;
        private readonly AutoResetEvent _signal = new AutoResetEvent(false);
        private readonly AutoResetEvent _signal2 = new AutoResetEvent(false);
        private readonly AutoResetEvent _signal3 = new AutoResetEvent(true);
        private readonly AutoResetEvent _signal4 = new AutoResetEvent(true);
        private delegate void UpdateProgessCallback(Int64 BytesRead, Int64 TotalBytes);
        class file
        {
            public string url;
            public string saveaddress;
        }
        public Form1()
        {

            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            ServicePointManager.DefaultConnectionLimit = 1000;
            System.Net.ServicePointManager.MaxServicePointIdleTime = 10000;
            CheckForIllegalCrossThreadCalls = false;
        }
        void start()
        {
            foreach (file item in queue)
            {
                segmentnum = int.Parse(textBox3.Text);
                progressBar1.Value = 0;
                progressBar2.Value = 0;
                progressBar3.Value = 0;
                progressBar4.Value = 0;
                listBox1.SelectedIndex = queue.IndexOf(item);
                filedownload(item);

            }

        }
        object[] selfhealing1(string url, long total, object len)
        {

            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create((string)url);
            HttpWebResponse myHttpWebResponse;

            object[] args = new object[2];

            while (true)
            {
                try
                {

                    myHttpWebRequest.AddRange(total, (long)len);




                    myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                    args[0] = myHttpWebRequest;
                    args[1] = myHttpWebResponse;
                    break;
                }
                catch (Exception)
                {


                    Thread.Sleep(1000);
                    label3.Text = "connecting...";

                }
            }

            return args;



        }
        Stream selfhealing2(HttpWebRequest myHttpWebRequest, HttpWebResponse myHttpWebResponse, Stream streamResponse, long total, object len, object url)
        {

            _signal4.WaitOne();
            label3.Text = "connecting...";
            while (true)
            {
                try
                {
                    if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    {

                        myHttpWebRequest = (HttpWebRequest)WebRequest.Create((string)url);

                        myHttpWebRequest.AddRange(total, ((long)len));
                        myHttpWebRequest.Timeout = 10000;

                        myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();

                        streamResponse = myHttpWebResponse.GetResponseStream();

                        break;

                    }





                }
                catch (Exception)
                {

                    streamResponse.Flush();
                    myHttpWebRequest.KeepAlive = false;

                    streamResponse.Close();
                    streamResponse.Dispose();
                    myHttpWebResponse.Close();
                    myHttpWebResponse.Dispose();
                    myHttpWebRequest.Abort();
                    label3.Text = "connecting...";
                }
            }
            _signal4.Set();
            return streamResponse;


        }
        private void partdownload(object args)
        {
            Array argArray = new object[5];
            argArray = (Array)args;
            string url = (string)argArray.GetValue(0);
            string path = (string)argArray.GetValue(1);
            object len = argArray.GetValue(2);
            object start = argArray.GetValue(3);
            object index = argArray.GetValue(4);

            Stream strLocal = new FileStream((string)path, FileMode.Create, FileAccess.Write, FileShare.None);

            long total = (long)start;





            object[] https = selfhealing1(url, total, len);
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)https[0];
            HttpWebResponse myHttpWebResponse = (HttpWebResponse)https[1];

            Int64 fileSize = myHttpWebResponse.ContentLength;


            Stream streamResponse = myHttpWebResponse.GetResponseStream();
            _signal2.Set();
            int bytesSize = 0;


            byte[] downBuffer = new byte[2048];
            int counter = 1000;

            while (total < (long)len)
            {

                if (counter == 0)
                {

                    counter = 1000;


                    streamResponse = selfhealing2(myHttpWebRequest, myHttpWebResponse, streamResponse, total, len, url);
                }
                try
                {

                    downBuffer = new byte[2048];
                    Application.DoEvents();
                    streamResponse.ReadTimeout = 2000;
                    bytesSize = streamResponse.Read(downBuffer, 0, downBuffer.Length - 10);
                }
                catch (Exception)
                {

                    streamResponse = selfhealing2(myHttpWebRequest, myHttpWebResponse, streamResponse, total, len, url);
                    continue;
                }

                total += bytesSize;
                downdize += bytesSize;

                strLocal.Write(downBuffer, 0, bytesSize);
                if (((int)index) == 0)
                    progressBar1.Invoke(new UpdateProgessCallback(this.UpdateProgress), new object[] { total - (long)start, (long)len - (long)start });
                else if (((int)index) == 1)
                    progressBar2.Invoke(new UpdateProgessCallback(this.UpdateProgress1), new object[] { total - (long)start, (long)len - (long)start });
                else if (((int)index) == 2)
                    progressBar3.Invoke(new UpdateProgessCallback(this.UpdateProgress2), new object[] { total - (long)start, (long)len - (long)start });
                else if (((int)index) == 3)
                    progressBar4.Invoke(new UpdateProgessCallback(this.UpdateProgress3), new object[] { total - (long)start, (long)len - (long)start });
                if (bytesSize == 0)
                {
                    counter--;
                }
            }

            strLocal.Close();
            streamResponse.Close();
            myHttpWebResponse.Close();


            _signal.Set();
        }
        private void filedownload(file f)
        {
            HttpWebRequest myHttpWebRequest;
            HttpWebResponse myHttpWebResponse;
            try
            {
                myHttpWebRequest = (HttpWebRequest)WebRequest.Create(f.url);


                myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
            }
            catch (Exception)
            {

                object[] https = selfhealing1(f.url, 0, 10);
                myHttpWebRequest = (HttpWebRequest)https[0];
                myHttpWebResponse = (HttpWebResponse)https[1];

            }

            long fileSize = myHttpWebResponse.ContentLength;
            myHttpWebResponse.Close();

            long size = (long)(fileSize / segmentnum);
            for (int i = 0; i < segmentnum; i++)
            {

                object ars;

                if (i < segmentnum - 1)
                    ars = new object[5] { f.url, f.saveaddress + i.ToString() + ".part", size * (i + 1), size * i, i };

                else
                    ars = new object[5] { f.url, (f.saveaddress + i.ToString() + ".part"), fileSize, size * i, i };

                Thread thrDownload = new Thread(new ParameterizedThreadStart(partdownload));


                thrDownload.Start(ars);

                _signal2.WaitOne();


            }

            for (int i = 0; i < segmentnum; i++)
            {
                _signal.WaitOne();
            }

            label3.Text = "joining";
            string s2 = f.saveaddress.Substring(0, f.saveaddress.LastIndexOf('\\'));
            JoinFiles(s2, f.saveaddress);

            DelFiles(s2);

            downdize = 0;
            label3.Text = "idle";

        }

        private void JoinFiles(string FolderInputPath, string FileOutputPath)
        {

            DirectoryInfo diSource = new DirectoryInfo(FolderInputPath);

            FileStream fsSource = new FileStream(FileOutputPath, FileMode.Append);


            foreach (FileInfo fiPart in diSource.GetFiles(@"*.part"))
            {

                Byte[] bytePart = System.IO.File.ReadAllBytes(fiPart.FullName);

                fsSource.Write(bytePart, 0, bytePart.Length);
            }

            fsSource.Close();
        }
        private void DelFiles(string FolderInputPath)
        {

            DirectoryInfo diSource = new DirectoryInfo(FolderInputPath);

            foreach (FileInfo fiPart in diSource.GetFiles(@"*.part"))
            {

                fiPart.Delete();
            }

        }
        private void ADD_Click(object sender, EventArgs e)
        {
            string url = textBox1.Text;
            file f = new file();
            f.url = textBox1.Text;
            f.saveaddress = textBox2.Text;
            listBox1.Items.Add(f.url);
            queue.Add(f);
            button1.Enabled = false;
            textBox1.Text = "";
            textBox2.Text = "";
        }
        private void browse_Click(object sender, EventArgs e)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.ShowDialog();
            textBox2.Text = s.FileName;
            button1.Enabled = true;

        }
        private void Start_Click(object sender, EventArgs e)
        {
            Thread thr = new Thread(new ThreadStart(start));
            thr.Start();

        }
        private void UpdateProgress(Int64 BytesRead, Int64 TotalBytes)
        {

            int PercentProgress = Convert.ToInt32((BytesRead * 100) / TotalBytes);

            progressBar1.Value = PercentProgress;

            label3.Text = "downloading";

        }
        private void UpdateProgress1(Int64 BytesRead, Int64 TotalBytes)
        {

            int PercentProgress = Convert.ToInt32((BytesRead * 100) / TotalBytes);

            progressBar2.Value = PercentProgress;

            label3.Text = "downloading";

        }
        private void UpdateProgress2(Int64 BytesRead, Int64 TotalBytes)
        {

            int PercentProgress = Convert.ToInt32((BytesRead * 100) / TotalBytes);

            progressBar3.Value = PercentProgress;

            label3.Text = "downloading";

        }
        private void UpdateProgress3(Int64 BytesRead, Int64 TotalBytes)
        {
            int PercentProgress = Convert.ToInt32((BytesRead * 100) / TotalBytes);

            progressBar4.Value = PercentProgress;

            label3.Text = "downloading";

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
