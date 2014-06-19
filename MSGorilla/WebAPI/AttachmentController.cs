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
namespace MSGorilla.WebApi
{
    public class AttachmentController : BaseController
    {
        AttachmentManager _attachmentManager = new AttachmentManager();

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

        [HttpGet]
        public HttpResponseMessage Download(string attachmentID)
        {
            string me = whoami();
            Attachment attachment = _attachmentManager.GetAttachmentInfo(attachmentID);

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(_attachmentManager.GetAttachment(attachment));
            result.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(attachment.Filetype);
            result.Content.Headers.Add("Content-Disposition", "attachment; filename=" + attachment.Filename);
            return result;
        }
	}
}