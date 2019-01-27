<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Admin.Master"
 Inherits="System.Web.Mvc.ViewPage<IEnumerable<CHOJ.Models.Wiki>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Management</h2>

    <table>
        <tr>
            <th></th>
            <th>
                Title
            </th>
            <th>
                AddTime
            </th>
        </tr>

    <% foreach (var item in Model) { %>
    
        <tr>
            <td>
            <%= Html.ActionLink("Edit", "Edit", new { item.Id })%> |
                <%= Html.ActionLink("Delete", "Delete", new { item.Id })%> |
                <%= Html.ActionLink("Details", "Details", new { item.Title })%>
            </td>
            <td>
                <%= Html.Encode(item.Title) %>
            </td>
            <td>
                <%= Html.Encode(String.Format("{0:g}", item.AddTime)) %>
            </td>
        </tr>
    
    <% } %>

    </table>

    <p>
        <%= Html.ActionLink("Create New", "Create") %>
    </p>

</asp:Content>

