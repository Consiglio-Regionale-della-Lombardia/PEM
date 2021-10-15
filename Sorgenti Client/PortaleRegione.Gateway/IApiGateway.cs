namespace PortaleRegione.Gateway
{
    public interface IApiGateway
    {
        IAdminGateway Admin { get; }
        IAttiGateway Atti { get; }
        IEMGateway Emendamento { get; }
        IEMGateway_Pubblico Emendamento_Pubblico { get; }
        IEsportaGateway Esporta { get; }
        INotificheGateway Notifiche { get; }
        IPersoneGateway Persone { get; }
        ISeduteGateway Sedute { get; }
        IStampeGateway Stampe { get; }

        void SetToken(string token);
    }
}