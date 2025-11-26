using Pronia_MVC.Models;
using Pronia_MVC.Utilities.Enums;
using System.IO;

namespace Pronia_MVC.Utilities.Extensions
{
    public static class FileValidator
    {
      public static bool ValidateType(this IFormFile file,string type)
        {
            return file.ContentType.Contains(type);
        }
        public static bool ValidateSize(this IFormFile file,FileSize fileSize,int size)
        {
            switch (fileSize)
            {
                case FileSize.KB:
                    return file.Length < size * 1024;
                    case FileSize.MB:
                    return file.Length < size * 1024*1024;
                    case FileSize.GB:
                    return file.Length < size * 1024*1024*1024;

                  
            }
            return false;
        }
        public static async Task<string> CreateFileAsync(this IFormFile file,params string[] roots)
        {
            string fileName = string.Concat(Guid.NewGuid(), Path.GetExtension(file.FileName));
            string path = string.Empty;
            for(int i=0; i<roots.Length; i++)
            {
                path = Path.Combine(path, roots[i]);
            }
            path=Path.Combine(path, fileName);
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }
        public static void DeleteFile(this string deletedName, params string[] roots)
        {
            string path = string.Empty;
            for (int i = 0; i < roots.Length; i++)
            {
                path = Path.Combine(path, roots[i]);
            }
            path = Path.Combine(path, deletedName);
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
