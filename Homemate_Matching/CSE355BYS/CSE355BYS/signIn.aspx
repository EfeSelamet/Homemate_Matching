<%@ Page Language="C#" AutoEventWireup="true" CodeFile="signIn.aspx.cs" Inherits="Homemate_Matching.signIn" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Giri� Yap</title>
    <link href="css/signInn.css" rel="stylesheet" />
</head>
<body>
    <div class="wrapper">
        <form id="form1" runat="server">
            <h2 class="mb-4">Giri� Yap</h2>

            <div class="input-box">
                <label for="txtUsername" class="form-label">Kullan�c� Ad�</label>
                <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" Placeholder="Kullan�c� Ad�n� Giriniz" />
            </div>

            <div class="input-box">
                <label for="txtPassword" class="form-label">�ifre</label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" Placeholder="�ifre Giriniz" />
            </div>

            <asp:Button ID="btnSignIn" runat="server" Text="Giri� Yap" CssClass="btn btn-primary" OnClick="btnSignIn_Click" />
            <br /><br />
            <asp:Button ID="btnSignUp" runat="server" Text="Kaydol" CssClass="btn btn-primary" OnClick="btnSignUp_Click" />
            <br /><br />
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red" />
            </form>

    </div>
</body>
</html>