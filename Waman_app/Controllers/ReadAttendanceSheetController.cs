using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Waman_app.Models;

namespace Waman_app.Controllers
{
    public class ReadAttendanceSheetController : Controller
    {
        public async Task<ActionResult> Index()
        {
            if (Request.Cookies["Inspectors"] == null)
                return Redirect("/");

			var a = JsonConvert.DeserializeObject<Inspectors>(Request.Cookies["Inspectors"].Value);

			var workerPlanningId = Request.QueryString["wpId"];

            await BaseClass.HttpClientSend("GET", $"WorkerPlannings/SignAsRead?id=" + workerPlanningId + "&inspectorId=" + a.id);
			return View();
        }

    }
}