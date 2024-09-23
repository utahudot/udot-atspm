namespace SpeedManagementImporter.Services
{
    public interface IFileUploader
    {
        public Task FileUploaderAsync(string filePath);
    }
}
