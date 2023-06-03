using System;

using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;

namespace pet_store.User
{
    public partial class Dog : System.Web.UI.Page
    {
        private readonly CartService _cartService = new CartService();

        private SqlConnection con = new SqlConnection(Connection.GetConnectionString());

        SqlCommand cmd;
        SqlDataAdapter sda;
        DataTable dt;

        private readonly CartService cartService = new CartService();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                getCategories();
                getProduct();
            }
        }
        private void getCategories()
        {
            con = new SqlConnection(Connection.GetConnectionString());
            cmd = new SqlCommand("Category_Crud", con);
            cmd.Parameters.AddWithValue("@Action", "ACTIVECAT");
            cmd.CommandType = CommandType.StoredProcedure;
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            rCategory.DataSource = dt;
            rCategory.DataBind();
        }

        private void getProduct()
        {
            con = new SqlConnection(Connection.GetConnectionString());
            var sql = new StringBuilder();
            sql.Append("SELECT                                  ");
            sql.Append("    p.*                                 ");
            sql.Append("    , c.Name AS CategoryName            ");
            sql.Append("FROM                                    ");
            sql.Append("    Products AS p JOIN Categories AS c  ");
            sql.Append("    ON c.CategoryId = p.CategoryID      ");
            sql.Append("WHERE                                   ");
            sql.Append("    p.IsActive = 1                      ");

            cmd = new SqlCommand(sql.ToString(), con);
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            //var ds = new DataSet();
            sda.Fill(dt);
            rProduct.DataSource = dt;
            rProduct.DataBind();
        }

        protected void rProduct_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (Session["userId"] != null)
            {
                int i = CheckExisted(Convert.ToInt32(e.CommandArgument));
                if (i == 0)
                {
                    con.Open();
                    var tx = con.BeginTransaction();
                    var sql = new StringBuilder();

                    sql.Append("insert                                      ");
                    sql.Append("into Carts(ProductId, Quantity, UserId)     ");
                    sql.Append("values (@ProductId, @Quantity, @UserId)     ");

                    cmd = new SqlCommand(sql.ToString(), con, tx);

                    cmd.Parameters.AddWithValue("@ProductId", e.CommandArgument);
                    cmd.Parameters.AddWithValue("@Quantity", 1);
                    cmd.Parameters.AddWithValue("@UserId", Session["userId"]);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        Response.Write("<script>alert('Error - " + ex.Message + " ');</script>");
                    }
                    finally
                    {
                        con.Close();
                    }
                }
                else
                {
                    //Add exitsting
                    _cartService.UpdateQuantity(i + 1, Convert.ToInt32(e.CommandArgument),
                        Convert.ToInt32(Session["userId"]));
                }
                lblMsg.Visible = true;
                lblMsg.Text = "Item addes successfully in your cart!";
                lblMsg.CssClass = "alert alert-success";
                Response.AddHeader("REFRESH", "1;URL=Cart.aspx");
            }
            else
            {
                Response.Redirect("Login.aspx");
            }
        }
        int CheckExisted(int productId)
        {
            var sql = new StringBuilder();
            var _dt = new DataTable();
            con = new SqlConnection(Connection.GetConnectionString());

            sql.Append("select                          ");
            sql.Append("    c.*                         ");
            sql.Append("from                            ");
            sql.Append("    Carts as c                  ");
            sql.Append("where                           ");
            sql.Append("    c.productid = @ProductId    ");
            sql.Append("    and c.userid = @UserId      ");

            cmd = new SqlCommand(sql.ToString(), con);

            cmd.Parameters.AddWithValue("@ProductId", productId);
            cmd.Parameters.AddWithValue("@UserId", Session["userId"]);

            sda = new SqlDataAdapter(cmd);
            sda.Fill(_dt);

            if (_dt.Rows.Count > 0 && _dt.Rows[0]["Quantity"] is DBNull)
            {
                if (_dt.Rows[0]["Quantity"] is DBNull)
                {
                    return 0;
                }

                return Convert.ToInt32(_dt.Rows[0]["Quantity"]);
            }
            else
            {
                return 0;
            }

        }
    }
}