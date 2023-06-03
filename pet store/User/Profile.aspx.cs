using System;

using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace pet_store.User
{
    public partial class Profile : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter sda;
        DataTable dt;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["userId"] == null)
                {
                    Response.Redirect("Login.aspx");
                }
                else
                {
                    getUserDetails();
                    getOrderHistory();
                }
            }
        }
        void getUserDetails()
        {
            con = new SqlConnection(Connection.GetConnectionString());
            cmd = new SqlCommand("User_Crud", con);
            cmd.Parameters.AddWithValue("@Action", "SELECT4PROFILE");
            cmd.Parameters.AddWithValue("@UserId", Session["userId"]);
            cmd.CommandType = CommandType.StoredProcedure;
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            rUserProfile.DataSource = dt;
            rUserProfile.DataBind();
            if (dt.Rows.Count == 1)
            {
                Session["name"] = dt.Rows[0]["Name"].ToString();
                Session["email"] = dt.Rows[0]["Email"].ToString();
                Session["imageUrl"] = dt.Rows[0]["ImageUrl"].ToString();
                Session["createDate"] = dt.Rows[0]["CreateDate"].ToString();

            }
        }
        void getOrderHistory()
        {
            con = new SqlConnection(Connection.GetConnectionString());
            var sql = new StringBuilder();

            sql.Append("select													                ");
            sql.Append("    o.OrderNo											                ");
            sql.Append("    , o.Status 											                ");
            sql.Append("    , o.OrderDate										                ");
            sql.Append("    , o.PaymentId							                            ");
            sql.Append("    , o.Quatity								                            ");
            sql.Append("    , product.Name											            ");
            sql.Append("    , product.Price											            ");
            sql.Append("    , (product.Price * o.Quatity) as TotalPrice				            ");
            sql.Append("    , payment.PaymentMode as PaymentMethod	                            ");
            sql.Append("    , payment.CardNo 							                        ");
            sql.Append("from 									                                ");
            sql.Append("    Orders as o join Payment as payment					                ");
            sql.Append("        on payment.PaymentId = o.PaymentId join Products as product	    ");
            sql.Append("        on product.ProductId = o.ProductId                              ");
            sql.Append("where									                                ");
            sql.Append("    o.UserId = @UserId					                                ");

            cmd = new SqlCommand(sql.ToString(), con);

            cmd.Parameters.AddWithValue("@UserId", Session["userId"]);
            sda = new SqlDataAdapter(cmd);
            var ds = new DataSet();
            sda.Fill(ds);

            ds.Tables[0].Columns.Add("SrNo", typeof(int));

            if (ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ds.Tables[0].Rows[i]["SrNo"] = i;
                }
            }
            else
            {
                rPurchaseHistory.FooterTemplate = new CustomTemplate(ListItemType.Footer);
            }

            rPurchaseHistory.DataSource = ds;
            rPurchaseHistory.DataBind();
        }

        private sealed class CustomTemplate : ITemplate
        {
            private ListItemType ListItemType { get; set; }
            public CustomTemplate(ListItemType type)
            {
                ListItemType = type;
            }
            public void InstantiateIn(Control container)
            {
                if (ListItemType == ListItemType.Footer)
                {
                    var footer = new LiteralControl("<tr><td><b>Hi!!! Why not order dog for you.</b><a href='Dog.aspx' class='badge badge-info ml-2'>Click to Order</a></td></tr></tbody></table>");
                    container.Controls.Add(footer);
                }
            }
        }
    }
}