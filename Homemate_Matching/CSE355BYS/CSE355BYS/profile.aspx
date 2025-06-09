<%@ Page Language="C#" MasterPageFile="~/template.Master" AutoEventWireup="true" CodeFile="profile.aspx.cs" Inherits="Homemate_Matching.profile" %>

<asp:Content ContentPlaceHolderID="PageTitle" runat="server">
    My Profile
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-5">
        <div class="card shadow-lg border-0 rounded-lg mx-auto profile-card">
            
            <div class="card-header bg-white text-center py-4 rounded-top-lg profile-header-bg">
                <div class="profile-avatar mb-3">
                    <asp:Image ID="imgUserProfile" runat="server" AlternateText="Profile Picture" 
                               CssClass="img-fluid rounded-circle border border-white border-4 shadow-sm" />
                </div>
                <%-- The user's name will now be dynamically placed here from the code-behind --%>
                <asp:Label ID="lblName" runat="server" CssClass="text-white display-6" />
            </div>

            <div class="card-body p-4">

                <div class="section-container mb-4">
                    <h4 class="mb-3 text-primary fw-bold section-title d-flex align-items-center">
                        <i class="bi bi-info-circle me-2"></i> Hakkımda
                    </h4>
                    <div class="profile-section-box">
                        <asp:Label ID="lblAttributes" runat="server" />
                    </div>
                </div>
                
                <%-- UPDATED: My Home section is now wrapped in a Panel for visibility control --%>
                <asp:Panel ID="pnlMyHome" runat="server" Visible="false">
                    <div class="section-container mb-4">
                        <h4 class="mb-3 text-primary fw-bold section-title d-flex align-items-center">
                            <i class="bi bi-house-door me-2"></i> Evim
                        </h4>
                        <div class="profile-section-box">
                            <asp:Label ID="lblHouseInfo" runat="server" />
                            <%-- This div will contain the house pictures --%>
                            <div id="houseImagesDiv" runat="server" class="mt-3"></div>
                        </div>
                    </div>
                </asp:Panel>

                <div class="section-container mb-4">
                    <h4 class="mb-3 text-primary fw-bold section-title d-flex align-items-center">
                        <i class="bi bi-heart me-2"></i> Ev arkadaşı Tercihlerim
                    </h4>
                    <div class="profile-section-box">
                        <asp:Label ID="lblPreferences" runat="server" />
                    </div>
                </div>
                
                <hr class="my-5 divider-line" />

                <div class="d-grid gap-3 d-md-flex justify-content-md-center mt-4 profile-actions">
                    <asp:Button ID="userAttributeBtn" runat="server" Text="Hakkımdayı Değiştir" CssClass="btn btn-primary-custom" OnClick="userAttributeBtn_Click" />
                    <asp:Button ID="userPreferenceBtn" runat="server" Text="Tercihleri Değiştir" CssClass="btn btn-primary-custom" OnClick="userPreferenceBtn_Click" />
                    <asp:Button ID="matchUp" runat="server" Text="Yeni Ev Arkadaşı Bul" CssClass="btn btn-secondary-custom" OnClick="matchBtn_Click" />
                </div>
            </div>
        </div>
    </div>

    <style>
        /* Profile Specific Styles */

        .profile-card {
            max-width: 800px !important;
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15) !important;
            border-radius: 15px !important;
            overflow: visible; /* Changed to visible to allow avatar to pop out */
            background-color: rgba(255, 255, 255, 0.98);
            position: relative; /* For avatar positioning */
        }

        .profile-header-bg {
            background: linear-gradient(to right, #6a82fb, #fc5c7d); /* Vibrant gradient */
            position: relative;
            padding-top: 6rem !important; /* Space for avatar */
            padding-bottom: 2rem !important;
            border-bottom: none !important;
            border-top-left-radius: 15px; /* Ensure top corners are rounded */
            border-top-right-radius: 15px;
        }

        .profile-avatar {
            position: absolute;
            top: 0;
            left: 50%;
            transform: translate(-50%, -50%); /* Center horizontally and half way up */
            background-color: white; /* Background for the border */
            padding: 5px; /* Creates the white ring */
            border-radius: 50%;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
            z-index: 10;
        }

        .profile-avatar img {
            width: 180px; /* Slightly larger avatar */
            height: 180px;
            object-fit: cover;
            border-radius: 50%;
            border: 4px solid #fff; /* White border around the image itself */
        }

        .profile-header-bg h3, .profile-header-bg p {
            color: white !important;
            text-shadow: 0 1px 3px rgba(0,0,0,0.2);
            position: relative;
            z-index: 11;
        }
        .profile-header-bg h3 {
            font-size: 2.5rem; /* Larger name */
        }
        .profile-header-bg p {
            font-size: 1.2rem; /* Larger status */
        }


        .profile-info-grid .profile-detail-item {
            background-color: #f8f9fa;
            border: 1px solid #e9ecef;
            border-radius: 10px; /* Slightly more rounded */
            padding: 18px 20px; /* More padding */
            margin-bottom: 15px;
            transition: all 0.2s ease;
        }
        .profile-info-grid .profile-detail-item:hover {
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1); /* More pronounced shadow on hover */
            transform: translateY(-3px); /* More pronounced lift */
        }

        .profile-detail-item label {
            font-size: 1rem; /* Slightly larger label */
            display: flex;
            align-items: center;
            color: #6c757d; /* Darker muted text */
            font-weight: 500;
        }
        .profile-detail-item label i {
            font-size: 1.2rem; /* Larger icon */
            margin-right: 10px;
            color: #0d6efd; /* Bootstrap primary blue for icons */
        }
        
        .profile-section-box {
            background-color: #ffffff;
            border: 1px solid #e0e0e0;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
            padding: 20px !important;
            border-radius: 10px !important;
        }

        .section-container {
            border: 1px solid #e9ecef; /* Light border around the whole section container */
            border-radius: 12px;
            padding: 20px;
            background-color: #ffffff; /* White background for the whole section block */
            box-shadow: 0 4px 15px rgba(0,0,0,0.08); /* More prominent shadow for sections */
        }
        .section-container:not(:last-child) {
            margin-bottom: 30px; /* Space between section containers */
        }

        .section-title {
            margin-top: 0 !important; /* Reset default margin */
            margin-bottom: 15px !important;
            color: #183153 !important;
            font-size: 1.7rem; /* Slightly larger section titles */
            display: flex;
            align-items: center;
            padding-bottom: 10px; /* Space for border-bottom */
            border-bottom: 2px solid #f0f2f5; /* Subtle separation line */
        }
        .section-title i {
            font-size: 1.8rem; /* Larger icon for titles */
            color: #183153; /* Match title color */
        }
        
        /* Toggle button for collapsible sections */
        .edit-section-btn {
            color: #6c757d; /* Muted color */
            transition: transform 0.3s ease;
            text-decoration: none;
            font-size: 1.2rem;
            line-height: 1; /* Prevent icon from jumping */
        }
        .edit-section-btn i {
            font-size: 1.2rem;
        }
        .edit-section-btn[aria-expanded="true"] i {
            transform: rotate(180deg); /* Rotate arrow when expanded */
        }


        .divider-line {
            border-top: 1px dashed #ced4da;
            margin-top: 3.5rem !important;
            margin-bottom: 3.5rem !important;
        }

        /* Custom Button Styles (re-iterated for clarity, but ideally in templateMaster.css) */
        .btn-primary-custom, .btn-secondary-custom {
            background-color: #183153; /* Dark Blue */
            color: white;
            border: none;
            padding: 12px 28px; /* Slightly more padding */
            border-radius: 30px;
            font-weight: 600;
            cursor: pointer;
            transition: all 0.3s ease-in-out;
            box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
        }

        .btn-primary-custom:hover {
            background-color: #ffd401; /* Yellow */
            color: #183153;
            transform: translateY(-2px);
            box-shadow: 0 6px 15px rgba(0, 0, 0, 0.15);
        }

        .btn-secondary-custom {
            background-color: #28a745; /* Green for "Find Homemates" */
            border-color: #28a745;
        }
        .btn-secondary-custom:hover {
            background-color: #218838;
            border-color: #1e7e34;
        }

        /* Badge/Pill Styles for Attributes/Preferences (if you implement them) */
        .badge {
            padding: 0.6em 1em;
            border-radius: 20px; /* Pill shape */
            font-size: 0.9em;
            font-weight: 600;
        }
        .bg-primary-subtle { background-color: #e0f2f7 !important; }
        .text-primary { color: #0d6efd !important; }

        .bg-secondary-subtle { background-color: #e9ecef !important; }
        .text-secondary { color: #6c757d !important; }

        .bg-info-subtle { background-color: #e2f4ff !important; }
        .text-info { color: #0dcaf0 !important; }

        .bg-success-subtle { background-color: #d1e7dd !important; }
        .text-success { color: #198754 !important; }

        .bg-warning-subtle { background-color: #fff3cd !important; }
        .text-warning { color: #ffc107 !important; }

        .bg-danger-subtle { background-color: #f8d7da !important; }
        .text-danger { color: #dc3545 !important; }

        /* Responsive adjustments for profile page */
        @media (max-width: 576px) {
            .profile-header-bg {
                padding-top: 4rem !important;
                padding-bottom: 1.5rem !important;
            }
            .profile-avatar img {
                width: 120px;
                height: 120px;
            }
            .profile-header-bg h3 {
                font-size: 2rem;
            }
            .profile-header-bg p {
                font-size: 1rem;
            }
            .section-title {
                font-size: 1.4rem;
            }
            .profile-actions .btn {
                width: 100%;
                margin-bottom: 10px;
                padding: 10px 20px;
            }
            .section-container {
                padding: 15px;
            }
        }

            .attributes-grid {
                display: grid;
                /* This creates a responsive grid that adds more columns as space allows */
                grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
                gap: 12px; /* Space between items */
                padding-top: 10px;
            }

            .attribute-item {
                display: flex;
                align-items: center;
                background-color: #f8f9fa; /* Light grey background */
                padding: 12px 15px;
                border-radius: 8px; /* Rounded corners */
                border: 1px solid #e9ecef; /* Subtle border */
                transition: all 0.2s ease-in-out;
            }

            .attribute-item:hover {
                transform: translateY(-2px); /* Slight lift on hover */
                box-shadow: 0 4px 8px rgba(0,0,0,0.05); /* Soft shadow on hover */
            }

            .attribute-icon {
                font-size: 1.4rem; /* Larger icon size */
                margin-right: 12px;
                line-height: 1;
            }

            .attribute-label {
                font-weight: 600; /* Bolder label */
                color: #495057;
                margin-right: 8px;
            }

            .attribute-value {
                color: #212529;
            }
    </style>
</asp:Content>