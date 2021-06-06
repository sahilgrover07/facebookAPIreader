using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SocialMediaReader.Controllers
{

    [Authorize]


    public class FacebookController : Controller
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Facebook
        public ActionResult Index()
        {
            return View();
        }
        public async Task<ActionResult> Posts()
        {

            var currentClaims = await UserManager.GetClaimsAsync(HttpContext.User.Identity.GetUserId());
            var accestoken = currentClaims.FirstOrDefault(x => x.Type == "urn:tokens:facebook");
            if (accestoken == null)
            {
                return (new HttpStatusCodeResult(HttpStatusCode.NotFound, "Token not found"));
            }
            string url = "https://graph.facebook.com/me?fields=id,name,feed.limit(1000){message,story,created_time,attachments{description,type,url,title,media,target},comments{id,from,message}}&access_token="+ accestoken.Value;

       //     string url = String.Format("https://graph.facebook.com/me?fields=id,name,feed.limit(1000){message,story,created_time,attachments{description,type,url,title,media,target},comments{id,from,message}}&access_token=", accestoken.Value);
           // string url = String.Format("https://graph.facebook.com/me/?fields=id,name,feed.limit(100){{message,created_time,attachments}}&access_token={0}", accestoken.Value);
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string result = await reader.ReadToEndAsync();

                dynamic jsonObj = System.Web.Helpers.Json.Decode(result);
                Models.SocialMedia.Facebook.Posts posts = new Models.SocialMedia.Facebook.Posts(jsonObj);
                ViewBag.JSON = result;

                //Write JSON data to file
                string json = JsonConvert.SerializeObject(jsonObj);
                string path = @"C:\Big Data Analytics\Social data mining Technique\Assignment\fbdata.json";

                using (TextWriter tw = new StreamWriter(path))
                {
                    //foreach (var p in json)
                    // {
                    tw.WriteLine(json);
                    // }

                }

                //Insert data to MongoDB
                var connectionString = "mongodb+srv://bhavna:BDATSocialMedia@cluster0-m58tv.mongodb.net/test?retryWrites=true&w=majority";

                var client = new MongoClient(connectionString);

                var database = client.GetDatabase("SocialMedia");

                string text = System.IO.File.ReadAllText(path);
                var collection = database.GetCollection<BsonDocument>("Facebook");
                var aa = new BsonDocument
                        {
                            {"a","aa"},
                            {"b","bb"}

                        };
                collection.InsertOneAsync(aa);
                client.DropDatabase("SocialMedia" +
                    "");
                MongoDB.Bson.BsonDocument docu
                    = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(result);
                collection.InsertOneAsync(docu);

                //var document = BsonSerializer.Deserialize<BsonDocument>(text);



                //await collection.InsertOneAsync(document);



                return View(posts);



            }



        }
    }
}