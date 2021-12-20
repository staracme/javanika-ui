﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FrontEnd.Models;
using Newtonsoft.Json;
using System.Net;
using System.Net.Mail;
using QRCoder;
using System.Drawing;
using SA.LA;
using System.Threading.Tasks;

namespace FrontEnd.Controllers
{
    public class PlaceOrderResponse
    {
        public string status { get; set; }
        public int ticketID { get; set; }
    }

    public class BookingSummaryController : Controller
    {
        public class PaymentResponse
        {
            public string orderID { get; set; }
            public string sys_orderID { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public decimal amount { get; set; }
            public string ticketType { get; set; }
            public decimal discountedAmount { get; set; }
            public int no_of_tickets { get; set; }
            public string status { get; set; }
            public string intent { get; set; }
            public int eventID { get; set; }
        }

        public class JSONResponse
        {
            public int eventID { get; set; }
        }
        private static readonly log4net.ILog logger
      = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        // GET: BookingSummary
        JAVADBEntities db = new JAVADBEntities();
        public ActionResult Index()
        {


            ExceptionHandlingEntity exceptionEntity = new ExceptionHandlingEntity()
            {
                Title = "Booking Summary Controller",
                Source = "SMTP Email()",
                Message = "asdf"
            };
            Task.Run(() => ExceptionLogger.ExceptionHandler(exceptionEntity));




            string sessionID = Common.GetSessionID();
            int eventID = Convert.ToInt32(Request["eventID"]);
            var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();
            ViewData["Event"] = evt;

            Session["CustomerID"] = (!string.IsNullOrEmpty(Request["customerID"]) ? Convert.ToInt32(Request["customerID"]) : Common.GetCustomerID());


            var couponsUsed = (db
                        .tblOrderCoupons
                        .Where(c => c.SessionID == sessionID && c.OrderID == null)
                        .ToList());



            var seats = db.tblSeatSelections.Where(t => t.OrderID == null && t.SessionID == sessionID && t.EventID == eventID).ToList();

            ViewData["Seats"] = string.Join(",", seats.Select(s => s.tblSeat.SeatNumber));


            return View(db.Database.SqlQuery<SelectedSeats>("select TierName, count(*) as 'NoOfSeats', Sum(Price) as 'TotalCost' from vw_seat_selection where EventID = " + eventID + " and sessionID = '" + sessionID + "' group by TierName").ToList());
        }

