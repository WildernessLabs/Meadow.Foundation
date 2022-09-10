using System;

namespace Meadow.Foundation.Web.Maple
{
    public abstract class FileResult : ActionResult
    {
        private string _fileDownloadName;

        /// <summary>
        /// Gets the Content-Type header for the response
        /// </summary>
        public string ContentType { get; private set; }

        public FileResult(string contentType)
        {
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
        }

        /// <summary>
        /// Gets the file name that will be used in the Content-Disposition header of the response.
        /// </summary>
        public string FileDownloadName
        {
            get { return _fileDownloadName ?? string.Empty; }
            set { _fileDownloadName = value; }
        }
    }
}