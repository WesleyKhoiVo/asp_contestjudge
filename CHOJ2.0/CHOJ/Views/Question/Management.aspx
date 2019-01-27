<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Admin.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<CHOJ.Models.Question>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Management</h2>
<div>
<ul>
<%foreach(var g in ViewData["GroupList"] as IEnumerable< CHOJ.Models.Group>)
 {
      %>
      <%=Html.ActionLink(g.Title, "Management", new { groupId=g.Id})%>
      <%
 }%>
</ul>
</div>
    <table>
        <tr>
            <th></th>
            <th>
                Title
            </th>
            <th>
                TimeLimit
            </th>
            <th>
                MemoryLimit
            </th>
            <th>
                AcceptedCount
            </th>
            <th>
                SubmitCount
            </th>
            <th>
                AddTime
            </th>
            <th>
                IsTrue
            </th>
 
        </tr>

    <% foreach (var item in Model) { %>
    
        <tr>
            <td>
             <%= Html.ActionLink("Edit", "Edit", new { item.Id }) %> |
                <%= Html.ActionLink("Delete", "Delete", new { item.Id }) %>
            </td>
            <td>
            <%=Html.ActionLink(item.Title, "Index", new { item.Id})%>
                
            </td>
            <td>
                <%= Html.Encode(item.TimeLimit) %>
            </td>
            <td>
                <%= Html.Encode(item.MemoryLimit) %>
            </td>
            <td>
                <%= Html.Encode(item.AcceptedCount) %>
            </td>
            <td>
                <%= Html.Encode(item.SubmitCount) %>
            </td>
            <td>
                <%= Html.Encode(String.Format("{0:g}", item.AddTime)) %>
            </td>
            <td>
                <%= Html.Encode(item.IsTrue) %>
            </td>
        </tr>
    
    <% } %>

    </table>

    <p>
        <%= Html.ActionLink("Create New", "Create") %>
    </p>

</asp:Content>

