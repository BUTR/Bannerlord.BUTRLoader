/*
using Bannerlord.ModuleManager;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bannerlord.BUTRLoader.Helpers
{
    internal record RequestData([property: JsonProperty("modData")] List<RequestModData> ModData);
    internal record RequestModData([property: JsonProperty("id")] string Id, [property: JsonProperty("version")] string Version, [property: JsonProperty("url")] string Url);
    internal record ResponseData([property: JsonProperty("modData")] List<RequestModData> ModData);
    internal record ResponseModData([property: JsonProperty("id")] string Id, [property: JsonProperty("newVersion")] string NewVersion);

    internal static class UpdateChecker
    {
        private const string UpdateCheckerUrl = "/update-checker";

        public static async Task<ResponseData?> GetAsync(IEnumerable<ModuleInfoExtended> moduleInfos, bool checkLauncher = true)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var baseUrlAttr = assembly.GetCustomAttributes<AssemblyMetadataAttribute>().FirstOrDefault(a => a.Key == "BUTRBaseUrl");
            if (baseUrlAttr is null)
                return null;

            var modData = moduleInfos
                .Where(m => !string.IsNullOrWhiteSpace(m.Url))
                .Select(m => new RequestModData(m.Id, m.Version.ToString(), m.Url))
                .ToList();
            if (checkLauncher)
            {
                modData.Add(new RequestModData(
                    "Bannerlord.BUTRLoader",
                    $"v{typeof(UpdateChecker).Assembly.GetName().Version}",
                    ""));
            }
            var json = JsonConvert.SerializeObject(new RequestData(modData));
            var data = Encoding.UTF8.GetBytes(json);

            var httpWebRequest = WebRequest.CreateHttp(baseUrlAttr.Value + UpdateCheckerUrl);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.ContentLength = data.Length;
            httpWebRequest.UserAgent = $"BUTRLoader v{typeof(UpdateChecker).Assembly.GetName().Version}";

            using var requestStream = await httpWebRequest.GetRequestStreamAsync().ConfigureAwait(false);
            await requestStream.WriteAsync(data, 0, data.Length).ConfigureAwait(false);

            if (await httpWebRequest.GetResponseAsync().ConfigureAwait(false) is HttpWebResponse response && response.GetResponseStream() is { } stream)
            {
                using var responseReader = new StreamReader(stream);
                var str = await responseReader.ReadLineAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<ResponseData>(str);
            }

            return null;
        }
    }
}
*/