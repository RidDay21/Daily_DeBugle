using Microsoft.AspNetCore.Components.Forms;

namespace DailyDeBugle.Helpers
{
    public static class FileUploadHelper
    {
        public static async Task<string> UploadFileAsync(
            IBrowserFile file, 
            string subfolder,
            long maxFileSize = 5 * 1024 * 1024) // 5MB
        {
            try
            {
                Console.WriteLine($"Загрузка файла: {file.Name}");
                
                // Простая валидация
                if (file.Size > maxFileSize)
                {
                    throw new Exception($"Файл слишком большой ({file.Size / 1024 / 1024}MB). Максимум: {maxFileSize / 1024 / 1024}MB");
                }
                
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                {
                    throw new Exception($"Недопустимый формат файла. Разрешено: {string.Join(", ", allowedExtensions)}");
                }
                
                // Создаем уникальное имя
                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadsDir = Path.Combine("wwwroot", "uploads", subfolder);
                
                // Создаем директорию
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }
                
                var filePath = Path.Combine(uploadsDir, fileName);
                
                // ПРОСТОЙ способ сохранения
                await using var fs = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(maxFileSize).CopyToAsync(fs);
                
                var resultPath = $"/uploads/{subfolder}/{fileName}";
                Console.WriteLine($"Файл сохранен по пути: {resultPath}");
                
                return resultPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки файла: {ex.Message}");
                throw;
            }
        }
        
        public static void DeleteFile(string filePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath) && filePath.StartsWith("/uploads/"))
                {
                    var fullPath = Path.Combine("wwwroot", filePath.TrimStart('/'));
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
            }
            catch
            {
                // Игнорируем ошибки при удалении
            }
        }
    }
}