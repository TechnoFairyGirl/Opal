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
		[STAThread]
		static void Main()
		{
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
