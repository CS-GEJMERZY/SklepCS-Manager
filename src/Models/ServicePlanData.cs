
namespace Plugin.Models
{
    public class ServicePlanData
    {
        public string Name { get; set; } = "Super VIP";
        public int Amount { get; set; } = 30;
        public string Unit { get; set; } = "days";
        public string SmsCost { get; set; } = "23.32";
        public string SmsMessage { get; set; } = "pukawka";
        public string SmsNumber { get; set; } = "123456";
        public string PlanUniqueCode { get; set; } = "s31";
        public int PlanValue { get; set; } = 2300; // plan price * 100
    }
}
