// $(document).ready(function () {
//     // Function to scroll message area to bottom
//     function scrollToBottomMessages() {
//         var chatMessagesDiv = document.getElementById('chatMessagesArea');
//         if (chatMessagesDiv) {
//             chatMessagesDiv.scrollTop = chatMessagesDiv.scrollHeight;
//         }
//     }
//     scrollToBottomMessages(); // Initial scroll

//     // Handle clicking on a conversation item (using event delegation for dynamically loaded items if any)
//     // This is now handled by full page reload (asp-action). If pure AJAX sidebar update is needed, this would change.
//     // $('#conversationList').on('click', '.conversation-item', function (e) {
//     //     e.preventDefault();
//     //     const userId = $(this).data('userid');
//     //     if (userId && userId !== currentChatPartnerId) {
//     //         loadChatWindow(userId);
//     //         // Update URL without full reload (optional)
//     //         // window.history.pushState({ userId: userId }, "", "/TinNhan/Index/" + userId);
//     //     }
//     // });

//     // function loadChatWindow(userId) {
//     //     currentChatPartnerId = userId;
//     //     $('#conversationList .conversation-item').removeClass('active');
//     //     $('#conversationList .conversation-item[data-userid="' + userId + '"]').addClass('active');
//     //     $('#hiddenNguoiNhanId').val(userId); // Update hidden field in form

//     //     $.ajax({
//     //         url: '/TinNhan/LayChiTietHoiThoai/' + userId,
//     //         type: 'GET',
//     //         success: function (response) {
//     //             $('#chatMainWindow').html(response);
//     //             scrollToBottomMessages();
//     //             // Clear unread badge for this user
//     //             $('#unread-badge-' + userId).remove();
//     //         },
//     //         error: function (xhr, status, error) {
//     //             console.error("Lỗi khi tải chi tiết hội thoại: ", error);
//     //             $('#chatMainWindow').html('<div class="no-conversation-selected"><p>Không thể tải cuộc trò chuyện. Vui lòng thử lại.</p></div>');
//     //         }
//     //     });
//     // }


//     // Handle sending a new message
//     $('#chatMainWindow').on('submit', '#formGuiTinNhan', function (e) {
//         e.preventDefault();
//         var form = $(this);
//         var noiDungInput = $('#inputNoiDungTinNhan');
//         var validationSummary = $('#validationSummaryGuiTinNhan');
//         validationSummary.html(''); // Clear previous errors

//         if (!noiDungInput.val().trim()) {
//             validationSummary.html('Vui lòng nhập nội dung tin nhắn.');
//             noiDungInput.focus();
//             return;
//         }

//         $.ajax({
//             url: form.attr('action'),
//             type: form.attr('method'),
//             data: form.serialize(), // Sends form data including AntiForgeryToken
//             success: function (response) {
//                 onMessageSentSuccess(response);
//             },
//             error: function (xhr, status, error) {
//                 onMessageSentFailure(xhr, status, error);
//             }
//         });
//     });


// });

// function onMessageSentSuccess(response) {
//     if (response.success && response.tinNhanMoi) {
//         const tinNhan = response.tinNhanMoi;
//         const chatMessagesArea = $('#chatMessagesArea');
        
//         // Determine avatar based on whether it's the current user or not
//         // For the current user (employer), you might have a specific avatar path or get it from a user claim.
//         const senderAvatar = tinNhan.laCuaToi 
//             ? ( $('#userAvatarNavbar').attr('src') || '/images/avatars/default_employer.png') // Assuming you have an avatar in navbar like _LoginPartial
//             : (tinNhan.avatarNguoiGui || '/images/avatars/default_user.png');

//         const messageHtml = `
//             <div class="message-item ${tinNhan.laCuaToi ? 'sent' : 'received'}" data-message-id="${tinNhan.id}">
//                 ${!tinNhan.laCuaToi ? `<div class="message-avatar"><img src="${senderAvatar}" alt="Avatar"></div>` : ''}
//                 <div class="message-content-wrapper">
//                     <div class="message-content">
//                         ${tinNhan.noiDung.replace(/\n/g, '<br />')}
//                     </div>
//                     <div class="message-meta">
//                         ${tinNhan.laCuaToi ? 'Bạn' : tinNhan.tenNguoiGui} • ${new Date(tinNhan.ngayGui).toLocaleString('vi-VN', { hour: '2-digit', minute: '2-digit', day: '2-digit', month: '2-digit', year: 'numeric' })}
//                     </div>
//                 </div>
//                 ${tinNhan.laCuaToi ? `<div class="message-avatar"><img src="${senderAvatar}" alt="Avatar"></div>` : ''}
//             </div>`;
        
