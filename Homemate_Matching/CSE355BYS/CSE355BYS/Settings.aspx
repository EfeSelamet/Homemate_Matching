<%@ Page Language="C#" MasterPageFile="~/template.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="Homemate_Matching.WebForm3" %>

<%-- Bu bölüm, Master Page'deki head ContentPlaceHolder'ını doldurur --%>
<asp:Content ID="ContentHead" ContentPlaceHolderID="head" runat="server">
    <link href="~/css/settings-style.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div style="min-height: 100vh; display: flex; justify-content: center; align-items: flex-start; padding-top: 60px;">
        <div class="card shadow p-5 d-flex flex-column" style="width: 100%; max-width: 600px;">

            <asp:Label ID="lblMessage" runat="server" CssClass="mb-3" EnableViewState="false"></asp:Label>

            <p><strong>Kullanıcı Adı:</strong> <asp:Label ID="lblUsername" runat="server"></asp:Label></p>
            <p><strong>Şifre:</strong> <span style="letter-spacing: 0.3em;">••••••••</span></p>

            <div id="nameSurnameSection" class="mb-4" style="background-color: #f1f5f9; border-radius: 12px; padding: 20px; box-shadow: inset 0 0 10px #e2e8f0;">
                <h5 class="mb-3">İsim ve Soyisim Güncelle</h5>

                <p class="mb-1">Yeni İsim:</p>
                <asp:TextBox ID="txtNewName" runat="server" CssClass="form-control mb-2" placeholder="Yeni isminizi girin"></asp:TextBox>

                <p class="mb-1">Yeni Soyisim:</p>
                <asp:TextBox ID="txtNewSurname" runat="server" CssClass="form-control mb-3" placeholder="Yeni soyisminizi girin"></asp:TextBox>

                <asp:Button ID="btnSaveNameSurname" runat="server" Text="İsim ve Soyismi Kaydet" CssClass="btn btn-primary" OnClick="btnSaveNameSurname_Click" />
            </div>

            <br />

            <button id="btnTogglePassword" class="btn btn-outline-primary" type="button">Şifre Değiştir</button>
            <div id="passwordSection" style="display:none;" class="mb-4">
                <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control mb-2" placeholder="Yeni şifre" TextMode="Password"></asp:TextBox>
                <asp:Button ID="btnSavePassword" runat="server" Text="Kaydet" CssClass="btn btn-primary" OnClick="btnSavePassword_Click" />
            </div>
            <br />

            <button id="btnToggleDelete" class="btn btn-outline-danger" type="button">Hesabımı Sil</button>
            <div id="deleteSection" style="display:none;" class="mb-4">
                <div class="p-3 rounded" style="background-color: #fff3f3; border: 1px solid #f5c6cb;">
                    <p class="text-danger mb-3"><strong>Bu işlem geri alınamaz. Emin misiniz?</strong></p>
                    <asp:Button ID="btnConfirmDelete" runat="server" Text="Evet, hesabımı sil" CssClass="btn btn-danger btn-lg" OnClick="btnConfirmDelete_Click" />
                    <button type="button" class="btn btn-secondary" id="btnCancelDelete">Vazgeç</button>
                </div>
            </div>

        </div>
    </div>

    <script>
        window.onload = function () {
            // btnToggleUsername ve usernameSection sayfanda olmadığı için kaldırıldı

            document.getElementById("btnTogglePassword").addEventListener('click', function () {
                var section = document.getElementById("passwordSection");
                section.style.display = section.style.display === 'block' ? 'none' : 'block';
            });

            document.getElementById("btnToggleDelete").addEventListener('click', function () {
                var section = document.getElementById("deleteSection");
                section.style.display = section.style.display === 'block' ? 'none' : 'block';
            });

            document.getElementById("btnCancelDelete").addEventListener('click', function () {
                document.getElementById("deleteSection").style.display = 'none';
            });
        };
    </script>
</asp:Content>
