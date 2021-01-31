using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Meadow.Foundation.DataLoggers
{
    public class ThingSpeak
    {
        #region Properties

        /// <summary>
        ///     Get or set the ThingSpeak WriteKey.  This key allows this class to identify itself 
        ///     with ThingSpeak and log data with the service.
        /// </summary>
        public string WriteKey { get; set; }

        /// <summary>
        ///     URI of the ThingSpeak api.
        /// </summary>
        public string URI { get; set;  }

        #endregion Properties

        #region Constructor(s)

        /// <summary>
        ///     Create a new ThingSpeak object.
        /// </summary>
        /// <param name="writeKey">Write key.</param>
        public ThingSpeak(string writeKey)
        {
            WriteKey = writeKey;
            URI = "https://api.thingspeak.com/update";
        }

        #endregion Constructor(s)

        #region Methods

        /// <summary>
        ///     Send a single value to ThingSpeak
        /// </summary>
        /// <param name="value">Value to send to ThingSpeak.</param>
        public void PostValue(string value)
        {
            PostData("field1=" + value);
        }

        /// <summary>
        ///     Post a series of values to ThingSpeak.
        /// </summary>
        /// <param name="values">Array of values to send to ThingSpeak.</param>
        public void PostValues(params string[] values)
        {
            string data = string.Empty;
            for (int index = 0; index < values.Length; index++)
            {
                if (data != string.Empty)
                {
                    data += "&";
                }
                data += "field" + (index + 1).ToString() + "=" + values[index];
            }
            PostData(data);
        }

        /// <summary>
        ///     Post the specified data to ThingSpeak.
        /// </summary>
        /// <remarks>
        ///     The data should be URL encoded in the format:
        /// 
        ///     field1=10.2&field2=15
        /// </remarks>
        /// <param name="data">Data to send.</param>
        /// <returns>Record number for the reading(s) just added.</returns>
        private int PostData(string data)
        {
            int retryCount = 0;
            int result = 0;
            while (retryCount < 3)
            {
                try
                {
                    var request = (HttpWebRequest)WebRequest.Create(URI);
                    
                    request.Headers.Add("X-THINGSPEAKAPIKEY: " + WriteKey);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    byte[] bytesToSend = UTF8Encoding.UTF8.GetBytes(data);
                    request.ContentLength = bytesToSend.Length;

                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(bytesToSend, 0, bytesToSend.Length);
                    }

                    var response = (HttpWebResponse) request.GetResponse();

                    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                    result = int.Parse(responseString);
                    retryCount = 4;
                    
                }
                catch
                {
                    retryCount++;
                    if (retryCount == 3)
                    {
                        throw;
                    }
                    Thread.Sleep(1000);
                }
            }
            return(result);
        }

        #endregion Methods
    }
}