        public ActionResult Summary()
        {
            string sessionID = Common.GetSessionID();
            int eventID = Convert.ToInt32(Request["eventID"]);

            var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();
            ViewData["Event"] = evt;

            var couponsUsed = (db
                        .tblOrderCoupons
                        .Where(c => c.SessionID == sessionID && c.OrderID == null)
                        .SingleOrDefault());

            var items = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '" + sessionID + "' group by SessionID").ToList();

            var selectedSeatsTicketDetails = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, Price, EBPrice  from tblSeatSelections where EventID = " + eventID + " and SessionID = '" + sessionID + "' ").ToList();

            decimal amount = items.Sum(i => i.TotalPrice);
            amount = selectedSeatsTicketDetails.Sum(x => x.Price);
            items[0].ActualPrice = amount;

            var seats = db.tblSeatSelections.Where(t => t.OrderID == null && t.SessionID == sessionID && t.EventID == eventID).ToList();

            ViewData["Seats"] = string.Join(",", seats.Select(s => s.tblSeat.SeatNumber));

            if (couponsUsed != null)
            {
                if (couponsUsed.tblCoupon.DiscountIn == "Percentage")
                {
                    string tiers = couponsUsed.tblCoupon.tblCouponGroup.Tiers;

                    var seat_items = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '" + sessionID + "' AND SEATID IN(SELECT SEATID FROM tblSeat WHERE SeatRowID IN(SELECT SeatRowID FROM tblSeatRows WHERE BlockID IN(SELECT BLOCKID FROM tblEventLayoutBlocks WHERE TierID IN(" + tiers + ")))) group by SessionID").ToList();

                    decimal coupon_discount = Convert.ToDecimal(couponsUsed.tblCoupon.Discount);
                    decimal seat_amount = seat_items.Sum(i => i.TotalPrice);
                    decimal seat_discount = ((seat_amount * coupon_discount / 100));

                    decimal final_amount = amount - seat_discount;

                    if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                    {
                        decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                        decimal process_amount = ((final_amount * process_fees_perc / 100));

                        final_amount = (final_amount + process_amount);

                        ViewData["ProcessingPercentage"] = process_fees_perc;
                        ViewData["ProcessingFee"] = process_amount;
                    }

                    ViewData["DiscountAmount"] = seat_discount;
                    ViewData["Amount"] = final_amount;
                    ViewData["CouponUsed"] = couponsUsed;
                }
                else if (couponsUsed.tblCoupon.DiscountIn == "Value")
                {

                    string tiers = couponsUsed.tblCoupon.tblCouponGroup.Tiers;

                    decimal coupon_discount = (Convert.ToDecimal(couponsUsed.tblCoupon.Discount));

                    decimal final_amount = amount - coupon_discount;

                    if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                    {
                        decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                        decimal process_amount = ((final_amount * process_fees_perc / 100));
                        final_amount = (final_amount + process_amount);

                        ViewData["ProcessingPercentage"] = process_fees_perc;
                        ViewData["ProcessingFee"] = process_amount;
                    }


                    ViewData["DiscountAmount"] = coupon_discount;
                    ViewData["Amount"] = final_amount;
                    ViewData["CouponUsed"] = couponsUsed;
                }
            }
            else
            {
                OrderInput input = CheckIsEarlyBirdAndisTicketCountValid(eventID, selectedSeatsTicketDetails);
                if (input.TicketType == "EARLYBIRD")
                {
                    input.TotalAmount = selectedSeatsTicketDetails.Sum(x => x.EBPrice);
                }
                else
                {
                    input.TotalAmount = selectedSeatsTicketDetails.Sum(x => x.Price);
                }

                amount = input.TotalAmount;
                //to check current no of tickets does not exceed thre remaining stock of tickets
                //bool isTicketCountValid = CheckSeatsAvailability(eventID, selectedSeatsTicketDetails.Count);

                if (input.isTicketCountValid && evt.ProcessingFee != null && evt.ProcessingFee > 0)
                {
                    decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                    decimal process_amount = ((amount * process_fees_perc / 100));
                    amount = (amount + process_amount);
                    ViewData["ProcessingPercentage"] = process_fees_perc;
                    ViewData["ProcessingFee"] = process_amount;
                    TempData["TicketType"] = input.TicketType;
                    ViewData["isEarlyBird"] = (input.TicketType == "EARLYBIRD") ? 1 : 0;
                    items[0].Price = input.TotalAmount;
                    items[0].DiscountedAmount = items[0].ActualPrice - items[0].Price;
                    ViewData["DiscountedAmount"] = items[0].DiscountedAmount;
                }
                ViewData["Amount"] = amount;
            }

            if (Request.Cookies["IsRefreshed"] != null)
            {
                return Redirect("/Home");
            }
            else
            {
                Response.Cookies["IsRefreshed"].Value = "1";
                return View(items);
            }
        }

        //public bool CheckSeatsAvailability(int eventID, int no_of_tickets)
        //{
        //    //= Convert.ToInt32(Request["eventID"]);
        //    //= Convert.ToInt32(Request["no_of_tickets"]);

        //    var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();

        //    BookResponse response = new BookResponse();
        //    //check if stock is available
        //    var soldTicketsCount = db.tblTicketOrders.Where(e => e.EventID == eventID
        //                                        && e.Status == "SUCCESS"
        //                                        && e.PaymentStatus == "COMPLETED").ToList();

        //    int totalEBSeatsAllowed = Convert.ToInt32(evt.EBTickets);
        //    int totalSeatsAvailable = (totalEBSeatsAllowed - soldTicketsCount.Count);

