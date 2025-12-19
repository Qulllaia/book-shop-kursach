namespace dtos
{
    public record CreateOrderRequest(string status, int userid, int itemid, int orderid);
}
