using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using Gma.UserActivityMonitor;
using CoreAudioApi;

namespace VolumeHooker
{
    public partial class Form1 : Form
    {
        private MMDevice device;
        private Graphics g;
        private Color bg = SystemColors.InactiveCaptionText;
        private SolidBrush brush = new SolidBrush(Color.White);
        private int y;

        public Form1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(30, 120);
            g = Graphics.FromImage(pictureBox1.Image);

            //HookManager.KeyDown += HookManager_KeyDown;
            //HookManager.KeyUp += HookManager_KeyUp;

            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
            device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
            DrawVolume((int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100));
            device.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
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
            int val = (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            //notifyIcon1.BalloonTipTitle = "KeyboardHook";
            //notifyIcon1.BalloonTipText = string.Format("KeyDown - {0} - {1}", e.KeyCode, val);
        }

        private void HookManager_KeyUp(object sender, KeyEventArgs e)
        {

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
            Application.Exit();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
