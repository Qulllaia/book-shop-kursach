namespace models
{
    public class Order
    {
        public long orderid { get; set; }
        public long itemid { get; set; }
        public long userid { get; set; }
        public string status { get; set; }
    }
}
