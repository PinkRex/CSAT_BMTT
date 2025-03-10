namespace CSAT_BMTT.Models
{
    public class RequestModel
    {
        public int Id { get; set; }
        public string CitizenIdentificationNumber { get; set; }
        public string CitizenName { get; set; }
        public string Status { get; set; }
    }

    public class RequestViewModel
    {
        public List<RequestModel> RequestSentList { get; set; }
        public List<RequestModel> RequestRecievedList { get; set; }
    }
}
