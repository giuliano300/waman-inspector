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
    public class HomeController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Logon(Login login)
        {
            var v = new Inspectors();
            HttpResponseMessage get = await BaseClass.HttpClientSend("POST", "Inspectors/Login", login);
            if (get.IsSuccessStatusCode)
                v = await get.Content.ReadAsAsync<Inspectors>();

            if (v == null)
            {
                TempData["errormessage"] = "Incorrect username or password!";
                return Redirect("/Home/Index");
            }
            SetLogin(v);

            return Redirect("/Home/Dashboard");
        }



        public void SetLogin(Inspectors a)
        {
            Session[BaseClass.WamanAuthTokenSessionKey] = a.authToken;

            HttpCookie ConnessoWamanInspectors = new HttpCookie("ConnessoWamanInspectors");
            ConnessoWamanInspectors.Value = "true";
            ConnessoWamanInspectors.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Add(ConnessoWamanInspectors);

            HttpCookie Inspectors = new HttpCookie("Inspectors");
            Inspectors.Value = JsonConvert.SerializeObject(a);
            Inspectors.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Add(Inspectors);
        }


        public void StartinspectionCookie()
        {
            HttpCookie Startinspection = new HttpCookie("Startinspection");
            Startinspection.Value = "true";
            Startinspection.Expires = DateTime.Now.AddDays(1);
            Response.Cookies.Add(Startinspection);
        }



        public void CloseInspectionCookie()
        {
            HttpCookie Startinspection = new HttpCookie("Startinspection");
            Startinspection.Value = null;
            Startinspection.Expires = DateTime.Now.AddDays(-1);
            Response.Cookies.Set(Startinspection);
        }



        public ActionResult Index()
        {
            return View();
        }



        public ActionResult RestorePwd()
        {
            return View();
        }



        public ActionResult ChangePwd()
        {
            var g = Request.QueryString["u"];
            ViewBag.g = g;
            return View();
        }



        [HttpGet]
        public async Task<bool> RequestChangePwd(string email)
        {
            await BaseClass.HttpClientSend("GET", "Emails/GetRecoveryPwdInspectors?email=" + email);
            return true;
        }



        [HttpGet]
        public async Task<bool> SaveChangePwd(string guid, string pwd)
        {
            bool v = false;
            var get = await BaseClass.HttpClientSend("GET", "ChangePwdRequests/UpdatePasswordInspectors?guid=" + guid + "&newPwd=" + pwd);
            if (get.IsSuccessStatusCode)
                v = await get.Content.ReadAsAsync<bool>();

            return v;
        }


        [HttpGet]
        public async Task<bool> SaveUpdPwd(string pwd)
        {
            var a = JsonConvert.DeserializeObject<Inspectors>(Request.Cookies["Inspectors"].Value);
            bool v = false;
            var get = await BaseClass.HttpClientSend("GET", "Inspectors/ChangePwd?id=" + a.id + "&pwd=" + pwd);
            if (get.IsSuccessStatusCode)
                v = await get.Content.ReadAsAsync<bool>();

            return v;
        }


        public async Task<ActionResult> Dashboard()
        {
            var inspectorCookie = Request.Cookies["Inspectors"]?.Value;
            if (inspectorCookie == null)
                return Redirect("/");

            var inspector = JsonConvert.DeserializeObject<Inspectors>(inspectorCookie);

            List<CompleteInspectorPlanning> plans = new List<CompleteInspectorPlanning>();
            int openPlanningsCount = 0;

            bool hasTodayInspections = false;

            try
            {
                HttpResponseMessage response = await BaseClass.HttpClientSend("GET", $"InspectorPlannings?inspectorId={inspector.id}&day={DateTime.Now.Day}&month={DateTime.Now.Month}&year={DateTime.Now.Year}");
                if (response.IsSuccessStatusCode)
                {
                    plans = await response.Content.ReadAsAsync<List<CompleteInspectorPlanning>>();
                    openPlanningsCount = plans.Count(p => !p.inspectorPlanning.closed && !p.inspectorPlanning.deleted);
                    hasTodayInspections = plans.Any(p => p.inspectorPlanning.date.Date == DateTime.Now.Date && !p.inspectorPlanning.closed && !p.inspectorPlanning.deleted);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Failed to retrieve the inspection plan. Exception: " + ex.Message;
            }

            // Salva il conteggio nel cookie
            HttpCookie OpenPlanningsCount = new HttpCookie("OpenPlanningsCount");
            OpenPlanningsCount.Value = openPlanningsCount.ToString();
            OpenPlanningsCount.Expires = DateTime.Now.AddHours(24);
            Response.Cookies.Add(OpenPlanningsCount);

            ViewBag.HasTodayInspections = hasTodayInspections;

            return View(plans);
        }



        public async Task<ActionResult> InspectionPlan()
        {
            var inspectorCookie = Request.Cookies["Inspectors"]?.Value;
            if (inspectorCookie == null)
                return Redirect("/");

            var inspector = JsonConvert.DeserializeObject<Inspectors>(inspectorCookie);
            ViewBag.InspectorId = inspector.id;

            var date = DateTime.Now;

            //bool isInspectionStarted = false;

            // Chiama l'API per ottenere il piano di ispezione completo
            List<CompleteInspectorPlanning> plans = new List<CompleteInspectorPlanning>();
            HttpResponseMessage response = await BaseClass.HttpClientSend("GET", $"InspectorPlannings?inspectorId={inspector.id}&day={date.Day}&month={date.Month}&year={date.Year}");
            if (response.IsSuccessStatusCode)
            {
                plans = await response.Content.ReadAsAsync<List<CompleteInspectorPlanning>>();
            }
            else
            {
                ViewBag.ErrorMessage = "Failed to retrieve the inspection plan.";
                return View(new List<CompleteInspectorPlanning>());
            }

            // Filtra solo le ispezioni non completate
            plans = plans.Where(p => !p.inspectorPlanning.closed).ToList();

            return View(plans);
        }



        public async Task<ActionResult> InspectionArchive(int? month = null, int? year = null, string f = null, string locationName = null)
        {
            ViewBag.totalItems = 0;
            var inspectorCookie = Request.Cookies["Inspectors"]?.Value;
            if (inspectorCookie == null)
                return Redirect("/");

            var inspector = JsonConvert.DeserializeObject<Inspectors>(inspectorCookie);

            int page = 0;
            if (Request.QueryString["page"] != null)
                page = Convert.ToInt32(Request.QueryString["page"]);

            int m = DateTime.Now.Month;
            if (month != null)
                m = Convert.ToInt32(month);

            int y = DateTime.Now.Year;
            if (year != null)
                y = Convert.ToInt32(year);

            ViewBag.month = m;
            ViewBag.year = y;

            string query = "InspectorPlannings?inspectorId=" + inspector.id + "&pagination=true&page=" + page + "&pagesize=25&closed=true&month=" + m + "&year=" + y;
            

            if (!string.IsNullOrEmpty(f)) query += $"&f={HttpUtility.UrlEncode(f)}";
            if (!string.IsNullOrEmpty(locationName)) query += $"&locationName={HttpUtility.UrlEncode(locationName)}";

            List<CompleteInspectorPlanning> plans = new List<CompleteInspectorPlanning>();
            HttpResponseMessage response = await BaseClass.HttpClientSend("GET", query);
            if (response.IsSuccessStatusCode)
                plans = await response.Content.ReadAsAsync<List<CompleteInspectorPlanning>>();
            else
            {
                ViewBag.ErrorMessage = "Failed to retrieve the inspection archive.";
                return View(new List<CompleteInspectorPlanning>());
            }

            if (plans == null)
                return View(new List<CompleteInspectorPlanning>());


            int totalItems = 0;
            var get = await BaseClass.HttpClientSend("GET", "InspectorPlannings/TotalItems?inspectorId=" + inspector.id + "&month=" + m + "&year=" + y + "&locationName=" + HttpUtility.UrlEncode(locationName));
            if (get.IsSuccessStatusCode)
                totalItems = await get.Content.ReadAsAsync<int>();

            ViewBag.totalItems = totalItems;


            return View(plans);
        }




        public async Task<ActionResult> InspectionStep1(int id)
        {
            CompleteInspectorPlanning inspectionDetails = null;
            HttpResponseMessage response = await BaseClass.HttpClientSend("GET", $"InspectorPlannings/{id}");
            if (response.IsSuccessStatusCode)
                inspectionDetails = await response.Content.ReadAsAsync<CompleteInspectorPlanning>();

            if (inspectionDetails == null)
            {
                ViewBag.ErrorMessage = "Inspection details not found.";
                return RedirectToAction("InspectionPlan");
            }

            return View(inspectionDetails);
        }

        public async Task<ActionResult> InspectionReport(int id)
        {
            CompleteInspectorPlanning inspectionDetails = null;
            HttpResponseMessage response = await BaseClass.HttpClientSend("GET", $"InspectorPlannings/{id}");
            if (response.IsSuccessStatusCode)
                inspectionDetails = await response.Content.ReadAsAsync<CompleteInspectorPlanning>();

            if (inspectionDetails == null)
            {
                ViewBag.ErrorMessage = "Inspection details not found.";
                return RedirectToAction("InspectionPlan");
            }

            return View(inspectionDetails);
        }


        [HttpPost]
        public async Task<ActionResult> SaveInspectionStep1(FormCollection formData)
        {
            try
            {
                int planningId = int.Parse(formData["inspectorPlanningId"]);

                foreach (var key in formData.AllKeys.Where(k => k.StartsWith("timeSheet_")))
                {
                    var detailId = int.Parse(key.Replace("timeSheet_", ""));

                    var updateDto = new InspectorPlanningDetails
                    {
                        timeSheet = bool.Parse(formData[$"timeSheet_{detailId}"]),
                        foundAtWork = bool.Parse(formData[$"foundAtWork_{detailId}"]),
                        otherIssues = formData[$"otherIssues_{detailId}"],
						signature = formData[$"himg_sig_{detailId}"],
					};

                    var j = JsonConvert.SerializeObject(updateDto);

                    await BaseClass.HttpClientSend("POST", $"InspectorPlanningDetails/UpdatePartial/{detailId}", updateDto);
                }

                string comments = formData["comments"];
                if (!string.IsNullOrEmpty(comments))
                    await BaseClass.HttpClientSend("POST", $"InspectorPlannings/UpdateComments/{planningId}", comments);

                return RedirectToAction("InspectionEnd", new { id = planningId });
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while saving the inspection. " + ex.Message;
                return RedirectToAction("InspectionPlan");
            }
        }




        public async Task<ActionResult> InspectionEnd(int id)
        {
            try
            {
                CompleteInspectorPlanning inspectionDetails = null;
                HttpResponseMessage response = await BaseClass.HttpClientSend("GET", $"InspectorPlannings/{id}");
                if (response.IsSuccessStatusCode)
                    inspectionDetails = await response.Content.ReadAsAsync<CompleteInspectorPlanning>();

                if (inspectionDetails == null)
                {
                    ViewBag.ErrorMessage = "Inspection details not found.";
                    return RedirectToAction("InspectionPlan");
                }

                foreach (var detail in inspectionDetails.details)
                {
                    var jobTypeName = await GetJobTypeName(detail.worker.jobTypeId);
                    detail.jobTypeName = jobTypeName;
                }

                return View(inspectionDetails);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while retrieving the inspection. Exception: " + ex.Message;
                return RedirectToAction("InspectionPlan");
            }
        }



        [HttpPost]
        public async Task<ActionResult> SaveInspectionEnd(FormCollection formData)
        {
            try
            {
                int planningId = int.Parse(formData["inspectorPlanningId"]);
                string comments = formData["entityComment"];
                string signature = formData["signature"];
                bool confirmed = formData["inspectionConfirmation"] == "true";
				string inspectorId = formData["inspectorId"];
                string inspectorAbsenceComment = formData["inspectorAbsenceComment"];
                string sendReportToAdministration = formData["sendReportToAdministration"];

                var updateDto = new InspectionEndDto
                {
                    entityComment = comments,
                    signature = signature,
                    confirm = confirmed,
                    inspectorAbsenceComment = inspectorAbsenceComment,
                    sendReportToAdministration = Convert.ToBoolean(sendReportToAdministration)
                };

                // Salva i dati dell'ispezione
                await BaseClass.HttpClientSend("POST", $"InspectorPlannings/SaveInspectionEnd/{planningId}", updateDto);

                // Ottieni il conteggio delle ispezioni aperte
                List<CompleteInspectorPlanning> plans = new List<CompleteInspectorPlanning>();
                int openInspectionsCount = 0;
                HttpResponseMessage response = await BaseClass.HttpClientSend("GET", $"InspectorPlannings?inspectorId={inspectorId}&day={DateTime.Now.Day}&month={DateTime.Now.Month}&year={DateTime.Now.Year}");
                if (response.IsSuccessStatusCode)
                {
                    plans = await response.Content.ReadAsAsync<List<CompleteInspectorPlanning>>();
                    openInspectionsCount = plans.Count(p => !p.inspectorPlanning.closed);

                    HttpResponseMessage responsePdf = await BaseClass.HttpClientSend("GET", $"InspectorPlannings/GeneratePdf?id={planningId}");

                    if (!response.IsSuccessStatusCode)
                        return Json(new { success = false, message = "Failed to generate Pdf." });
                }

                // Controlla se tutte le ispezioni sono chiuse
                if (openInspectionsCount == 0)
                {
                    // Salva i dati di stop inspection
                    await SaveStopInspectionData();

                    // Invia l'intent per fermare il tracking
                    //string urlApp = $"intent://myapp/stopInspection?inspectorId={inspectorId}#Intent;scheme=myapp;package=com.easywaytechnology.waman;action=com.easywaytechnology.waman.ACTION_STOP_INSPECTION;end";
                    string urlApp = $"myapp://myapp/stopInspection?inspectorId={inspectorId}";

                    return Json(new { success = true, message = "All inspections closed. Stopped tracking.", stopIntent = urlApp });
                }

                // Ritorna una risposta positiva senza intent
                return Json(new { success = true, message = "Inspection closed. Remaining inspections are still open." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<bool> SaveStartInspectionData()
        {
            try
            {
                var inspectorCookie = Request.Cookies["Inspectors"]?.Value;
                if (inspectorCookie == null)
                    return false;

                var inspector = JsonConvert.DeserializeObject<Inspectors>(inspectorCookie);

                // Verifica se esiste già un tracking avviato per oggi
                HttpResponseMessage checkResponse = await BaseClass.HttpClientSend("GET", $"TrackInspectorDemo/ExistCurrentTrack?inspectorId={inspector.id}");
                if (checkResponse.IsSuccessStatusCode)
                {
                    var existingTrack = await checkResponse.Content.ReadAsAsync<TrackInspectorDemo>();
                    if (existingTrack != null)
                    {
                        HttpResponseMessage setBlockedFalse = await BaseClass.HttpClientSend("POST", $"TrackInspectorDemo/Unblock?id={existingTrack.id}");
                        if (setBlockedFalse.IsSuccessStatusCode)
                            return true;
                    }

                }

                // Crea un nuovo tracking
                var trackData = new TrackInspectorDemo()
                {
                    date = DateTime.Now,
                    deleted = false,
                    end = null,
                    trackStatusId = 1,
                    inspectorId = inspector.id,
                    start = DateTime.Now.TimeOfDay,
                    blocked = false,
                    lastUpdate = DateTime.Now
                };

                HttpResponseMessage response = await BaseClass.HttpClientSend("POST", "TrackInspectorDemo/New", trackData);

                StartinspectionCookie();

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }
            return false;
        }



        [HttpPost]
        public async Task<bool> SaveStopInspectionData()
        {
            try
            {
                var inspectorCookie = Request.Cookies["Inspectors"]?.Value;
                if (inspectorCookie == null)
                    return false;

                var inspector = JsonConvert.DeserializeObject<Inspectors>(inspectorCookie);

                // Verifica se esiste un tracking corrente
                HttpResponseMessage getResponse = await BaseClass.HttpClientSend("GET", $"TrackInspectorDemo/ExistCurrentTrack?inspectorId={inspector.id}");
                if (!getResponse.IsSuccessStatusCode)
                    return false;

                var trackData = await getResponse.Content.ReadAsAsync<TrackInspectorDemo>();
                if (trackData == null)
                    return false;

                // Aggiorna i dati del tracking
                trackData.end = DateTime.Now.TimeOfDay;
                trackData.trackStatusId = 2;

                HttpResponseMessage updateResponse = await BaseClass.HttpClientSend("POST", $"TrackInspectorDemo/Update/{trackData.id}", trackData);

                CloseInspectionCookie();

                return updateResponse.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }
            return false;
        }


        [HttpGet]
        public async Task<JsonResult> CheckCurrentTracking()
        {
            try
            {
                var inspectorCookie = Request.Cookies["Inspectors"]?.Value;
                if (inspectorCookie == null)
                {
                    // Non c'è un ispettore loggato
                    return Json(new { trackingExists = false, blocked = false }, JsonRequestBehavior.AllowGet);
                }

                // Deserializza ispettore
                var inspector = JsonConvert.DeserializeObject<Inspectors>(inspectorCookie);

                // Chiama GET /TrackInspectorDemo/ExistAndCheckCurrentTrack?inspectorId=xxx
                var response = await BaseClass.HttpClientSend("GET", $"TrackInspectorDemo/ExistCurrentTrack?inspectorId={inspector.id}");
                if (response.IsSuccessStatusCode)
                {
                    // Legge la risposta come TrackInspectorDemoDto (che include blocked)
                    var trackDto = await response.Content.ReadAsAsync<TrackInspectorDemo>();
                    if (trackDto != null)
                    {
                        // Se blocked è bool? => se null, lo consideriamo come true
                        bool isBlocked = trackDto.blocked == null ? true : trackDto.blocked.Value;

                        return Json(new
                        {
                            trackingExists = true,
                            blocked = isBlocked
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            // Se arriva qui => o errore o non c'è un track corrente
            return Json(new { trackingExists = false, blocked = false }, JsonRequestBehavior.AllowGet);
        }



        public async Task<ActionResult> InfoBox()
        {
            try
            {
                List<InfoBoxToInspectors> infoBoxData = new List<InfoBoxToInspectors>();
                HttpResponseMessage response = await BaseClass.HttpClientSend("GET", "InfoBoxToInspectors");

                if (response.IsSuccessStatusCode)
                    infoBoxData = await response.Content.ReadAsAsync<List<InfoBoxToInspectors>>();
                else
                    ViewBag.ErrorMessage = "Failed to retrieve information.";

                return View(infoBoxData);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while retrieving the information. Exception: " + ex.Message;
                return View(new List<InfoBoxToInspectors>());
            }
        }



        private async Task<string> GetJobTypeName(int jobTypeId)
        {
            string jobTypeName = null;
            HttpResponseMessage response = await BaseClass.HttpClientSend("GET", $"JobTypes/{jobTypeId}");
            if (response.IsSuccessStatusCode)
            {
                var jobType = await response.Content.ReadAsAsync<JobTypes>();
                jobTypeName = jobType?.name;
            }
            return jobTypeName;
        }



        // FIREBASE 

        //public void StartFirebase()
        //{
        //    if (FirebaseApp.DefaultInstance == null)
        //    {
        //        // Assicurati di avere il file delle credenziali nel path giusto
        //        var serviceAccountFilePath = Server.MapPath("/FileFirebase/waman-84d44-7285fea0f201.json");
        //        try
        //        {
        //            FirebaseApp.Create(new AppOptions
        //            {
        //                Credential = GoogleCredential.FromFile(serviceAccountFilePath)
        //            });
        //        }
        //        catch (Exception ex) {
        //            var e = ex.Message;
        //        }
        //    }
        //}

        //[HttpGet]
        //public async Task<string> SendPushNotification(int inspectorId)
        //{
        //    StartFirebase();

        //    var token = "";
        //    HttpResponseMessage getToken = await BaseClass.HttpClientSend("GET", "FirebaseDeviceRegistration/GetToken?inspectorId=" + inspectorId);
        //    if(getToken.IsSuccessStatusCode) 
        //        token = await getToken.Content.ReadAsStringAsync();
        //    try
        //    {
        //        var message = new Message()
        //        {
        //            Token = token.Trim('"'),
        //            Data = new Dictionary<string, string>()
        //            {
        //                { "action", "startInspection" },
        //                { "inspectorId", inspectorId.ToString() }
        //            },
        //            Android = new AndroidConfig
        //            {
        //                Priority = Priority.High,
        //            }
        //        };

        //        string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;

        //        return "Response: " + response + "Message sent: " + message; 
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //}

    }
}
