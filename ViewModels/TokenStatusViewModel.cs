using tpfred2.Models;

namespace tpfred2.ViewModels
{
    public class TokenStatusViewModel : BaseViewModel
    {
        public string Date { get; }
        public int RequestsToday { get; }
        public long BytesToday { get; }
        public string Plan { get; }
        public string PlanExpires { get; }
        public int DailyReqLimit { get; }
        public long DailyBytesLimit { get; }
        public string Status { get; }

        public TokenStatusViewModel(StatusData data)
        {
            Date = data.date;
            RequestsToday = data.requests_today;
            BytesToday = data.bytes_today;
            Plan = data.plan;
            PlanExpires = data.plan_expires ?? "";
            DailyReqLimit = data.daily_requests_limit;
            DailyBytesLimit = data.daily_bytes_limit;
            Status = data.status;
        }
    }
}
