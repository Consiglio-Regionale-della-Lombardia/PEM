namespace GeneraStampeJob
{
    public class ThreadWorkerModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string UrlAPI { get; set; }
        public string UrlCLIENT { get; set; }
        public string NumMaxTentativi { get; set; }
        public string CartellaLavoroTemporanea { get; set; }
        public string CartellaLavoroStampe { get; set; }
        public string RootRepository { get; set; }
        public string EmailFrom { get; set; }
        public string PDF_LICENSE { get; set; }
    }
}