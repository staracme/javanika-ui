using AutoMapper;
using Dapper;
using FrontEnd.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FrontEnd.Controllers
{
    public class GalleryController : Controller
    {
        JAVADBEntities db = new JAVADBEntities();
        // GET: Gallery
        public ActionResult Index()
        {
            //List<EventsViewModel> eventsVM = null;
            EventsViewModel pastEventsVM = null;
            //int eventID = (string.IsNullOrEmpty(Request["eventID"]))
            //    ? 0 : Convert.ToInt32(Request["eventID"]);
            //if (eventID > 0)
            //{
            //TempData["eventID"] = eventID;
            pastEventsVM = new EventsViewModel();
            //var res = db.tblEvents.Where(p => p.EventID == eventID).ToList().SingleOrDefault();
            //Mapper.Map(res, pastEventsVM);
            TempData["ListEvents"] = getEvents();
                //TempData["EventRequest"] = getEventRequest(eventID);
            //}
            //else
            //{
            //    eventsVM = TempData["ListEvents"] as List<EventsViewModel>;
            //}
            return View(pastEventsVM);
            //return View();
        }

        public ActionResult SearchEvents(EventRequest eventRequest)
        {
            List<tblEvent> eventsVM = new List<tblEvent>();
            List<EventsViewModel> eventsResponse = new List<EventsViewModel>();
            TempData["EventRequest"] = eventRequest;
            TempData["ListEvents"] = getEvents();
            eventsResponse = getEvents(eventRequest);
            TempData["SearchEvents"] = eventsResponse[0];
            TempData["eventImageList"] = getEventImageList(eventsResponse[0].EventID); ;
            TempData["isSearch"] = true;
            return View("~/Views/Gallery/Index.cshtml");
        }

        private List<EventsImageListViewModel> getEventImageList(decimal eventId)
        {
            List<EventsImageListViewModel> eventImageListVM = null;
            eventImageListVM = new List<EventsImageListViewModel>();
            var res = db.tblPastEvents.Where(e => e.EventID == eventId).ToList();
            Mapper.Map(res, eventImageListVM);
            return eventImageListVM;
        }

        public string getPastEventImages(int eventId)
        {
            List<EventsImageListViewModel> eventImageListVM = null;
            eventImageListVM = new List<EventsImageListViewModel>();
            var res = db.tblPastEvents.Where(e => e.EventID == eventId).ToList();
            Mapper.Map(res, eventImageListVM);
            return JsonConvert.SerializeObject(eventImageListVM);
        }

        private List<EventsViewModel> getEvents()
        {
            List<EventsViewModel> eventsVM = null;
            eventsVM = new List<EventsViewModel>();
            var res = db.tblEvents.OrderByDescending(p => p.EventDate).ToList();
            Mapper.Map(res, eventsVM);
            return eventsVM;
        }
        private List<EventsViewModel> getEvents(EventRequest eventRequest)
        {
            List<EventsViewModel> orderModels = null;
            string constr = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();
            using (SqlConnection con = new SqlConnection(constr))
            {
                orderModels = new List<EventsViewModel>();
                var parameters = new DynamicParameters();
                parameters.Add("@EventId", eventRequest.EventId);
                parameters.Add("@rowsPerPage", eventRequest.rowsPerPage);
                parameters.Add("@pageNum", eventRequest.pageNum);

                using (SqlMapper.GridReader multi = con.QueryMultiple("GetEvents"
                    , parameters
                    , commandType: System.Data.CommandType.StoredProcedure))
                {
                    orderModels = multi.Read<EventsViewModel>().ToList();
                }
            }
            return orderModels;
        }

        private EventRequest getEventRequest(int eventID)
        {
            EventRequest eventRequest = new EventRequest();
            eventRequest.EventId = eventID;
            eventRequest.pageNum = 1;
            eventRequest.rowsPerPage = (string.IsNullOrEmpty(Request["rowsPerPage"]))
                ? 5 : Convert.ToInt32(Request["rowsPerPage"]);
            return eventRequest;
        }
    }
}