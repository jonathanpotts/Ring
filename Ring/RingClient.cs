using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Ring.Models;

namespace Ring
{
    /// <summary>
    /// Provides authenticated access to the Ring API. The <see cref="AuthToken"/> can be used to create future instances.
    /// </summary>
    public class RingClient
    {
        /// <summary>
        /// The API version used for the Ring API.
        /// </summary>
        private const string ApiVersion = "9";

        /// <summary>
        /// The base absolute URI for the Ring API.
        /// </summary>
        private const string ApiUri = "https://api.ring.com";

        /// <summary>
        /// The relative URI used to create a new session.
        /// </summary>
        private const string NewSessionUri = "/clients_api/session";
        /// <summary>
        /// The relative URI used to access the user's Ring devices.
        /// </summary>
        private const string DevicesUri = "/clients_api/ring_devices";
        /// <summary>
        /// The relative URI used to access the user's active dings.
        /// </summary>
        private const string ActiveDingsUri = "/clients_api/dings/active";
        /// <summary>
        /// The relative URI used to access the user's ding history.
        /// </summary>
        private const string DingHistoryUri = "/clients_api/doorbots/history";
        /// <summary>
        /// The relative URI used to access the recording of a specific ding.
        /// </summary>
        private const string DingRecordingUri = "/clients_api/dings/{id}/recording";

        /// <summary>
        /// The User-Agent header values that the Ring API expects.
        /// </summary>
        private readonly IReadOnlyList<ProductInfoHeaderValue> UserAgentHeaderValues = new List<ProductInfoHeaderValue>()
        {
            new ProductInfoHeaderValue("Dalvik", "1.6.0"),
            new ProductInfoHeaderValue("(Linux; U; Android 4.4.4; Build/KTU84Q)")
        };
        /// <summary>
        /// The Accept-Encoding header values that the Ring API expects.
        /// </summary>
        private readonly IReadOnlyList<StringWithQualityHeaderValue> AcceptEncodingHeaderValues = new List<StringWithQualityHeaderValue>()
        {
            new StringWithQualityHeaderValue("gzip"),
            new StringWithQualityHeaderValue("deflate")
        };

        /// <summary>
        /// Data sent with the new session request to authenticate the client with the Ring API.
        /// </summary>
        private readonly IReadOnlyDictionary<string, string> NewSessionData = new Dictionary<string, string>()
        {
            
            { "device[os]", "android" },
            { "device[hardware_id]", Guid.NewGuid().ToString() },
            { "device[app_brand]", "ring" },
            { "device[metadata][device_model]", "Visual Studio Emulator for Android" },
            { "device[metadata][resolution]", "600x800" },
            { "device[metadata][app_version]", "1.7.29" },
            { "device[metadata][app_installation_date]", "" },
            { "device[metadata][os_version]", "4.4.4" },
            { "device[metadata][manufacturer]", "Microsoft" },
            { "device[metadata][is_tablet]", "true" },
            { "device[metadata][linphone_initialized]", "true" },
            { "device[metadata][language]", "en" },
            { "api_version", ApiVersion }
        };

        /// <summary>
        /// The auth token used to authenticate requests against the Ring API.
        /// </summary>
        public string AuthToken { get; private set; }

