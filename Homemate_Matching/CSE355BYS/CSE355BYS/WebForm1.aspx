<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="Homemate_Matching.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>My Chat Page</title>
    <style>
        /* Basic styling for layout */
        .chat-container { display: flex; }
        .main-chat { flex-grow: 1; padding-right: 20px; }
        .user-list-container { width: 200px; border-left: 1px solid #ccc; padding-left: 10px; height: 400px; overflow-y: auto; }
        .user-list-container h3 { margin-top: 0; }
        #userList li { cursor: pointer; padding: 5px; }
        #userList li:hover { background-color: #f0f0f0; }
        .private-chat-area { margin-top: 20px; border-top: 1px dashed #ccc; padding-top: 10px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="chat-container">
            <div class="main-chat">
                <h2>Simple Chat (Public)</h2>
                <div id="chatArea" style="border: 1px solid #ccc; height: 200px; overflow-y: scroll; margin-bottom: 10px; padding: 5px;">
                </div>
                <div>
                    <%--
                        If using Forms Authentication and IUserIdProvider,
                        this displayName field should ideally be pre-filled with the authenticated username
                        from server-side code (e.g., in Page_Load of WebForm1.aspx.cs) if the user is logged in.
                        Example for Page_Load:
                        if (User.Identity.IsAuthenticated) {
                            displayName.Value = User.Identity.Name; // Assuming displayName is <input type="text" id="displayName" runat="server" />
                            // Or if it's not runat="server", you can register a startup script to set its value.
                        }
                        For this example, we'll keep it as a manual input for simplicity of the ASPX-only change,
                        but be aware of this for a real authenticated system.
                    --%>
                    <label for="displayName">Your Name (for public chat):</label>
                    <input type="text" id="displayName" />
                </div>
                <div>
                    <label for="message">Message (to public):</label>
                    <input type="text" id="message" style="width: 80%;" />
                    <input type="button" id="sendmessage" value="Send Public" />
                </div>

                <div id="privateChatArea" class="private-chat-area">
                    <h3>Private Messages</h3>
                    {/* Private messages will be appended here by JavaScript */}
                </div>
            </div>

            <div class="user-list-container">
                <h3>Online Users</h3>
                <ul id="userList">
                    {/* User list will be populated here by JavaScript */}
                </ul>
            </div>
        </div>

        <%-- Hidden field for display name (original - may or may not be needed depending on final logic) --%>
        <%-- <input type="hidden" id="displayname" /> --%>

        <script src='<%= ResolveUrl("~/Scripts/jquery-3.7.1.min.js") %>'></script>
        <script src='<%= ResolveUrl("~/Scripts/jquery.signalR-2.4.3.min.js") %>'></script>
        <script src='<%= ResolveUrl("~/signalr/hubs") %>'></script>

        <script type="text/javascript">
            $(function () {
                var chat = $.connection.chatHub;
                var currentAuthenticatedUsername = ""; // Will be set after connection if user is authenticated

                // Function to display public messages
                chat.client.addNewMessageToPage = function (name, message) {
                    var encodedName = $('<div />').text(name).html();
                    var encodedMsg = $('<div />').text(message).html();
                    $('#chatArea').append('<div><strong>' + encodedName +
                        '</strong>:&nbsp;&nbsp;' + encodedMsg + '</div>');
                };

                // Function to display private messages
                chat.client.receivePrivateMessage = function (fromUser, toUser, message, isEcho) {
                    console.log("Private message received: from " + fromUser + " to " + toUser + " - " + message);
                    var chatParty = isEcho ? toUser : fromUser;
                    var messagePrefix = isEcho ? `To ${toUser}:` : `From ${fromUser}:`;

                    $('#privateChatArea').append(
                        `<div><strong>${$('<div />').text(messagePrefix).html()}</strong> ${$('<div />').text(message).html()}</div>`
                    );
                };

                // Client method to handle error messages from the server
                chat.client.receiveErrorMessage = function (errorMessage) {
                    console.error("Server Error: " + errorMessage);
                    alert("Server Error: " + errorMessage);
                };

                // Client method to update user list when server broadcasts it
                chat.client.userListUpdated = function (userListFromServer) {
                    console.log("User list updated by server: ", userListFromServer);
                    $('#userList').empty();

                    // currentAuthenticatedUsername should be set correctly if user is logged in.
                    // If #displayName is used, ensure it's populated with the *actual* authenticated username.
                    currentAuthenticatedUsername = $('#displayName').val();
                    if (!currentAuthenticatedUsername && chat.hub && chat.hub.connection && chat.hub.connection.state === $.signalR.connectionState.connected) {
                        // Fallback or fetch authenticated user name if possible, e.g. from a server variable
                        // For now, we rely on #displayName being populated.
                        // In a real app with IUserIdProvider, the server knows the user.
                        // This client-side 'currentAuthenticatedUsername' is mainly for UI (e.g., not listing self).
                    }

                    userListFromServer.forEach(function (user) {
                        if (user !== currentAuthenticatedUsername) { // Don't list the user themselves
                            $('#userList').append(`<li data-username="${$('<div />').text(user).html()}">${$('<div />').text(user).html()}</li>`);
                        }
                    });

                    $('#userList li').off('click').on('click', function () {
                        var recipient = $(this).data('username');
                        var privateMsg = prompt(`Send private message to ${recipient}:`);
                        if (privateMsg && privateMsg.trim() !== "") {
                            chat.server.sendPrivateMessage(recipient, privateMsg);
                        }
                    });
                };

                $('#message').focus();

                // Start the connection.
                $.connection.hub.start().done(function () {
                    console.log("SignalR Connected!");

                    // If using authentication, the server (IUserIdProvider) knows the user.
                    // The client might want to know its own username for UI purposes.
                    // This is where you would ideally set currentAuthenticatedUsername based on server-provided info
                    // or the pre-filled #displayName.
                    // For instance, if User.Identity.Name is available on the server:
                    // In Page_Load (WebForm1.aspx.cs):
                    // if (User.Identity.IsAuthenticated) {
                    //    Page.ClientScript.RegisterStartupScript(this.GetType(), "SetUsername", $"var authenticatedUser = '{User.Identity.Name}';", true);
                    //    displayName.Value = User.Identity.Name; // If it's an input
                    // }
                    // Then in JS: if (typeof authenticatedUser !== 'undefined') { currentAuthenticatedUsername = authenticatedUser; }

                    currentAuthenticatedUsername = $('#displayName').val(); // Re-evaluate after connection, in case it's set by server script

                    $('#sendmessage').click(function () {
                        var senderName = $('#displayName').val();
                        var publicMessage = $('#message').val();
                        if (senderName && publicMessage) {
                            chat.server.send(senderName, publicMessage); // Public message
                            $('#message').val('').focus();
                        } else {
                            alert('Please enter your name and a message for public chat.');
                        }
                    });

                    // Initial fetch of user list once connected
                    refreshUserList();

                }).fail(function (error) {
                    console.log('Could not connect to SignalR: ' + error);
                });

                function refreshUserList() {
                    if (chat.server && typeof chat.server.getOnlineUsers === 'function') {
                        chat.server.getOnlineUsers().done(function (userList) {
                            // The userListUpdated client method will handle rendering
                            // No need to duplicate rendering logic here if userListUpdated is comprehensive.
                            // chat.client.userListUpdated(userList); // This would also work.
                            // Or let the server push via Clients.All.userListUpdated on connect/disconnect.
                            // For an explicit refresh button or initial load, directly calling the render logic is fine:

                            $('#userList').empty();
                            currentAuthenticatedUsername = $('#displayName').val(); // Ensure it's up-to-date

                            userList.forEach(function (user) {
                                if (user !== currentAuthenticatedUsername) {
                                    $('#userList').append(`<li data-username="${$('<div />').text(user).html()}">${$('<div />').text(user).html()}</li>`);
                                }
                            });
                            $('#userList li').off('click').on('click', function () {
                                var recipient = $(this).data('username');
                                var privateMsg = prompt(`Send private message to ${recipient}:`);
                                if (privateMsg && privateMsg.trim() !== "") {
                                    chat.server.sendPrivateMessage(recipient, privateMsg);
                                }
                            });

                        }).fail(function (error) {
                            console.error("Could not get online users from refreshUserList: " + error);
                        });
                    } else {
                        console.warn("chat.server.getOnlineUsers is not available yet or not defined on the hub.");
                    }
                }

                // This initial call to refreshUserList might be too early if connection isn't instant
                // It's better called inside .done() of hub.start() or rely on server push.
                // if ($.connection.hub.state === $.signalR.connectionState.connected) {
                //    refreshUserList();
                // }
            });
        </script>
    </form>
</body>
</html>