//         // If it's the first message, remove the "no messages" placeholder
//         if (chatMessagesArea.find('.text-center.text-muted').length > 0) {
//             chatMessagesArea.html('');
//         }

//         chatMessagesArea.append(messageHtml);
//         $('#inputNoiDungTinNhan').val(''); // Clear input
//         scrollToBottomMessages();

//         // Update sidebar (last message and time)
//         const conversationItem = $('#conversationList .conversation-item[data-userid="' + tinNhan.nguoiNhanId + '"]');
//         if (conversationItem.length) {
//             conversationItem.find('.conversation-last-message').text(tinNhan.noiDung.substring(0,30) + (tinNhan.noiDung.length > 30 ? '...' : ''));
//             conversationItem.find('.conversation-time').text(new Date(tinNhan.ngayGui).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }));
//             // Move this conversation to the top of the list
//             $('#conversationList').prepend(conversationItem);
//         } else {
//             // This means a new conversation was started.
//             // A full refresh or a more complex sidebar update logic would be needed here.
//             // For simplicity, we can prompt for a refresh or just let the user refresh manually.
//             // Or, if the Index action is called with a nguoiLienHeId, it will naturally appear.
//             // For now, we'll just log it.
//             console.log("Tin nhắn mới gửi đến người dùng chưa có trong danh sách. Cần làm mới danh sách.");
//              window.location.reload(); // Simple refresh to show new conversation
//         }

//         $('#validationSummaryGuiTinNhan').html('');

//     } else {
//         let errorsHtml = '<ul>';
//         if (response.errors && Array.isArray(response.errors)) {
//             response.errors.forEach(function(error) {
//                 errorsHtml += '<li>' + error + '</li>';
//             });
//         } else if (response.message) {
//              errorsHtml += '<li>' + response.message + '</li>';
//         } else {
//             errorsHtml += '<li>Đã có lỗi xảy ra khi gửi tin nhắn.</li>';
//         }
//         errorsHtml += '</ul>';
//         $('#validationSummaryGuiTinNhan').html(errorsHtml).addClass('text-danger');
//     }
// }

// function onMessageSentFailure(xhr, status, error) {
//     console.error("Lỗi AJAX khi gửi tin nhắn:", status, error, xhr.responseText);
//     $('#validationSummaryGuiTinNhan').html('Không thể gửi tin nhắn. Vui lòng thử lại. Lỗi: ' + (xhr.responseJSON?.message || error )).addClass('text-danger');
// }
// Global variables from the Razor view (Index.cshtml)
// const currentLoggedInUserId = ...; (defined in Index.cshtml script block)
// let currentChatPartnerId = ...; (defined in Index.cshtml script block)

