using System;
using System.Drawing;
using System.Linq;

namespace Opal
{
	partial class MainWindow
	{
		static void DrawPieGraph(Image destination, double percent)
		{
			using var g = Graphics.FromImage(destination);
			var bounds = new Rectangle(Point.Empty, destination.Size);

			g.Clear(Color.Black);
			g.FillEllipse(Brushes.DarkSlateGray, bounds);

			var brush = percent < 80 ? Brushes.LimeGreen : percent < 90 ? Brushes.Gold : Brushes.Red;

			g.FillPie(brush, bounds, -90, (float)((percent / 100d) * 365d));

			var half = new Size(bounds.Width / 2, bounds.Height / 2);
			var sliver = new Size((int)(bounds.Width * 0.3), (int)(bounds.Height * 0.3));
			var middle = new Rectangle(
				half.Width - sliver.Width, half.Height - sliver.Height, 
				sliver.Width * 2, sliver.Height * 2);

			g.FillEllipse(Brushes.Black, middle);

			var format = new StringFormat();
			format.LineAlignment = StringAlignment.Center;
			format.Alignment = StringAlignment.Center;

			g.DrawString($"{Math.Round(percent)}%", new Font("Sans Serif", 14), Brushes.White, middle, format);
		}

		void MonitorInit()
		{
			monitorPane.Font = new Font(monitorPane.Font.FontFamily, (float)Config.testFontSize);

			diskLabel1.Text = Config.storageName1;
			diskLabel2.Text = Config.storageName2;

			diskGraph1.Image = new Bitmap(diskGraph1.Width, diskGraph1.Height);
			diskGraph2.Image = new Bitmap(diskGraph2.Width, diskGraph2.Height);
		}

		void MonitorEvent(object state)
		{
			var testResults = new Tuple<Test, bool>[0];
			try { testResults = Config.tests.Select(test => Tuple.Create(test, test.Run())).ToArray(); }
			catch (Exception) { }

			var diskText1 = "";
			var diskText2 = "";
			var diskPercent1 = 0d;
			var diskPercent2 = 0d;

			try
			{
				if (Config.storagePath1 != null)
					DiskInfo.GetDiskDetails(Config.storagePath1, out diskText1, out diskPercent1);
				if (Config.storagePath2 != null)
					DiskInfo.GetDiskDetails(Config.storagePath2, out diskText2, out diskPercent2);
			}
			catch (Exception) { }

			Invoke(new Action(() =>
			{
				monitorPane.Clear();

				monitorPane.SelectionTabs = new int[] { Config.testTabWidth };

				foreach (var result in testResults)
				{
					monitorPane.AppendText($"{result.Item1.Name}\t");
					if (result.Item2) monitorPane.AppendText("UP\n", Color.LimeGreen);
					else monitorPane.AppendText("DOWN\n", Color.Red);
				}

				ActiveControl = screen1;

				diskDetails1.Text = diskText1;
				diskDetails2.Text = diskText2;

				DrawPieGraph(diskGraph1.Image, diskPercent1);
				DrawPieGraph(diskGraph2.Image, diskPercent2);

				diskGraph1.Refresh();
				diskGraph2.Refresh();
			}));
		}
	}
}
