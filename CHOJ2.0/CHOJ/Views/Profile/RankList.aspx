<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<CHOJ.Models.Profile>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Rank List</h2>

    <table  class="GV" cellspacing="0" rules="all" border="1" style="border-collapse: collapse;">
        <tr>
        <th>Rank</th>
            <th>
                NickName
            </th>
            <th>
                Email
            </th>
            <th>
                Sex
            </th>
            <th>
                School
            </th>
            <th>
                SchoolDetails
            </th>
            <th>
                Submit
            </th>
            <th>
                Accepted
            </th>
            <th>
                RegisterTime
            </th>
          
        </tr>

    <%
        int i = 1;
        foreach (var item in Model) { %>
    
        <tr>
        <td><%=i++ %></td>
            <td>
                <%= Html.Encode(item.NickName) %>
            </td>
            <td>
                <%= Html.Encode(item.Email) %>
            </td>
            <td>
                <%= Html.Encode(item.Sex) %>
            </td>
            <td>
                <%= Html.Encode(item.School) %>
            </td>
            <td>
                <%= Html.Encode(item.SchoolDetails) %>
            </td>
            <td>
                <%= Html.Encode(item.Submit) %>
            </td>
            <td>
                <%= Html.Encode(item.Accepted) %>
            </td>
            <td>
                <%= Html.Encode(String.Format("{0:g}", item.RegisterTime)) %>
            </td>

        </tr>
    
    <% } %>

    </table>

</asp:Content>

