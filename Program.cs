using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using SimpleHttp;

namespace Opal
{
	static class Program
	{
		const int delay = 4;

		static void ShiftOutData(SerialPort sp, int bits, params byte[] data)
		{
			sp.BreakState = false;
			sp.RtsEnable = false;
			sp.DtrEnable = false;

			Thread.Sleep(delay);

			for (var i = 0; i < bits; i++)
			{
				var byt = data[bits / 8];
				var bit = ((byt >> (i % 8)) & 1) == 1;

				sp.RtsEnable = bit;
				Thread.Sleep(delay);
				sp.DtrEnable = true;
				Thread.Sleep(delay);
				sp.DtrEnable = false;
				Thread.Sleep(delay);
			}

			sp.BreakState = true;
			Thread.Sleep(delay);
			sp.BreakState = false;
			Thread.Sleep(delay);
		}

		[STAThread]
		static void Main()
		{
			//using var sp = new SerialPort("/dev/ttyUSB3");
			//sp.Open();
			//ShiftOutData(sp, 4, 0b1000);

			ServicePointManager.ServerCertificateValidationCallback =
				(sender, certificate, chain, sslPolicyErrors) => true;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			var window = new MainWindow();

			var server = new HttpServer(80);

			server.AddStaticFile("/", "view.html");

			server.AddExactRoute("GET", "/monitor.png", (request, response) =>
			{
				response.ContentType = "image/png";
				response.Headers.Add("Cache-Control", "max-age=0, must-revalidate");
				response.WriteBodyData(window.screenshotRight ?? new byte[] { });
			});

			server.AddExactRoute("GET", "/cameras.jpg", (request, response) =>
			{
				response.ContentType = "image/jpeg";
				response.Headers.Add("Cache-Control", "max-age=0, must-revalidate");
				response.WriteBodyData(window.screenshotLeft ?? new byte[] { });
			});

			server.Start();

			Application.Run(window);
		}
	}
}
