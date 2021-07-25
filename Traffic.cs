using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace Opal
{
	partial class MainWindow
	{
		static void DrawLineGraph(Image destination, double percent)
		{
			using var g = Graphics.FromImage(destination);
			var bounds = new Rectangle(Point.Empty, destination.Size);

			g.DrawImage(destination, -1, 0);
			g.DrawLine(Pens.DarkSlateGray, bounds.Width - 1, bounds.Height, bounds.Width - 1, 0);

			var pen = percent < 60 ? Pens.LimeGreen : percent < 80 ? Pens.Gold : Pens.Red;

			g.DrawLine(pen, bounds.Width - 1, bounds.Height, bounds.Width - 1,
				bounds.Height - (float)(bounds.Height * (percent / 100)));
		}

		static JObject PfApiCall(string query)
		{
			var timestamp = DateTime.UtcNow.ToString("yyyyMMddZHHmmss");
			var nonce = Util.Random(4).ToHexString();
			var hash = Util.Sha256($"{Config.pfApiSecret}{timestamp}{nonce}".GetBytes()).ToHexString();
			var auth = $"{Config.pfApiKey}:{timestamp}:{nonce}:{hash}";

			var http = new WebClient();
			http.Headers.Add("fauxapi-auth", auth);
			var json = JObject.Parse(http.DownloadString($"{Config.pfUrl}/fauxapi/v1/?{query}"));

			return json;
		}

		void TrafficInit()
		{
			uploadBox.Image = new Bitmap(uploadBox.Width, uploadBox.Height);
			downloadBox.Image = new Bitmap(downloadBox.Width, downloadBox.Height);
		}

		long inBytes = 0;
		long outBytes = 0;

		readonly Stopwatch trafficStopwatch = new Stopwatch();

		void TrafficEvent(object state)
		{
			var inBytesNew = inBytes;
			var outBytesNew = outBytes;

			try
			{
				var stats = PfApiCall("action=interface_stats&interface=bce0")["data"]["stats"];

				inBytesNew = (long)stats["inbytes"];
				outBytesNew = (long)stats["outbytes"];
			}
			catch (Exception) { }

			var elapsed = trafficStopwatch.ElapsedMilliseconds / 1000d;
			trafficStopwatch.Restart();

			if (elapsed > 0)
			{
				var inSpeed = (inBytesNew - inBytes) * 8 / elapsed / 1e6;
				var outSpeed = (outBytesNew - outBytes) * 8 / elapsed / 1e6;

				Invoke(new Action(() =>
				{
					DrawLineGraph(uploadBox.Image, outSpeed / Config.trafficUploadMax * 100);
					DrawLineGraph(downloadBox.Image, inSpeed / Config.trafficDownloadMax * 100);

					uploadBox.Refresh();
					downloadBox.Refresh();
				}));
			}

			inBytes = inBytesNew;
			outBytes = outBytesNew;
		}
	}
}
