using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace UqloadDownloader
{
    public partial class Form1 : Form
    {
        WebClient client1 = new WebClient();
        public static int global_push = 0;
        public static int current_download = 0;
        public static string app_path = Application.StartupPath;

        string[] nal = { "a","b","c","d","e","f","g","h","i","j","k","l","m","n","o","p","q","r","s","t","u","v","w","x","y","z", "1","2","3","4","5","6","7","8","9","0"};

        string GetRandomName(int LettersCount)
        {
            string result = "";

            Random rand1 = new Random();

            for (int count = 1; count <= LettersCount; count++)
            {
                int letter_index = rand1.Next(0, (nal.Length - 1));
                result += nal[letter_index];
            }

            return result;
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //client1.Headers.Add("Referer", "https://uqload.com/embed-9f6ysykvhnmt.html");
            //client1.DownloadFileAsync(new Uri("https://m40.uqload.org/3rfkmzf4djw2q4drdkl7d7vglvngp2nyxrpni6554omg42jolwkizyrowz5a/v.mp4"), "G:\\Merde.mp4");

            if (textBox2.Text != "") {
                DownloadHandler new_download = new DownloadHandler(textBox1.Text, panel1, global_push, textBox2.Text);
            }
            else
            {
                string rand_name = GetRandomName(32);
                DownloadHandler new_download = new DownloadHandler(textBox1.Text, panel1, global_push, rand_name);
            }
        }
    }
}
