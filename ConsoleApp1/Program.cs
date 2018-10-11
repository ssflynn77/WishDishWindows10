using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {

            var xxx = System.IO.File.ReadAllLines(@"C:\test\dining.html");
            foreach (var eachLine in xxx)
            {
                var entityIdStart = eachLine.IndexOf("data-entityID=") + 15;
                var entityIdEnd = eachLine.IndexOf(";") - entityIdStart;
                var entityId = eachLine.Substring(entityIdStart, entityIdEnd);

                var entityTypeStart = eachLine.IndexOf("entityType=") + 11;
                var entityTypeLength = eachLine.IndexOf("\"", entityTypeStart) - entityTypeStart;
                var entityType = eachLine.Substring(entityTypeStart, entityTypeLength);

                var nameSearchString = "<h2 class=\"cardName\">";
                var nameStart = eachLine.IndexOf(nameSearchString) + nameSearchString.Length;
                var nameLength = eachLine.IndexOf("<", nameStart) - nameStart;
                var name = eachLine.Substring(nameStart, nameLength);

                var locationSearchString = "<span class=\"line1\" aria-label=\"location\">";
                var locationStart   = eachLine.IndexOf(locationSearchString) + locationSearchString.Length;
                var locationLength = eachLine.IndexOf("<", locationStart) - locationStart;
                var location = eachLine.Substring(locationStart, locationLength);


                var urlSearchString = "a href=\"";
                var urlStart = eachLine.IndexOf(urlSearchString) + urlSearchString.Length;
                var urlLength = eachLine.IndexOf("\"", urlStart) - urlStart;
                var url = eachLine.Substring(urlStart, urlLength);

                //  returnList.Add(new Restaurant() { EntityId = 1, EntityType = "test", Name = "name", Location = "location", Url = "url" });
                Console.WriteLine($"returnList.Add(new Restaurant() {{ EntityId = {entityId}, EntityType = \"{entityType}\", Name = \"{name}\", Location = \"{location}\", Url = \"{url}\" }});");
            }

            var waitforit = Console.ReadLine();

        }
    }
}
