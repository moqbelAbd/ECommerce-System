namespace EcommerceSystem.Helpers
{
    public static class ImageUploadHelper
    {
        // Save an uploaded image under wwwroot/images/{folder} and return its web path
        public static async Task<string> SaveImageAsync(IFormFile imageFile, string folder, IWebHostEnvironment environment)
        {
            var uploadsFolder = Path.Combine(environment.WebRootPath, "images", folder);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return $"/images/{folder}/{fileName}";
        }
    }
}
