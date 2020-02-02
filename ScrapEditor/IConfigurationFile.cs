namespace ScrapEditor
{
    public interface IConfigurationFile
    {
        string DevID { get; set; }
        string DevPassword { get; set; }
        string SoftName { get; set; }
        string DBLink { get; set; }
        string DBCertPath { get; set; }
        string DBName { get; set; }
        string DefaultUser { get; set; }
        string DefaultPassword { get; set; }
    }
}