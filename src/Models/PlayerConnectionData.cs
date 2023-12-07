public class PlayerConnectionData
{
    public string AuthType { get; set; } = string.Empty;
    public string Flags { get; set; } = string.Empty;
    public int Immunity { get; set; } = 0;
    public DateTime End { get; set; } = DateTime.MinValue;
}