$(document).ready(function () {
    // Apply initial theme color from ViewModel (if passed and used)
    // const initialThemeColor = $('body').data('theme-color') || '#007bff';
    // document.documentElement.style.setProperty('--primary-color', initialThemeColor);
    // document.documentElement.style.setProperty('--sent-bubble-bg', initialThemeColor);
    // $('.chat-color-option[data-color="' + initialThemeColor + '"]').addClass('selected');


    // Auto-resize textarea for chat input
    const messageInput = document.getElementById('inputNoiDungTinNhan');
    if (messageInput) {
        const initialHeight = messageInput.scrollHeight;
        messageInput.addEventListener('input', function () {
            this.style.height = 'auto';
            let newHeight = this.scrollHeight;
            if (newHeight > 120) newHeight = 120; // Max height from CSS
            else if (newHeight < initialHeight) newHeight = initialHeight; // Min height
            this.style.height = newHeight + 'px';
        });

        messageInput.addEventListener('keydown', function (event) {
            if (event.key === 'Enter' && !event.shiftKey) {
                event.preventDefault();
                const form = $('#formGuiTinNhan');
                if (form.length && $(this).val().trim() !== "") { // Check if form exists and input is not empty
                     $('#btnGuiTinNhan').click(); // Trigger click on submit button to ensure data-ajax handlers fire
                }
            }
        });
    }

    // Handle clicking on a conversation item
    // This is now handled by page navigation (<a> tag), but if you switch to full AJAX:
    // $(document).on('click', '.conversation-item', function (e) {
    //     e.preventDefault();
    //     const userId = $(this).data('userid');
    //     if (userId === currentChatPartnerId) return; // Already selected

    //     $('.conversation-item').removeClass('active');
    //     $(this).addClass('active');
    //     $(this).find('.unread-badge').remove(); // Optimistically remove unread badge

    //     currentChatPartnerId = userId;
    //     loadChatWindow(userId);
    // });


    // Change Chat Color Modal Interaction
    $(document).on('click', '#changeChatColorOption', function (e) {
        e.preventDefault();
        var chatColorModal = new bootstrap.Modal(document.getElementById('chatColorModal'));
        // Highlight currently selected color in modal
        const currentThemeColor = getComputedStyle(document.documentElement).getPropertyValue('--primary-color').trim();
        $('#chatColorModal .chat-color-option').removeClass('selected');
        $('#chatColorModal .chat-color-option[data-color="' + currentThemeColor + '"]').addClass('selected');
        chatColorModal.show();
        $('body').addClass('modal-open-chat'); // Prevent body scroll
    });

    $('#chatColorModal').on('hidden.bs.modal', function () {
        $('body').removeClass('modal-open-chat');
    });

    $(document).on('click', '.chat-color-option', function () {
        const color = $(this).data('color');
        $('.chat-color-option').removeClass('selected');
        $(this).addClass('selected');

        document.documentElement.style.setProperty('--primary-color', color);
        document.documentElement.style.setProperty('--active-bg-color', color);
        document.documentElement.style.setProperty('--sent-bubble-bg', color);
        
        // Persist theme color (AJAX to server)
        const token = $('input[name="__RequestVerificationToken"]').val();
        $.ajax({
            url: '/TinNhan/SetThemeColor', // Make sure this route exists
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(color),
            headers: {
                'RequestVerificationToken': token
            },
            success: function(response) {
                if(response.success) {
                    console.log('Theme color saved.');
                } else {
                    console.error('Failed to save theme color:', response.message);
                    // Optionally revert color if save fails
                }
            },
            error: function(xhr, status, error) {
                console.error('Error saving theme color:', error);
            }
        });


        // var chatColorModal = bootstrap.Modal.getInstance(document.getElementById('chatColorModal'));
        // if(chatColorModal) chatColorModal.hide();
    });

    // Placeholder for message delete action
    $(document).on('click', '.message-actions-hover .btn-icon-light[title*="Xóa tin nhắn"]', function () {
        var messageItem = $(this).closest('.message-item');
        var messageId = messageItem.data('message-id');
        if (confirm('Bạn có chắc chắn muốn xóa tin nhắn này? (Chức năng này hiện chỉ là mẫu)')) {
            // TODO: AJAX call to server to delete message with messageId
            // On success: messageItem.remove();
            alert('Chức năng xóa tin nhắn (ID: ' + messageId + ') cần được phát triển thêm phía server.');
            // messageItem.fadeOut(300, function() { $(this).remove(); }); // Example removal
        }
    });

    // Initial scroll for existing messages if a chat is loaded
    if (currentChatPartnerId && currentChatPartnerId !== null) {
        scrollToBottomMessages();
    }
});


