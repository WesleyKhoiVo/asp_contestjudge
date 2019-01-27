<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true"
    Inherits="System.Web.Mvc.ViewPage<Profile>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        <%=ViewData["Page_Title"] %></h2>
    <form action="" method="post">
    <%=Html.ValidationSummary() %>
    <fieldset>
        <p>
            <label>
                Nick Name</label><%=Html.TextBox("NickName")%></p>
                
        <p>
            <label>
                Name</label><%=Html.TextBox("Name")%></p>
        <p>
            <label>
                Email</label><%=Html.TextBox("Email")%></p>
        <p>
            <label>
                Sex</label><%=Html.TextBox("Sex")%></p>
        <p>
            <label>
                School</label><%=Html.TextBox("School")%></p>
        <p>
            <label>
                SchoolDetails</label><%=Html.TextBox("SchoolDetails")%></p>
        <p>
            <input type="submit" value="update" /></p>
    </fieldset>
    </form>
</asp:Content>
