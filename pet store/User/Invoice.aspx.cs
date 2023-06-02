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
            List<InvoiceDto> invoices = new List<InvoiceDto>();
            con = new SqlConnection(Connection.GetConnectionString());
            var sql = new StringBuilder();
            sql.Append("SELECT                                      ");
            sql.Append("    o.OrderNo                               ");
            sql.Append("    , o.Quatity                            ");
            sql.Append("    , o.OrderDate                           ");
            sql.Append("    , o.Status                              ");
            sql.Append("    , p.Name                                ");
            sql.Append("    , p.Price                               ");
            sql.Append("    , (p.Price * p.Quantity) as TotalPrice  ");
            sql.Append("FROM                                        ");
            sql.Append("    Orders AS o JOIN Products AS p          ");
            sql.Append("    ON p.ProductId = o.ProductId            ");
            sql.Append("WHERE                                       ");
            sql.Append("    o.PaymentId = @PaymentId                ");
            sql.Append("    AND o.UserId = @UserId                  ");
            cmd = new SqlCommand(sql.ToString(), con);

            cmd.Parameters.AddWithValue("@PaymentId", Convert.ToInt32(Request.QueryString["id"]));
            cmd.Parameters.AddWithValue("@UserId", Session["userId"]);

            sda = new SqlDataAdapter(cmd);
            var ds = new DataSet();
            sda.Fill(ds);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    var _orderDate = (DateTime)r["OrderDate"];

                    var invoice = new InvoiceDto
                    {
                        Name = (string)r["Name"],
                        OrderDate = _orderDate,
                        OrderNo = (string)r["OrderNo"],
                        Price = (int)r["Price"],
                        Quatity = (int)r["Quantity"],
                        Status = (string)r["Status"],
                        TotalPrice = (int)r["TotalPrice"]
                    };
                    invoices.Add(invoice);
                }
            }
            //var dt = new DataTable();
            //DataRow dr = dt.NewRow();
            //dr["TotalPrice"] = grandTotal;
            //dt.Rows.Add(dr);
            return invoices;
        }

        protected void lbDownloadInvoice_Click(object sender, EventArgs e)
        {
            try
            {
                string downloadPath = @"D:\";
                List<InvoiceDto> invoices = GetOrderDetails();
                ExportToPdf(invoices, downloadPath, "Order Invoice");

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

        void ExportToPdf(List<InvoiceDto> invoices, String strPdfPath, string strHeader)
        {
            //FileStream fs = new FileStream(strPdfPath, FileMode.Create, FileAccess.Write, FileShare.None);
            //Document document = new Document();
            //document.SetPageSize(PageSize.A4);
            //PdfWriter writer = PdfWriter.GetInstance(document, fs);
            //document.Open();

            ////Report Header
            //BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            //Font fntHead = new Font(bfntHead, 16, 1, Color.GRAY);
            //Paragraph prgHeading = new Paragraph();
            //prgHeading.Alignment = Element.ALIGN_CENTER;
            //prgHeading.Add(new Chunk(strHeader.ToUpper(), fntHead));
            //document.Add(prgHeading);

            ////Author
            //Paragraph prgAuthor = new Paragraph();

            //// BaseFont: sử dụng để tạo và quản lý các font chữ trong tài liệu PDF.
            //// btnAuthor: tên biến được khai báo để lưu trữ đối tượng BaseFont mới được tạo

            //BaseFont btnAuthor = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            //Font fntAuthor = new Font(btnAuthor, 8, 2, Color.GRAY);
            //prgAuthor.Alignment = Element.ALIGN_RIGHT;
            //prgAuthor.Add(new Chunk("Order From :Pet Store", fntAuthor));
            //prgAuthor.Add(new Chunk("\nOrder Date : " + dtblTable.Rows[0]["OrderDate"].ToString(), fntAuthor));
            //document.Add(prgAuthor);

            ////Add a line seperation
            //Paragraph p = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, Color.BLACK, Element.ALIGN_LEFT, 1)));
            //document.Add(p);

            ////Add line break
            //document.Add(new Chunk("\n", fntHead));

            ////Write the table
            //PdfPTable table = new PdfPTable(dtblTable.Columns.Count - 2);
            ////Table header
            //BaseFont btnColumnHeader = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            //Font fntColumnHeader = new Font(btnColumnHeader, 9, 1, Color.WHITE);
            //for (int i = 0; i < dtblTable.Columns.Count - 2; i++)
            //{
            //    PdfPCell cell = new PdfPCell();
            //    cell.BackgroundColor = Color.GRAY;
            //    cell.AddElement(new Chunk(dtblTable.Columns[i].ColumnName.ToUpper(), fntColumnHeader));
            //    table.AddCell(cell);
            //}
            ////table Data
            //Font fntColumnData = new Font(btnColumnHeader, 8, 1, Color.BLACK);
            //for (int i = 0; i < dtblTable.Rows.Count; i++)
            //{
            //    for (int j = 0; j < dtblTable.Columns.Count - 2; j++)
            //    {
            //        PdfPCell cell = new PdfPCell();
            //        cell.AddElement(new Chunk(dtblTable.Rows[i][j].ToString(), fntColumnData));
            //        table.AddCell(cell);
            //    }
            //}

            //document.Add(table);
            //document.Close();
            //writer.Close();
            //fs.Close();
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