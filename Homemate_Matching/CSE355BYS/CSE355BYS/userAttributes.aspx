<%@ Page Language="C#" MasterPageFile="~/template.Master" AutoEventWireup="true" CodeFile="userAttributes.aspx.cs" Inherits="Homemate_Matching.userAttributes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-5" style="max-width: 720px;">
        <div class="card shadow">
            <div class="card-header bg-primary text-white">
                <h3 class="mb-0">Özelliklerini Gir</h3>
            </div>
            <div class="card-body">

                <asp:Label ID="lblMessage" runat="server" ForeColor="Red" CssClass="d-block mb-3" />

                <div class="mb-4">
                    <label class="form-label fw-semibold">Cinsiyet</label><br />
                    <div class="form-check form-check-inline">
                        <input class="form-check-input" type="radio" name="genderOptions" id="genderMale" value="m"
                               onchange="document.getElementById('<%= txtGender.ClientID %>').value='m';">
                        <label class="form-check-label" for="genderMale">Erkek</label>
                    </div>
                    <div class="form-check form-check-inline">
                        <input class="form-check-input" type="radio" name="genderOptions" id="genderFemale" value="f"
                               onchange="document.getElementById('<%= txtGender.ClientID %>').value='f';">
                        <label class="form-check-label" for="genderFemale">Kadın</label>
                    </div>
                    <asp:TextBox ID="txtGender" runat="server" CssClass="form-control" Style="display:none;" />
                </div>

                <div class="mb-4">
                    <label class="form-label fw-semibold">Uyku Düzeni</label>
                    <asp:DropDownList ID="ddlSleepSchedule" runat="server" CssClass="form-control">
                        <asp:ListItem Text="Sabah Kuşu" Value="Sabah Kuşu" />
                        <asp:ListItem Text="Gece Kuşu" Value="Gece Kuşu" />
                        <%-- "Önemsiz" seçeneği kaldırıldı --%>
                    </asp:DropDownList>
                </div>

                <div class="mb-3 form-check">
                    <asp:CheckBox ID="chkSmoker" runat="server" CssClass="form-check-input" />
                    <label class="form-check-label" for="<%= chkSmoker.ClientID %>">Sigara kullanırım</label>
                </div>

                <div class="mb-3 form-check">
                    <asp:CheckBox ID="chkDrinker" runat="server" CssClass="form-check-input" />
                    <label class="form-check-label" for="<%= chkDrinker.ClientID %>">Alkol kullanırım</label>
                </div>

                <div class="mb-3 form-check">
                    <asp:CheckBox ID="chkIsStudent" runat="server" CssClass="form-check-input" />
                    <label class="form-check-label" for="<%= chkIsStudent.ClientID %>">Öğrenci misin?</label>
                </div>

                <div class="mb-4">
                    <label class="form-label fw-semibold">Hijyen (1-10): <span id="sliderHygieneValue"><%= string.IsNullOrEmpty(txtHygiene.Text) ? "5" : txtHygiene.Text %></span></label>
                    <input type="range" id="sliderHygiene" class="form-range" min="1" max="10"
                           value="<%= string.IsNullOrEmpty(txtHygiene.Text) ? "5" : txtHygiene.Text %>"
                           oninput="document.getElementById('<%= txtHygiene.ClientID %>').value = this.value; document.getElementById('sliderHygieneValue').textContent = this.value;" />
                    <asp:TextBox ID="txtHygiene" runat="server" Style="display:none;" />
                </div>

                <div class="mb-4">
                    <label class="form-label fw-semibold">Gürültü (1-10): <span id="sliderNoisinessValue"><%= string.IsNullOrEmpty(txtNoisiness.Text) ? "5" : txtNoisiness.Text %></span></label>
                    <input type="range" id="sliderNoisiness" class="form-range" min="1" max="10"
                           value="<%= string.IsNullOrEmpty(txtNoisiness.Text) ? "5" : txtNoisiness.Text %>"
                           oninput="document.getElementById('<%= txtNoisiness.ClientID %>').value = this.value; document.getElementById('sliderNoisinessValue').textContent = this.value;" />
                    <asp:TextBox ID="txtNoisiness" runat="server" Style="display:none;" />
                </div>

                <div class="mb-4">
                    <label class="form-label fw-semibold">Misafir Sıklığı (1-10): <span id="sliderGuestFrequencyValue"><%= string.IsNullOrEmpty(txtGuestFrequency.Text) ? "5" : txtGuestFrequency.Text %></span></label>
                    <input type="range" id="sliderGuestFrequency" class="form-range" min="1" max="10"
                           value="<%= string.IsNullOrEmpty(txtGuestFrequency.Text) ? "5" : txtGuestFrequency.Text %>"
                           oninput="document.getElementById('<%= txtGuestFrequency.ClientID %>').value = this.value; document.getElementById('sliderGuestFrequencyValue').textContent = this.value;" />
                    <asp:TextBox ID="txtGuestFrequency" runat="server" Style="display:none;" />
                </div>

                <%-- Profile Picture Upload Section --%>
                <div class="mb-4">
                    <label class="form-label fw-semibold">Profil Fotoğrafı Yükle</label>
                    <asp:FileUpload ID="fileUploadProfilePic" runat="server" CssClass="form-control" />
                    <asp:Button ID="btnUpload" runat="server" Text="Fotoğraf Yükle" OnClick="btnUpload_Click" CssClass="btn btn-secondary mt-2" />
                    <asp:Image ID="imgProfile" runat="server" Width="150px" Height="150px" Visible="false" CssClass="mt-3 img-thumbnail" />
                    <asp:Label ID="lblUploadStatus" runat="server" ForeColor="Red" CssClass="d-block mt-2"></asp:Label>
                </div>
                
                <div class="mb-3 form-check">
                    <asp:CheckBox ID="chkHasHouse" runat="server" CssClass="form-check-input" AutoPostBack="true" OnCheckedChanged="chkHasHouse_CheckedChanged" />
                    <label class="form-check-label" for="<%= chkHasHouse.ClientID %>">Kalacak Hazır evin var mı?</label>
                </div>

                <asp:Panel ID="pnlHouseInfo" runat="server" Visible="false" CssClass="bg-light p-3 rounded border">
                    <h5 class="fw-bold mt-3 mb-3">Ev Bilgileri</h5>

                    <div class="row g-3">
                        <div class="col-md-6">
                            <label class="form-label" for="<%= txtRent.ClientID %>">Kira</label>
                            <asp:TextBox ID="txtRent" runat="server" CssClass="form-control" TextMode="Number" min="0" step="1"/>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label" for="<%= ddlLocation.ClientID %>">Konum</label>
                            <asp:DropDownList ID="ddlLocation" runat="server" CssClass="form-control">
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
                                <%-- "Önemsiz" seçeneği kaldırıldı --%>
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label" for="<%= txtRooms.ClientID %>">Oda sayısı</label>
                            <asp:TextBox ID="txtRooms" runat="server" CssClass="form-control" TextMode="Number" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label" for="<%= txtSaloons.ClientID %>">Salon sayısı</label>
                            <asp:TextBox ID="txtSaloons" runat="server" CssClass="form-control" TextMode="Number" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label" for="<%= txtSurfaceArea.ClientID %>">Alan (m²)</label>
                            <asp:TextBox ID="txtSurfaceArea" runat="server" CssClass="form-control" TextMode="Number" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label" for="<%= txtFlatFloor.ClientID %>">Dairedeki Kat Sayısı</label>
                            <asp:TextBox ID="txtFlatFloor" runat="server" CssClass="form-control" TextMode="Number" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label" for="<%= txtBuildingFloor.ClientID %>">Binadaki Kat Sayısı</label>
                            <asp:TextBox ID="txtBuildingFloor" runat="server" CssClass="form-control" TextMode="Number" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label" for="<%= txtBuildingFloorCount.ClientID %>">Daire Kaçıncı Katta</label>
                            <asp:TextBox ID="txtBuildingFloorCount" runat="server" CssClass="form-control" TextMode="Number" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label" for="<%= txtHeating.ClientID %>">Isıtma Tipi</label>
                            <asp:TextBox ID="txtHeating" runat="server" CssClass="form-control" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label" for="<%= txtBuildingAge.ClientID %>">Bina Yaşı</label>
                            <asp:TextBox ID="txtBuildingAge" runat="server" CssClass="form-control" TextMode="Number" />
                        </div>
                        <div class="col-12">
                            <label class="form-label" for="<%= txtDescription.ClientID %>">Ek Bilgi</label>
                            <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                        </div>

                        <div class="col-12">
                            <label for="housePictures">Ev Fotoğrafı Yükle:</label>
                            <input type="file" id="housePictures" name="housePictures" accept=".jpg,.jpeg,.png" multiple />
                            <asp:Button ID="Button1" runat="server" Text="Fotoğraf Yükle" OnClick="btnUploadHousePics_Click" />
                            <asp:Button ID="Button2" runat="server" Text="Fotoğrafları Sil" OnClick="btnDeleteHousePics_Click" />
                        </div>

                    </div>
                </asp:Panel>

                <div class="mt-4">
                    <asp:Button ID="btnSubmit" runat="server" Text="Kaydet" CssClass="btn btn-primary w-100" OnClick="btnSubmit_Click" />
                </div>

                <asp:Label ID="Label1" runat="server" ForeColor="green" CssClass="mt-3 d-block" />
            </div>
        </div>
    </div>
</asp:Content>