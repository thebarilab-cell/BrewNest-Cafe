using Microsoft.AspNetCore.Mvc;

namespace BrewNestCafe.Controllers
{
    public class AboutController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.OurStory = "Founded in 2024, BrewNest Cafe started as a small passion project by coffee enthusiasts who believed in serving the perfect cup of coffee. Our mission is to create a cozy nest where everyone feels at home.";
            ViewBag.OurValues = new List<string>
            {
                "Quality Ingredients",
                "Sustainable Sourcing",
                "Community Focus",
                "Exceptional Service"
            };
            ViewBag.TeamMembers = new List<TeamMember>
            {
                new TeamMember { Name = "Sarah Johnson", Position = "Head Barista", Image = "team1.jpg" },
                new TeamMember { Name = "Michael Chen", Position = "Pastry Chef", Image = "team2.jpg" },
                new TeamMember { Name = "Emma Wilson", Position = "Manager", Image = "team3.jpg" }
            };

            return View();
        }
    }

    public class TeamMember
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Image { get; set; }
    }
}