<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<CHOJ.Models.Group>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="center">
        <ul>
            <%foreach (CHOJ.Models.Group dr in ViewData.Model)
              { 
       
            %>
            <li>
                <%=Html.QuestionList(dr.Title, dr.Id)%></li>
            <%} %>
        </ul>
    </div>
</asp:Content>
