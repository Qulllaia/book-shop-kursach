namespace controller
{
    static class OrderStatus
    {
        public const string ORDERED = "ORDERED";
        public const string DELIVERING = "DELIVERING";
        public const string WAPPR = "WAPPR";
        public const string CANCELLED = "CANCELLED";

        public static bool isValid(string inputStatus)
        {
            var statuses = typeof(OrderStatus)
                .GetFields(
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
                )
                .Select(f => (string)f.GetValue(null))
                .ToList();

            return statuses.Exists(s => s.Equals(inputStatus));
        }
    }
}
