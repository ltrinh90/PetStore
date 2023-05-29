using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace pet_store.User
{
    public partial class Login : System.Web.UI.Page
    {
        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter sda;
        DataTable dt;
        protected void Page_Load(object sender, EventArgs e)
        {
            if(Session["userId"] != null)
            {
                Response.Redirect("Default.aspx");
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            if(txtUsername.Text.Trim() == "Admin" && txtPassword.Text.Trim() == "123")
            {
                Session["admin"] = txtUsername.Text.Trim();
                Response.Redirect("../Admin/Dashboard.aspx");
            }
            else
            {
                con = new SqlConnection(Connection.GetConnectionString());
                var queryStringBuilder = new StringBuilder();

                queryStringBuilder.Append("SELECT                           ");
                queryStringBuilder.Append("     u.*                         ");
                queryStringBuilder.Append("FROM                             ");
                queryStringBuilder.Append("     Users AS u                  ");
                queryStringBuilder.Append("WHERE                            ");
                queryStringBuilder.Append("     u.Username = @Username      ");
                queryStringBuilder.Append("     AND u.Password  = @Password ");

                cmd = new SqlCommand(queryStringBuilder.ToString(), con);
                cmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                cmd.Parameters.AddWithValue("@Password", txtPassword.Text.Trim());
                sda = new SqlDataAdapter(cmd);
                
                dt = new DataTable();
                sda.Fill(dt);

                if(dt.Rows.Count == 1)
                {
                    Session["username"] = txtUsername.Text.Trim();
                    Session["userId"] = dt.Rows[0]["userId"];
                    Response.Redirect("Default.aspx");
                }
                else
                {
                    lblMsg.Visible = true;
                    lblMsg.Text = "Invalid Credentials..!";
                    lblMsg.CssClass = "alert alert-danger";
                }
            }    
        }
    }
}