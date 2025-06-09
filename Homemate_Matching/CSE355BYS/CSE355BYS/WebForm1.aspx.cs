using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Homemate_Matching
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        // In WebForm1.aspx.cs
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (User.Identity.IsAuthenticated)
                {
                    // If displayName is a server control:
                    // displayName.Value = User.Identity.Name; // Assuming <input type="text" id="displayName" runat="server" />

                    // Or, if it's a client-side only input, register a script:
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "SetDisplayName",
                        $"document.getElementById('displayName').value = '{HttpUtility.JavaScriptStringEncode(User.Identity.Name)}';", true);
                }
            }
        }
    }
}