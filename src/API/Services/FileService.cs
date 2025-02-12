namespace API.Services;

public class FileService
{
    public string GenerateName(string fileName)
    {
        return Guid.NewGuid().ToString() + '.' + fileName.Split('.').Last();   
    }

    public async Task SaveImage(IFormFile file, string name, string root)
    {
        var directory = Path.Combine(root, "static");
        Directory.CreateDirectory(directory);
        await using var stream = File.Create(Path.Combine(directory, name));
        await file.CopyToAsync(stream);
    }
}