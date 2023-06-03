﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace pet_store
{

    public class CartService
    {
        private SqlConnection con = new SqlConnection(Connection.GetConnectionString());
        private SqlCommand cmd;
        private SqlDataAdapter sda;

        public bool UpdateQuantity(int quantity, int productId, int userId)
        {
            con.Open();
            bool isUpdated = false;
            var tx = con.BeginTransaction();
            var sql = new StringBuilder();

            sql.Append("update Carts                ");
            sql.Append("set                         ");
            sql.Append("    Quantity = @Quantity    ");
            sql.Append("where                       ");
            sql.Append("    ProductId = @ProductId  ");
            sql.Append("    and UserId = @UserId    ");

            cmd = new SqlCommand(sql.ToString(), con, tx);

            cmd.Parameters.AddWithValue("@ProductId", productId);
            cmd.Parameters.AddWithValue("@Quantity", quantity);
            cmd.Parameters.AddWithValue("@UserId", userId);

            try
            {
                cmd.ExecuteNonQuery();
                tx.Commit();
                isUpdated = true;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                isUpdated = false;
                System.Web.HttpContext.Current.Response.Write("<script>alert('Error - " + ex.Message + " ');</script>");
            }
            finally
            {
                con.Close();
            }
            return isUpdated;
        }
        public int Count(int userId)
        {
            con = new SqlConnection(Connection.GetConnectionString());
            cmd = new SqlCommand("Cart_Crud", con);
            cmd.Parameters.AddWithValue("@Action", "SELECT");
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.CommandType = CommandType.StoredProcedure;
            sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            return dt.Rows.Count;
        }
    }
}