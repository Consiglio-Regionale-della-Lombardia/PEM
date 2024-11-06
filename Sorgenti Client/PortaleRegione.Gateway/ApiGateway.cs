namespace PortaleRegione.Gateway
{
    public class ApiGateway : IApiGateway
    {
        private string _token;

        public ApiGateway()
        {
            Persone = new PersoneGateway();
            Emendamento_Pubblico = new EMGateway_Pubblico();
            DASI_Pubblico = new DASIGateway_Pubblico();
        }

        public ApiGateway(string token)
        {
            _token = token;

            Sedute = new SeduteGateway(_token);
            Persone = new PersoneGateway(_token);
            Notifiche = new NotificheGateway(_token);
            Stampe = new StampeGateway(_token);
            Esporta = new EsportaGateway(_token);
            Emendamento = new EMGateway(_token);
            Atti = new AttiGateway(_token);
            Admin = new AdminGateway(_token);
            DASI = new DASIGateway(_token);
            Legislature = new LegislatureGateway(_token);
            Templates = new TemplatesGateway(_token);
        }

        public void SetToken(string token)
        {
            _token = token;
        }

        public ISeduteGateway Sedute { get; }
        public IPersoneGateway Persone { get; }
        public INotificheGateway Notifiche { get; }
        public IStampeGateway Stampe { get; }
        public IEsportaGateway Esporta { get; }
        public IEMGateway_Pubblico Emendamento_Pubblico { get; }
        public IDASIGateway_Pubblico DASI_Pubblico { get; }
        public IEMGateway Emendamento { get; }
        public IAttiGateway Atti { get; }
        public IAdminGateway Admin { get; }
        public IDASIGateway DASI { get; set; }
        public ILegislatureGateway Legislature { get; set; }
        public ITemplatesGateway Templates { get; set; }
    }
}