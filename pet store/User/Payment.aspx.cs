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

            transaction = con.BeginTransaction();
            var sql = new StringBuilder();
            sql.Append("insert into payment(name, cardno, expirydate, cvvno, address, paymentmode)  ");
            sql.Append("values (@name, @cardno, @expirydate, @cvv, @address, @paymentmethod)        ");
            cmd = new SqlCommand()
            {
                CommandText = sql.ToString(),
                Transaction = transaction,
                Connection = con
            };

            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@cardNo", cardNo);
            cmd.Parameters.AddWithValue("@expirydate", expiryDate);
            cmd.Parameters.AddWithValue("@cvv", cvv);
            cmd.Parameters.AddWithValue("@address", address);
            cmd.Parameters.AddWithValue("@paymentmethod", paymentMethod);
            try
            {
                cmd.ExecuteNonQuery();

                var _sqlDataAdapter = new SqlDataAdapter
                {
                    SelectCommand = new SqlCommand("select max(p.paymentid) as PaymentID from payment as p", con, transaction)
                };
                var ds = new DataSet();
                _sqlDataAdapter.Fill(ds);

                paymentId = (int)ds.Tables[0].Rows[0]["PaymentID"];

                var _sql = new StringBuilder();
                _sql.Append("select c.productid, p.name, p.imageurl, p.price, c.quantity        ");
                _sql.Append("from carts as c join products as p on p.productid = c.productid    ");
                _sql.Append("where c.userid = @userid                                           ");
                cmd = new SqlCommand(_sql.ToString(), con, transaction);
                cmd.Parameters.AddWithValue("@userid", Session["userId"]);
                var _ds = new DataSet();
                var sqlDataAdapter = new SqlDataAdapter(cmd);
                
                sqlDataAdapter.Fill(_ds);

                for (var i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    var dr = _ds.Tables[0].Rows[i];

                    productId = (int)dr["ProductId"];
                    quantity = (int)dr["Quantity"];

                    // Update Product Quantity
                    UpdateQuantity(productId, quantity, transaction, con);
                    
                    // Delete Cart Item
                    DeleteCartItem(productId, transaction, con);

                    dt.Rows.Add(Utils.GetUniqueId(), productId, quantity, (int)Session["userId"], "Pending",
                        paymentId, Convert.ToDateTime(DateTime.Now));
                }

                // Save order details
                if (dt.Rows.Count > 0)
                {
                    cmd = new SqlCommand("Save_Orders", con, transaction);
                    cmd.Parameters.AddWithValue("@tblOrders", dt);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                lblMsg.Visible = true;
                lblMsg.Text = "Your item orderes successful!!!";
                lblMsg.CssClass = "alert alert-success";
                Response.AddHeader("REFRESH", "1;URL=Invoice.aspx?id=" + paymentId);
            }
            catch (Exception e)
            {
                transaction.Rollback();
                Response.Write("<script><alert('" + e.Message + "');</script>");
            }
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
            var sqlDataAdapter = new SqlDataAdapter(cmd);
            var ds = new DataSet();
            sqlDataAdapter.Fill(ds);
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i ++)
                {
                    var dr1 = ds.Tables[0].Rows[i];

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