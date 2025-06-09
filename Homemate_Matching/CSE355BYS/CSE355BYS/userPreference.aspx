<%@ Page Language="C#" MasterPageFile="~/template.Master" AutoEventWireup="true" CodeBehind="userPreference.aspx.cs" Inherits="Homemate_Matching.userPreferences" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-5" style="max-width: 720px;">
        <div class="card shadow">
            <div class="card-header bg-primary text-white">
                <h3 class="mb-0">Kullanıcı Tercihleri</h3>
            </div>
            <div class="card-body">

                <asp:Label ID="lblMessage" runat="server" ForeColor="Red" CssClass="d-block mb-3" />

                <div class="mb-4">
                    <label class="form-label fw-semibold">Cinsiyet Tercihi</label><br />
                    <div class="form-check form-check-inline">
                        <input type="radio" class="form-check-input" name="genderPref" id="genderPrefMaleRadio" onclick="document.getElementById('<%= txtGenderPref.ClientID %>').value='m';" />
                        <label class="form-check-label" for="genderPrefMaleRadio">Erkek</label>
                    </div>
                    <div class="form-check form-check-inline">
                        <input type="radio" class="form-check-input" name="genderPref" id="genderPrefFemaleRadio" onclick="document.getElementById('<%= txtGenderPref.ClientID %>').value='f';" />
                        <label class="form-check-label" for="genderPrefFemaleRadio">Kadın</label>
                    </div>
                    <div class="form-check form-check-inline">
                        <input type="radio" class="form-check-input" name="genderPref" id="genderPrefIrrelevantRadio" onclick="document.getElementById('<%= txtGenderPref.ClientID %>').value='i';" />
                        <label class="form-check-label" for="genderPrefIrrelevantRadio">Önemsiz</label>
                    </div>
                    <asp:TextBox ID="txtGenderPref" runat="server" Style="display:none;" />
                </div>

                <div class="mb-4">
                    <label class="form-label fw-semibold">Ev Arkadaşının Tercih Edilen Uyku Düzeni</label>
                    <asp:DropDownList ID="ddlSleepPref" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Sabah Kuşu" Value="Sabah Kuşu" />
                        <asp:ListItem Text="Gece Kuşu" Value="Gece Kuşu" />
                        <asp:ListItem Text="Önemsiz" Value="Önemsiz" />
                    </asp:DropDownList>
                </div>

                <div class="mb-3 form-check">
                    <asp:CheckBox ID="chkSmokePref" runat="server" CssClass="form-check-input" />
                    <label class="form-check-label" for="<%= chkSmokePref.ClientID %>">Sigara kullanmasın</label>
                </div>

                <div class="mb-3 form-check">
                    <asp:CheckBox ID="chkDrinkPref" runat="server" CssClass="form-check-input" />
                    <label class="form-check-label" for="<%= chkDrinkPref.ClientID %>">Alkol kullanmasın</label>
                </div>
                <div class="mb-3 form-check">
                    <asp:CheckBox ID="chkStudentPref" runat="server" CssClass="form-check-input" />
                    <label class="form-check-label" for="<%= chkStudentPref.ClientID %>">Öğrenci Olsun</label>
                </div>
                <div class="mb-4">
                    <label class="form-label fw-semibold">Minimum Kira Bütçesi</label>
                    <asp:TextBox ID="txtMinBudget" runat="server" CssClass="form-control" TextMode="Number" min="0" step="1"/>
                </div>

                <div class="mb-4">
                    <label class="form-label fw-semibold">Maximum Kira Bütçesi</label>
                    <asp:TextBox ID="txtMaxBudget" runat="server" CssClass="form-control" TextMode="Number" min="0" step="1"/>
                </div>

                <div class="mb-4">
                    <label class="form-label fw-semibold">Evin Tercih Edilen Konumu</label>
                    <asp:DropDownList ID="ddlLocationPref" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Ataşehir" Value="Ataşehir" />
                        <asp:ListItem Text="Başakşehir" Value="Başakşehir" />
                        <asp:ListItem Text="Bağcılar" Value="Bağcılar" />
                        <asp:ListItem Text="Bahçelievler" Value="Bahçelievler" />
                        <asp:ListItem Text="Avcılar" Value="Avcılar" />
                        <asp:ListItem Text="Beylikdüzü" Value="Beylikdüzü" />
                        <asp:ListItem Text="Kadıköy" Value="Kadıköy" />
                        <asp:ListItem Text="Kartal" Value="Kartal" />
                        <asp:ListItem Text="Pendik" Value="Pendik" />
                        <asp:ListItem Text="Üsküdar" Value="Üsküdar" />
                        <asp:ListItem Text="Önemsiz" Value="Önemsiz" />
                    </asp:DropDownList>
                </div>

                <div class="mb-4">
                    <label class="form-label fw-semibold">Hijyenin Önemi(1-10): <span id="sliderHygienePrefValue"><%= string.IsNullOrEmpty(txtHygieneImportance.Text) ? "5" : txtHygieneImportance.Text %></span></label>
                    <input type="range" id="sliderHygienePref" class="form-range" min="1" max="10"
                           value="<%= string.IsNullOrEmpty(txtHygieneImportance.Text) ? "5" : txtHygieneImportance.Text %>"
                           oninput="document.getElementById('<%= txtHygieneImportance.ClientID %>').value = this.value; document.getElementById('sliderHygienePrefValue').textContent = this.value;" />
                    <asp:TextBox ID="txtHygieneImportance" runat="server" Style="display:none;" />
                </div>

                <div class="mb-4">
                    <label class="form-label fw-semibold">Ses Hassasiyeti(1-10): <span id="sliderNoisePrefValue"><%= string.IsNullOrEmpty(txtNoiseSensitivity.Text) ? "5" : txtNoiseSensitivity.Text %></span></label>
                    <input type="range" id="sliderNoisePref" class="form-range" min="1" max="10"
                           value="<%= string.IsNullOrEmpty(txtNoiseSensitivity.Text) ? "5" : txtNoiseSensitivity.Text %>"
                           oninput="document.getElementById('<%= txtNoiseSensitivity.ClientID %>').value = this.value; document.getElementById('sliderNoisePrefValue').textContent = this.value;" />
                    <asp:TextBox ID="txtNoiseSensitivity" runat="server" Style="display:none;" />
                </div>

                <div class="mb-4">
                    <label class="form-label fw-semibold">Misafir Tahammül Seviyesi(1-10): <span id="sliderGuestPrefValue"><%= string.IsNullOrEmpty(txtGuestPref.Text) ? "5" : txtGuestPref.Text %></span></label>
                    <input type="range" id="sliderGuestPref" class="form-range" min="1" max="10"
                           value="<%= string.IsNullOrEmpty(txtGuestPref.Text) ? "5" : txtGuestPref.Text %>"
                           oninput="document.getElementById('<%= txtGuestPref.ClientID %>').value = this.value; document.getElementById('sliderGuestPrefValue').textContent = this.value;" />
                    <asp:TextBox ID="txtGuestPref" runat="server" Style="display:none;" />
                </div>


                <div class="mt-4">
                    <asp:Button ID="btnSubmitPref" runat="server" Text="Tercihleri Kaydet" CssClass="btn btn-primary w-100" OnClick="btnSubmitPref_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>