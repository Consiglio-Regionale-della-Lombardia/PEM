namespace PortaleRegione.Gateway
{
    public class ApiGateway : IApiGateway
    {
        private string _token;

        public ApiGateway()
        {

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
            Emendamento_Pubblico = new EMGateway_Pubblico();
            Atti = new AttiGateway(_token);
            Admin = new AdminGateway(_token);
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
        public IEMGateway Emendamento { get; }
        public IAttiGateway Atti { get; }
        public IAdminGateway Admin { get; }
    }
}