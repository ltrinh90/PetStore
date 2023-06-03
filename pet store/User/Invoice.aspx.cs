using iTextSharp.text;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace pet_store.User
{
    public partial class Invoice : System.Web.UI.Page
    {

        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter sda;
        DataTable dt;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["userId"] != null)
                {
                    if (Request.QueryString["id"] != null)
                    {
                        rOrderItem.DataSource = GetOrderDetails();
                        rOrderItem.DataBind();
                    }
                }
                else
                {
                    Response.Redirect("Login.aspx");
                }
            }
        }
        DataTable GetOrderDetails()
        {
            con = new SqlConnection(Connection.GetConnectionString());
            var sql = new StringBuilder();

            sql.Append("select													");
            sql.Append("    ROW_NUMBER() over (order by (select 1)) as [SrNo]	");
            sql.Append("    , o.OrderNo											");
            sql.Append("    , p.Name											");
            sql.Append("    , p.Price											");
            sql.Append("    , o.Quatity as Quantity								");
            sql.Append("    , (p.Price * o.Quatity) as TotalPrice				");
            sql.Append("    , o.OrderDate										");
            sql.Append("    , o.Status 											");
            sql.Append("from													");
            sql.Append("    Orders o 											");
            sql.Append("    inner join Products p 								");
            sql.Append("        on p.ProductId = o.ProductId 					");
            sql.Append("where													");
            sql.Append("    o.PaymentId = @PaymentId 							");
            sql.Append("    and o.UserId = @UserId								");

            cmd = new SqlCommand(sql.ToString(), con);
            
            cmd.Parameters.AddWithValue("@Action", "INVOICBYID");
            cmd.Parameters.AddWithValue("@PaymentId", Convert.ToInt32(Request.QueryString["id"]));
            cmd.Parameters.AddWithValue("@UserId", Session["userId"]);
            
            dt = new DataTable();
            
            sda = new SqlDataAdapter(cmd);
            sda.Fill(dt);

            decimal grandTotal = 0;

            foreach (DataRow drow in dt.Rows)
            {
                grandTotal += Convert.ToDecimal(drow["TotalPrice"]);
            }

            Label_TotalPrice.Text = $"Total Price: {grandTotal}$";
            return dt;
        }

        protected void lbDownloadInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                String fileName = $"invoice_${DateTime.Now:yyyyMMddhhmmss}";
                DataTable dtbl = GetOrderDetails();
                Utils.ExportToPdf(dtbl, fileName, "Order Invoice");

                WebClient client = new WebClient();
                byte[] buffer = client.DownloadData($"{Constants.DOWNLOAD_FOLDER}\\{fileName}");
                if (buffer != null)
                {
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-length", buffer.Length.ToString());
                    Response.BinaryWrite(buffer);
                }
            }
            catch (Exception ex)
            {
                lblMsg.Visible = true;
                lblMsg.Text = "Error Message:- " + ex.Message.ToString();
            }
        }


    }
    public class InvoiceDto
    {
        public string OrderNo { get; set; }
        public int Quatity { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int TotalPrice { get; set; }
    }
}