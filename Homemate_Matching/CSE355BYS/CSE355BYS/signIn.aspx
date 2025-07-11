﻿<%@ Page Language="C#" AutoEventWireup="true" CodeFile="signIn.aspx.cs" Inherits="Homemate_Matching.signIn" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Giriş Yap</title>
    <link href="css/signInn.css" rel="stylesheet" />
</head>
<body>
    <div class="wrapper">
        <form id="form1" runat="server">
            <h2 class="mb-4">Giriş Yap</h2>

            <div class="input-box">
                <label for="txtUsername" class="form-label">Kullanıcı Adı</label>
                <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" Placeholder="Kullanıcı Adını Giriniz" />
            </div>

            <div class="input-box">
                <label for="txtPassword" class="form-label">Şifre</label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" Placeholder="Şifre Giriniz" />
            </div>
            <br /><br />

            <asp:Button ID="btnSignIn" runat="server" Text="Giriş Yap" CssClass="btn btn-primary" OnClick="btnSignIn_Click" />
            <br /><br />
            <asp:Button ID="btnSignUp" runat="server" Text="Kaydol" CssClass="btn btn-primary" OnClick="btnSignUp_Click" />
            <br /><br />
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />
            </form>

    </div>
</body>
</html>