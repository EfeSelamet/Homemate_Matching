<%@ Page Language="C#" MasterPageFile="~/template.Master" AutoEventWireup="true" CodeBehind="matchUp.aspx.cs" Inherits="Homemate_Matching.matchUp" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5">
        <asp:Panel ID="pnlMatchContent" runat="server">
            <div class="card shadow-lg border-0 rounded-lg mx-auto profile-card">
                <div class="card-header bg-white text-center py-4 rounded-top-lg profile-header-bg">
                    <div class="profile-avatar mb-3">
                        <asp:Image ID="imgPotUserProfile" runat="server" AlternateText="Potential Match Profile Picture"
                                   CssClass="img-fluid rounded-circle border border-white border-4 shadow-sm" />
                    </div>
                    <asp:Label ID="lblName" runat="server" CssClass="text-white display-6" />
                </div>
                <div class="card-body p-4">
                    <div class="section-container mb-4">
                        <h4 class="section-title"><i class="bi bi-person-lines-fill me-2"></i>Kullanıcı Bilgileri</h4>
                        <div class="profile-section-box">
                            <asp:Label ID="lblBirthDate" runat="server" CssClass="d-block" />
                        </div>
                    </div>
                    <div class="section-container">
                        <h4 class="section-title"><i class="bi bi-person-check-fill me-2"></i>Kullanıcı Özellikleri</h4>
                        <div class="profile-section-box">
                            <asp:Label ID="lblAttributes" runat="server" />
                        </div>
                    </div>
                    <asp:Panel ID="pnlHouseInfo" runat="server" Visible="false">
                        <div class="section-container">
                            <h4 class="section-title"><i class="bi bi-house-door-fill me-2"></i>Ev Bilgileri</h4>
                            <div class="profile-section-box">
                                <asp:Label ID="lblHouseInfo" runat="server" />
                                <%-- This div will contain the house pictures --%>
                                <div id="houseImagesDiv" runat="server" class="mt-3"></div>
                            </div>
                        </div>
                    </asp:Panel>

                    <asp:Label ID="lblError" runat="server" ForeColor="Red" CssClass="d-block text-center mt-3" />
                    <div class="match-buttons d-flex justify-content-around mt-5">
                        <%-- UPDATED: Changed from asp:Button to asp:LinkButton to render HTML icons --%>
                        <asp:LinkButton ID="rejectBtn" runat="server" CssClass="btn btn-reject" OnClick="rejectBtn_Click">
                            <i class='bi bi-x-lg me-2'></i>Reddet
                        </asp:LinkButton>
                        <asp:LinkButton ID="acceptBtn" runat="server" CssClass="btn btn-accept" OnClick="acceptBtn_Click">
                            <i class='bi bi-heart-fill me-2'></i>Kabul Et
                        </asp:LinkButton>
                    </div>
                </div>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlNoMatches" runat="server" Visible="false">
             <div class="card shadow-lg border-0 rounded-lg mx-auto profile-card text-center p-5">
                 <h2 class="display-4 text-muted"><i class="bi bi-people-fill"></i></h2>
                 <h3 class="mt-3">Başka Kullanıcı Yok</h3>
                 <p class="text-muted mt-2">Eþleþen Herkesi Görüntülediniz, Daha Sonra Tekarar Gelin.</p>
            </div>
        </asp:Panel>
    </div>
    <style>
    /* --- Styles copied from profile.aspx for consistency --- */
    .profile-card {
        max-width: 800px !important;
        box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15) !important;
        border-radius: 15px !important;
        overflow: visible;
        background-color: rgba(255, 255, 255, 0.98);
        position: relative;
        /* UPDATED: Added margin to push card down from the top nav bar */
        margin-top: 6rem;
    }

    .profile-header-bg {
        background: linear-gradient(to right, #6a82fb, #fc5c7d);
        position: relative;
        padding-top: 6rem !important;
        padding-bottom: 2rem !important;
        border-bottom: none !important;
        border-top-left-radius: 15px;
        border-top-right-radius: 15px;
    }

    .profile-avatar {
        position: absolute;
        top: 0;
        left: 50%;
        transform: translate(-50%, -50%);
        background-color: white;
        padding: 5px;
        border-radius: 50%;
        box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
        z-index: 10;
    }

    .profile-avatar img, .profile-avatar a img { /* Target image inside link too */
        width: 180px;
        height: 180px;
        object-fit: cover;
        border-radius: 50%;
    }
    
    .section-container {
        margin-bottom: 20px;
    }
    
    .section-title {
        font-size: 1.2rem;
        font-weight: bold;
        color: #183153;
        margin-bottom: 10px;
        padding-bottom: 5px;
        border-bottom: 2px solid #f0f2f5;
    }

    .profile-section-box {
        background-color: #f8f9fa;
        border: 1px solid #e0e0e0;
        padding: 15px;
        border-radius: 10px;
    }

    /* --- Attribute Grid Style (copied from previous task) --- */
    .attributes-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
        gap: 12px;
    }
    .attribute-item {
        display: flex; align-items: center; background-color: #ffffff; padding: 10px; border-radius: 8px; border: 1px solid #dee2e6;
    }
    .attribute-icon { font-size: 1.4rem; margin-right: 12px; }
    .attribute-label { font-weight: 600; color: #495057; margin-right: 8px; }

    /* --- Button Styles for Accept/Reject --- */
    .match-buttons .btn {
        font-size: 1.2rem;
        font-weight: bold;
        padding: 12px 30px;
        border-radius: 50px;
        width: 180px;
        transition: all 0.3s ease;
        border: none;
        box-shadow: 0 4px 15px rgba(0,0,0,0.1);
        display: inline-flex; /* Helps align icon and text */
        align-items: center;
        justify-content: center;
    }
    
    .btn-accept {
        background-color: #28a745;
        color: white;
    }
    .btn-accept:hover {
        background-color: #218838;
        color: white; /* Ensure text color persists on hover */
        transform: translateY(-3px);
        box-shadow: 0 6px 20px rgba(40, 167, 69, 0.4);
    }

    .btn-reject {
        background-color: #dc3545;
        color: white;
    }
    .btn-reject:hover {
        background-color: #c82333;
        color: white; /* Ensure text color persists on hover */
        transform: translateY(-3px);
        box-shadow: 0 6px 20px rgba(220, 53, 69, 0.4);
    }

    .house-info-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
    gap: 12px;
    }

    .house-item {
        display: flex;
        align-items: flex-start; /* Align icon to the top of the text */
        background-color: #ffffff;
        padding: 10px 12px;
        border-radius: 8px;
        border: 1px solid #dee2e6;
    }

    /* This class makes the description span the full width of the grid */
    .house-item-full {
        grid-column: 1 / -1;
    }

    .house-icon {
        font-size: 1.4rem;
        margin-right: 12px;
        line-height: 1.2; /* Adjust line height for better alignment */
        color: #0d6efd; /* Use a theme color for icons */
    }

    .house-item strong {
        font-weight: 600;
        color: #495057;
        margin-right: 5px;
    }
</style>
</asp:Content>