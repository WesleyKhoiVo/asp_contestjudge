<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true"  Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Login</h2>
    <div>
        <a href="<%=ViewData["LiveLogin"]  %>">LogOn or Register with Windows Live Account</a>
    </div>
<%--    <p>
        Please enter your username and password below. If you don't have an account,
        please <%= Html.ActionLink("register", "Register") %>.
    </p>
    <%
        IList<string> errors = ViewData["errors"] as IList<string>;
        if (errors != null) {
            %>
                <ul class="error">
                <% foreach (string error in errors) { %>
                    <li><%= Html.Encode(error) %></li>
                <% } %> 
                </ul>
            <%
        }
         %><form method="post" action="<%= Html.AttributeEncode(Url.Action("Login")) %>">
        <div>
            <table>
                <tr>
                    <td>Username:</td>
                    <td><%= Html.TextBox("username") %></td>
                </tr>
                <tr>
                    <td>Password:</td>
                    <td><%= Html.Password("password") %></td>
                </tr>
                <tr>
                    <td></td>
                    <td><input type="checkbox" name="rememberMe" value="true" /> Remember me?</td>
                </tr>
                <tr>
                    <td></td>
                    <td><input type="submit" value="Login" /></td>
                </tr>
            </table>
        </div>
        <%=Html.Hidden("ReturnUrl")%>
    </form>--%>
</asp:Content>
