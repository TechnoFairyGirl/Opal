using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace Opal
{
	static class Entensions
	{
		public static TResult Let<T, TResult>(this T arg, Func<T, TResult> func) => func(arg);
		public static T Also<T>(this T arg, Action<T> func) { func(arg); return arg; }

		public static string ToTitleCase(this string str) =>
			CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str);

		public static void AppendText(this RichTextBox box, string text, Color color)
		{
			box.SelectionStart = box.TextLength;
			box.SelectionLength = 0;
			box.SelectionColor = color;
			box.AppendText(text);
			box.SelectionColor = box.ForeColor;
		}

		public static string ToHexString(this byte[] bytes) =>
			BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();

		public static byte[] GetBytes(this string str) =>
			Encoding.UTF8.GetBytes(str);

		public static string GetString(this byte[] bytes) =>
			Encoding.UTF8.GetString(bytes);
	}

	static class Util
	{
		public static byte[] Random(int length)
		{
			using var rng = new RNGCryptoServiceProvider();
			var bytes = new byte[length];
			rng.GetBytes(bytes);
			return bytes;
		}

		public static byte[] Sha256(byte[] input)
		{
			using var sha256 = SHA256.Create();
			return sha256.ComputeHash(input);
		}

		public static Process StartProcess(
			string executablePath,
			string arguments,
			bool hide = false,
			Action<string> outputCb = null,
			Action<int> exitCb = null)
		{
			var startInfo = new ProcessStartInfo();
			startInfo.FileName = executablePath;
			startInfo.Arguments = arguments;
			startInfo.UseShellExecute = false;
			startInfo.CreateNoWindow = hide;
			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;

			var process = new Process();
			process.StartInfo = startInfo;
			process.EnableRaisingEvents = true;

			if (outputCb != null)
			{
				process.OutputDataReceived += (sender, e) => outputCb(e.Data);
				process.ErrorDataReceived += (sender, e) => outputCb(e.Data);
			}

			if (exitCb != null)
				process.Exited += (sender, e) => exitCb(process.ExitCode);

			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();

			return process;
		}
	}
}
