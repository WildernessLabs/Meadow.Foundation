using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Meadow.Foundation.DataLoggers
{
    public class AdafruitIO
    {
        #region Properties

        /// <summary>
        ///     Get or set the AdafruitIO AIO key.  This key allows this class to identify itself 
        ///     with AdafruitIO and log data with the service.
        /// </summary>
        public string WriteKey { get; set; }

        /// <summary>
        ///     URI of the AdafruitIO api.
        /// </summary>
        public string URI { get; set; }

        #endregion Properties

        #region Constructor(s)

        /// <summary>
        ///     Create a new AdafruitIO object.
        /// </summary>
        /// <param name="writeKey">Write key.</param>
        public AdafruitIO(string writeKey)
        {
            WriteKey = writeKey;
            URI = "https://io.adafruit.com/api/v2/";
        }

        #endregion Constructor(s)

        #region Methods

        /// <summary>
        ///     Send a single value to AdafruitIO
        /// </summary>
        /// <param name="value">Value to send to AdafruitIO.</param>
        public void PostValue(SensorReading value)
        {
            PostValues(new SensorReading[] { value });
        }

        /// <summary>
        ///     Post a series of values to AdafruitIO.
        /// </summary>
        /// <param name="values">Array of values to send to AdafruitIO.</param>
        public void PostValues(params SensorReading[] values)
        {
            string data = "{ \"feeds\": [";
            for (int index = 0; index < values.Length; index++)
            {
                if (index > 0)
                {
                    data += ",\n";
                }
                data += " { \"key\": \"" + values[index].Key + "\", \"value\": \"" + values[index].Value + "\" }";
            }
            data += "],\n \"created_at\": \"" + values[0].CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ") + "\"}";
            PostData(data);
        }

        /// <summary>
        ///     Post the specified data to AdafruitIO.
        /// </summary>
        /// <remarks>
        ///     The data should be JSON ecoded as follows:
        /// 
        ///     {
        ///         "feeds": [
        ///             {
        ///                 "key": "windspeed",
        ///                 "value": "12"
        ///             },
        ///             {
        ///                 "key": "winddirection",
        ///                 "value": "20"
        ///             }
        ///         ],
        ///         "created_at": "2017-12-27T13:20:00Z"
        ///     }
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
                    
                    request.Headers.Add("X-XIO-KEY: " + WriteKey);
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
            return (result);
        }

        #endregion Methods
    }
}
