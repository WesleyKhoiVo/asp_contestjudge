<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Admin.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<CHOJ.Models.Group>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Group Management</h2>

    <table>
        <tr>
            <th></th>
            <th>
                ID
            </th>
            <th>
                Name
            </th>
            <th>
                QuestionCount
            </th>
            <th>
                Order
            </th>
            <th>
                Type
            </th>
            <th>
                BeginTime
            </th>
            <th>
                EndTime
            </th>
        </tr>

    <% foreach (var item in Model) { %>
    
        <tr>
            <td>
                <%= Html.ActionLink("Delete", "Delete", new { id=item.Id })%> |
            </td>
            <td>
                <%= Html.Encode(item.Id) %>
            </td>
            <td>
                <%= Html.Encode(item.Title) %>
            </td>
            <td>
                <%= Html.Encode(item.QuestionCount) %>
            </td>
            <td>
                <%= Html.Encode(item.Order) %>
            </td>
            <td>
                <%= Html.Encode(item.Type) %>
            </td>
            <td>
                <%= Html.Encode(String.Format("{0:g}", item.BeginTime)) %>
            </td>
            <td>
                <%= Html.Encode(String.Format("{0:g}", item.EndTime)) %>
            </td>
        </tr>
    
    <% } %>

    </table>
    <form action="<%=Url.Action("Create") %>" method="post">
    <fieldset>
        <legend>Add</legend>
        <p>
            <label>
                Title</label><%=Html.TextBox("Title") %>
        </p>
        <p>
            <input type="submit" value="Create" /></p>
    </fieldset>
    </form>
</asp:Content>

