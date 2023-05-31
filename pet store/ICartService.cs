namespace pet_store.Service.Interface
{
    public interface ICartService
    {
        void UpdateQuantity(int quantity, int productId, int userId);

        int Count(int userID);
    }
}
