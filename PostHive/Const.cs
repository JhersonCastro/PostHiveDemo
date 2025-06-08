namespace PostHive
{
    /// <summary>
    /// The Const for the application
    /// </summary>
    public static class Const
    {
        public const string url = "http://posthive.tryasp.net";
        //public const string url = "http://localhost:5016";
        //public const string url = "https://posthive.me";

        public const int MaxFileSize = 5242880;
        public const byte MaxFileCount = 5;
        public const long MaxFileSizePost = 512*Mb;
        public const int Mb = 1024 * 1024;
    }
}
