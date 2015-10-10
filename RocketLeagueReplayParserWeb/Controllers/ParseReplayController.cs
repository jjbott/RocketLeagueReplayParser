using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace RocketLeagueReplayParserWeb.Controllers
{
    public class ParseReplayController : Controller
    {
        // GET: ParseReplay
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase upload)
        {
            try
            {
                var filename = string.Format("{0}-{1}", (Guid.NewGuid()).ToString(), upload.FileName);
                var path = Path.Combine(Server.MapPath("~/replays"), filename);
                upload.SaveAs(path);

                string log;
                var replay = RocketLeagueReplayParser.Replay.Deserialize(path, out log);

                log += replay.ToDebugString();

                return File(Encoding.UTF8.GetBytes(log), "text/plain", upload.FileName + ".log");
            }
            catch(Exception)
            {
                // Gosh I should probably log this so I know when my parser is broken.
                return new HttpStatusCodeResult(500); 
            }

        }

    }
}