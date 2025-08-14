namespace Muhasebe.Business.Models.AppModel
{
    public class ExchangeRate
    {
        public string BaseCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public decimal Rate { get; set; }
        public DateTime UpdateTime { get; set; }
        public decimal Change { get; set; }
        public bool IsUp { get; set; }
    }
}
