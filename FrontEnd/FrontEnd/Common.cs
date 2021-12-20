using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FrontEnd.Controllers;
using FrontEnd.Models;
namespace FrontEnd
{
    public class Common
    {
        public static string GetViewImagePath()
        {
            return System.Configuration.ConfigurationManager.AppSettings["ViewImagePath"].ToString();
        }

        public static string GetQRPath()
        {
            return System.Configuration.ConfigurationManager.AppSettings["QRPath"].ToString();
        }

        public static string SelectSeat(int eventID, int seatID)
        {
            try
            {
                JAVADBEntities db = new JAVADBEntities();

                string sessionID = GetSessionID();

                var isSeatBooked = db.tblSeatSelections.Where(t => t.SeatID == seatID && t.EventID == eventID && t.SessionID == "" && t.OrderID != null).Any();

                var curr_seat = db.tblSeats.Where(s => s.SeatID == seatID).SingleOrDefault();

                var getBlock = db.tblBlocks.Where(t => t.BlockID == curr_seat.tblSeatRow.BlockID).SingleOrDefault();

                var getPrice = db.tblEventLayoutBlocks.Where(e => e.EventID == eventID && e.BlockID == getBlock.BlockID).SingleOrDefault();
                var Price = getPrice.Price;
                var EBPrice = getPrice.EBPrice;

                //seat can be selected only if it is not purchased
                if (isSeatBooked == false)
                {
                    tblSeatSelection seat = new tblSeatSelection();
                    seat.SessionID = sessionID;
                    seat.EventID = eventID;
                    seat.SeatID = seatID;
                    seat.Price = getPrice.Price;
                    seat.EBPrice = getPrice.EBPrice;
                    seat.CreatedDate = DateTime.Now;
                    db.tblSeatSelections.Add(seat);
                    db.SaveChanges();

                    tblSeatSelectionsLog log = new tblSeatSelectionsLog();
                    log.SessionID = sessionID;
                    log.EventID = eventID;
                    log.SeatID = seatID;
                    seat.Price = getPrice.Price;
                    seat.EBPrice = getPrice.EBPrice;
                    log.CreatedDate = DateTime.Now;
                    db.tblSeatSelectionsLogs.Add(log);
                    db.SaveChanges();

                    return "OK";
                }
                else
                {
                    //remove session associated seat data if this ticket is already booked
                    var getSeat = db.tblSeatSelections.Where(s => s.SeatID == seatID && s.SessionID == sessionID && s.OrderID == null && s.EventID == eventID).SingleOrDefault();
                    db.tblSeatSelections.Remove(getSeat);
                    db.SaveChanges();
                    return "BOOKED";
                }
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    return ex.InnerException.Message;
                else
                    return ex.Message;
            }
        }

        private static OrderInput CheckIsEarlyBirdApplicable(int eventID, int noOfTickets, dynamic getPrice)
        {
            JAVADBEntities db = new JAVADBEntities();
            var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();
            
            var soldTicketsCount = db.tblTicketOrders.Where(e => e.EventID == eventID
                                        && e.Status == "SUCCESS"
                                        && e.PaymentStatus == "COMPLETED").ToList();

            decimal totalAmount = (noOfTickets * getPrice.Price);

            var input = new OrderInput
            {
                CustomerName = "",
                CustomerEmail = "",
                NoOfTickets = noOfTickets,
                Price = getPrice.Price,
                TotalAmount = totalAmount
            };
            //Logic to check Early Bird ticket allocation base on configurtion 
            //datewise ot seatwise
            if (evt.EarlyBirdTicketSelection == EarlyBirdTicketType.DATEWISE.ToString())
            {
                DateTime currentDate = System.DateTime.Today;
                if (currentDate >= evt.StartDate && currentDate <= evt.EndDate)
                {
                    input.TicketType = TicketType.EARLYBIRD.ToString();
                    input.Price = (decimal)getPrice.EBPrice;
                }
                else
                {
                    input.TicketType = TicketType.GENERAL.ToString();
                    input.Price = (decimal)getPrice.Price;
                }
            }
            else if (evt.EarlyBirdTicketSelection == EarlyBirdTicketType.SEATWISE.ToString())
            {
                int percentEBSeats = (int)evt.PercentEBSeats;
                int totalEBSeatsAllowed = (int)((evt.TicketStock * percentEBSeats) / 100);

                if (totalEBSeatsAllowed >= noOfTickets && evt.TicketsAvailable >= noOfTickets)
                {
                    input.TicketType = TicketType.EARLYBIRD.ToString();
                    input.Price = (decimal)getPrice.EBPrice;
                }
                else
                {
                    input.TicketType = TicketType.GENERAL.ToString();
                    input.Price = (decimal)getPrice.Price;
                }
            }
            input.TotalAmount = (input.NoOfTickets * input.Price);

            int totalSeatsAvailable = (int)(evt.TicketsAvailable - soldTicketsCount.Count);
            return input;
        }

        public static string RemoveSeat(int eventID, int seatID)
        {
            JAVADBEntities db = new JAVADBEntities();
            string sessionID = GetSessionID();

            //remove session associated seat data if this ticket is already booked
            //var getSeat = db.tblSeatSelections.Where(s => s.SeatID == seatID && s.SessionID == sessionID && s.OrderID == null && s.EventID == eventID).SingleOrDefault();
            var getSeat = db.tblSeatSelections.Where(s => s.SeatID == seatID && s.SessionID == sessionID && s.OrderID == null && s.EventID == eventID).ToList();

            if(getSeat != null)
            {
                foreach (var item in getSeat)
                {
                    db.tblSeatSelections.Remove(item);
                }
                db.SaveChanges();
            }
            return "OK";
        }
        

     
        //retrieves sessionid stored in the cookie
        public static string GetSessionID()
        {
            if (HttpContext.Current.Request.Cookies["Session"] != null)
            {
                return HttpContext.Current.Request.Cookies["Session"]["SessionID"];
            }
            else
            {
                HttpContext.Current.Response.Cookies["Session"]["SessionID"] = HttpContext.Current.Session.SessionID;

                HttpContext.Current.Response.Cookies["Session"].Expires = DateTime.Now.AddDays(3);

                return HttpContext.Current.Request.Cookies["Session"]["SessionID"];
            }  
        }

        public static bool IsLoggedIn()
        {
            if (HttpContext.Current.Request.Cookies["Customer"] != null)
                return true;
            else
                return false;
        }

        public static int GetCustomerID()
        {
            if (HttpContext.Current.Request.Cookies["Customer"] != null && HttpContext.Current.Request.Cookies["Customer"]["CustomerID"] != null)
                return Convert.ToInt32(HttpContext.Current.Request.Cookies["Customer"]["CustomerID"]);
            else
                return 0;
                   
        }

        public static string GetContent(string content, int length)
        {
            if (!string.IsNullOrEmpty(content) && length > 0)
            {
                int config_length = length;

                if (content.Length > config_length)
                    return content.Substring(0, config_length) + "... ";
                else
                    return content;
            }
            else
                return "N/A";

        }
    }
}