using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs; // Required for IRequest
using System.Security.Principal; // Required for IPrincipal

// Make sure you have a reference to Microsoft.AspNet.SignalR.Core

public class DatabaseUserIdProvider : IUserIdProvider
{
    public string GetUserId(IRequest request)
    {
        // This assumes your existing registration system authenticates users
        // and makes their username available through the standard IPrincipal interface.
        // For ASP.NET applications (including Web Forms, MVC with .NET Framework),
        // this is typically available via request.User.Identity.Name after login.

        if (request?.User?.Identity != null && request.User.Identity.IsAuthenticated)
        {
            // Use the username from your existing authentication system
            return request.User.Identity.Name;
        }

        // If the user is not authenticated, you might return null or
        // handle it differently based on your application's logic.
        // For private messaging, usually only authenticated users are relevant.
        return null;
    }
}