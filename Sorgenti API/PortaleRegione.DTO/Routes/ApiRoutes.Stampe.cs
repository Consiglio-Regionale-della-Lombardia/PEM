namespace PortaleRegione.DTO.Routes
{
    public static partial class ApiRoutes
    {
        public static class Stampe
        {
            // api/stampe
            private const string Base = Root + "/stampe";

            public const string GetAll = Base + "/all";
            public const string Get = Base + "/{id}";
            public const string Delete = Base + "/{id}";
            public const string Download = Base + "/{id}/download";
            public const string Reset = Base + "/{id}/reset";
            public const string AddInfo = Base + "/{id}/info/add/{message}";
            public const string GetInfo = Base + "/{id}/info";
            public const string GetAllInfo = Base + "/all/info";
        }
    }
}