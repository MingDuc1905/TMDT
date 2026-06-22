using System.Web.Mvc;

namespace ShipFood.Controllers
{
    public class TestController : Controller
    {
        public ActionResult Index()
        {
            return Content("✅ Server is running! ShipFood ASP.NET MVC 5 on .NET 4.7.2");
        }

        public ActionResult DbTest()
        {
            try
            {
                using (var db = new ShipFood.Models.dbFoodyEntities())
                {
                    int tables = db.tbDanhMucs.Count();
                    return Content("✅ Database connected! Categories count: " + tables);
                }
            }
            catch (System.Exception ex)
            {
                return Content("❌ Database error: " + ex.Message + " | Inner: " + (ex.InnerException != null ? ex.InnerException.Message : "none"));
            }
        }
    }
}

