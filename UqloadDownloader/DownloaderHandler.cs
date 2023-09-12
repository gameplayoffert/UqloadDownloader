using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.Threading;
using System.ComponentModel;

namespace UqloadDownloader
{
    class DownloadHandler // This class is used to manage and handle download
    {
        Panel Handle_Panel = null;

        WebClient new_client = new WebClient();

        ProgressBar dl_pgb = null;
        Panel dl_panel = null;
        Label dl_info = null;

        long max_dl_size = 0;
        long curr_dl_size = 0;

        bool download_finished = false;

        string StringReplace(string Sentence, string ToReplace, string Pattern)
        {
            string result = Sentence;

            result = result.Replace(ToReplace, Pattern);

            return result;
        }

        public DownloadHandler(string DownloadURL, Panel PanelToHandle, int push_val, string file_title = null)
        {
            bool can_download = false;
            int download_type = 1;
            string good_url = "";

            new_client.DownloadFileCompleted += OnDownloadCompleted;
            new_client.DownloadProgressChanged += OnDownloadProgress;

            try
            {
                if (DownloadURL != "")
                {
                    if (Regex.IsMatch(DownloadURL, @"^https?://uqload\.(com|io)/embed\-"))
                    {
                        string page_str = new_client.DownloadString(DownloadURL);
                        TextBox local_tb = new TextBox();

                        local_tb.Text = page_str;

                        if(string.IsNullOrEmpty(file_title))
                        {
                            var rmatch = Regex.Match(page_str, "chromecast: { media: {title: \"([^\"]*)\"}", RegexOptions.IgnoreCase);

                            if(rmatch.Success)
                            file_title = rmatch.Groups[1].Value;
                        }

                        foreach (string tb_line in local_tb.Lines)
                        {
                            string base_beg = "sources: [" + '"' + "https://";
                            string base_beg2 = "sources: [" + '"';
                            string base_end = '"' + "],";

                            if (tb_line.Contains(base_beg))
                            {
                                string url_to_work = tb_line;
                                url_to_work = StringReplace(url_to_work, base_beg2, "");
                                url_to_work = StringReplace(url_to_work, base_end, "");
                                url_to_work = url_to_work.Trim();

                                good_url = url_to_work;

                                break;
                            }
                        }

                        download_type = 1;
                        can_download = true;
                    }
                    else if (DownloadURL.Contains(".mp4"))
                    {
                        download_type = 2;
                        can_download = true;
                    }
                }
                else
                {
                    throw new Exception("Invalid Url Or Error !");
                }
            }
            catch
            {
                MessageBox.Show("Can't Download\nInvalid Url Or Error", "Error !", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (can_download)
            {
                Form1.global_push += 1;
                Form1.current_download += 1;

                Handle_Panel = PanelToHandle;

                Panel Panel_Base = new Panel();
                Panel_Base.BackColor = Color.Salmon;

                if (push_val == 0)
                {
                    Panel_Base.Location = new Point(20, 20);
                    Panel_Base.Size = new Size(524, 150);
                }
                else if (push_val > 0)
                {
                    int little_math = 20 + ((20 * push_val) + (150 * push_val));
                    Panel_Base.Location = new Point(20, little_math);
                    Panel_Base.Size = new Size(524, 150);
                }

                Label download_name_lb = new Label();
                download_name_lb.AutoSize = false;
                download_name_lb.BackColor = Color.DarkSalmon;
                download_name_lb.ForeColor = Color.Blue;
                download_name_lb.Font = new Font("Arial", 10, (FontStyle.Bold | FontStyle.Italic));
                download_name_lb.TextAlign = ContentAlignment.MiddleCenter;
                download_name_lb.Text = "Title : " + file_title + ".mp4";
                download_name_lb.Location = new Point(0, 0);
                download_name_lb.Size = new Size(524, 30);
                Panel_Base.Controls.Add(download_name_lb);

                Label download_info_lb = new Label();
                download_info_lb.AutoSize = false;
                download_info_lb.BackColor = Color.DarkCyan;
                download_info_lb.ForeColor = Color.FromArgb(0, 255, 0);
                download_info_lb.Font = new Font("Arial", 10, (FontStyle.Bold | FontStyle.Italic));
                download_info_lb.TextAlign = ContentAlignment.MiddleCenter;
                download_info_lb.Text = "Waiting";
                download_info_lb.Location = new Point(0, 120);
                download_info_lb.Size = new Size(524, 30);
                Panel_Base.Controls.Add(download_info_lb);

                ProgressBar dl_progress = new ProgressBar();
                dl_progress.Location = new Point(0, 50);
                dl_progress.Size = new Size(524, 50);
                Panel_Base.Controls.Add(dl_progress);

                dl_pgb = dl_progress;
                dl_panel = Panel_Base;
                dl_info = download_info_lb;

                PanelToHandle.Controls.Add(Panel_Base);

                ThreadPool.QueueUserWorkItem(ETA_Calculator_Thread);

                if (download_type == 1)
                {
                    new_client.Headers.Add("Referer", DownloadURL);
                    new_client.DownloadFileAsync(new Uri(good_url), Form1.app_path + "\\" + file_title + ".mp4");
                }
                else if (download_type == 2)
                {
                    new_client.Headers.Remove("Referer");
                    new_client.DownloadFileAsync(new Uri(DownloadURL), Form1.app_path + "\\" + file_title + ".mp4");
                }
            };
        }

        public void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            download_finished = true;
            Form1.current_download -= 1;
        }

        public void OnDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            dl_pgb.Maximum = 100;
            dl_pgb.Value = e.ProgressPercentage;

            max_dl_size = e.TotalBytesToReceive;
            curr_dl_size = e.BytesReceived;
        }

