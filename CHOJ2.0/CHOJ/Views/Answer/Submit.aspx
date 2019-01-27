<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true"
	 Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<div class="center">
		<form action="<%=Url.Action("SubmitProcess")%>" method="post">
		Problem Id :<%=Html.TextBox("QuestionId")%><br />
		Compiler :<%=Html.DropDownList("Compiler")%><br />
		Code :<textarea name="Code" rows="2" cols="20" style="height: 237px; width: 416px;"></textarea><br />
		<input type="submit" value="Submit" /> 
		</form> 
	</div>
</asp:Content>
