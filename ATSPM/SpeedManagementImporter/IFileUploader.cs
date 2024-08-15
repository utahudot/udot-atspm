namespace SpeedManagementImporter
{
    public interface IFileUploader
    {
        public Task FileUploaderAsync(string filePath);
    }
}
