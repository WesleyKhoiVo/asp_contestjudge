<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<CHOJ.Models.Wiki>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%= Html.Encode(Model.Title) %></h2>
    <p><%= Html.Encode(String.Format("{0:g}", Model.AddTime)) %></p>
    <p><%= Model.Body%></p>
</asp:Content>

