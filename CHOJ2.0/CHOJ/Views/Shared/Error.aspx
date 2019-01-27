<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<HandleErrorInfo>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Sorry, an error occurred while processing your request.
    </h2>
    <%
            var exceptions = new Stack<Exception>();
            for (Exception ex = ViewData.Model.Exception; ex != null; ex = ex.InnerException)
            {
                exceptions.Push(ex);
            }
        %>
        <%
            foreach (var ex in exceptions)
            {
%>
        <div class="notes">
            <% //= Html.Encode(ex.GetType().FullName)%>
            <%=ex.Message%>
        </div>
<div style="color:Red">
<pre style="font-size: medium;"><%= Html.Encode(ex.StackTrace)%></pre>
</div>
        <%
            }
%>
</asp:Content>
