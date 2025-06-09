using System;
using System.Web; // Required for HttpUtility
using System.Web.UI;

namespace Homemate_Matching
{
    public partial class WebForm2 : System.Web.UI.Page // Assuming WebForm2 is message.aspx
    {
        // This public property can be accessed from the ASPX page if needed,
        // but we'll use RegisterStartupScript for cleaner JS integration.
        public string CurrentAuthenticatedUserName { get; private set; } = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (User.Identity.IsAuthenticated)
                {
                    CurrentAuthenticatedUserName = User.Identity.Name;
                    // Make the username available to client-side JavaScript
                    // HttpUtility.JavaScriptStringEncode is important to prevent XSS if username has special characters
                    string script = $"var currentAuthenticatedUser = '{HttpUtility.JavaScriptStringEncode(CurrentAuthenticatedUserName)}';";
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "SetCurrentUserScript", script, true);
                }
                else
                {
                    // If not authenticated, redirect to login or handle appropriately
                    // For this chat page, authentication is essential for user identification
                    // Response.Redirect("~/LoginPage.aspx"); // Example
                    // For now, we'll let the client-side handle UI for non-auth users gracefully,
                    // but server-side SignalR will rely on Context.User.Identity.Name.
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "SetCurrentUserScript", "var currentAuthenticatedUser = null;", true);
                }
            }
        }
    }
}