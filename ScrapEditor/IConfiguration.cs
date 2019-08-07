namespace ScrapEditor
{
    public interface IConfiguration
    {
        string DevID { get; set; }
        string DevPassword { get; set; }
        string SoftName { get; set; }
    }
}