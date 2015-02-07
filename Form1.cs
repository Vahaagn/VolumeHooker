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
		private float y;
		private bool bMouse = false;
		Timer tWait;
		Timer tFadeIn;
		Timer tFadeOut;

		// The path to the key where Windows looks for startup applications
		RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

		public Form1()
		{
			InitializeComponent();

			checkAutoRun();

			tWait = new Timer();
			tFadeIn = new Timer();
			tFadeOut = new Timer();
			tWait.Interval = 500;
			tFadeIn.Interval = 10;
			tFadeOut.Interval = 10;
			tWait.Tick += tWait_Tick;
			tFadeIn.Tick += tFadeIn_Tick;
			tFadeOut.Tick += tFadeOut_Tick;

			pictureBox1.Image = new Bitmap(30, 120);
			g = Graphics.FromImage(pictureBox1.Image);

			HookManager.KeyDown += HookManager_KeyDown;
			HookManager.KeyUp += HookManager_KeyUp;

			MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
			device = DevEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
			DrawVolume(device.AudioEndpointVolume.MasterVolumeLevelScalar);
			device.AudioEndpointVolume.OnVolumeNotification += new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification);
		}

		private void checkAutoRun()
		{
			// Check to see the current state (running at startup or not)
			if ( rkApp.GetValue("VolumeHook") == null ) {
				// The value doesn't exist, the application is not set to run at startup
				menu_runOnStartup.Checked = false;
			}
			else {
				// The value exists, the application is set to run at startup
				menu_runOnStartup.Checked = true;
			}
		}

		void tWait_Tick(object sender, EventArgs e)
		{
			tWait.Stop();
			tFadeOut.Start();
		}
		void tFadeIn_Tick(object sender, EventArgs e)
		{
			if ( this.Opacity != 1 ) {
				this.Opacity += 0.10;
			}

			if ( this.Opacity == 1 ) {
				tFadeIn.Stop();
			}
		}
		void tFadeOut_Tick(object sender, EventArgs e)
		{
			if ( this.Opacity != 0 ) {
				this.Opacity -= 0.05;
			}

			if ( this.Opacity == 0 ) {
				tFadeOut.Stop();
			}
		}

		void AudioEndpointVolume_OnVolumeNotification(AudioVolumeNotificationData data)
		{
			if ( this.InvokeRequired ) {
				this.Invoke(new AudioEndpointVolumeNotificationDelegate(AudioEndpointVolume_OnVolumeNotification), new object[1] {data});
			}
			else {
				DrawVolume(data.MasterVolume);
			}
		}

		private void HookManager_KeyDown(object sender, KeyEventArgs e)
		{
			if ( ( e.KeyCode == Keys.VolumeUp ) || ( e.KeyCode == Keys.VolumeDown ) ) {
				tWait.Stop();
				tFadeOut.Stop();
				this.TopMost = true;
				this.TopLevel = true;
				if ( !tFadeIn.Enabled ) {
					tFadeIn.Start();
				}
			}
		}

		private void HookManager_KeyUp(object sender, KeyEventArgs e)
		{
			if ( ( ( e.KeyCode == Keys.VolumeUp ) || ( e.KeyCode == Keys.VolumeDown ) ) && !bMouse ) {
				tWait.Start();
			}
		}

		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			if ( e.Button == System.Windows.Forms.MouseButtons.Left ) {
				DrawVolume((float) ( 100 - e.Location.Y ) / 100);
				SetVolume();
			}
		}

		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			if ( e.Button == System.Windows.Forms.MouseButtons.Left ) {
				DrawVolume((float) ( 100 - e.Location.Y ) / 100);
				SetVolume();
			}
		}

		private void SetVolume()
		{
			device.AudioEndpointVolume.MasterVolumeLevelScalar = y;
		}

		private void DrawVolume(float vol)
		{
			if ( ( vol >= 0 ) && ( vol <= 1 ) )
				y = vol;
			else if ( vol > 1 )
				y = 1;
			else
				y = 0;
			label1.Text = String.Format("{0}%", (int) ( Math.Round(y * 100) ));
			Draw();
		}

		private void Draw()
		{
			g.Clear(bg);

			g.FillRectangle(brush, new Rectangle(0, 100 - (int) ( y * 100 ), 25, 100));
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
			if ( !menu_runOnStartup.Checked ) {
				menu_runOnStartup.Checked = true;
				// Add the value in the registry so that the application runs at startup
				rkApp.SetValue("VolumeHook", Application.ExecutablePath.ToString());
			}
			else {
				menu_runOnStartup.Checked = false;
				// Remove the value from the registry so that the application doesn't start
				rkApp.DeleteValue("VolumeHook", false);
			}
		}

		private void Form1_MouseEnter(object sender, EventArgs e)
		{
			bMouse = true;
			tWait.Stop();
			tFadeOut.Stop();
			tFadeIn.Start();
		}

		private void Form1_MouseLeave(object sender, EventArgs e)
		{
			bMouse = false;
			tWait.Start();
		}
	}
}
