<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true"
	 Inherits="System.Web.Mvc.ViewPage<Question>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
		
		<h3><%=Model.Title %></h3>
		<div class="center">
			<span class="limit">Time limit:</span><%=Model.TimeLimit%> ms			<span class="limit">Memory limit:</span><%=Model.MemoryLimit%> K 
			<span class="limit">Total submit:</span><%=Model.SubmitCount%>&nbsp;&nbsp; 
			<span class="limit">Accepted submit: </span><%=Model.AcceptedCount%>
		</div>
		<div id="main">
			<div id="contentHeadLeft">
				<div id="contentHeadRight">
					<div id="contentHeadCenter">
					</div>
				</div>
			</div>
			<div id="contentBodyLeft">
				<div id="contentBodyRight">
					<div id="contentBodyCenter">
						<div id="content">
							<div id="entries">
						<%=Html.QuestionFormat(Model.Body) %>
							</div>
						</div>
						<div class="clear">
							&nbsp;</div>
					</div>
				</div>
			</div>
			<div id="contentFootLeft">
				<div id="contentFootRight">
					<div id="contentFootCenter">
					</div>
				</div>
			</div>
		</div>
		<div class="center">
			<%=Html.SubmitLink("Submit",Model.Id) %>
			<a href="<%=ViewData["comefrom"]  %>">Back</a> 
			<%=Html.ActionLink("Status", "QuestionStatus", "Answer", new { qId = Model.Id, title =Model.Title}, null)%>
			
		</div>
</asp:Content>