        /// <summary>
        /// Data sent with subsequent requests to authenticate the client with the Ring API.
        /// </summary>
        private Dictionary<string, string> AuthedSessionData
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "api_version", ApiVersion },
                    { "auth_token", AuthToken }
                };
            }
        }

        /// <summary>
        /// Create an authenticated connection to the Ring API using an auth token.
        /// </summary>
        /// <param name="authToken">Ring API auth token</param>
        public RingClient(string authToken)
        {
            AuthToken = authToken;

            var response = SendRequestAsync(HttpMethod.Get, DevicesUri, AuthedSessionData).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("The Ring API returned the following error: " + response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Create an authenticated connection to the Ring API using a Ring account username and password. This should only be used when an auth token has not been created or has expired.
        /// </summary>
        /// <param name="username">Ring account username</param>
        /// <param name="password">Ring account password</param>
        public RingClient(string username, string password)
        {
            var response = SendRequestAsync(HttpMethod.Post, NewSessionUri, NewSessionData, true, username, password).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("The Ring API returned the following error: " + response.ReasonPhrase);
            }

            var jsonObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var authToken = (string)jsonObject["profile"]["authentication_token"];

            if (authToken == null || authToken.Length <= 0)
            {
                throw new Exception("The Ring API did not return the auth token.");
            }

            AuthToken = authToken;

            response = SendRequestAsync(HttpMethod.Get, DevicesUri, AuthedSessionData).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("The Ring API returned the following error: " + response.ReasonPhrase);
            }
        }

        /// <summary>
        /// Sends a request to the Ring API asynchronously.
        /// </summary>
        /// <param name="method">The HTTP method to use for the request.</param>
        /// <param name="relativeUri">The relative URI to send the request to.</param>
        /// <param name="data">The data to send as part of the request.</param>
        /// <param name="autoRedirect">Specifies if the client should automatically redirect if requested.</param>
        /// <param name="username">The username used to authenticate the request when an auth token is not available.</param>
        /// <param name="password">The password used to authenticate the request when an auth token is not available.</param>
        /// <returns>The response received from the Ring API.</returns>
        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string relativeUri, IReadOnlyDictionary<string, string> data, bool autoRedirect = true, string username = null, string password = null)
        {
            var httpHandler = new HttpClientHandler();
            httpHandler.AllowAutoRedirect = autoRedirect;

            if (username != null && password != null)
            {
                httpHandler.Credentials = new NetworkCredential(username, password);
                httpHandler.PreAuthenticate = true;
            }

            var httpClient = new HttpClient(httpHandler);
            httpClient.BaseAddress = new Uri(ApiUri, UriKind.Absolute);

            httpClient.DefaultRequestHeaders.AcceptEncoding.Clear();
            foreach (var value in AcceptEncodingHeaderValues)
            {
                httpClient.DefaultRequestHeaders.AcceptEncoding.Add(value);
            }

            httpClient.DefaultRequestHeaders.UserAgent.Clear();
            foreach (var value in UserAgentHeaderValues)
            {
                httpClient.DefaultRequestHeaders.UserAgent.Add(value);
            }

            if (method == HttpMethod.Get)
            {
                var queryString = "?" + await new FormUrlEncodedContent(data).ReadAsStringAsync();
                return await httpClient.GetAsync(new Uri(relativeUri + queryString, UriKind.Relative));
            }
            else if (method == HttpMethod.Post)
            {
                return await httpClient.PostAsync(new Uri(relativeUri, UriKind.Relative), new FormUrlEncodedContent(data));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets a list containing the devices that the user has access to.
        /// </summary>
        /// <returns>The list of devices that the user has access to.</returns>
        public async Task<List<Device>> GetDevicesAsync()
        {
            var response = await SendRequestAsync(HttpMethod.Get, DevicesUri, AuthedSessionData);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("The Ring API returned the following error: " + response.ReasonPhrase);
            }

            var devices = new List<Device>();

            var jsonObject = JObject.Parse(await response.Content.ReadAsStringAsync());

            foreach (var token in jsonObject["doorbots"].Children().AsJEnumerable())
            {
                devices.Add(new Device()
                {
                    Id = (ulong)token["id"],
                    Description = (string)token["description"],
                    Address = (string)token["address"],
                    Latitude = (double)token["latitude"],
                    Longitude = (double)token["longitude"],
                    BatteryLife = (int)token["battery_life"],
                    Type = DeviceType.Doorbell
                });
            }

            foreach (var token in jsonObject["authorized_doorbots"].Children().AsJEnumerable())
            {
                devices.Add(new Device()
                {
                    Id = (ulong)token["id"],
                    Description = (string)token["description"],
                    Address = (string)token["address"],
                    Latitude = (double)token["latitude"],
                    Longitude = (double)token["longitude"],
                    BatteryLife = (int)token["battery_life"],
                    Type = DeviceType.AuthorizedDoorbell
                });
            }

            foreach (var token in jsonObject["chimes"].Children().AsJEnumerable())
            {
                devices.Add(new Device()
                {
                    Id = (ulong)token["id"],
                    Description = (string)token["description"],
                    Address = (string)token["address"],
                    Latitude = (double)token["latitude"],
                    Longitude = (double)token["longitude"],
                    BatteryLife = -1,
                    Type = DeviceType.Chime
                });
            }

            foreach (var token in jsonObject["stickup_cams"].Children().AsJEnumerable())
            {
                devices.Add(new Device()
                {
                    Id = (ulong)token["id"],
                    Description = (string)token["description"],
                    Address = (string)token["address"],
                    Latitude = (double)token["latitude"],
                    Longitude = (double)token["longitude"],
                    BatteryLife = (int)token["battery_life"],
                    Type = DeviceType.Cam
                });
            }

            return devices;
        }

        /// <summary>
        /// Gets a list containing the active dings that the user has access to.
        /// </summary>
        /// <returns>The list of active dings that the user has access to.</returns>
        public async Task<List<Ding>> GetActiveDingsAsync()
        {
            var response = await SendRequestAsync(HttpMethod.Get, ActiveDingsUri, AuthedSessionData);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("The Ring API returned the following error: " + response.ReasonPhrase);
            }

            var devices = await GetDevicesAsync();

            var dings = new List<Ding>();

            var jsonArray = JArray.Parse(await response.Content.ReadAsStringAsync());

            foreach (var token in jsonArray.Children().AsJEnumerable())
            {
                DingType type;

                var kind = (string)token["kind"];

                if (kind == "motion")
                {
                    type = DingType.Motion;
                }
                else if (kind == "ding")
                {
                    type = DingType.Ring;
                }
                else
                {
                    type = DingType.Unknown;
                }

                dings.Add(new Ding()
                {
                    Id = (ulong)token["id"],
                    CreatedAt = DateTime.Parse((string)token["created_at"]),
                    Answered = (bool)token["answered"],
                    RecordingIsReady = ((string)token["recording"]["status"] == "ready"),
                    Device = devices.Where(d => d.Id == (ulong)token["doorbot"]["id"]).FirstOrDefault(),
                    Type = type
                });
            }

            return dings;
        }

        /// <summary>
        /// Gets a list containing the recent dings that the user has access to.
        /// </summary>
        /// <param name="limit">The maximum number of dings to list.</param>
        /// <returns>The list of recent dings that the user has access to.</returns>
        public async Task<List<Ding>> GetDingsAsync(int limit = 30)
        {
            var data = AuthedSessionData;
            data.Add("limit", limit.ToString());

            var response = await SendRequestAsync(HttpMethod.Get, DingHistoryUri, data);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("The Ring API returned the following error: " + response.ReasonPhrase);
            }

            var devices = await GetDevicesAsync();

            var dings = new List<Ding>();

            var jsonArray = JArray.Parse(await response.Content.ReadAsStringAsync());
            
            foreach (var token in jsonArray.Children().AsJEnumerable())
            {
                DingType type;

                var kind = (string)token["kind"];

                if (kind == "motion")
                {
                    type = DingType.Motion;
                }
                else if (kind == "ding")
                {
                    type = DingType.Ring;
                }
                else
                {
                    type = DingType.Unknown;
                }

                dings.Add(new Ding()
                {
                    Id = (ulong)token["id"],
                    CreatedAt = DateTime.Parse((string)token["created_at"]),
                    Answered = (bool)token["answered"],
                    RecordingIsReady = ((string)token["recording"]["status"] == "ready"),
                    Device = devices.Where(d => d.Id == (ulong)token["doorbot"]["id"]).FirstOrDefault(),
                    Type = type
                });
            }

            return dings;
        }

        /// <summary>
        /// Gets the URI of the recording for the specified ding.
        /// </summary>
        /// <param name="ding">The ding to get the recording URI for.</param>
        /// <returns>The URI for the recording of the provided ding.</returns>
        public async Task<Uri> GetRecordingUriAsync(Ding ding)
        {
            if (!ding.RecordingIsReady)
            {
                throw new ArgumentException("The provided ding does not have a recording available.");
            }

            var uri = DingRecordingUri.Replace("{id}", ding.Id.ToString());

            var response = await SendRequestAsync(HttpMethod.Get, uri, AuthedSessionData, false, null, null);

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.Found)
            {
                throw new Exception("The Ring API returned the following error: " + response.ReasonPhrase);
            }

            return response.Headers.Location;
        }
    }
}
