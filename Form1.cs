using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.UserActivityMonitor;
using CoreAudioApi;
using Microsoft.Win32;

namespace VolumeHooker
{
    public partial class Form1 : Form
    {
        private MMDevice device;
        private Graphics g;
        private Color bg = SystemColors.InactiveCaptionText;
        private SolidBrush brush = new SolidBrush(Color.White);
        private int y;
        Timer timer1;

        // The path to the key where Windows looks for startup applications
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public Form1()
        {
            InitializeComponent();

            checkAutoRun();

            timer1 = new Timer();
            timer1.Interval = 400;
            timer1.Tick += timer1_Tick;

            pictureBox1.Image = new Bitmap(30, 120);
            g = Graphics.FromImage(pictureBox1.Image);

            HookManager.KeyDown += HookManager_KeyDown;
            HookManager.KeyUp += HookManager_KeyUp;

            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
            device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            DrawVolume((int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100));
            device.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
        }

        private void checkAutoRun()
        {
            // Check to see the current state (running at startup or not)
            if (rkApp.GetValue("VolumeHook") == null)
            {
                // The value doesn't exist, the application is not set to run at startup
                menu_runOnStartup.Checked = false;
            }
            else
            {
                // The value exists, the application is set to run at startup
                menu_runOnStartup.Checked = true;
            }
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            while (this.Opacity != 0)
            {
                this.Opacity -= 0.05;
                System.Threading.Thread.Sleep(10);
            }
        }

        void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
        {
            if (this.InvokeRequired)
            {
                object[] Params = new object[1];
                Params[0] = data;
                this.Invoke(new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification), Params);
            }
            else
            {
                DrawVolume((int)(data.MasterVolume * 100));
            }
        }

        private void HookManager_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.VolumeUp) || (e.KeyCode == Keys.VolumeDown))
            {
                timer1.Stop();
                while (this.Opacity != 1)
                {
                    this.Opacity += 0.10;
                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        private void HookManager_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.KeyCode == Keys.VolumeUp) || (e.KeyCode == Keys.VolumeDown))
            {
                timer1.Start();
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                DrawVolume(100 - e.Location.Y);
                SetVolume();
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                DrawVolume(100 - e.Location.Y);
                SetVolume();
            }
        }

        private void SetVolume()
        {
            device.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)y / 100.0f);
        }

        private void DrawVolume(int vol)
        {
            if ((vol >= 0) && (vol <= 100))
                y = vol;
            else if (vol > 100)
                y = 100;
            else
                y = 0;
            label1.Text = String.Format("{0}%", y);
            Draw();
        }

        private void Draw()
        {
            g.Clear(bg);
            
            g.FillRectangle(brush, new Rectangle(0, 100 - y, 25, 100));
            //brush.Dispose();
            //g.Dispose();

            pictureBox1.Refresh();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HookManager.KeyDown -= HookManager_KeyDown;
            HookManager.KeyUp -= HookManager_KeyUp;
            Application.Exit();
        }

        private void menu_runOnStartup_Click(object sender, EventArgs e)
        {
            if (!menu_runOnStartup.Checked)
            {
                menu_runOnStartup.Checked = true;
                // Add the value in the registry so that the application runs at startup
                rkApp.SetValue("VolumeHook", Application.ExecutablePath.ToString());
            }
            else
            {
                menu_runOnStartup.Checked = false;
                // Remove the value from the registry so that the application doesn't start
                rkApp.DeleteValue("VolumeHook", false);
            }
        }
    }
}
