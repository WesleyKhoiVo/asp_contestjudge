<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true"
	 Inherits="System.Web.Mvc.ViewPage<IEnumerable<Answer>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<span>
	<span><%=Html.ActionLink("Home","Index","Home") %></span>
	<span class="textStyle">&gt;</span><span
		class="textSample">Status</span></span>
	<div>
		<div>
			<table class="GV" cellspacing="0" rules="all" border="1" style="border-collapse: collapse;">
				<tr class="Login_td">
					<th scope="col" style="width: 100px;">Submit Time</th>
					<th scope="col">Title</th>
					<th scope="col" style="width: 150px;">Result</th>
					<th scope="col" style="width: 50px;">Compiler</th>
					<th scope="col" style="width: 70px;">Time</th>
					<th scope="col" style="width: 70px;">Memory</th>
					<th scope="col" style="width: 80px;">User</th>
				</tr>
				<%
					bool line = false ;
					foreach (Answer a in Model) {
						line = !line;
		   %>
		   		<tr class="<%=line?"rb":"re" %>">
					<td><%=a.AddTime.ToString("MM-dd hh:mm")%></td>
					<td><%=Html.QuestionLink((a.QuestionTitle??"")+" ", a.QuestionId)%></td>
					<td><%=Html.Status(a.Status,a.Id)%></td>
					<td><%=a.Complier%></td>
					<td><%=a.UseTime %></td>
					<td><%=a.UseMemory %></td>
					<td><%=a.UserName%></td>
				</tr>
				<%} %>				

			</table>
		</div>
	</div>
</asp:Content>
