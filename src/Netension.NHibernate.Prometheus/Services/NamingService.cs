namespace Netension.NHibernate.Prometheus.Services
{
    internal static class NamingService
    {
        private static string _prefix;

        public static void SetPrefix(string prefix)
        {
            _prefix = prefix;
        }

        public static string GetFullName(string name)
        {
            return $"{_prefix}_{name}".TrimStart('_');
        }
    }
}
