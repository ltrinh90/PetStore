<%@ Page Title="" Language="C#" MasterPageFile="~/User/User.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="pet_store.User.Default" %>

<%@ Import Namespace="pet_store" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <!-- offer section -->
  <section class="offer_section layout_padding-bottom">
    <div class="offer_container">
      <div class="container ">
        <div class="row">
            <asp:Repeater ID="rCategory" runat="server">
                <ItemTemplate>
          <div class="col-md-6  ">
            <div class="box ">
              <div class="img-box">
                  <a href="Dog.aspx?id=<%# Eval("CategoryId") %>">
                       <img src="<%# Utils.GetImageUrl( Eval("ImageUrl")) %>" alt="">
                  </a>
               
              </div>
              <div class="detail-box">
                <h5><%# Eval("Name") %></h5>
                <h6>
                  <span>20%</span> Off
                </h6>
                <a href="Dog.aspx?id=<%# Eval("CategoryId") %>">
                  Order Now <svg version="1.1" id="Capa_1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" x="0px" y="0px" viewBox="0 0 456.029 456.029" style="enable-background:new 0 0 456.029 456.029;" xml:space="preserve">
                    <g>
                      <g>
                        <path d="M345.6,338.862c-29.184,0-53.248,23.552-53.248,53.248c0,29.184,23.552,53.248,53.248,53.248
                     c29.184,0,53.248-23.552,53.248-53.248C398.336,362.926,374.784,338.862,345.6,338.862z" />
                      </g>
                    </g>
                    <g>
                      <g>
                        <path d="M439.296,84.91c-1.024,0-2.56-0.512-4.096-0.512H112.64l-5.12-34.304C104.448,27.566,84.992,10.67,61.952,10.67H20.48
                     C9.216,10.67,0,19.886,0,31.15c0,11.264,9.216,20.48,20.48,20.48h41.472c2.56,0,4.608,2.048,5.12,4.608l31.744,216.064
                     c4.096,27.136,27.648,47.616,55.296,47.616h212.992c26.624,0,49.664-18.944,55.296-45.056l33.28-166.4
                     C457.728,97.71,450.56,86.958,439.296,84.91z" />
                      </g>
                    </g>
                    <g>
                      <g>
                        <path d="M215.04,389.55c-1.024-28.16-24.576-50.688-52.736-50.688c-29.696,1.536-52.224,26.112-51.2,55.296
                     c1.024,28.16,24.064,50.688,52.224,50.688h1.024C193.536,443.31,216.576,418.734,215.04,389.55z" />
                      </g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                    <g>
                    </g>
                  </svg>
                </a>
              </div>
            </div>
          </div>
                  
                </ItemTemplate>
            </asp:Repeater>
          
        </div>
      </div>
    </div>
  </section>

  <!-- end offer section -->
    <div class="mt-5" style="background-color:rgb(129 255 205 / 0.74);" id="contact">
    <div class="container py-md-1 ">
        <div class="row py-lg-2 py-sm-4">
            <div class="col-lg-12">
                <div class="w3_pvt-contact-top">
                    <h2>Get in touch </h2>
                    <ul class="d-flex header-wthreelayouts pt-0 flex-column">
                        <li>
                            <span class="fa fa-home" data-blast="color"></span>
                            <p class="d-inline">730 Lê Đức Thọ, P15 Gò Vấp, Tp.HCM</p>
                        </li>
                        <li class="my-3">
                            <span class="fa fa-envelope-open" data-blast="color"></span>
                            <a href="mailto:example@email.com" class="text-secondary">
                   petstore90@gmail.com
                            </a>
                        </li>
                        <li>
                            <span class="fa fa-phone" data-blast="color"></span>
                            <p class="d-inline">(+84 28) 38 632 052</p>
                        </li>
                    </ul>
                </div>
                <div class="col-lg-12 mt-4">
                    <!-- register form grid -->
                    <div class="register-top1">
                        <form action="#" method="post" class="register-wthree">
                            <div class="form-group">
                                <div class="row">
                                    <div class="col-lg-3 pl-lg-0">
                                        <label>
                                            Full name
                                        </label>
                                        <input class="form-control" type="text" placeholder="A B C" name="email"
                                               required="" data-blast="borderColor">
                                    </div>
                                    <div class="col-lg-3 my-lg-0 my-4">
                                        <label>
                                            Email
                                        </label>
                                        <input class="form-control" type="email" placeholder="example@email.com"
                                               name="email" required="" data-blast="borderColor">
                                    </div>
                                    <div class="col-lg-3">
                                        <label>
                                            Mobile
                                        </label>
                                        <input class="form-control" type="text" placeholder="+84 000 111 222" name="email"
                                               required="" data-blast="borderColor">
                                    </div>
                                    <div class="col-lg-3 d-flex align-items-end pr-lg-0 mt-lg-0 mt-5">
                                        <button type="submit" data-blast="bgColor" class="btn btn-w3_pvt btn-block w-50 text-white font-weight-bold text-uppercase bg-danger mt-4">
                                            Send
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                    <!--  //register form grid ends here -->
                </div>
            </div>
        </div>
    </div>
</div>
<!-- map -->
<div class="google-map mt-5 ">
  <%--  <iframe src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3919.4635085474065!2d106.66529475080205!3d10.775767862113694!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x31752f9023a3a85d%3A0xdee5c99a7b02feab!2sHuflit!5e0!3m2!1svi!2s!4v1640006787383!5m2!1svi!2s" width="100%" height="450" style="border:0;" allowfullscreen="" loading="lazy"></iframe></div>
<!--// map-->--%>

<!-- //contact -->
</asp:Content>
