using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace pet_store.User
{
    public partial class Payment : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataReader dr, dr1;
        DataTable dt;
        SqlTransaction transaction = null;
        string _name = string.Empty;
        string _cardNo = string.Empty;
        string _expiryDate = string.Empty;
        string _cvv = string.Empty;
        string _address = string.Empty; string _paymentMethod = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["userId"] == null)
                {
                    Response.Redirect("Login.aspx");
                }
            }
        }

        protected void lbCardSubmit_Click(object sender, EventArgs e)
        {
            _name = txtName.Text.Trim();
            _cardNo = txtCardNo.Text.Trim();
            _cardNo = string.Format("************{0}", txtCardNo.Text.Trim().Substring(12, 4));
            _expiryDate = txtExpMonth.Text.Trim() + "/" + txtExpYear.Text.Trim();
            _cvv = txtCvv.Text.Trim();
            _address = txtAddress.Text.Trim();
            _paymentMethod = "card";
            if (Session["userId"] != null)
            {
                OrderPayment(_name, _cardNo, _expiryDate, _cvv, _address, _paymentMethod);
            }
            else
            {
                Response.Redirect("Login.aspx");
            }
        }

        protected void lbCodSubmit_Click(object sender, EventArgs e)
        {
            _address = txtCODAddress.Text.Trim();
            _paymentMethod = "cod";
            if (Session["userId"] != null)
            {
                OrderPayment(_name, _cardNo, _expiryDate, _cvv, _address, _paymentMethod);
            }
            else
            {
                Response.Redirect("Login.aspx");
            }

        }
        void OrderPayment(string name, string cardNo, string expiryDate, string cvv, string address, string paymentMethod)
        {
            int paymentId; int productId; int quantity;
            dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[7] {
            new DataColumn("OrderNo", typeof(string)),
            new DataColumn("ProductId", typeof(int)),
            new DataColumn("Quantity", typeof(int)),
            new DataColumn("UserId", typeof(int)),
            new DataColumn("Status", typeof(string)),
            new DataColumn("PaymentId", typeof(int)),
            new DataColumn("OrderDate", typeof(DateTime)),
            });
            con = new SqlConnection(Connection.GetConnectionString());
            con.Open();
            #region Sql Transaction
            transaction = con.BeginTransaction();
            var queryStringBuilder = new StringBuilder();

            //INSERT INTO dbo.Payment(Name, CardNo, ExpiryDate,CvvNo,Address,PaymentMethod)
            //VALUES(@Name, @CardNo, @ExpiryDate, @Cvv, @Address, @PaymentMethod)
            queryStringBuilder.Append("INSERT INTO                      ");
            queryStringBuilder.Append("     Payment (                   ");
            queryStringBuilder.Append("         Name                    ");
            queryStringBuilder.Append("         , CardNo                ");
            queryStringBuilder.Append("         , ExpiryDate            ");
            queryStringBuilder.Append("         , CvvNo                 ");
            queryStringBuilder.Append("         , Address               ");
            queryStringBuilder.Append("         , PaymentMode           ");
            queryStringBuilder.Append("     )                           ");
            queryStringBuilder.Append("VALUES                           ");
            queryStringBuilder.Append("     (                           ");
            queryStringBuilder.Append("         @Name                   ");
            queryStringBuilder.Append("         , @CardNo               ");
            queryStringBuilder.Append("         , @ExpiryDate           ");
            queryStringBuilder.Append("         , @Cvv                  ");
            queryStringBuilder.Append("         , @Address              ");
            queryStringBuilder.Append("         , @PaymentMethod        ");
            queryStringBuilder.Append("     )                           ");

            cmd = new SqlCommand(queryStringBuilder.ToString(), con, transaction);
            cmd.Parameters.AddWithValue("@Name", name);
            cmd.Parameters.AddWithValue("@CardNo", cardNo);
            cmd.Parameters.AddWithValue("@ExpiryDate", expiryDate);
            cmd.Parameters.AddWithValue("@Cvv", cvv);
            cmd.Parameters.AddWithValue("@Address", address);
            cmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
            //cmd.Parameters.Add("@InsertedId", SqlDbType.Int);
            //cmd.Parameters["InsertedId"].Direction = ParameterDirection.Output;
            try
            {
                cmd.ExecuteNonQuery();
                transaction.Commit();

                //paymentId = Convert.ToInt32(cmd.Parameters["@InsertedId"].Value);
                var sqlDataAdapter = new SqlDataAdapter(new SqlCommand("SELECT MAX(PaymentID) AS PaymentID FROM Payment", con, transaction));
                var ds = new DataSet();
                sqlDataAdapter.Fill(ds);

                paymentId = int.Parse((string)ds.Tables[0].Rows[0]["PaymentID"]);

                #region Getting Cart Item's
                cmd = new SqlCommand("Cart_Crud", con, transaction);
                cmd.Parameters.AddWithValue("@Action", "SELECT");
                cmd.Parameters.AddWithValue("@UserId", Session["userId"]);
                cmd.CommandType = CommandType.StoredProcedure;
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    productId = (int)dr["ProductId"];
                    quantity = (int)dr["Quantity"];
                    // Update Product Quantity
                    UpdateQuantity(productId, quantity, transaction, con);
                    // Delete Cart Item
                    DeleteCartItem(productId, transaction, con);
                    // Delete Cart Item End
                    dt.Rows.Add(Utils.GetUniqueId(), productId, quantity, (int)Session["userId"], "Pending",
                        paymentId, Convert.ToDateTime(DateTime.Now));
                }
                dr.Close();
                #endregion Getting Cart Item's

                #region Order Details
                if (dt.Rows.Count > 0)
                {
                    cmd = new SqlCommand("Save_Orders", con, transaction);
                    cmd.Parameters.AddWithValue("@tblOrders", dt);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }
                #endregion Order Details

                lblMsg.Visible = true;
                lblMsg.Text = "Your item orderes successful!!!";
                lblMsg.CssClass = "alert alert-success";
                Response.AddHeader("REFRESH", "1;URL=Invoice.aspx?id=" + paymentId);
            }
            catch (Exception e)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception ex)
                {
                    Response.Write("<script><alert('" + ex.Message + "');</script>");
                }
            }
            #endregion Sql Transaction 
            finally
            {
                con.Close();
            }
        }
        void UpdateQuantity(int _productId, int _quantity, SqlTransaction sqlTransaction, SqlConnection sqlConnection)
        {
            int dbQuantity;
            cmd = new SqlCommand("Product_Crud", sqlConnection, sqlTransaction);
            cmd.Parameters.AddWithValue("@Action", "GETBYID");
            cmd.Parameters.AddWithValue("@ProductId", _productId);
            cmd.CommandType = CommandType.StoredProcedure;
            try
            {
                dr1 = cmd.ExecuteReader();
                while (dr1.Read())
                {
                    dbQuantity = (int)dr1["Quantity"];
                    if (dbQuantity > _quantity && dbQuantity > 2)
                    {
                        dbQuantity = dbQuantity - _quantity;
                        cmd = new SqlCommand("Product_Crud", sqlConnection, sqlTransaction);
                        cmd.Parameters.AddWithValue("@Action", "QTYUPDATE");
                        cmd.Parameters.AddWithValue("@Quantity", dbQuantity);
                        cmd.Parameters.AddWithValue("@ProductId", _productId);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteNonQuery();
                    }
                }
                dr1.Close();
            }
            catch (Exception exe)
            {
                Response.Write("<script>alert('" + exe.Message + "');</script>");
            }
            finally
            {

            }
        }
        void DeleteCartItem(int _productId, SqlTransaction sqlTransaction, SqlConnection sqlConnection)
        {
            cmd = new SqlCommand("Cart_Crud", sqlConnection, sqlTransaction);
            cmd.Parameters.AddWithValue("@Action", "DELETE");
            cmd.Parameters.AddWithValue("@ProductId", _productId);
            cmd.Parameters.AddWithValue("@UserId", Session["userId"]);
            cmd.CommandType = CommandType.StoredProcedure;
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception exe)
            {
                Response.Write("<script>alert('" + exe.Message + "');</script>");
            }
        }

    }
}