using PastaOrderfood.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PastaOrderfood.Account;
using System.Data.SqlTypes;
using PastaOrderfood.App_Class;

namespace PastaOrderfood.Controllers
{
    public class HomeController : Controller
    {
        PastaOrderEntities db = new PastaOrderEntities();
        List<OrderDetail> cart;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // GET: Pasta
        public ActionResult Menu()
        {
            var pastas = db.Pastas.Include("Categories").OrderBy(m => m.pasta_sort).ToList();

            ViewBag.cate = db.Categories.OrderBy(m => m.category_id).ToList();
            return View(pastas);
        }
        [HttpPost]
        public JsonResult Menu(string ItemId)
        {
            int check;
            bool conversionSuccessful = int.TryParse(ItemId, out check);
            int acount = 0;
            var product = db.Pastas.Where(m => m.rowid == check).ToList();

            if (Session["cart"] == null )
            {
                cart = new List<OrderDetail>();
                cart.Add(new OrderDetail()
                {
                    itemId = product[acount].rowid,
                    quantity = 1
                });
                Session["cart"] = cart;
            }
            else
            {
                cart = (List<OrderDetail>)Session["cart"];
                cart.Add(new OrderDetail()
                {
                    itemId = product[acount].rowid,
                    quantity = 1
                });
                Session["cart"] = cart;
                
            }
            CartItem.CartCount = cart.Count();

            return Json(new { Success = true, Counter = cart.Count(), S = Session["cart"] }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult GetCategoriesSearchData(string SearchValue)
        {
            List<Pastas> tBLPosts = new List<Pastas>();
            if (SearchValue == "all")
            {
                tBLPosts = db.Pastas.OrderBy(m=>m.pasta_sort).ToList();
                return Json(tBLPosts, JsonRequestBehavior.AllowGet);
            }
            try
            {
                int Id = Convert.ToInt32(SearchValue);
                tBLPosts = db.Pastas.Where(x => x.category_id == Id || SearchValue == null).ToList();
            }
            catch (FormatException)
            {
                Console.WriteLine("{0} is not Int", SearchValue);
            }
            return Json( tBLPosts , JsonRequestBehavior.AllowGet);
        }


        public ActionResult DeleteSession()
        {
            Session.Remove("cart");
            return RedirectToAction("Menu");
        }

        public ActionResult Cart()
        {
            if (Session["cart"] != null)
            {
                cart = (List<OrderDetail>)Session["cart"];
                ViewBag.token = true;
            }
            else
            {
                cart = new List<OrderDetail>();
                ViewBag.token = false;
            }

            List<Pastas> result = new List<Pastas>();
            List<Cart> cartStore = new List<Cart>();

            foreach (var item in cart)
            {
                result = db.Pastas.Where(m => m.rowid == item.itemId).ToList();
                cartStore.Add(new Cart()
                {
                    pasta_name = result[0].pasta_name,
                    itemId = result[0].rowid,
                    pasta_img = result[0].pasta_img,
                    unitprice = (int)result[0].pasta_price,
                    quantity = 1
                });
                Session["cartStore"] = cartStore;
            }

            return View(cartStore);
        }

        [HttpPost]
        public ActionResult Cart(List<string> q)
        {
            int i = 0;
            cart = (List<OrderDetail>)Session["cart"];
            List<Pastas> result = new List<Pastas>();
            List<Cart> cartStore = new List<Cart>();
            foreach (var item in cart)
            {
                
                result = db.Pastas.Where(m => m.rowid == item.itemId).ToList();
                cartStore.Add(new Cart()
                {
                    pasta_name = result[0].pasta_name,
                    itemId = result[0].rowid,
                    pasta_img = result[0].pasta_img,
                    unitprice = (int)result[0].pasta_price,
                    quantity = int.Parse(q[i])
                });
                i =+ 1;
                Session["cartStore"] = cartStore;
            }

            return RedirectToAction("Next");
        }





        public ActionResult Next()
        {
            Session["total"] = "";
            int total = 0;
            List<Cart> cartStore = new List<Cart>();
            cartStore = (List<Cart>)Session["cartStore"];
            ViewBag.cartStore = cartStore;

            foreach (var item in cartStore)
            {
                total += item.quantity * item.unitprice;
            }
            Session["total"] = total;
            string UserNo = UserAccount.UserNo;
            var user = db.Users.Where(m => m.mno == UserNo).ToList();
            return View(user);

        }

        [HttpPost]
        public ActionResult Next(string name ,string fn ,string phone,string location_1, string location_2, string email,string payFn, string isLogin)
        {
            OrderDetail od = new OrderDetail();
            List<Cart> cartStore = new List<Cart>();
            cartStore = (List<Cart>)Session["cartStore"];
            int totalA = 0;
            foreach (var item in cartStore)
            {
                totalA += item.quantity * item.unitprice;
            }
            Order O = new Order();
            O.order_name = name;
            O.order_phone = phone;
            O.order_location = location_1 + location_2;
            O.order_email = email;
            O.order_date = DateTime.Now;
            O.order_status = "接單中";
            O.order_fn = fn;
            O.order_payFn = payFn;
            O.order_total = totalA;
            O.isLogin = int.Parse(isLogin);
            db.Order.Add(O);
            db.SaveChanges();
            var order = db.Order.OrderByDescending(m => m.order_id).FirstOrDefault();
            int b = order.order_id;
            foreach (var item in cartStore)
            {

                od.orderid = b;
                od.itemId = item.itemId;
                od.quantity =item.quantity;
                db.OrderDetail.Add(od);
                db.SaveChanges();
            }
            return RedirectToAction("thinkOrder");


        }
        public ActionResult thinkOrder()
        {
            Session.Remove("total");
            Session.Remove("cartStore");
            Session.Remove("cart");
            Session.Remove("CartCount");
            return View();
        }

        public ActionResult ClearCart()
        {
            Session.Remove("cart");
            Session.Remove("cartStore");
            Session.Remove("CartCount");
            return RedirectToAction("Cart");
        }
    }
}