// AJAX form submission Callbacks (from data-ajax attributes)
function onBeginSendMessage() {
    $('#btnGuiTinNhan').prop('disabled', true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>');
    $('#validationSummaryGuiTinNhan').text(''); // Clear previous errors
}

function onCompleteSendMessage() {
     $('#btnGuiTinNhan').prop('disabled', false).html('<i class="fas fa-paper-plane"></i>');
     const messageInput = document.getElementById('inputNoiDungTinNhan');
     if (messageInput) {
        messageInput.style.height = 'auto'; // Reset height after send
        messageInput.style.height = messageInput.scrollHeight + 'px'; // Recalc
        if (parseInt(messageInput.style.height) < 38) messageInput.style.height = '38px'; // Ensure min height
     }
}


function onMessageSentSuccess(response) {
    if (response.success && response.tinNhanMoi) {
        const tinNhanMoi = response.tinNhanMoi;
        const chatMessagesArea = $('#chatMessagesArea');

        // Get current user's avatar from body data attribute (set in _LayoutTinNhan.cshtml)
        const currentUserAvatar = $('body').data('current-user-avatar') || '/images/avatars/default_employer.png';

        // Sanitize HTML from response if needed, though server should send safe HTML
        const safeNoiDung = $('<div>').text(tinNhanMoi.noiDung).html().replace(/\n/g, "<br />");


        const messageHtml = `
            <div class="message-item sent" data-message-id="${tinNhanMoi.id}">
                <div class="message-content-wrapper">
                    <div class="message-bubble">
                        <div class="message-content">
                            ${safeNoiDung}
                        </div>
                        <div class="message-actions-hover">
                            <button class="btn btn-sm btn-icon-light" title="Xóa tin nhắn (chức năng sắp có)"><i class="fas fa-trash-alt"></i></button>
                        </div>
                    </div>
                    <div class="message-meta">
                        <span title="${tinNhanMoi.ngayGuiFormatted}">Bạn • ${new Date(tinNhanMoi.ngayGui).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false })}</span>
                        <i class="fas fa-check message-status sent-unread" title="Đã gửi"></i>
                    </div>
                </div>
                <div class="message-avatar self">
                    <img src="${currentUserAvatar}" alt="Avatar của bạn" title="Bạn">
                </div>
            </div>
        `;
        chatMessagesArea.append(messageHtml);
        
        $('#noMessagesPlaceholder').hide(); // Hide "no messages" placeholder if it was visible

        const messageInput = $('#inputNoiDungTinNhan');
        messageInput.val('');
        // Trigger input event to resize textarea after clearing
        const event = new Event('input', { bubbles: true });
        messageInput[0].dispatchEvent(event);


        scrollToBottomMessages();

        // Update conversation list in sidebar
        const conversationItem = $(`#conversationList a.conversation-item[data-userid="${response.nguoiNhanId}"]`);
        if (conversationItem.length) {
            let lastMessageText = tinNhanMoi.noiDung;
            if (lastMessageText.length > 25) lastMessageText = lastMessageText.substring(0, 22) + "...";
            
            conversationItem.find(`#last-message-${response.nguoiNhanId}`).html(`<span>Bạn: </span>${$('<div>').text(lastMessageText).html()}`); // Sanitize
            conversationItem.find(`#last-message-time-${response.nguoiNhanId}`).text(new Date(tinNhanMoi.ngayGui).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false }));
            
            // Move conversation to top
            $('#conversationList').prepend(conversationItem);
        } else {
            // Create new conversation item if it doesn't exist (e.g., first message)
            // This would require more info (avatar of recipient) or a reload/SignalR update for full accuracy
            // For now, we'll assume user reloads or it's handled by SignalR later
            console.log("New conversation started with user ID: " + response.nguoiNhanId + ". Consider reloading conversation list or using SignalR.");
            // Potentially, make an AJAX call to get the conversation list item HTML and prepend it.
        }

    } else {
        let errorSummary = "Gửi tin nhắn thất bại.";
        if (response.errors && response.errors.length > 0) {
            errorSummary = response.errors.join("\n");
        } else if (response.message) {
            errorSummary = response.message;
        }
        $('#validationSummaryGuiTinNhan').text(errorSummary);
    }
}

function onMessageSentFailure(xhr, status, error) {
    console.error("AJAX error:", status, error, xhr.responseText);
    $('#validationSummaryGuiTinNhan').text("Lỗi máy chủ hoặc kết nối mạng. Vui lòng thử lại.");
}


