using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace DisneyLib
{
    public class DisneyRequests
    {
         static List<Restaurant> RestaurantList = DisneyLib.Restaurant.GetRestaurantsFromSite();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchDate">Date in yyyy-MM-dd format</param>
        /// <param name="searchTime">Time in HH:mm military format</param>
        /// <param name="partySize">Size of the party</param>
        /// <returns></returns>
        public static async Task<Availability[]> GetTimesAsync(string searchDate, string searchTime, int partySize)
        {
            try
            {
                var token = await GetAccessToken();

                var uri = $"https://disneyworld.disney.go.com/api/wdpro/explorer-service/public/finder/dining-availability/80007798;entityType=destination?searchDate={searchDate}&partySize={partySize}&searchTime={searchTime}";//22%3A00";
                var request = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, new Uri(uri));
                request.Headers.Add("Authorization", "BEARER " + token.access_token);
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                request.Headers.Add("Referer", "https://disneyworld.disney.go.com/dining/");
                request.Headers.Add("type", "dining");

                var agentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";

                var client = new Windows.Web.Http.HttpClient();
                client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
                client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
                client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");
                client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-us");
                client.DefaultRequestHeaders.UserAgent.ParseAdd(agentString);
                try
                {
                    var test = await client.SendRequestAsync(request);
                    var content = test.Content.ToString();


                    var avail = (from each in JObject.Parse(content)["availability"]
                                 where ((JProperty)each).Name.Contains("rest") || ((JProperty)each).Name.Contains("Dining-Event")
                                 orderby ((JProperty)each).Name ascending
                                 select new Availability
                                 {
                                     Restaurant = RestaurantList.FirstOrDefault(x => x.EntityId == int.Parse(((JProperty)each).Name.Split(';')[0])),
                                     Offers = (from eachAvail in each.Values("availableTimes")
                                               from eachOffer in eachAvail.Values("offers").Values()
                                               select new Offer
                                               {
                                                   TimeString = eachOffer["time"].Value<string>(),
                                                   DateAndTime = eachOffer["dateTime"].Value<DateTime>(),
                                               }).ToList(),
                                 }).OrderBy(x => x.Restaurant?.Name).ToArray();

                    var unknownRestaurants = (from each in JObject.Parse(content)["availability"]
                                              where ((JProperty)each).Name.Contains("rest") || ((JProperty)each).Name.Contains("Dining-Event")
                                              orderby ((JProperty)each).Name ascending
                                              select new Availability
                                              {
                                                  RestaurantId = int.Parse(((JProperty)each).Name.Split(';')[0]),
                                                  Restaurant = RestaurantList.FirstOrDefault(x => x.EntityId == int.Parse(((JProperty)each).Name.Split(';')[0])),
                                                  Offers = (from eachAvail in each.Values("availableTimes")
                                                            from eachOffer in eachAvail.Values("offers").Values()
                                                            select new Offer
                                                            {
                                                                TimeString = eachOffer["time"].Value<string>(),
                                                                DateAndTime = eachOffer["dateTime"].Value<DateTime>(),
                                                            }).ToList(),
                                              }).Where(x => x.Restaurant == null).ToArray();

                    return avail;
                }
                //Catch exception if trying to add a restricted header.
                catch (ArgumentException e1)
                {
                    // Console.WriteLine(e.Message);
                }
                catch (Exception e1)
                {
                    // Console.WriteLine("Exception is thrown. Message is :" + e.Message);
                }

                return null;
            }
            catch (Exception ex)
            {
                var xxxx = 1;
                throw;
            }
        }

        public static async Task<AccessToken> GetAccessToken()
        {
            var uri = "https://disneyworld.disney.go.com/authentication/get-client-token/";
            var request = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, new Uri(uri));
            var client = new Windows.Web.Http.HttpClient();

            //'Accept-Encoding: gzip, deflate, br'
            //'Accept-Language: en-US,en;q=0.9'
            //'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36'
            //'Content-Type: application/x-www-form-urlencoded; charset=UTF-8'
            //'Accept: application/json, text/javascript, */*; q = 0.01'
            //'Referer: https://disneyworld.disney.go.com/dining/'
            //'X-Requested-With: XMLHttpRequest'
            //'Connection: keep-alive'
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36");
            // Cannot find Content-Type header, it bombs out
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/javascript, */*; q = 0.01");
            request.Headers.Add("Referer", "https://disneyworld.disney.go.com/dining/");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");

            try
            {
                var test = await client.SendRequestAsync(request);
                var content = test.Content.ToString();
                dynamic test1 = JsonConvert.DeserializeObject<dynamic>(content);

                var testString = "{\"access_token\":\"2a8fa91da07b4798adf3e3b4ea6bca1a\",\"expires_in\":518}";
                Console.WriteLine(testString);

                var token = JsonConvert.DeserializeObject<AccessToken>(content);
                return token;
            }
            //Catch exception if trying to add a restricted header.
            catch (ArgumentException e1)
            {
                // Console.WriteLine(e.Message);
            }
            catch (Exception e1)
            {
                // Console.WriteLine("Exception is thrown. Message is :" + e.Message);
            }
            return null;
        }

        public static async Task<string> GetPep_CSRF(Restaurant restaurant)
        {
            var returnString = string.Empty;

            var uri = restaurant.Url;
            var request = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Get, new Uri(uri));
            var client = new Windows.Web.Http.HttpClient();

            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36");
            // Cannot find Content-Type header, it bombs out
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            request.Headers.Add("Referer", "https://disneyworld.disney.go.com/dining/");
            //request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Host", "disneyworld.disney.go.com");
            client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");

            try
            {
                var test = await client.SendRequestAsync(request);
                var content = test.Content.ToString();

                var pattern = "name=\"pep_csrf\" id=\"pep_csrf\" value=\"(.*)\">";
                Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);

                MatchCollection matches = rgx.Matches(content);
                returnString = matches[0].Groups[1].Value;

            }
            //Catch exception if trying to add a restricted header.
            catch (ArgumentException e1)
            {
                // Console.WriteLine(e.Message);
            }
            catch (Exception e1)
            {
                // Console.WriteLine("Exception is thrown. Message is :" + e.Message);
            }

            return returnString;
        }

        public static async Task<RestaurantAvail> GetRestaurantTimes(Restaurant restaurant, string searchDate, string searchTime, int partySize)
        {
            RestaurantAvail returnValue = new RestaurantAvail() { Restaurant = restaurant };

            var pep_csft = await GetPep_CSRF(restaurant);

            var uri = "https://disneyworld.disney.go.com/finder/dining-availability/";
            var request = new Windows.Web.Http.HttpRequestMessage(Windows.Web.Http.HttpMethod.Post, new Uri(uri));
            var client = new Windows.Web.Http.HttpClient();

            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-US,en;q=0.9");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36");
            client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");

            client.DefaultRequestHeaders.Accept.ParseAdd("application/json, text/javascript, */*; q = 0.01");
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Host", "disneyworld.disney.go.com");
            request.Headers.Add("Origin", "https://disneyworld.disney.go.com");
            // request.Headers.Add("Referer", "https://disneyworld.disney.go.com/dining/animal-kingdom-lodge/boma-flavors-of-africa/");
            request.Headers.Add("Referer", restaurant.Url);


            var values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("pep_csrf", pep_csft));
            values.Add(new KeyValuePair<string, string>("searchDate", searchDate));//"2018-06-13"));
            values.Add(new KeyValuePair<string, string>("skipPricing", "true"));
            values.Add(new KeyValuePair<string, string>("searchTime", searchTime));//, "18:30"));
            values.Add(new KeyValuePair<string, string>("partySize", partySize.ToString()));// "2"));
            values.Add(new KeyValuePair<string, string>("id", $"{restaurant.EntityId};entityType={restaurant.EntityType}"));
            values.Add(new KeyValuePair<string, string>("type", "dining"));

            request.Content = new Windows.Web.Http.HttpFormUrlEncodedContent(values);


            try
            {

                var test = await client.SendRequestAsync(request);

                var content = test.Content.ToString();

                var hasAvailabilityPattern = "data-hasavailability=\"(.*)\"";
                var hasAvailabilityRegex = new Regex(hasAvailabilityPattern, RegexOptions.IgnoreCase);
                var hasAvailabiltyResult = hasAvailabilityRegex.Matches(content)[0].Groups[1].Value;


                var infoTextPattern = "diningReservationInfoText.*?\">\\n*?(.*?)\\n*<";
                var infoTextRegex = new Regex(infoTextPattern, RegexOptions.IgnoreCase);
                var infoTextResult = infoTextRegex.Matches(content)[0].Groups[1].Value;
                returnValue.InfoText = infoTextResult;

                var infoTitlePattern = "diningReservationInfoTitle\\snotAvailable\">\\n*(.*?)\\n*?<";
                var infoTitelRegex = new Regex(infoTitlePattern, RegexOptions.IgnoreCase);
                var infoTitleMatches = infoTitelRegex.Matches(content);
                if (infoTitleMatches.Count > 0)
                { returnValue.InfoTitle = infoTitleMatches[0].Groups[1].Value; }

                if (hasAvailabiltyResult == "1")
                {
                    var pattern = ".*?href=\"(.*\\/)\">.*\\s.*\\s.*>(.*)<";

                    Regex rgx = new Regex(pattern, RegexOptions.IgnoreCase);

                    MatchCollection matches = rgx.Matches(content);
                    foreach (var eachTime in matches.OfType<Match>())
                    {
                        returnValue.TimesAvailabile.Add(new TimesAvail()
                        {
                            URL = eachTime.Groups[1].Value,
                            Time = eachTime.Groups[2].Value
                        });
                    }
                }



            }
            //Catch exception if trying to add a restricted header.
            catch (ArgumentException e1)
            {
                // Console.WriteLine(e.Message);
            }
            catch (Exception e1)
            {
                // Console.WriteLine("Exception is thrown. Message is :" + e.Message);
            }
            return returnValue;
        }
    }

    [JsonObject]
    public class AccessToken
    {
        /*
         * 
         * {{"access_token":"640c388043d04916a7cc3ba624bbd201","expires_in":712}}
          "access_token": "8d9a848c4d204c28b7380c9217dd4db5",
            "expires_in": 664
         */
        public AccessToken() { }

        [JsonProperty(PropertyName = "access_token")]
        public string access_token { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public string expires_in { get; set; }
    }
}
