using pet_store.Service.Implement;
using pet_store.Service.Interface;
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
        decimal grandTotal = 0;
        private readonly CartService cartService = new CartService();

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
            var ds = new DataSet();
            sda.Fill(ds);
            if (ds.Tables[0].Rows.Count == 0)
            {
                //rCartItem.FooterTemplate = null;
                rCartItem.FooterTemplate = new CustomTemplate(ListItemType.Footer);
            }
            rCartItem.DataSource = ds;
            rCartItem.DataBind();
        }
        protected void rCartItem_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
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
                    Session["cartCount"] = cartService.Count(Convert.ToInt32(Session["userId"]));
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
                for (int item = 0; item < rCartItem.Items.Count; item++)
                {
                    if (rCartItem.Items[item].ItemType == ListItemType.Item || rCartItem.Items[item].ItemType == ListItemType.AlternatingItem)
                    {
                        TextBox quantity = rCartItem.Items[item].FindControl("txtQuantity") as TextBox;
                        HiddenField _productId = rCartItem.Items[item].FindControl("hdnProductId") as HiddenField;
                        HiddenField _quantity = rCartItem.Items[item].FindControl("hdnQuantity") as HiddenField;

                        int quantityFromCart = Convert.ToInt32(quantity.Text);


                        int ProductId = Convert.ToInt32(_productId.Value);


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
                            cartService.UpdateQuantity(updatedQuantity, ProductId, Convert.ToInt32(Session["userId"]));
                        }
                    }
                }
                getCartItems();
            }
            if (e.CommandName == "checkout")
            {
                string pName = string.Empty;
                // First will check item quantity
                for (int item = 0; item < rCartItem.Items.Count; item++)
                {
                    if (rCartItem.Items[item].ItemType == ListItemType.Item || rCartItem.Items[item].ItemType == ListItemType.AlternatingItem)
                    {
                        HiddenField _productId = rCartItem.Items[item].FindControl("hdnProductId") as HiddenField;
                        HiddenField _cartQuantity = rCartItem.Items[item].FindControl("hdnQuantity") as HiddenField;
                        HiddenField _productQuantity = rCartItem.Items[item].FindControl("hdnPrdQuantity") as HiddenField;
                        Label productNameLabel = rCartItem.Items[item].FindControl("lblName") as Label;
                        pName = productNameLabel.Text ?? string.Empty;
                        //int productId = Convert.ToInt32(r.Value);

                        int cartQuantity = Convert.ToInt32(_cartQuantity.Value);

                        int productQuantity = Convert.ToInt32(_productQuantity.Value);
                        if (cartQuantity > productQuantity)
                        {
                            lblMsg.Visible = true;
                            lblMsg.Text = "Item <b>" + pName + "</b> is out of stock :(";
                            lblMsg.CssClass = "alert alert-warning";
                        }
                        else
                        {
                            pName = productNameLabel.Text.ToString();
                            Response.Redirect("Payment.aspx");
                            return;
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