// Function to load chat window content via AJAX (if not using full page reloads for conversation switching)
// function loadChatWindow(userId) {
//     $('#chatMainWindow').html('<div class="d-flex justify-content-center align-items-center h-100"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Đang tải...</span></div></div>');
//     $.ajax({
//         url: `/TinNhan/LayChiTietHoiThoai/${userId}`,
//         type: 'GET',
//         success: function (partialViewResult) {
//             $('#chatMainWindow').html(partialViewResult);
//             currentChatPartnerId = userId; // Update global var
//             scrollToBottomMessages();
//             // Re-initialize textarea autosize for the new partial
//             const newMessageInput = document.getElementById('inputNoiDungTinNhan');
//             if (newMessageInput) {
//                 const initialHeight = newMessageInput.scrollHeight;
//                 newMessageInput.addEventListener('input', function () { /* ... same as above ... */ });
//                 newMessageInput.addEventListener('keydown', function (event) { /* ... same as above ... */ });
//             }
//         },
//         error: function (err) {
//             console.error("Lỗi tải chi tiết hội thoại:", err);
//             $('#chatMainWindow').html('<div class="no-conversation-selected"><p>Không thể tải cuộc trò chuyện. Vui lòng thử lại.</p></div>');
//         }
//     });
// }

// Ensure scrollToBottomMessages is globally accessible if called from Index.cshtml
window.scrollToBottomMessages = function() {
    var chatMessagesDiv = document.getElementById('chatMessagesArea');
    if (chatMessagesDiv) {
        // Use a small timeout to ensure rendering is complete before scrolling
        setTimeout(() => {
            chatMessagesDiv.scrollTop = chatMessagesDiv.scrollHeight;
        }, 50);
    }
}

// SignalR connection (example placeholder - requires server-side Hub)
// $(function () {
//     if (typeof signalR !== 'undefined') {
//         const connection = new signalR.HubConnectionBuilder()
//             .withUrl("/chatHub") // Ensure this matches your Hub route
//             .configureLogging(signalR.LogLevel.Information)
//             .build();

//         async function start() {
//             try {
//                 await connection.start();
//                 console.log("SignalR Connected.");
//                 // Join a group for the current user
//                 if (currentLoggedInUserId) {
//                     connection.invoke("JoinGroup", currentLoggedInUserId.toString()).catch(err => console.error(err.toString()));
//                 }
//             } catch (err) {
//                 console.error("SignalR Connection Error: ", err);
//                 setTimeout(start, 5000);
//             }
//         };

//         connection.onclose(async () => {
//             await start();
//         });

//         // Listen for new messages
//         connection.on("ReceiveMessage", (messageViewModel) => {
//             console.log("Message received from SignalR:", messageViewModel);
//             // Check if the received message belongs to the currently open chat
//             if (messageViewModel.nguoiGuiId === currentChatPartnerId && !messageViewModel.laCuaToi) {
//                 appendReceivedMessage(messageViewModel); // You'll need to write this function
//                 // Mark as read if chat window is active and focused
//                 if (document.hasFocus()) { // Basic check
//                     connection.invoke("MarkMessageAsRead", messageViewModel.id, currentChatPartnerId).catch(err => console.error(err));
//                 }
//             } else if (!messageViewModel.laCuaToi) { // Message from another chat or when no chat is open
//                 updateConversationListSidebar(messageViewModel, true); // true for unread
//             }
//             // Update the conversation list for sender as well (last message, time)
//             updateConversationListSidebar(messageViewModel, false); // false for not necessarily unread for sender
//         });

//         // Listen for read receipts
//         connection.on("MessagesRead", (readerId, conversationPartnerId, messageIds) => {
//             if (currentLoggedInUserId === readerId && currentChatPartnerId === conversationPartnerId) {
//                 // This means the *other user* (conversationPartnerId) has read messages *I* sent.
//                 // This logic is a bit off. If I am readerId, it means *I* read messages from conversationPartnerId.
//                 // Let's adjust: If I am the sender and conversationPartnerId is the currentChatPartnerId.
//             }
//             // If I (currentLoggedInUserId) am the SENDER of the messages
//             // and the person who read them (readerId) is my currentChatPartnerId
//             if (currentChatPartnerId === readerId) {
//                 messageIds.forEach(msgId => {
//                     $(`.message-item[data-message-id="${msgId}"].sent .message-status`)
//                         .removeClass('fa-check').addClass('fa-check-double read')
//                         .attr('title', 'Đã xem');
//                 });
//             }
//         });
//         start();
//     } else {
//         console.warn("SignalR client library not found.");
//     }
// });

// function appendReceivedMessage(msg) { /* ... logic to append HTML for a received message ... */ }
// function updateConversationListSidebar(msg, isNewUnread) { /* ... logic to update sidebar ... */ }