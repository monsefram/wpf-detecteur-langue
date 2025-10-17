namespace tpfred2.Models
{
    public class DetectionItem
    {
        public string LongName { get; set; } = "";
        public double Confidence { get; set; }
        public bool IsReliable { get; set; }
        public override string ToString() => LongName;
    }

    public class LangItem { public string code { get; set; } = ""; public string name { get; set; } = ""; }

    public class StatusData
    {
        public string date { get; set; } = "";
        public int requests_today { get; set; }
        public long bytes_today { get; set; }
        public string plan { get; set; } = "";
        public string? plan_expires { get; set; }
        public int daily_requests_limit { get; set; }
        public long daily_bytes_limit { get; set; }
        public string status { get; set; } = "";
    }
}
