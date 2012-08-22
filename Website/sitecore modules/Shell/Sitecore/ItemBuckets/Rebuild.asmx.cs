using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using Sitecore.Configuration;
using Sitecore.ItemBucket.Kernel.Util;
using Sitecore.ItemBuckets.BigData.RemoteIndex;

namespace Sitecore.ItemBucket
{
    /// <summary>
    /// Summary description for Rebuild
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Rebuild : System.Web.Services.WebService
    {

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string Build(string indexName, string returnPath)
        {
            var index = RemoteSearchManager.GetIndex(indexName) as RemoteIndex;
            if (index != null)
            {
                index.SilentRebuild();
            }
            var indexLocalFolder = Settings.IndexFolder.Replace("/", "");
            WebServiceCall();
            Copy((Path.Combine(Config.RemoteIndexLocation, indexName)),  indexLocalFolder + "\\" + indexName);
            WebServiceCallEnable();
            return "Done";
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void Reciept()
        {
            Settings.Indexing.Enabled = false;
        }


        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void EnableIndexing()
        {
            Settings.Indexing.Enabled = true;
        }

        private string WebServiceCall()
        {
            
            // create the web request with the url to the web
            // service with the method name added to the end
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(Sitecore.Configuration.Settings.GetSetting("RemoteIndexingReceipt"));

            // add the parameters as key valued pairs making
            // sure they are URL encoded where needed
            ASCIIEncoding encoding = new ASCIIEncoding();

            byte[] postData = encoding.GetBytes("");
            httpReq.ContentType = "application/x-www-form-urlencoded";
            httpReq.Method = "POST";
            httpReq.ContentLength = postData.Length;

            Stream ReqStrm = httpReq.GetRequestStream();
            ReqStrm.Write(postData, 0, postData.Length);
            ReqStrm.Close();

          
            // get the response from the web server and
            // read it all back into a string variable
            HttpWebResponse httpResp = (HttpWebResponse)httpReq.GetResponse();
            StreamReader respStrm = new StreamReader(
               httpResp.GetResponseStream(), Encoding.ASCII);
            var returnString = respStrm.ReadToEnd();
            httpResp.Close();
            respStrm.Close();
            return returnString;
        }

        private string WebServiceCallEnable()
        {

            // create the web request with the url to the web
            // service with the method name added to the end
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(Sitecore.Configuration.Settings.GetSetting("RemoteIndexingReceiptEnable"));

            // add the parameters as key valued pairs making
            // sure they are URL encoded where needed
            ASCIIEncoding encoding = new ASCIIEncoding();

            httpReq.ContentType = "application/x-www-form-urlencoded";
  
            byte[] postData = encoding.GetBytes("");
         
            httpReq.Method = "POST";
            httpReq.ContentLength = postData.Length;

            // convert the request to a steeam object and send it on its way
            Stream ReqStrm = httpReq.GetRequestStream();
            ReqStrm.Write(postData, 0, postData.Length);
            ReqStrm.Close();


            // get the response from the web server and
            // read it all back into a string variable
            HttpWebResponse httpResp = (HttpWebResponse)httpReq.GetResponse();
            StreamReader respStrm = new StreamReader(
               httpResp.GetResponseStream(), Encoding.ASCII);
            var returnString = respStrm.ReadToEnd();
            httpResp.Close();
            respStrm.Close();
            return returnString;
        }


        public static void Copy(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
            DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

            CopyAll(diSource, diTarget);
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

    }
}
