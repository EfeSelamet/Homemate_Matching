using Homemate_Matching.Data;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// Assuming your DAL and ViewModels are in a namespace like YourProjectName.Data
// using YourProjectName.Data;

namespace Homemate_Matching.Hubs // Or YourProjectName.Hubs
{
    public class ChatHub : Hub
    {
        private readonly static ChatDataAccess _dal = new ChatDataAccess(); // Instantiate your DAL
        // Or use dependency injection if you have it set up

        // Method to get ALL registered users
        // This method now returns List<UserViewModel> from Homemate_Matching.Data
        public List<UserViewModel> GetAllRegisteredUsers()
        {
            return _dal.GetAllUsers();
        }
        public List<UserViewModel> GetAllMatchedUsers(string username)
        {
            return _dal.GetAllMatchedUsers(username);
        }
        // This method now returns List<MessageViewModel> from Homemate_Matching.Data
        public List<MessageViewModel> GetMessageHistory(string otherUsername)
        {
            string currentUsername = Context.User.Identity.Name;
            if (string.IsNullOrEmpty(currentUsername) || string.IsNullOrEmpty(otherUsername))
            {
                return new List<MessageViewModel>();
            }
            return _dal.GetMessageHistory(currentUsername, otherUsername);
        }

        public async Task SendPrivateMessage(string recipientUsername, string messageContent)
        {
            string senderUsername = Context.User.Identity.Name;

            if (string.IsNullOrEmpty(senderUsername) || string.IsNullOrEmpty(recipientUsername) || string.IsNullOrEmpty(messageContent))
            {
                Clients.Caller.receiveErrorMessage("Invalid message, sender, or recipient.");
                return;
            }

            DateTime messageTimestamp = DateTime.UtcNow; // Use UTC for consistency

            // 1. Save the message to the database
            try
            {
                _dal.SaveMessage(senderUsername, recipientUsername, messageContent, messageTimestamp);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.Error.WriteLine($"Error saving message: {ex.Message}");
                Clients.Caller.receiveErrorMessage("Failed to send message. Please try again.");
                // Optionally, do not send the real-time message if DB save fails,
                // or notify the user that it wasn't saved.
                return;
            }

            // 2. Send the real-time message to the recipient
            Clients.User(recipientUsername).receivePrivateMessage(senderUsername, recipientUsername, messageContent, messageTimestamp, false); // false for isEcho

            // 3. Send an echo of the message back to the Sender
            Clients.Caller.receivePrivateMessage(senderUsername, recipientUsername, messageContent, messageTimestamp, true); // true for isEcho
        }

        // Your OnConnected and OnDisconnected methods can still be used to manage
        // a list of *currently online* users if you want to display an "online" status indicator.
        // For simplicity, I'm omitting that from this direct answer, but it would be a good addition.
        // Example:
        private static readonly HashSet<string> OnlineUsers = new HashSet<string>();

        public override Task OnConnected()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                string username = Context.User.Identity.Name;
                OnlineUsers.Add(username);
                // Could also call: Clients.All.userOnline(username);
                Clients.All.userListUpdated(GetAllRegisteredUsersWithOnlineStatus()); // Send updated list with online status
            }
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                string username = Context.User.Identity.Name;
                OnlineUsers.Remove(username);
                // Could also call: Clients.All.userOffline(username);
                Clients.All.userListUpdated(GetAllRegisteredUsersWithOnlineStatus()); // Send updated list with online status
            }
            return base.OnDisconnected(stopCalled);
        }

        // Helper to merge all users with online status
        private List<UserViewModel> GetAllRegisteredUsersWithOnlineStatus()
        {
            var allUsers = _dal.GetAllMatchedUsers(Context.User.Identity.Name);
            foreach (var user in allUsers) { user.IsOnline = OnlineUsers.Contains(user.Username); }
            return allUsers; // Assuming UserViewModel has an IsOnline property
        }
        // Note: The receivePrivateMessage client method will need to accept timestamp too.
    }

}