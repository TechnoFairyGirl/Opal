using SimpleHttp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Opal
{
	class MediaDecoder
	{
		public static Bitmap BufferToBitmap(int width, int height, byte[] buffer) =>
			new Bitmap(width, height, width * 4, PixelFormat.Format32bppRgb,
				Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0));

		public static void StartDecoding(
			string mediaPath,
			int width,
			int height,
			double frameRate,
			Action<int, int, byte[]> frameCb,
			Action<string> logCb = null,
			Action<int> exitCb = null)
		{
			var ffmpegPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? 
				"ffmpeg.exe" : "ffmpeg";
			var pipeName = Util.Random(8).ToHexString();
			var pipePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
				$"\\\\.\\pipe\\{pipeName}" : $"unix://tmp/CoreFxPipe_{pipeName}";

			Task.Run(() =>
			{
				using var server = new NamedPipeServerStream(pipeName);

				Util.StartProcess(ffmpegPath,
					$"-nostats -y -max_delay 500000 -i \"{mediaPath}\" " +
					$"-c:v rawvideo -pix_fmt rgb32 -s {width}x{height} -r {frameRate} -f rawvideo \"{pipePath}\"",
					true, logCb, exitCb);

				server.WaitForConnection();

				var buffer = new byte[width * height * 4];
				using var bufferStream = new MemoryStream(buffer);

				while (server.IsConnected)
				{
					bufferStream.Position = 0;
					server.CopyBlockTo(bufferStream, buffer.LongLength);
					frameCb(width, height, buffer);
				}
			});
		}
	}
}
