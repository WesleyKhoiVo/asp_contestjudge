<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Admin.Master" Inherits="System.Web.Mvc.ViewPage<CHOJ.Models.Question>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Create</h2>

    <%= Html.ValidationSummary("Create was unsuccessful. Please correct the errors and try again.") %>

    <% using (Html.BeginForm()) {%>

        <fieldset>
            <legend>Fields</legend>
            <p>
                <label for="Title">Title:</label>
                <%= Html.TextBox("Title") %>
                <%= Html.ValidationMessage("Title", "*") %>
            </p>
            <p>
                <label for="Body">Body:</label>
                <%= Html.TextArea("Body", new { style="height:200px;width:400px;"})%>
                <%= Html.ValidationMessage("Body", "*") %>
            </p>
            <p>
                <label for="TimeLimit">TimeLimit:</label>
                <%= Html.TextBox("TimeLimit") %>ms
                <%= Html.ValidationMessage("TimeLimit", "*") %>
            </p>
            <p>
                <label for="MemoryLimit">MemoryLimit:</label>
                <%= Html.TextBox("MemoryLimit") %> K
                <%= Html.ValidationMessage("MemoryLimit", "*") %>
            </p>
            <p>
                <label for="GroupId">Group:</label>
                <%= Html.DropDownList("GroupId") %>
                <%= Html.ValidationMessage("GroupId", "*") %>
            </p>
            <p>
                <label for="Test">Test:</label>
                <%= Html.TextArea("Test", new { style = "height:200px;width:400px;" })%>
                <%= Html.ValidationMessage("Test", "*") %>
            </p>
            <p>
                <input type="submit" value="Create" />
            </p>
        </fieldset>

    <% } %>

    <div>
        <%=Html.ActionLink("Back to List", "Index") %>
    </div>

</asp:Content>

