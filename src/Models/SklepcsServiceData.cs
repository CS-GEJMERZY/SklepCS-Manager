
namespace SklepCSManager;
public class ServiceSmsData
{
    public string Name { get; set; } = "Super VIP";
    public int Count { get; set; } = 30;
    public string Unit { get; set; } = "days";
    public string SmsCodeValue { get; set; } = "23.32";
    public string SmsMessage { get; set; } = "pukawka";
    public string SmsNumber { get; set; } = "123456";
    public string PlanCode { get; set; } = "s31";

    // That is actually plan price * 100%
    // PlanValue/100 is real price
    public int PlanValue { get; set; } = 2300;
}
