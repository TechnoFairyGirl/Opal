using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace Opal
{
	partial class MainWindow
	{
		const string wxUrl = "https://api.openweathermap.org/data/2.5/onecall?" +
			"appid={0}&lon={1}&lat={2}&exclude=minutely,hourly,alerts&units=imperial";
		const string wxIconUrl = "http://openweathermap.org/img/wn/{0}@4x.png";

		void WeatherEvent(object state)
		{
			var temp = 0d;
			var maxTemp = 0d;
			var minTemp = 0d;
			var weather = "";
			var icon = (Bitmap)null;

			try
			{
				using var webClient = new WebClient();

				var json = JObject.Parse(
					webClient.DownloadString(string.Format(wxUrl, Config.wxAppId, Config.wxLon, Config.wxLat)));

				temp = Math.Round((double)json["current"]["temp"]);
				maxTemp = Math.Round((double)json["daily"][0]["temp"]["max"]);
				minTemp = Math.Round((double)json["daily"][0]["temp"]["min"]);
				weather = ((string)json["current"]["weather"][0]["description"]).ToTitleCase();

				var iconId = (string)json["current"]["weather"][0]["icon"];
				using var iconStream = new MemoryStream(webClient.DownloadData(string.Format(wxIconUrl, iconId)));
				icon = new Bitmap(iconStream);
			}
			catch (Exception) { }

			Invoke(new Action(() =>
			{
				wxCurTemp.Text = $"{temp} °F";
				wxMaxTemp.Text = $"{maxTemp} °F";
				wxMinTemp.Text = $"{minTemp} °F";
				wxCondition.Text = weather;

				if (wxIcon.Image != null)
					wxIcon.Image.Dispose();

				wxIcon.Image = icon;
			}));
		}
	}
}
