using pet_store.Service.Interface;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace pet_store.Service.Implement
{
    public class CartService : ICartService
    {
        public void UpdateQuantity(int quantity, int productId, int userId)
        {
            var con = new SqlConnection(Connection.GetConnectionString());
            var cmd = new SqlCommand("Cart_Crud", con);

            cmd.Parameters.AddWithValue("@Action", "UPDATE");
            cmd.Parameters.AddWithValue("@ProductId", productId);
            cmd.Parameters.AddWithValue("@Quantity", quantity);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.CommandType = CommandType.StoredProcedure;
            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Web.HttpContext.Current.Response.Write("<script>alert('Error - " + ex.Message + " ');</script>");
            }
            finally
            {
                con.Close();
            }
        }
        public int Count(int userId)
        {
            var con = new SqlConnection(Connection.GetConnectionString());
            var cmd = new SqlCommand("Cart_Crud", con);
            cmd.Parameters.AddWithValue("@Action", "SELECT");
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.CommandType = CommandType.StoredProcedure;
            var sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            return dt.Rows.Count;
        }
    }
}