        public void ETA_Calculator_Thread(Object stateInfo)
        {
            // Formula : T = D / S
            // T = Time; D = Distance; S = Speed;

            int dl_speed = 0; // In KB
            string good_speed = "";

            long diff_1 = 0;
            long diff_2 = 0;
            long final_diff = 0;

            long eta_time = 0;

            while (true)
            {
                diff_1 = (curr_dl_size);

                Thread.Sleep(1000);

                diff_2 = (curr_dl_size);

                final_diff = diff_2 - diff_1;
                dl_speed = (int)(final_diff / 1000);

                if (dl_speed > 0) {
                    eta_time = (((max_dl_size - curr_dl_size) / 1000) / dl_speed);
                }

                if (dl_info != null)
                {
                    DateTime eta_converted = new DateTime();
                    eta_converted = eta_converted.AddSeconds(eta_time);

                    string final_time = "";

                    if (eta_converted.Hour > 0)
                    {
                        string end_h = "";
                        string end_m = "";

                        if (eta_converted.Hour > 1)
                        {
                            end_h = "s";
                        }

                        if (eta_converted.Minute > 1)
                        {
                            end_m = "s";
                        }

                        final_time = (eta_converted.Hour.ToString() + " Hour" + end_h + " ") + (eta_converted.Minute.ToString() + " Minute" + end_m);
                    }
                    else if (eta_converted.Minute > 0)
                    {
                        string end_m = "";
                        string end_s = "";

                        if (eta_converted.Minute > 1)
                        {
                            end_m = "s";
                        }

                        if (eta_converted.Second > 1)
                        {
                            end_s = "s";
                        }

                        final_time = (eta_converted.Minute.ToString() + " Minute" + end_m + " ") + (eta_converted.Second.ToString() + " Second" + end_s);
                    }
                    else
                    {
                        string end_s = "";

                        if (eta_converted.Second > 1)
                        {
                            end_s = "s";
                        }

                        final_time = (eta_converted.Second.ToString() + " Second" + end_s);
                    }

                    if (dl_speed >= 1000)
                    {
                        good_speed = ((dl_speed / 1000).ToString()) + " mb/s";
                    }
                    else
                    {
                        good_speed = dl_speed.ToString() + " kb/s";
                    }

                    if (!download_finished) {
                        dl_info.Invoke(new MethodInvoker(delegate { dl_info.Text = "ETA : " + final_time + " | Speed : " + good_speed; }));
                    }
                    else
                    {
                        Thread.Sleep(500);

                        dl_info.Invoke(new MethodInvoker(delegate { dl_info.Text = "Finished"; }));
                        break;
                    }
                    //dl_info.Text = "ETA : " + final_time  + " | Speed : " + good_speed;
                }
            }
        }
    }
}
