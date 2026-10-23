
using SorteFederation.BlobApi.Models;
using System.IO.Compression;

namespace SorteFederation.BlobApi.Services;

public interface IFileUploadService
{ 
    Task<bool> UploadZipFile(FileUploadDto model);
}

public class FileUploadService : IFileUploadService
{
    public FileUploadService()
    {

    }

   /* 
    public async Task<bool> ImageResizing(FileUploadDto model)
    {
        try
        {
            await using (var memoryStream = new MemoryStream())
            {
                await model.Files.CopyToAsync(memoryStream);

                await using var imageFile = new FileStream(model.FilePath, FileMode.Create, FileAccess.Write);

                memoryStream.WriteTo(imageFile);
            }

            using (var image = Image.Load(model.FilePath))
            {
                image.Mutate(x => x
                    .Resize(0, 512)
                    .EntropyCrop()
                );

                var target = Path.Combine(myFilepath, $"l_{fileName}.jpg");
                image.Save(target, new JpegEncoder());
            }

            using (var image = Image.Load(model.FilePath))
            {
                image.Mutate(x => x
                    .Resize(0, 256)
                    .EntropyCrop()
                );

                var target = Path.Combine(myFilepath, $"m_{fileName}.jpg");
                image.Save(target, new JpegEncoder());
            }

            using (var image = Image.Load(model.FilePath))
            {
                image.Mutate(x => x.Resize(0, 96).EntropyCrop());

                var target = Path.Combine(myFilepath, $"s_{fileName}.jpg");
                image.Save(target, new JpegEncoder());

            }

            return result;

        }
        catch (Exception ex)
        {
            return false;
        }
    }
   */

    public async Task<bool> UploadZipFile(FileUploadDto model)
    {
        var tempDirrectory = Path.Combine(Directory.GetCurrentDirectory(), "content", "temp");

        var imagePath = Path.Combine(tempDirrectory, $"{model.RelationId}.zip");
        var extractPath = Path.Combine(tempDirrectory, $"{model.RelationId}");
        var tileDefinitionPath = Path.Combine(extractPath, "metadata.json");

        try
        {
            if (File.Exists(imagePath))
                File.Delete(imagePath);

            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            await using (var memoryStream = new MemoryStream())
            {
                await model.Files.CopyToAsync(memoryStream);

                await using var imageFile = new FileStream(imagePath, FileMode.Create, FileAccess.Write);

                memoryStream.WriteTo(imageFile);
            }

            if (!File.Exists(imagePath))
                throw new Exception("image path not found");

            ZipFile.ExtractToDirectory(imagePath, extractPath);

            if (File.Exists(imagePath))
                File.Delete(imagePath);


            if (!File.Exists(tileDefinitionPath))
                throw new Exception("tile definition not found");

            // entity.TileAvailable = true;
            return true;

        }
        catch (Exception e)
        {
            if (File.Exists(imagePath))
                File.Delete(imagePath);

            if (Directory.Exists(extractPath))
                Directory.Delete(extractPath, true);

            return false;
        }

    }
}
