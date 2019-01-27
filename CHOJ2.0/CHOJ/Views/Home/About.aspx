<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true"
    Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        About Us</h2>
    <ul>
        <li><a href="http://www.codeplex.com/onlinejudge">Source Code on Codeplex</a></li>
        <li><a href="http://www.eice.com.cn">Team WebSite</a></li>
        <li><a href="http://www.cnblogs.com/chsword">Blog</a></li>
        <li><a href="http://oj.eice.com.cn">Demo online</a></li>
    </ul>
    
    <%
        foreach(var c in System.CodeDom.Compiler.CodeDomProvider.GetAllCompilerInfo())
        {
            Writer.Write("Language<br>");
            foreach (var l in c.GetLanguages())
                Writer.Write(l + "<br>");
            
            Writer.Write("Extensions<br>");
            foreach (var l in c.GetExtensions())
                Writer.Write(l + "<br>");
        }
%>
</asp:Content>
