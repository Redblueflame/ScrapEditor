namespace ScrapEditor.ScrapLogic
{
    public class RegionalInfo<T>
    {
        public RegionalInfo(string region, T value)
        {
            Region = region;
            Value = value;
        }
        public string Region { get; set; }
        public T Value { get; set; }
    }
}