        //    //to check current no of tickets does not exceed thre remaining stock of tickets
        //    bool isTicketCountValid = totalSeatsAvailable > no_of_tickets;
        //    return isTicketCountValid;
        //}
        private OrderInput CheckIsEarlyBirdAndisTicketCountValid(int eventID, List<SelectedSeatsV5> selectedSeatsTicketDetails)
        {
            var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();
            //decimal price = (decimal)evt.TicketPrice;
            //decimal price = selectedSeatsTicketDetails.Sum(e => e.Price);
            //decimal EBPrice = selectedSeatsTicketDetails.Sum(e => e.EBPrice);

            int noOfTickets = selectedSeatsTicketDetails.Count;

            var soldTicketsCount = db.tblTicketOrders.Where(e => e.EventID == eventID
                                        && e.Status == "SUCCESS"
                                        && e.PaymentStatus == "COMPLETED").ToList();

            //decimal totalAmount = (noOfTickets * price);

            var input = new OrderInput
            {
                CustomerName = "",
                CustomerEmail = "",
                NoOfTickets = noOfTickets,
            };

            int totalEBSeatsAllowed = Convert.ToInt32(evt.EBTickets);
            input.totalSeatsAvailable = (totalEBSeatsAllowed - soldTicketsCount.Count);

            //Logic to check Early Bird ticket allocation base on configurtion 
            //datewise ot seatwise
            if (evt.EarlyBirdTicketSelection == EarlyBirdTicketType.DATEWISE.ToString())
            {
                DateTime currentDate = System.DateTime.Today;
                if (currentDate >= evt.StartDate && currentDate <= evt.EndDate)
                {
                    if (totalEBSeatsAllowed >= noOfTickets && input.totalSeatsAvailable >= noOfTickets)
                    {
                        input.TicketType = TicketType.EARLYBIRD.ToString();
                    }
                    else
                    {
                        input.TicketType = TicketType.GENERAL.ToString();
                    }
                }
                else
                {
                    input.TicketType = TicketType.GENERAL.ToString();
                }
            }
            else if (evt.EarlyBirdTicketSelection == EarlyBirdTicketType.SEATWISE.ToString())
            {
                //int percentEBSeats = (int)evt.PercentEBSeats;
                //int totalEBSeatsAllowed = (int)((evt.TicketStock * percentEBSeats) / 100);
                if (totalEBSeatsAllowed >= noOfTickets && input.totalSeatsAvailable >= noOfTickets)
                {
                    input.TicketType = TicketType.EARLYBIRD.ToString();
                }
                else
                {
                    input.TicketType = TicketType.GENERAL.ToString();
                }
            }

            input.isTicketCountValid = (input.totalSeatsAvailable > input.NoOfTickets);
            return input;
        }
        public string BookUnconfirmedSeats()
        {
            try
            {
                string sessionID = Common.GetSessionID();
                int orderID = Convert.ToInt32(Request["orderID"]);
                int eventID = Convert.ToInt32(Request["eventID"]);


                //remove unconfirmed seats for this session id
                var unconfirmed_seats = db.tblSeatSelections.Where(t => t.EventID == eventID && t.SessionID == sessionID && t.OrderID == null).ToList();

                foreach (var seat in unconfirmed_seats)
                {
                    seat.OrderID = orderID;
                    seat.EventID = eventID;
                    seat.SessionID = "";
                    db.SaveChanges();
                }


                //remove unconfirmed coupons for this session id
                var unconfirmed_coupons = db.tblOrderCoupons.Where(t => t.tblCoupon.EventID == eventID && t.SessionID == sessionID && t.OrderID == null).ToList();

                foreach (var coupon in unconfirmed_coupons)
                {
                    coupon.OrderID = orderID;
                    coupon.SessionID = "";
                    db.SaveChanges();
                }
                return "OK";
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                    return ex.InnerException.Message;
                else
                    return ex.Message;
            }
        }


