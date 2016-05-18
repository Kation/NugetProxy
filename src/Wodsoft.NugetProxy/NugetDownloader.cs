using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Wodsoft.NugetProxy
{
    public class NugetDownloader
    {
        public NugetDownloader(string url)
        {
            Url = url;
        }

        public string Url { get; private set; }

        public event NugetDownloadProgress Progress;

        public MemoryStream Stream { get; private set; }

        public string ContentType { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }

        public async Task Download()
        {
            Stream = new MemoryStream();
            StatusCode = HttpStatusCode.Accepted;
            HttpWebRequest webRequest = HttpWebRequest.CreateHttp(Url);
            HttpWebResponse webResponse;
            try
            {
                webResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
            }
            catch (WebException ex)
            {
                StatusCode = ((HttpWebResponse)ex.Response).StatusCode;
                ContentType = ((HttpWebResponse)ex.Response).ContentType;
                return;
            }
            var stream = webResponse.GetResponseStream();
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[4096];
                    int length = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (length == 0)
                        break;
                    await Stream.WriteAsync(buffer, 0, length);
                    if (Progress != null)
                        await Progress(this, buffer, length);
                }
            }
            catch
            {
                StatusCode = HttpStatusCode.GatewayTimeout;
                Stream.Position = 0;
                ContentType = webResponse.ContentType;
                return;
            }
            StatusCode = HttpStatusCode.OK;
            ContentType = webResponse.ContentType;
            Stream.Position = 0;
        }
    }

    public delegate Task NugetDownloadProgress(NugetDownloader sender, byte[] buffer, int length);
}
