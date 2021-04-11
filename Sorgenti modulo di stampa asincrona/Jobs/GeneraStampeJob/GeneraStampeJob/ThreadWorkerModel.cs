namespace GeneraStampeJob
{
    public class ThreadWorkerModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string urlAPI { get; set; }
        public string urlCLIENT { get; set; }
        public string CartellaStampeLink { get; set; }
        public string NumMaxTentativi { get; set; }
        public string CartellaLavoroTemporanea { get; set; }
        public string CartellaLavoroStampe { get; set; }
        public string RootRepository { get; set; }
        public string EmailFrom { get; set; }
    }
}