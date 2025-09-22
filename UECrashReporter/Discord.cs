using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UECrashReporter
{
    class Discord
    {
        static string s_WebhookUrl = Properties.Resources.DiscordWebhook;
        static string s_CrashReportEmbedColor = Properties.Resources.CrashReportEmbedColor;

        private static readonly HttpClient s_HttpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public static async Task SendToDiscord(CrashInfo a_CrashInfo, string a_CrashDescription, string a_AppName = "")
        {
            string filenamePrefix = a_AppName != string.Empty ? a_AppName + "-" : "";
            string versionString = a_CrashInfo.m_BuildVersion != string.Empty ? a_CrashInfo.m_BuildVersion + "-" : "";
            string fileName = filenamePrefix + versionString + "Crash-" + DateTime.UtcNow.ToString("yyyy-MM-dd--HH-mm") + ".zip";

            if (s_CrashReportEmbedColor == string.Empty)
            {
                s_CrashReportEmbedColor = "16745472";
            }

            string embedStr = string.Empty;

            if (a_CrashDescription != string.Empty)
            {
                a_CrashDescription = a_CrashDescription.Replace("\r", "").Replace("\n", "\\n");

                embedStr = "{" +
                    "\"embeds\": " +
                        "[" +
                            "{" +
                                "\"title\": \"Crash Report\"," +
                                "\"description\": \"" + a_CrashDescription + "\"," +
                                "\"color\": \"" + s_CrashReportEmbedColor + "\"" +
                            "}" +
                        "]" +
                    "}";
            }

            try
            {
                if (embedStr == string.Empty && a_CrashInfo == null)
                {
                    return;
                }

                var content = new MultipartFormDataContent();

                if (a_CrashInfo != null)
                {
                    ByteArrayContent crashContent = new ByteArrayContent(a_CrashInfo.ZipCrashInfo());

                    content.Add(crashContent, "file", fileName);
                }

                if (embedStr != string.Empty)
                {
                    content.Add(new StringContent(embedStr, Encoding.UTF8, "application/json"), "payload_json");
                }

                await s_HttpClient.PostAsync(s_WebhookUrl, content);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
