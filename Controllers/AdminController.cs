using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BrewNestCafe.Data;
using BrewNestCafe.Models;
using Microsoft.AspNetCore.Authorization;

namespace BrewNestCafe.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var stats = new DashboardStats
            {
                TotalMenuItems = await _context.MenuItems.CountAsync(),
                TotalFeedbacks = await _context.Feedbacks.CountAsync(),
                PendingFeedbacks = await _context.Feedbacks.CountAsync(f => !f.IsApproved),
                UnreadMessages = await _context.ContactMessages.CountAsync(m => !m.IsRead),
                TotalGalleryItems = await _context.Galleries.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync()
            };

            ViewBag.Stats = stats;
            return View();
        }

        // ... rest of the code
    

    // Add this class in the same file or create separate file
    public class DashboardStats
    {
        public int TotalMenuItems { get; set; }
        public int TotalFeedbacks { get; set; }
        public int PendingFeedbacks { get; set; }
        public int UnreadMessages { get; set; }
        public int TotalGalleryItems { get; set; }
        public int TotalCategories { get; set; }
}

        // Menu CRUD
        public async Task<IActionResult> MenuCRUD()
        {
            var menuItems = await _context.MenuItems.Include(m => m.Category).ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(menuItems);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMenuItem(
     string Name,
     string Description,
     decimal Price,
     int CategoryId,
     IFormFile ImageFile
 )
        {
            try
            {
                string imagePath = null;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    imagePath = "/images/" + fileName;
                }

                var menuItem = new MenuItem
                {
                    Name = Name,
                    Description = Description,
                    Price = Price,
                    CategoryId = CategoryId,
                    ImageUrl = imagePath, // ✅ image saved
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.MenuItems.Add(menuItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Item added successfully!";
                return RedirectToAction("MenuCRUD");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("MenuCRUD");
            }
        }


        // Add this method in AdminController for MenuCRUD
        [HttpGet]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (menuItem == null)
            {
                return NotFound();
            }

            return Json(new
            {
                id = menuItem.Id,
                name = menuItem.Name,
                description = menuItem.Description,
                price = menuItem.Price,
                imageUrl = menuItem.ImageUrl,
                categoryId = menuItem.CategoryId,
                isAvailable = menuItem.IsAvailable
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMenuItem(int id, MenuItem menuItem, IFormFile? imageFile)
        {
            if (id != menuItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingItem = await _context.MenuItems.FindAsync(id);
                    if (existingItem == null)
                    {
                        return NotFound();
                    }

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "menu");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        existingItem.ImageUrl = "/images/menu/" + uniqueFileName;
                    }

                    existingItem.Name = menuItem.Name;
                    existingItem.Description = menuItem.Description;
                    existingItem.Price = menuItem.Price;
                    existingItem.CategoryId = menuItem.CategoryId;
                    existingItem.IsAvailable = menuItem.IsAvailable;

                    _context.Update(existingItem);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Menu item updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MenuItemExists(menuItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return RedirectToAction(nameof(MenuCRUD));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem != null)
            {
                _context.MenuItems.Remove(menuItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Menu item deleted successfully!";
            }

            return RedirectToAction(nameof(MenuCRUD));
        }

        // Feedback Management
        public async Task<IActionResult> FeedbackList()
        {
            var feedbacks = await _context.Feedbacks
                .OrderByDescending(f => f.SubmittedAt)
                .ToListAsync();

            return View(feedbacks);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveFeedback(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                feedback.IsApproved = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Feedback approved!";
            }

            return RedirectToAction(nameof(FeedbackList));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Feedback deleted!";
            }

            return RedirectToAction(nameof(FeedbackList));
        }

        // Gallery Management
        public async Task<IActionResult> GalleryManager()
        {
            var galleryItems = await _context.Galleries
                .OrderByDescending(g => g.UploadedAt)
                .ToListAsync();

            return View(galleryItems);
        }

        [HttpPost]
        public async Task<IActionResult> UploadToGallery(IFormFile imageFile, string? title, string? description)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "gallery");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                var galleryItem = new Gallery
                {
                    ImageUrl = "/images/gallery/" + uniqueFileName,
                    Title = title,
                    Description = description,
                    UploadedAt = DateTime.UtcNow
                };

                _context.Galleries.Add(galleryItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Image uploaded to gallery!";
            }

            return RedirectToAction(nameof(GalleryManager));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGalleryItem(int id)
        {
            var galleryItem = await _context.Galleries.FindAsync(id);
            if (galleryItem != null)
            {
                _context.Galleries.Remove(galleryItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Gallery item deleted!";
            }

            return RedirectToAction(nameof(GalleryManager));
        }

        private bool MenuItemExists(int id)
        {
            return _context.MenuItems.Any(e => e.Id == id);
        }

        // Contact Messages View
        public async Task<IActionResult> ViewMessages()
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.Date)
                .ToListAsync();

            return View(messages);
        }

        // Mark as Read
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Message marked as read!";
            }

            return RedirectToAction(nameof(ViewMessages));
        }

        // Delete Message
        [HttpPost]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);
            if (message != null)
            {
                _context.ContactMessages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Message deleted successfully!";
            }

            return RedirectToAction(nameof(ViewMessages));
        }

        // Get Message Details for Modal (Optional)
        [HttpGet]
        public async Task<IActionResult> GetMessage(int id)
        {
            var message = await _context.ContactMessages.FindAsync(id);

            if (message == null)
            {
                return NotFound();
            }

            return PartialView("_MessageDetails", message);
        }

    }
}