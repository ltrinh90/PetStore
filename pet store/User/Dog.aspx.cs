using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI.WebControls;

namespace pet_store.User
{
    public partial class Dog : System.Web.UI.Page
    {
        SqlConnection sqlConnection;
        SqlCommand sqlCommand;
        SqlDataAdapter sqlDataAdapter;

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
            sqlDataAdapter = new SqlDataAdapter();
            sqlConnection = new SqlConnection(Connection.GetConnectionString());
            var queryStringBuilder = new StringBuilder();

            queryStringBuilder.Append("SELECT   ");
            queryStringBuilder.Append("     c.*     ");
            queryStringBuilder.Append("FROM     ");
            queryStringBuilder.Append("     Categories AS c");

            sqlDataAdapter.SelectCommand = new SqlCommand(queryStringBuilder.ToString(), sqlConnection);

            var dataTable = new DataTable();
            sqlDataAdapter.Fill(dataTable);

            rCategory.DataSource = dataTable;
            rCategory.DataBind();
        }

        private void getProduct()
        {
            sqlDataAdapter = new SqlDataAdapter();
            sqlConnection = new SqlConnection(Connection.GetConnectionString());
            var queryStringBuilder = new StringBuilder();

            queryStringBuilder.Append("SELECT                                   ");
            queryStringBuilder.Append("     p.*                                 ");
            queryStringBuilder.Append("     , c.Name AS CategoryName            ");
            queryStringBuilder.Append("FROM     ");
            queryStringBuilder.Append("     Products AS p JOIN Categories AS c  ");
            queryStringBuilder.Append("     ON p.CategoryID = c.CategoryID      ");

            sqlDataAdapter.SelectCommand = new SqlCommand(queryStringBuilder.ToString(), sqlConnection);

            DataSet dataSet = new DataSet();
            sqlDataAdapter.Fill(dataSet);

            rProduct.DataSource = dataSet;
            rProduct.DataBind();
        }

        protected void rProduct_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (Session["userId"] != null)
            {
                bool isCartItemUpdated = false;
                int i = CheckExist(Convert.ToInt32(e.CommandArgument));
                if (i == 0)
                {
                    // Adding new item in cart
                    sqlConnection = new SqlConnection(Connection.GetConnectionString());
                    sqlConnection.Open();
                    using (var transaction = sqlConnection.BeginTransaction())
                    {
                        try
                        {
                            var queryStringBuilder = new StringBuilder();

                            queryStringBuilder.Append("INSERT                               ");
                            queryStringBuilder.Append("INTO Carts (ProductID, Quantity)  ");
                            queryStringBuilder.Append("VALUES (@ProductID, @Quantity)       ");

                            sqlCommand = new SqlCommand(queryStringBuilder.ToString(), sqlConnection);
                            sqlCommand.Parameters.AddWithValue("@ProductId", e.CommandArgument);
                            sqlCommand.Parameters.AddWithValue("@Quantity", 1);
                            transaction.Commit();
                            sqlCommand.ExecuteNonQuery();
                        }
                        catch (Exception ex)
                        {
                            Response.Write("<script>alert('Error - " + ex.Message + " ');</script>");
                            transaction.Rollback();
                        }
                        finally
                        {
                            sqlConnection.Close();
                        }

                    }

                }
                else
                {
                    //Add exitsting
                    Utils utils = new Utils();
                    isCartItemUpdated = utils.updateCartQuantity(i + 1, Convert.ToInt32(e.CommandArgument),
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
        int CheckExist(int productId)
        {
            var dataTable = new DataTable();
            sqlDataAdapter = new SqlDataAdapter();
            sqlConnection = new SqlConnection(Connection.GetConnectionString());
            sqlCommand = new SqlCommand();
            var queryStringBuilder = new StringBuilder();

            queryStringBuilder.Append("SELECT                           ");
            queryStringBuilder.Append("     p.Quantity                  ");
            queryStringBuilder.Append("FROM                             ");
            queryStringBuilder.Append("     Products AS p               ");
            queryStringBuilder.Append("WHERE                            ");
            queryStringBuilder.Append("     p.ProductID = @ProductID    ");

            sqlCommand.Parameters.AddWithValue("@ProductID", productId);
            sqlCommand.CommandText = queryStringBuilder.ToString();
            sqlCommand.Connection = sqlConnection;

            sqlDataAdapter.SelectCommand = sqlCommand;

            sqlDataAdapter.Fill(dataTable);

            int quantity = 0;

            if (dataTable.Rows.Count > 0)
            {
                quantity = Convert.ToInt32(dataTable.Rows[0]["Quantity"]);

            }
            return quantity;
        }
        //public string  LowerCase(object obj)
        //{
        //    return obj.ToString().ToLower();
        //}
    }
}