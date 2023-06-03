using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Drawing.Printing;

using iTextSharp.text.pdf;
using iTextSharp.text;
using System.IO;
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
        List<InvoiceDto> GetOrderDetails()
        {
            double grandTotal = 0;
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
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                foreach (DataRow drow in dt.Rows)
                {
                    grandTotal += Convert.ToDouble(drow["TotalPrice"]);
                }
            }
            DataRow dr = dt.NewRow();
            dr["TotalPrice"] = grandTotal;
            dt.Rows.Add(dr);
            return dt;
        }

        protected void lbDownloadInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                string downloadPath = @"D:\abc\order_invoice.pdf";
                DataTable dtbl = GetOrderDetails();
                Utils.ExportToPdf(dtbl, downloadPath, "Order Invoice");

                WebClient client = new WebClient();
                Byte[] buffer = client.DownloadData(downloadPath);
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