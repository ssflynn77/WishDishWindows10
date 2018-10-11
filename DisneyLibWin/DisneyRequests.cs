using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisneyLibWin
{
    public class DisneyRequests
    {

        /*
         
curl 'https://disneyworld.disney.go.com/api/wdpro/explorer-service/public/finder/dining-availability/80007798;entityType=destination?searchDate=2018-02-02&partySize=2&searchTime=22%3A00' 
-H 'Authorization: BEARER 102f3b13a36340cd9cbedec4a4988fa4' 
-H 'Accept-Encoding: gzip, deflate, br' 
-H 'Accept-Language: en-us' 
-H 'User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36' 
-H 'Content-Type: application/x-www-form-urlencoded; charset=UTF-8' 
-H 'Accept: application/json' 
-H 'Referer: https://disneyworld.disney.go.com/dining/' 
-H 'X-Requested-With: XMLHttpRequest' 
-H 'Connection: keep-alive' --compressed

         */

        public static async Task<Availability[]> GetTimesAsync()
        {
            return await GetTimesAsync(DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"), "18:00");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchDate">Date in yyyy-MM-dd format</param>
        /// <param name="searchTime">Time in HH:mm military format</param>
        /// <returns></returns>
        public static async Task<Availability[]> GetTimesAsync(string searchDate, string searchTime)
        {
            var token = await GetAccessToken();
            var partySize = 2;

            var uri = $"https://disneyworld.disney.go.com/api/wdpro/explorer-service/public/finder/dining-availability/80007798;entityType=destination?searchDate={searchDate}&partySize={partySize}&searchTime={searchTime}";//22%3A00";
            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, new Uri(uri));
            request.Headers.Add("Authorization", "BEARER " + token.access_token);
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Referer", "https://disneyworld.disney.go.com/dining/");
            request.Headers.Add("type", "dining");

            var agentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36";

            var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
            client.DefaultRequestHeaders.Connection.ParseAdd("keep-alive");
            client.DefaultRequestHeaders.AcceptLanguage.ParseAdd("en-us");
            client.DefaultRequestHeaders.UserAgent.ParseAdd(agentString);
            try
            {
                var test = await client.SendAsync(request);
                var content = test.Content.ToString();
                var restaurants = Restaurant.GetRestaurants();


                var avail = (from each in JObject.Parse(content)["availability"]
                             where ((JProperty)each).Name.Contains("rest")
                             orderby ((JProperty)each).Name ascending
                             select new Availability
                             {
                                 Restaurant = restaurants.FirstOrDefault(x => x.EntityId == int.Parse(((JProperty)each).Name.Split(';')[0])),
                                 Offers = (from eachAvail in each.Values("availableTimes")
                                           from eachOffer in eachAvail.Values("offers").Values()
                                           select new Offer
                                           {
                                               TimeString = eachOffer["time"].Value<string>(),
                                               DateAndTime = eachOffer["dateTime"].Value<DateTime>(),
                                           }).ToList()
                                 ,
                             }).OrderBy(x => x.Restaurant.Name).ToArray();


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

        public static async Task<AccessToken> GetAccessToken()
        {

            var uri = "https://disneyworld.disney.go.com/authentication/get-client-token/";
            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, new Uri(uri));
            var client = new System.Net.Http.HttpClient();

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
                var test = await client.SendAsync(request);
                var content = test.Content.ToString();
                return JsonConvert.DeserializeObject<AccessToken>(content);
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
    }

    [JsonObject]
    public class AccessToken
    {
        /*
          "access_token": "8d9a848c4d204c28b7380c9217dd4db5",
            "expires_in": 664
         */

        [JsonProperty(PropertyName = "access_token")]
        public string access_token { get; set; }

        [JsonProperty(PropertyName = "expires_in")]
        public string exexpires_in { get; set; }
    }
}