        public void DeleteCart(string json)
        {
            string sessionID = Common.GetSessionID();
            int eventID = Convert.ToInt32(Request.Form["eventID"]);

            var unconfirmed_seats = db.tblSeatSelections.Where(t => t.EventID == eventID && t.SessionID == sessionID && t.OrderID == null).ToList();

            foreach (var seat in unconfirmed_seats)
                db.tblSeatSelections.Remove(seat);

            var unconfirmed_coupons = db.tblOrderCoupons.Where(t => t.tblCoupon.EventID == eventID && t.SessionID == sessionID && t.OrderID == null).ToList();

            foreach (var coupon in unconfirmed_coupons)
                db.tblOrderCoupons.Remove(coupon);

            db.SaveChanges();
        }

        public JsonResult PlaceOrder(PaymentResponse response)
        {
            //logger.Info("PlaceOrder starts here.");
            FinalPlaceOrderResponse resp = new FinalPlaceOrderResponse();

            try
            {
                string sessionID = Common.GetSessionID();
                int eventID = Convert.ToInt32(Request["eventID"]);

                string name = Request["name"];
                string email = Request["email"];
                string mobile = Request["mobile"];

                decimal discount_amount = 0;
                string coupon_code = "";
                decimal coupon_value = 0;

                var seats = db.tblSeatSelections.Where(t => t.OrderID == null && t.SessionID == sessionID && t.EventID == eventID).ToList();

                var coupon_used = db.tblOrderCoupons.Where(t => t.OrderID == null && t.SessionID == sessionID && t.tblCoupon.EventID == eventID).SingleOrDefault();

                //to check the last order serial number
                var orders = db.tblTicketOrders.Where(os => os.Status == "SUCCESS" && os.PaymentStatus == "COMPLETED" && os.EventID == eventID).ToList();
                int serialNo = (orders.Count() > 0 ? Convert.ToInt32((orders.OrderByDescending(os => os.OrderID).Take(1).SingleOrDefault().OrderNo) + 1) : 1);

                var evt = db.tblEvents.Where(e => e.EventID == eventID).SingleOrDefault();

                tblTicketOrder order = new tblTicketOrder();
                order.EventID = eventID;
                order.NoOfTickets = seats.Count();
                order.Name = name;
                order.Email = email;
                order.Mobile = mobile;
                order.Amount = Convert.ToDecimal(Request["amount"]);
                order.TicketType = (Convert.ToInt32(Request["ticketType"]) == 1) ? "EARLYBIRD" : "GENERAL";
                order.ActualPrice = Convert.ToDecimal(Request["actualPrice"]);
                order.DiscountAmount = Convert.ToDecimal(Request["discountedAmount"]);
                order.PriceAfterDiscount = Convert.ToDecimal(Request["priceAfterDiscount"]);
                order.CreatedDate = DateTime.Now;
                order.OrderNo = serialNo;
                order.Status = "SUCCESS";
                order.PaypalOrderID = response.orderID;
                order.PaymentStatus = response.status;
                order.Intent = response.intent;
                db.tblTicketOrders.Add(order);
                db.SaveChanges();
                foreach (var seat in seats)
                {
                    seat.OrderID = order.OrderID;
                    seat.SessionID = "";
                }
                db.SaveChanges();

                string DataContents = System.IO.File.ReadAllText(Server.MapPath("~/Templates/ticketseats.html"));

                var items = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '' and ORDERID = " + order.OrderID + " group by SessionID").ToList();


                string Receipt = System.IO.File.ReadAllText(Server.MapPath("~/Templates/receiptseats.html"));
                string type = "";


                if (coupon_used != null)
                {
                    coupon_used.SessionID = "";
                    coupon_used.OrderID = order.OrderID;

                    decimal amount = items.Sum(i => i.TotalPrice);

                    coupon_code = coupon_used.tblCoupon.CouponCode;

                    if (coupon_used.tblCoupon.DiscountIn == "Percentage")
                    {
                        string tiers = coupon_used.tblCoupon.tblCouponGroup.Tiers;

                        var seat_items = db.Database.SqlQuery<SelectedSeatsV5>("select SessionID, count(*) as 'NoOfSeats', sum(Price) as 'TotalPrice' from tblSeatSelections where EventID = " + eventID + " and SessionID = '' and orderid = " + order.OrderID + " AND SEATID IN(SELECT SEATID FROM tblSeat WHERE SeatRowID IN(SELECT SeatRowID FROM tblSeatRows WHERE BlockID IN(SELECT BLOCKID FROM tblEventLayoutBlocks WHERE TierID IN(" + tiers + ")))) group by SessionID").ToList();

                        coupon_value = Convert.ToDecimal(coupon_used.tblCoupon.Discount);

                        decimal seat_amount = seat_items.Sum(i => i.TotalPrice);

                        discount_amount = ((seat_amount * coupon_value / 100));

                        decimal final_amount = amount - discount_amount;

                        order.DiscountAmount = discount_amount;

                        if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                        {

                            decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                            decimal process_amount = ((final_amount * process_fees_perc / 100));

                            order.ProcessingFee = process_amount;
                            db.SaveChanges();

                            Receipt = Receipt.Replace("[perc]", Decimal.Round(process_fees_perc, 0).ToString() + "%");
                            Receipt = Receipt.Replace("[service_charge]", Decimal.Round(process_amount, 2).ToString());
                        }
                        else
                        {
                            Receipt = Receipt.Replace("[service_charge]", "0");
                            Receipt = Receipt.Replace("[prec]", "");
                        }
                        db.SaveChanges();
                    }
                    else if (coupon_used.tblCoupon.DiscountIn == "Value")
                    {

                        string tiers = coupon_used.tblCoupon.tblCouponGroup.Tiers;

                        coupon_value = Convert.ToDecimal(coupon_used.tblCoupon.Discount);

                        discount_amount = (Convert.ToDecimal(coupon_used.tblCoupon.Discount));

                        decimal final_amount = amount - discount_amount;

                        order.DiscountAmount = discount_amount;

                        if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                        {

                            decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                            decimal process_amount = ((final_amount * process_fees_perc / 100));
                            order.ProcessingFee = process_amount;


                            Receipt = Receipt.Replace("[perc]", Decimal.Round(process_fees_perc, 0).ToString() + "%");
                            Receipt = Receipt.Replace("[service_charge]", Decimal.Round(process_amount, 2).ToString());
                        }
                        else
                        {
                            Receipt = Receipt.Replace("[service_charge]", "0");
                            Receipt = Receipt.Replace("[perc]", "");
                        }

                        db.SaveChanges();
                    }


                    coupon_used.SessionID = "";
                    coupon_used.OrderID = order.OrderID;
                    coupon_used.tblCoupon.UsedDate = DateTime.Now;
                    coupon_used.tblCoupon.IsUsed = true;
                    db.SaveChanges();
                }
                else
                {
                    if (evt.ProcessingFee != null && evt.ProcessingFee > 0)
                    {
                        decimal amount = items.Sum(i => i.TotalPrice);
                        decimal process_fees_perc = Convert.ToDecimal(evt.ProcessingFee);
                        decimal process_amount = ((amount * process_fees_perc / 100));
                        order.ProcessingFee = process_amount;
                        db.SaveChanges();
                        Receipt = Receipt.Replace("[perc]", Decimal.Round(process_fees_perc, 0).ToString() + "%");
                        Receipt = Receipt.Replace("[service_charge]", Decimal.Round(process_amount, 2).ToString());
                    }
                    else
                    {
                        Receipt = Receipt.Replace("[service_charge]", "0");
                        Receipt = Receipt.Replace("[perc]", "");
                    }
                }

                int orderID = order.OrderID;

                //QRCodeGenerator qrGenerator = new QRCodeGenerator();
                //QRCodeData qrCodeData = qrGenerator.CreateQrCode(order.OrderID.ToString(), QRCodeGenerator.ECCLevel.Q);
                //QRCode qrCode = new QRCode(qrCodeData);
                //Bitmap qrCodeImage = qrCode.GetGraphic(20);
                //qrCodeImage.Save("C:\\Websites\\DevFrontEnd\\QRCodes\\" + orderID.ToString() + ".png");
                //order.QRCode = orderID.ToString() + ".png";
                logger.Info("b=> GenerateQRCode ");
                var QRCode = Utilities.GenerateQRCode(order, orderID);
                order.QRCode = QRCode;
                db.SaveChanges();
                logger.Info("a=> GenerateQRCode ");

                //var orderObject = db.tblTicketOrders.Where(t => t.OrderID == orderID).SingleOrDefault();
                //var evtObject = db.tblEvents.Where(e => e.EventID == order.EventID).SingleOrDefault();

                //Utilities.SetEmailContent(orderObject, evtObject, order.OrderID);
                logger.Info("b=> smtp name ");
                string username = System.Configuration.ConfigurationManager.AppSettings["SMTPUsername"].ToString();
                string password = System.Configuration.ConfigurationManager.AppSettings["SMTPPassword"].ToString();
                logger.Info("a=> smtp name ");
                try
                {
                    if (order.NoOfTickets > 0)
                    {
                        var booked_seats = db.tblSeatSelections.Where(s => s.OrderID == order.OrderID).ToList();
                        var o = db.tblTicketOrders.Where(os => os.OrderID == orderID).SingleOrDefault();
                        DataContents = DataContents.Replace("[srno]", o.OrderNo.ToString());
                        DataContents = DataContents.Replace("[event_name]", evt.EventName.ToString());
                        DataContents = DataContents.Replace("[image_name]", evt.EventBanner.ToString());
                        DataContents = DataContents.Replace("[tickets]", string.Join(",", booked_seats.Select(s => s.tblSeat.SeatNumber)));
                        DataContents = DataContents.Replace("[address]", evt.tblVenue.VenueName);
                        DataContents = DataContents.Replace("[date]", Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy") + " " + evt.ShowTime);
                        DataContents = DataContents.Replace("[image]", order.QRCode);

                        SmtpClient smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com", // smtp server address here…
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            Credentials = new System.Net.NetworkCredential(username, password),
                            Timeout = 30000,
                        };
                        try
                        {
                            MailMessage message = new MailMessage(username, order.Email, "Your Tickets", DataContents);
                            message.IsBodyHtml = true;
                            smtp.Send(message);
                        }
                        catch (Exception ex)
                        {
                            logger.Info("b=> Your Tickets >" + ex.Message.ToString());
                            logger.Info("b=> Your Tickets >" + ex.InnerException.Message.ToString());
                        }
                        tblMailLog log = new tblMailLog();
                        log.OrderID = order.OrderID;
                        log.Email = order.Email;
                        log.SentDate = DateTime.Now;
                        log.MailContent = DataContents;
                        db.tblMailLogs.Add(log);
                        db.SaveChanges();

                        decimal amountBeforeDiscount = Decimal.Round(Convert.ToDecimal(items.Sum(s => s.TotalPrice)), 2);

                        order.AmountPerTicket = (amountBeforeDiscount / order.NoOfTickets);
                        db.SaveChanges();

                        Receipt = Receipt.Replace("[name]", order.Name.ToString());
                        Receipt = Receipt.Replace("[order_no]", order.OrderID.ToString());

                        Receipt = Receipt.Replace("[sr_no]", order.OrderNo.ToString());
                        Receipt = Receipt.Replace("[type]", type);
                        Receipt = Receipt.Replace("[individual_price]", Convert.ToDecimal(amountBeforeDiscount / order.NoOfTickets).ToString());
                        Receipt = Receipt.Replace("[amount]", Decimal.Round(amountBeforeDiscount, 2).ToString());
                        Receipt = Receipt.Replace("[total_amount]", order.Amount.ToString());
                        Receipt = Receipt.Replace("[event_name]", evt.EventName.ToString());
                        Receipt = Receipt.Replace("[no_of_tickets]", booked_seats.Count().ToString());
                        Receipt = Receipt.Replace("[address]", evt.tblVenue.VenueName);
                        Receipt = Receipt.Replace("[date_time]", Convert.ToDateTime(order.CreatedDate).ToString("dddd, dd MMMM yyyy hh:mm tt"));
                        Receipt = Receipt.Replace("[event_date_time]", Convert.ToDateTime(evt.EventDate).ToString("dddd, dd MMMM yyyy") + " " + evt.ShowTime);
                        Receipt = Receipt.Replace("[image]", order.QRCode);
                        Receipt = Receipt.Replace("[seat_nos]", string.Join(",", booked_seats.Select(s => s.tblSeat.SeatNumber)));

                        if (discount_amount > 0 && coupon_code != "")
                        {
                            if (coupon_used.tblCoupon.DiscountIn == "Percentage")
                            {
                                Receipt = Receipt.Replace("[coupon_code]", coupon_code + "-" + coupon_value + "%");
                                Receipt = Receipt.Replace("[discount]", Decimal.Round(discount_amount, 2).ToString());
                            }
                            else if (coupon_used.tblCoupon.DiscountIn == "Value")
                            {
                                Receipt = Receipt.Replace("[coupon_code]", coupon_code);
                                Receipt = Receipt.Replace("[discount]", Decimal.Round(discount_amount, 2).ToString());
                            }
                        }
                        else
                            Receipt = Receipt.Replace("[class_name]", "hide_discount");


                        try
                        {
                            MailMessage message1 = new MailMessage(username, order.Email, "Your Receipt", Receipt);
                            message1.IsBodyHtml = true;
                            smtp.Send(message1);
                        }
                        catch (Exception ex)
                        {
                            logger.Info("b=> Your Receipt >" + ex.Message.ToString());
                            logger.Info("b=> Your Receipt >" + ex.InnerException.Message.ToString());
                        }

                        try
                        {
                            MailMessage message2 = new MailMessage(username, "javanikabooking@gmail.com", "New Order", Receipt);
                            message2.IsBodyHtml = true;
                            smtp.Send(message2);
                        }
                        catch (Exception ex)
                        {
                            logger.Info("b=> New Order >" + ex.Message.ToString());
                            logger.Info("b=> New Order >" + ex.InnerException.Message.ToString());
                        }

                        resp.orderID = order.OrderID.ToString();
                        resp.status = "OK";
                    }
                    else
                    {
                        string emailDistList = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["EmailDistList"]);
                        string[] emails = emailDistList.Split(',').ToArray();
                        try
                        {
                            SmtpClient smtp = new SmtpClient
                            {
                                Host = "smtp.gmail.com", // smtp server address here…
                                Port = 587,
                                EnableSsl = true,
                                DeliveryMethod = SmtpDeliveryMethod.Network,
                                Credentials = new System.Net.NetworkCredential(username, password),
                                Timeout = 30000,
                            };

                            foreach (var e in emails)
                            {
                                MailMessage message1 = new MailMessage(username, e, "Zero Seats Order Received", "You have received a zero seats order. Order number is " + order.OrderID);
                                message1.IsBodyHtml = true;
                                smtp.Send(message1);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Info("b=> Zero Seats Order Received >" + ex.Message.ToString());
                            logger.Info("b=> Zero Seats Order Received >" + ex.InnerException.Message.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    resp.status = ex.InnerException.Message;
                    ExceptionHandlingEntity exceptionEntity = new ExceptionHandlingEntity()
                    {
                        Title = "Booking Summary Controller",
                        Source = "SMTP Email()",
                        Message = ex.Message,
                        Exception = ex
                    };
                    Task.Run(() => ExceptionLogger.ExceptionHandler(exceptionEntity));
                }
            }
            catch (Exception ex)
            {
                resp.status = ex.InnerException.Message;
                ExceptionHandlingEntity exceptionEntity = new ExceptionHandlingEntity()
                {
                    Title = "Booking Summary Controller",
                    Source = "PlaceOrder()",
                    Message = ex.Message,
                    Exception = ex
                };
                Task.Run(() => ExceptionLogger.ExceptionHandler(exceptionEntity));
            }
            return Json(resp, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GuestBooking()
        {
            tblCustomer customer = new tblCustomer();
            customer.FirstName = Request["txtName"];
            customer.LastName = Request["txtLastName"];
            customer.CreatedDate = DateTime.Now;
            db.tblCustomers.Add(customer);
            db.SaveChanges();

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("Status", "OK");
            data.Add("CustomerID", customer.CustomerID.ToString());

            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}