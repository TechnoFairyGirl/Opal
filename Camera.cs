using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Opal
{
	partial class MainWindow
	{
		void StartCameras()
		{
			var frames = new PictureBox[]
				{ cameraFrame1, cameraFrame2, cameraFrame3, cameraFrame4, cameraFrame5, cameraFrame6 };
			var mediaUrls = Config.cameras;

			for (var i = 0; i < Math.Min(frames.Length, mediaUrls.Length); i++)
			{
				var url = mediaUrls[i];
				var frame = frames[i];

				if (string.IsNullOrEmpty(url))
					continue;

				void StartDecoding() => MediaDecoder.StartDecoding(url, 400, 225, 4,
					(width, height, buffer) =>
					{
						var image = MediaDecoder.BufferToBitmap(width, height, buffer);
						Invoke(new Action(() =>
						{
							if (frame.Image != null) frame.Image.Dispose();
							frame.Image = image;
						}));
					},
					logText => Console.WriteLine(logText),
					exitCode =>
					{
						Console.WriteLine($"FFmpeg exited with code {exitCode}. Restarting.");
						StartDecoding();
					});

				StartDecoding();
			}
		}
	}
}
