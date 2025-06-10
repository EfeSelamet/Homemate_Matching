<%@ Page Language="C#" AutoEventWireup="true" CodeFile="signUp.aspx.cs" Inherits="Homemate_Matching.signUp" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Kaydol</title>
    <link href="css/SignIn.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="wrapper">
            <h2 class="mb-4">Kaydolma Formu</h2>

            <div class="input-box">
                <label for="TextBox1" class="form-label">Ad</label>
                <asp:TextBox ID="TextBox1" runat="server" CssClass="form-control" Placeholder="Ad Giriniz" />
            </div>

            <div class="input-box">
                <label for="TextBox2" class="form-label">Soyad</label>
                <asp:TextBox ID="TextBox2" runat="server" CssClass="form-control" Placeholder="Soyad Giriniz" />
            </div>

            <div class="input-box">
                <label for="TextBox3" class="form-label">Telefon Numarası</label>
                <asp:TextBox ID="TextBox3" runat="server" CssClass="form-control" Placeholder="Telefon Numarası Giriniz" />
            </div>

            <div class="input-box">
                <label for="TextBox4" class="form-label">Kullanıcı Adı</label>
                <asp:TextBox ID="TextBox4" runat="server" CssClass="form-control" Placeholder="Kullanıcı Adı Giriniz" />
            </div>

            <div class="input-box">
                <label for="TextBox5" class="form-label">Şifre</label>
                <asp:TextBox ID="TextBox5" runat="server" CssClass="form-control" Placeholder="Şifre Giriniz" />
            </div>

            <div class="input-box">
                <label for="TextBox6" class="form-label">Doğum Günü Giriniz (YYYY-AA-GG)</label>
                <asp:TextBox ID="TextBox6" runat="server" CssClass="form-control" Placeholder="Doğum Günü Giriniz (YYYY-AA-GG)" />
            </div>

            <asp:Button ID="Button1" runat="server" CssClass="btn btn-primary" Text="Kaydol" OnClick="Button1_Click" />
            <br /><br />
            <asp:Button ID="btnSignUp" runat="server" Text="Giriş Yap" CssClass="btn btn-primary" OnClick="btnSignIn_Click" />
            </div>

        <asp:Label ID="lblMessage" runat="server" ForeColor="Green"></asp:Label>

    </form>
</body>
</html>