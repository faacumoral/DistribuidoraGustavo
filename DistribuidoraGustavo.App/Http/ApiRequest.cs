namespace DistribuidoraGustavo.App.Http
{
    public class ApiRequest
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public object Parameter { get; set; }
        public bool CheckAuth { get; set; } = true;

        private ApiRequest(string method, string url, object parameter, bool checkAuth)
        {
            this.Method = method;
            this.Url = url;
            this.Parameter = parameter;
            this.CheckAuth = checkAuth;
        }

        public static ApiRequest BuildGet(string url, object parameter = null, bool checkAuth = true)
            => new ("GET", url, parameter, checkAuth);

        public static ApiRequest BuildPost(string url, object parameter = null, bool checkAuth = true)
            => new ("POST", url, parameter, checkAuth);

        public static ApiRequest BuildDelete(string url, object parameter = null, bool checkAuth = true)
            => new ("DELETE", url, parameter, checkAuth);
    }
}
