using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace Opal
{
	static class Config
	{
		public static readonly JObject config;

		public static readonly string wxAppId;
		public static readonly double wxLon;
		public static readonly double wxLat;

		public static readonly double testFontSize;
		public static readonly int testTabWidth;
		public static readonly Test[] tests;

		public static readonly string storageName1;
		public static readonly string storagePath1;
		public static readonly string storageName2;
		public static readonly string storagePath2;

		public static readonly string pfApiKey;
		public static readonly string pfApiSecret;
		public static readonly string pfUrl;

		public static readonly double trafficUploadMax;
		public static readonly double trafficDownloadMax;

		public static readonly double wxUpdateInterval;
		public static readonly double monitorUpdateInterval;
		public static readonly double trafficUpdateInterval;

		public static readonly string[] cameras;

		static Config()
		{
			config = JObject.Parse(File.ReadAllText("config.json"));

			wxAppId = (string)config["wxAppId"];
			wxLon = (double)config["wxLon"];
			wxLat = (double)config["wxLat"];

			testFontSize = (double)config["testFontSize"];
			testTabWidth = (int)config["testTabWidth"];
			tests = ((JArray)config["tests"]).Select(t => Test.FromJson((JObject)t)).ToArray();

			storageName1 = (string)config["drives"][0]["name"];
			storagePath1 = (string)config["drives"][0]["path"];
			storageName2 = (string)config["drives"][1]["name"];
			storagePath2 = (string)config["drives"][1]["path"];

			pfApiKey = (string)config["pfApiKey"];
			pfApiSecret = (string)config["pfApiSecret"];
			pfUrl = (string)config["pfUrl"];

			trafficUploadMax = (double)config["trafficUploadMax"];
			trafficDownloadMax = (double)config["trafficDownloadMax"];

			wxUpdateInterval = (double)config["wxUpdateInterval"];
			monitorUpdateInterval = (double)config["monitorUpdateInterval"];
			trafficUpdateInterval = (double)config["trafficUpdateInterval"];

			cameras = ((JArray)config["cameras"]).Select(cam => cam.ToString()).ToArray();
		}
	}
}
