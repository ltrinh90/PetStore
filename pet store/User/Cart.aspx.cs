using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace pet_store.User
{
    public partial class Cart : System.Web.UI.Page
    {

        SqlConnection con;
        SqlCommand cmd;
        SqlDataAdapter sda;
        DataTable dt;
        decimal grandTotal = 0;

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
                    getCartItems();
                }
            }
        }

        void getCartItems()
        {
            con = new SqlConnection(Connection.GetConnectionString());
            cmd = new SqlCommand("Cart_Crud", con);
            cmd.Parameters.AddWithValue("@Action", "SELECT");
            cmd.Parameters.AddWithValue("@UserId", Session["userId"]);
            cmd.CommandType = CommandType.StoredProcedure;
            sda = new SqlDataAdapter(cmd);
            dt = new DataTable();
            sda.Fill(dt);
            rCartItem.DataSource = dt;
            if (dt.Rows.Count == 0)
            {
                rCartItem.FooterTemplate = null;
                rCartItem.FooterTemplate = new CustomTemplate(ListItemType.Footer);

            }
            rCartItem.DataBind();
        }
        protected void rCartItem_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            Utils utils = new Utils();
            if (e.CommandName == "remove")
            {
                con = new SqlConnection(Connection.GetConnectionString());
                cmd = new SqlCommand("DELETE FROM Products WHERE ProductID = @ProductID", con);
                cmd.Parameters.AddWithValue("@Action", "DELETE");
                cmd.Parameters.AddWithValue("@ProductID", e.CommandArgument);
                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    getCartItems();
                    // Cart count
                    Session["cartCount"] = utils.cartCount(Convert.ToInt32(Session["userId"]));
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('Error - " + ex.Message + " ');</script>");
                }
                finally
                {
                    con.Close();
                }
            }

            if (e.CommandName == "updateCart")
            {
                bool isCartUpdated = false;
                for (int item = 0; item < rCartItem.Items.Count; item++)
                {
                    if (rCartItem.Items[item].ItemType == ListItemType.Item || rCartItem.Items[item].ItemType == ListItemType.AlternatingItem)
                    {
                        TextBox quantity = rCartItem.Items[item].FindControl("txtQuantity") as TextBox;
                        HiddenField _productId = rCartItem.Items[item].FindControl("hdnProductId") as HiddenField;
                        HiddenField _quantity = rCartItem.Items[item].FindControl("hdnQuantity") as HiddenField;

                        int quantityFromCart = Convert.ToInt32(quantity.Text);

                        //int quantityFromCart;
                        //if (int.TryParse(quantity.Text, out quantityFromCart))
                        //{

                        //    int totalQuantity = quantityFromCart * 2;
                        //    Console.WriteLine("Total quantity: " + totalQuantity);

                        //}
                        //else
                        //{
                        //    Console.WriteLine("Invalid input...");
                        //}


                        int ProductId = Convert.ToInt32(_productId.Value);



                        //int quantityFromDB;
                        //if (int.TryParse(_quantity.Value, out quantityFromDB))
                        //{

                        //    int totalQuantity = quantityFromDB * 2;
                        //    Console.WriteLine("Total quantity: " + totalQuantity);

                        //}
                        //else
                        //{

                        //    Console.WriteLine("Invalid quantity value...");
                        //}

                        int quantityFromDB = Convert.ToInt32(_quantity.Value);

                        bool isTrue = false;
                        int updatedQuantity = 1;
                        if (quantityFromCart > quantityFromDB)
                        {
                            updatedQuantity = quantityFromCart;
                            isTrue = true;
                        }
                        else if (quantityFromCart < quantityFromDB)
                        {
                            updatedQuantity = quantityFromCart;
                            isTrue = true;
                        }
                        if (isTrue)
                        {
                            // Update cart item's  quantity in DB.
                            isCartUpdated = utils.updateCartQuantity(updatedQuantity, ProductId, Convert.ToInt32(Session["userId"]));
                        }
                    }
                }
                getCartItems();
            }
            if (e.CommandName == "checkout")
            {
                bool isTrue = false;
                string pName = string.Empty;
                // First will check item quantity
                for (int item = 0; item < rCartItem.Items.Count; item++)
                {
                    if (rCartItem.Items[item].ItemType == ListItemType.Item || rCartItem.Items[item].ItemType == ListItemType.AlternatingItem)
                    {
                        HiddenField _productId = rCartItem.Items[item].FindControl("hdnProductId") as HiddenField;
                        HiddenField _cartQuantity = rCartItem.Items[item].FindControl("hdnQuantity") as HiddenField;
                        HiddenField _productQuantity = rCartItem.Items[item].FindControl("hdnPrdQuantity") as HiddenField;
                        Label productName = rCartItem.Items[item].FindControl("lblName") as Label;
                        int productId = Convert.ToInt32(_productId.Value);

                        int cartQuantity = Convert.ToInt32(_cartQuantity.Value);
                        //int cartQuantity;
                        //if (int.TryParse(_cartQuantity.Value, out cartQuantity))
                        //{
                        //    // Chuyển đổi thành công, sử dụng giá trị của cartQuantity ở đây
                        //    Console.WriteLine("Giá trị của cartQuantity là: " + cartQuantity);
                        //}
                        //else
                        //{
                        //    // Xử lý lỗi khi chuỗi không hợp lệ
                        //    Console.WriteLine("Lỗi");
                        //}

                        int productQuantity = Convert.ToInt32(_productQuantity.Value);
                        if (productQuantity > cartQuantity && productQuantity > 2)
                        {
                            isTrue = true;
                            lblMsg.Visible = true;
                            lblMsg.Text = "Item <b>'" + pName + "' is out of stock:(";
                            lblMsg.CssClass = "alert alert-warning";
                        }
                        else
                        {
                            isTrue = true;
                            pName = productName.Text.ToString();
                            Response.Redirect("Payment.aspx");
                        }
                    }
                }
            }
        }
        protected void rCartItem_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Label totalPrice = e.Item.FindControl("lblTotalPrice") as Label;
                Label productPrice = e.Item.FindControl("lblPrice") as Label;
                TextBox quantity = e.Item.FindControl("txtQuantity") as TextBox;

                decimal calTotalPrice = 0;
                decimal productPriceValue = 0;
                decimal quantityValue = 0;

                if (decimal.TryParse(productPrice.Text, out productPriceValue) && decimal.TryParse(quantity.Text, out quantityValue))
                {
                    calTotalPrice = productPriceValue * quantityValue;
                    totalPrice.Text = calTotalPrice.ToString();
                    grandTotal += calTotalPrice;
                }
            }
            Session["grandTotalPrice"] = grandTotal;
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
                    var footer = new LiteralControl("<tr><td colspan='5'><b>Your Cart is empty.</b><a href='Dog.aspx' class='badge badge-info ml-2'>Continue Shopping</a></td></tr></tbody></table>");
                    container.Controls.Add(footer);
                }
            }
        }
    }
}