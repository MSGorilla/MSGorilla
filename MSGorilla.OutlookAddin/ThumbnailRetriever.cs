using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using MSGorilla.WebAPI.Client;

namespace MSGorilla.OutlookAddin
{
    public class ThumbnailRetriever
    {
        static string _folderPath;
        static string _defaultAvatarPath;
        static GorillaWebAPI _client;
        ///Content/Images/default_avatar.jpg
        ///null
        ////api/attachment/download?attachmentID=2916634;251997257652336_f77eebcc-fb69-4775-83aa-75bde32afb78.jpg
        static ThumbnailRetriever()
        {
            _client = Utils.GetGorillaClient();
            //System.Reflection.Assembly assemble = System.Reflection.Assembly.GetEntryAssembly();
            _folderPath = Path.Combine(
                Directory.GetCurrentDirectory(), 
                "msgorilla_cache");
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }

            _defaultAvatarPath = Path.Combine(_folderPath, "default_avatar.jpg");
            if(!File.Exists(_defaultAvatarPath))
            {
                WebClient client = new WebClient();
                client.DownloadFile("https://msgorilla.cloudapp.net/Content/Images/default_avatar.jpg", 
                    _defaultAvatarPath);
            }
        }

        public static string GetThumbnail(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return _defaultAvatarPath;
            }
            if(url.Equals("Content/Images/default_avatar.jpg",
                StringComparison.InvariantCultureIgnoreCase))
            {
                return _defaultAvatarPath;
            }

            url = url.ToLower();
            if (!url.StartsWith("/"))
            {
                url = "/" + url;
            }
            if (!url.StartsWith("/api/attachment/download?attachmentID=", 
                StringComparison.InvariantCultureIgnoreCase))
            {
                return _defaultAvatarPath;
            }


            string attachmentID = url.Replace("/api/attachment/download?attachmentid=", "");
            string thumbnailPath = Path.Combine(_folderPath, attachmentID);

            if (!File.Exists(thumbnailPath))
            {
                _client.DownloadAttachment(attachmentID, thumbnailPath);
            }

            return thumbnailPath;
        }
    }
}
