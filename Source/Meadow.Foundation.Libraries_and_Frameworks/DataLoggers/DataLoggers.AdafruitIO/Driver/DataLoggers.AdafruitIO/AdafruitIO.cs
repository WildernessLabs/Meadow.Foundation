using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.DataLoggers
{
    /// <summary>
    ///     AdafruitIO DataLogger
    /// </summary>
    /// <remarks>
    ///  The X-AIO-Key needs to be incuded in the header
    ///  The data should be JSON ecoded as follows:
    ///     {
    ///         "value": "12",
    ///         "created_at": "2017-12-27T13:20:00Z"
    ///     }
    /// </remarks>
    /// <example>
    ///     Single value to a feed in the default group
    ///     POST http://io.adafruit.com/api/v2/{UserName}/feeds/{FeedKey}/data
    ///     where FeedKey is the AdafruitIO key for the feed the data is to be added to.
    /// </example>
    /// <example>
    ///     Single value to a feed in a named group
    ///     POST http://io.adafruit.com/api/v2/{UserName}/feeds/{Group}.{FeedKey}/data
    ///     where Group is an AdafruitIO group key and FeedKey is the key for the feed in that group. 
    ///     
    /// TODO: As of beta 4.0 HttpClient does not support SSL so you can not use https:

    public class AdafruitIO
    {
        /// <summary>
        ///     Adafruit account profile username.
        ///     This name identifies the Adafruit IO user account that the feed belongs to.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Get or set the AdafruitIO AIO key.  This key allows this class to identify itself 
        ///     with AdafruitIO and log data with the service.
        /// </summary>
        public string IOKey { get; set; }

        /// <summary>
        ///     URI of the AdafruitIO api.
        /// </summary>
        public string URI { get; set; }

        /// <summary>
        ///     Adafruit feed group
        ///     This identifies the Adafruit feed group that will accessed.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        ///     Create a new AdafruitIO object.
        /// </summary>
        /// <param name="UserName">Adafruit username</param>
        /// <param name="IOkey">Write key.</param>
        /// 
        public AdafruitIO(string UserName, string IOkey, string Group = null)
        {
            this.Group = Group;
            this.UserName = UserName;
            this.IOKey = IOkey;
            URI = "http://io.adafruit.com/api/v2/";     //2020-11-22 - https is not implemented in B4.0
        }

        /// <summary>
        ///     Post a series of values to AdafruitIO.
        /// </summary>
        /// <param name="Values">Array of values to send to AdafruitIO.</param>
        public void PostValues(SensorReading[] Values)
        {
            foreach (var value in Values)
            {
                PostValue(value);
            }
        }

        /// <summary>
        ///     Send a single value to AdafruitIO
        /// </summary>
        /// <param name="Value">Value to send to AdafruitIO.</param>
        public void PostValue(SensorReading Value)
        {
            string apiUri = $"{URI}{UserName}/feeds/";
            if (Group != null)
            {
                apiUri += $"{Group}.";
            }
            apiUri += $"{Value.Key}/data";

            string dateString = Value.CreatedAt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            string postBody = "{\"value\":\"" + Value.Value + "\",\"created_at\":\"" + dateString + "\"}";

            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.Timeout = new TimeSpan(0, 5, 0);
                httpClient.DefaultRequestHeaders.Add("X-AIO-Key", IOKey);
                StringContent content = new StringContent(postBody, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = httpClient.PostAsync(apiUri, content).Result;
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Request timed out.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Request went sideways:\n - Source: {e.Source}" +
                    $"\n - Type: {e.GetType()}" +
                    $"\n - Message: {e.Message}\n - InnerException:\n{e.InnerException}" +
                    $" - StackTrace:\n{e.StackTrace}");
                }
            }
        }
    }
}