using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.IO;

using System.Diagnostics;

using MSGorilla.Library.Models;
using MSGorilla.Library;
namespace MSGorilla.WebAPI
{
    public class AttachmentController : BaseController
    {
        AttachmentManager _attachmentManager = new AttachmentManager();

        /// <summary>
        /// Upload attachments. Return attachment detail list.
        /// 
        /// Example output:
        /// [
        ///     {
        ///         "AttachmentID": "2916651;251998720140928_7b7b2ad2-dd71-424d-a918-279c832b0440.xml",
        ///         "FileID": "7b7b2ad2-dd71-424d-a918-279c832b0440.xml",
        ///         "Uploader": "user1",
        ///         "UploadTimestamp": "2014-06-24T03:30:59.0715892Z",
        ///         "Filename": "FederationMetadata.xml",
        ///         "Filetype": "text/xml",
        ///         "Filesize": 46403
        ///     }
        /// ]
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public List<Attachment> Upload()
        {
            string me = whoami();
            List<Attachment> uploadedAttachments = new List<Attachment>();

            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                var docfiles = new List<string>();
                foreach (string file in httpRequest.Files)
                {
                    HttpPostedFile postedFile = httpRequest.Files[file];
                    string filename = postedFile.FileName;
                    string type = postedFile.ContentType;
                    int length = postedFile.ContentLength;
                    var stream = postedFile.InputStream;

                    try
                    {
                        Attachment attachment = _attachmentManager.Upload(filename, type, length, stream, me);
                        uploadedAttachments.Add(attachment);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.ToString());
                    }
                }
            }
            return uploadedAttachments;
        }

        /// <summary>
        /// Download Attachment. Redirect to an azure blob.
        /// 
        /// Example response header:
        /// 
        /// Cache-Control:no-cache
        /// Content-Length:0
        /// Date:Tue, 24 Jun 2014 03:32:24 GMT
        /// Expires:-1
        /// Location:https://msgorilla.blob.core.windows.net/attachment/......
        /// ......
        /// </summary>
        /// <param name="attachmentID">attachment id</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage Download(string attachmentID)
        {
            string me = whoami();
            Attachment attachment = _attachmentManager.GetAttachmentInfo(attachmentID);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Redirect); // tried MOVED too
            response.Headers.Location = new Uri(_attachmentManager.GetDownloadLink(attachment));
            return response;


            //HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            //response.Content = new StreamContent(_attachmentManager.GetAttachment(attachment));
            //response.Content.Headers.ContentType =
            //    new System.Net.Http.Headers.MediaTypeHeaderValue(attachment.Filetype);
            //response.Content.Headers.Add("Content-Disposition", "attachment; filename=" + attachment.Filename);
            //return response;
        }
	}
}