<%@ Page Language="C#" MasterPageFile="~/template.Master" AutoEventWireup="true" CodeBehind="message.aspx.cs" Inherits="Homemate_Matching.WebForm2" %>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <div class="chat-wrapper d-flex mt-5">
        <div class="user-list">
            <h5 class="p-3 mb-0 bg-primary text-white">Kişiler</h5>
            <ul class="list-group list-group-flush" id="userList">
                <%-- User list will be populated by SignalR --%>
            </ul>
        </div>

        <div id="rightPanel" class="chat-container flex-grow-1">
            <div id="chooseMessage" style="height:600px; display:flex; justify-content:center; align-items:center; font-size:24px; color:#777;">
                Mesaj göndermek için birini seçmelisin.
            </div>

            <div id="chatUI" class="card shadow-sm flex-column" style="display:none; height:600px; border-radius:10px; overflow:hidden;">
                <div class="card-header bg-success text-white">
                    <h4 class="mb-0" id="chatHeader">Mesajlar</h4>
                </div>
                <div class="card-body chat-body" id="chatArea">
                    <%-- Chat messages will be appended here --%>
                </div>
                <div class="card-footer chat-footer">
                    <input type="text" id="messageInput" class="form-control mb-2" placeholder="Buraya göndermek istediğiniz mesajı yazınız..." onkeydown="checkEnter(event)" />
                    <button type="button" class="button" onclick="sendMessage()">Gönder</button>
                </div>
            </div>
        </div>
    </div>

    <style>
        /* Your existing styles are good and will be used */
        .button { padding: 1.3em 3em; font-size: 12px; text-transform: uppercase; letter-spacing: 2.5px; font-weight: 500; color: #000; background-color: #fff; border: none; border-radius: 45px; box-shadow: 0px 8px 15px rgba(0, 0, 0, 0.1); transition: all 0.3s ease 0s; cursor: pointer; outline: none; width: 100%; display: inline-block; }
        .button:hover { background-color: #23c483; box-shadow: 0px 15px 20px rgba(46, 229, 157, 0.4); color: #fff; transform: translateY(-7px); }
        .button:active { transform: translateY(-7px); }
        .chat-wrapper { width: 90%; max-width: 1400px; margin: auto; display: flex; gap: 20px; }
        .user-list { width: 25%; min-width: 220px; border: 1px solid #dee2e6; border-radius: 8px; background-color: #fff; height: 600px; overflow-y: auto; }
        .chat-container { width: 75%; }
        .chat-body { flex-grow: 1; overflow-y: auto; background-color: #f7f7f7; padding: 15px; height: calc(600px - 120px); /* Adjusted for header and footer */ }
        .message { display: flex; margin-bottom: 10px; }
        .message.left { justify-content: flex-start; }
        .message.right { justify-content: flex-end; }
        .bubble { max-width: 70%; padding: 10px 15px; border-radius: 18px; font-size: 15px; line-height: 1.4; word-wrap: break-word; box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1); }
        .message.left .bubble { background-color: #e4e6eb; color: #000; border-bottom-left-radius: 0; }
        .message.right .bubble { background-color: #d1f1c7; color: #000; border-bottom-right-radius: 0; }
        .chat-footer input[type="text"] { border-radius: 25px; padding-left: 20px; padding-right: 20px; }
        .chat-footer button { border-radius: 25px; font-weight: 600; }
        .list-group-item:hover { background-color: #f1f1f1; cursor: pointer; }
        .list-group-item.active { background-color: #23c483; color: white; } /* Style for selected user */
    </style>

    <%-- Ensure jQuery and SignalR scripts are loaded. If they are in your template.Master, these aren't needed here. --%>
    <%-- If not, uncomment or add them: --%>
    <script src='<%= ResolveUrl("~/Scripts/jquery-3.7.1.min.js") %>'></script>
    <script src='<%= ResolveUrl("~/Scripts/jquery.signalR-2.4.3.min.js") %>'></script>
    <script src='<%= ResolveUrl("~/signalr/hubs") %>'></script>

    <%-- message.aspx --%>
<%-- ... (your existing HTML structure and styles are mostly fine) ... --%>

<%-- Ensure jQuery and SignalR scripts are loaded. --%>
<script src='<%= ResolveUrl("~/Scripts/jquery-3.7.1.min.js") %>'></script>
<script src='<%= ResolveUrl("~/Scripts/jquery.signalR-2.4.3.min.js") %>'></script>
<script src='<%= ResolveUrl("~/signalr/hubs") %>'></script>

<script type="text/javascript">
    // currentAuthenticatedUser is set by server-side script from message.aspx.cs

    let selectedRecipientUsername = null;
    let chatHub = null;

    // Cache DOM elements
    const userListElement = document.getElementById("userList");
    const chatHeaderElement = document.getElementById("chatHeader");
    const chatAreaElement = document.getElementById("chatArea");
    const chooseMessageElement = document.getElementById("chooseMessage");
    const chatUIElement = document.getElementById("chatUI");
    const messageInputElement = document.getElementById("messageInput");

    $(function () {
        if (typeof $.connection === 'undefined' || typeof $.connection.chatHub === 'undefined') {
            console.error("SignalR hub proxy not loaded.");
            return;
        }
        chatHub = $.connection.chatHub;

        // --- Client-side methods called by the server ---
        chatHub.client.receivePrivateMessage = function (fromUser, toUser, message, timestamp, isEcho) {
            console.log(`Private message: from=${fromUser}, to=${toUser}, echo=${isEcho}, msg=${message}, ts=${timestamp}`);
            let relevantToCurrentChat = false;
            let messageAlignment = "left";

            if (isEcho && toUser === selectedRecipientUsername) {
                relevantToCurrentChat = true;
                messageAlignment = "right";
            } else if (!isEcho && fromUser === selectedRecipientUsername) {
                relevantToCurrentChat = true;
                messageAlignment = "left";
            }

            if (relevantToCurrentChat) {
                appendMessageToChatArea(message, messageAlignment, new Date(timestamp));
            } else if (!isEcho) {
                // Notify about message from another user if current chat is not with them
                console.log(`Received message from ${fromUser}, but chat window for them is not active.`);
                // You might add a badge or notification to the user list item for 'fromUser'
                const userLi = userListElement.querySelector(`li[data-username="${fromUser}"]`);
                if (userLi && !userLi.classList.contains('has-unread')) {
                    userLi.classList.add('has-unread'); // Add a class for styling unread messages
                    // You could also update a counter next to the user's name
                }
            }
        };

        chatHub.client.userListUpdated = function (usersFromServer) {
            // usersFromServer is now List<UserViewModel> which might include IsOnline
            console.log("User list updated:", usersFromServer);
            populateUserList(usersFromServer);
        };

        chatHub.client.receiveErrorMessage = function (errorMessage) {
            console.error("Server Error: " + errorMessage);
            alert("Error from server: " + errorMessage);
        };

        // --- Start SignalR connection ---
        $.connection.hub.start()
            .done(function () {
                console.log("SignalR connected. Authenticated user: " + currentAuthenticatedUser);
                if (!currentAuthenticatedUser) {
                    $('#chooseMessage').text("Please log in to use the chat feature.").show();
                    $('#chatUI').hide();
                    $('.user-list').hide();
                    return;
                }

                // Load initial user list (now all registered users)
                chatHub.server.getAllMatchedUsers(currentAuthenticatedUser).done(function (users) { // <--- CHANGED
                    populateUserList(users);
                }).fail(function (error) {
                    console.error("Error getting all registered users: " + error);
                });
            })
            .fail(function (error) {
                console.error("SignalR connection failed: " + error);
            });
    });

    function populateUserList(users) { // users is now List<UserViewModel>
        userListElement.innerHTML = "";
        if (users && users.length > 0) {
            users.forEach(userObj => { // userObj is UserViewModel
                if (userObj.Username !== currentAuthenticatedUser) {
                    const li = document.createElement("li");
                    li.className = "list-group-item";
                    // Ensure userObj.Username is used for data-username if that's the identifier
                    li.setAttribute("data-username", userObj.Username);

                    const safeUserDisplay = document.createTextNode(userObj.Username).textContent;
                    let onlineIndicator = userObj.IsOnline ? "<span style='color:green; font-size:0.8em;'> (Online)</span>" : "<span style='color:grey; font-size:0.8em;'> (Offline)</span>";
                    // If IsOnline is not provided, remove the indicator logic or default it.
                    // onlineIndicator = ""; // If IsOnline is not available yet

                    li.innerHTML = `<i class="bi bi-person-circle me-2 fs-4"></i> ${safeUserDisplay} ${onlineIndicator}`;
                    li.onclick = () => loadConversation(userObj.Username); // Pass Username
                    userListElement.appendChild(li);
                }
            });
        } else {
            userListElement.innerHTML = "<li class='list-group-item'>Henüz eşleşilen kullanıcı yok.</li>";
        }
    }

    function loadConversation(username) {
        if (selectedRecipientUsername === username) return;

        selectedRecipientUsername = username;
        console.log("Loading conversation with: " + selectedRecipientUsername);

        chooseMessageElement.style.display = "none";
        chatUIElement.style.display = "flex";

        const safeHeaderDisplay = document.createTextNode(username).textContent;
        chatHeaderElement.innerText = `Chat with ${safeHeaderDisplay}`;
        chatAreaElement.innerHTML = ""; // Clear previous messages

        // Remove unread indicator if any
        const userLi = userListElement.querySelector(`li[data-username="${username}"]`);
        if (userLi) {
            userLi.classList.remove('has-unread');
            // De-select previously active user, select new one
            const allUserItems = userListElement.querySelectorAll("li");
            allUserItems.forEach(item => item.classList.remove("active"));
            userLi.classList.add("active");
        }


        // Fetch and display message history
        if (chatHub && currentAuthenticatedUser) {
            chatHub.server.getMessageHistory(selectedRecipientUsername) // <--- NEW
                .done(function (messageHistory) { // messageHistory is List<MessageViewModel>
                    if (messageHistory && messageHistory.length > 0) {
                        messageHistory.forEach(msg => {
                            let alignment = (msg.SenderUsername === currentAuthenticatedUser) ? "right" : "left";
                            appendMessageToChatArea(msg.MessageContent, alignment, new Date(msg.Timestamp));
                        });
                    } else {
                        console.log("No message history with " + selectedRecipientUsername);
                    }
                    messageInputElement.focus();
                })
                .fail(function (error) {
                    console.error("Error fetching message history: " + error);
                    appendMessageToChatArea("Could not load message history.", "system-error", new Date());
                });
        }
        messageInputElement.focus();
    }

    function appendMessageToChatArea(text, alignment, timestamp) {
        const messageDiv = document.createElement("div");
        messageDiv.className = "message " + alignment;

        const bubbleDiv = document.createElement("div");
        bubbleDiv.className = "bubble";
        bubbleDiv.innerText = text;

        // Optionally display timestamp
        if (timestamp) {
            const timeSpan = document.createElement("span");
            timeSpan.style.fontSize = "0.7em";
            timeSpan.style.display = "block";
            timeSpan.style.textAlign = alignment === "right" ? "right" : "left";
            timeSpan.style.color = "#777";
            timeSpan.innerText = timestamp.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            // For more detailed timestamp: timestamp.toLocaleString();
            bubbleDiv.appendChild(timeSpan);
        }

        messageDiv.appendChild(bubbleDiv);
        chatAreaElement.appendChild(messageDiv);
        chatAreaElement.scrollTop = chatAreaElement.scrollHeight;
    }

    // Global functions for HTML onclick/onkeydown (sendMessage, checkEnter)
    function sendMessage() {
        const text = messageInputElement.value.trim();
        if (text === "" || !selectedRecipientUsername) {
            if (!selectedRecipientUsername) alert("Please select a user to chat with.");
            return;
        }

        if (chatHub && currentAuthenticatedUser) {
            // The message will be saved to DB by the server and then echoed back via receivePrivateMessage
            chatHub.server.sendPrivateMessage(selectedRecipientUsername, text)
                .fail(function (error) {
                    console.error("Error sending private message: " + error);
                });
            messageInputElement.value = "";
            messageInputElement.focus();
        } else {
            alert("Chat not connected or user not authenticated.");
        }
    }

    function checkEnter(event) {
        if (event.key === "Enter") {
            event.preventDefault();
            sendMessage();
        }
    }
</script>
</asp:Content>