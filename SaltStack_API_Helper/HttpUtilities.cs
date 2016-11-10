using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SaltAPI
{
    class HttpUtilities
    {
        public enum HttpRequestMethod
        {
            POST,
            GET
        }


        /// <summary>
        /// 忽略证书
        /// </summary>
        public static void IgnoreInvalidCertificates()
        {
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
        }
        private static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static string APIWebHelper(string url, HttpRequestMethod hrm, string PostJSON = "")
        {
            IgnoreInvalidCertificates();
            if (string.IsNullOrEmpty(url) && string.IsNullOrEmpty(PostJSON))
            {
                return null;
            }
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = hrm.ToString();
            request.Accept = RequestType.accept;
            request.Headers["x-auth-token"] = RequestType.xAuthToken;
            request.ContentType = RequestType.contentType;

            if (!string.IsNullOrEmpty(PostJSON))
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(PostJSON);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            }
            try
            {
                var httpResponse = (HttpWebResponse)request.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    return result;
                }
            }
            catch (WebException ex)
            {
                var httpResponse = (HttpWebResponse)ex.Response;
                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.NotFound: // 404
                        return "404";

                    case HttpStatusCode.InternalServerError: // 500
                        return "500";

                    case HttpStatusCode.Unauthorized: // 401
                        return "401";
                    case HttpStatusCode.NotAcceptable: // 406
                        return "406";
                    default:
                        return "000";
                }
            }
        }

    }
}
