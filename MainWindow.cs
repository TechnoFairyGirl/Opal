using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Timer = System.Threading.Timer;

namespace Opal
{
	partial class MainWindow : Form
	{
		readonly Timer clockTimer;
		readonly Timer weatherTimer;
		readonly Timer monitorTimer;
		readonly Timer screenshotTimer;
		readonly Timer trafficTimer;

		public byte[] screenshotRight;
		public byte[] screenshotLeft;

		public MainWindow()
		{
			InitializeComponent();

			MonitorInit();
			TrafficInit();

			void ClockEvent(object state)
			{
				var dt = DateTime.Now;
				Invoke(new Action(() =>
				{
					clockLabel.Text = dt.ToString("h:mm:ss tt");
					dateLabel.Text = dt.ToString("dddd, MMMM d");
				}));
			};

			void ScreenshotEvent(object state)
			{
				using var screenshotRightBitmap = new Bitmap(screen1.Width, screen1.Height);
				using var screenshotLeftBitmap = new Bitmap(screen2.Width, screen2.Height);

				Invoke(new Action(() =>
				{
					screen1.DrawToBitmap(screenshotRightBitmap, new Rectangle(Point.Empty, screenshotRightBitmap.Size));
					screen2.DrawToBitmap(screenshotLeftBitmap, new Rectangle(Point.Empty, screenshotLeftBitmap.Size));
				}));

				using var screenshotRightData = new MemoryStream();
				using var screenshotLeftData = new MemoryStream();

				screenshotRightBitmap.Save(screenshotRightData, ImageFormat.Png);
				screenshotLeftBitmap.Save(screenshotLeftData, ImageFormat.Jpeg);

				this.screenshotRight = screenshotRightData.ToArray();
				this.screenshotLeft = screenshotLeftData.ToArray();
			}

			screenshotRight = null;
			screenshotLeft = null;

			clockTimer = new Timer(ClockEvent);
			weatherTimer = new Timer(WeatherEvent);
			monitorTimer = new Timer(MonitorEvent);
			screenshotTimer = new Timer(ScreenshotEvent);
			trafficTimer = new Timer(TrafficEvent);

			ActiveControl = screen1;
		}

		private void MainWindow_Load(object sender, EventArgs e)
		{
			clockTimer.Change(0, 100);
			screenshotTimer.Change(0, 500);
			weatherTimer.Change(0, (int)(1000 * Config.wxUpdateInterval));
			monitorTimer.Change(0, (int)(1000 * Config.monitorUpdateInterval));
			trafficTimer.Change(0, (int)(1000 * Config.trafficUpdateInterval));

			StartCameras();
		}
	}
}
