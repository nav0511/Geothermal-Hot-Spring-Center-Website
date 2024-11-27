document.addEventListener("DOMContentLoaded", () => {
    // Lấy các phần tử từ DOM
    const chatBubble = document.getElementById("chat-bubble");
    const chatPopup = document.getElementById("chat-popup");
    const closeChatButton = document.getElementById("close-chat");
    const chatInput = document.getElementById("chat-input");
    const sendChatButton = document.getElementById("send-chat");
    const chatBody = document.getElementById("chat-body");

    

    // Kiểm tra nếu các phần tử cần thiết có tồn tại
    if (!chatBubble || !chatPopup || !closeChatButton || !chatInput || !sendChatButton || !chatBody) {
        console.error("Một trong các phần tử DOM không tồn tại.");
        return;
    }

    // Hàm mở popup chat
    function openChat() {
        // Hiển thị popup chat
        chatPopup.classList.remove("d-none");
    }

    // Hàm đóng popup chat
    function closeChat() {
        chatPopup.classList.add("d-none"); // Ẩn popup chat
    }

    // Mở hoặc đóng popup chat khi nhấn vào nút chat
    chatBubble.addEventListener("click", () => {
        chatPopup.classList.toggle("d-none"); // Toggle giữa mở và đóng popup chat
    });

    // Đóng popup khi nhấn nút "X"
    closeChatButton.addEventListener("click", () => {
        closeChat(); // Gọi hàm đóng popup
    });

    // Gửi tin nhắn
    sendChatButton.addEventListener("click", async () => {
        const message = chatInput.value.trim();

        if (message) {
            // Làm sạch hộp nhập liệu sau khi gửi
            chatInput.value = "";

            // Tạo phần tử tin nhắn người dùng
            addMessageToChatBody("user", message);

            // Gửi tin nhắn tới API và nhận phản hồi từ chatbot
            const botReply = await sendMessageToChatbot(message);

            // Hiển thị phản hồi từ chatbot
            if (botReply) {
                addMessageToChatBody("bot", botReply);
            }
        }
    });

    // Đảm bảo có thể gửi tin nhắn khi nhấn Enter
    chatInput.addEventListener("keydown", (event) => {
        if (event.key === "Enter") {
            sendChatButton.click(); // Gửi tin nhắn khi nhấn Enter
        }
    });

    // Hàm thêm tin nhắn vào giao diện chat
    function addMessageToChatBody(role, content) {
        const messageElement = document.createElement("div");
        messageElement.innerHTML = content;
        messageElement.classList.add("chat-message");
        messageElement.classList.add(role === "user" ? "user-message" : "bot-message");
        chatBody.appendChild(messageElement);

        // Cuộn tự động xuống dưới để hiển thị tin nhắn mới
        chatBody.scrollTop = chatBody.scrollHeight;
    }

    // Hàm gửi tin nhắn tới API Chatbot và nhận phản hồi
    async function sendMessageToChatbot(userMessage) {
        const apiUrl = window.apiUrls.myApi;
        try {
            const response = await fetch(apiUrl + "Chatbot", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ Prompt: userMessage })
            });
            if (!response.ok) {
                console.error("Error sending message to chatbot:", response.statusText);
                return "Đã có lỗi xảy ra khi gửi yêu cầu tới chatbot.";
            }

            const data = await response.json();
            const reply = data.reply;
            //const formattedReply = reply.replace(/\n/g, "<br>");
            return reply;
        } catch (error) {
            console.error("Error sending message to API:", error);
            return "Đã có lỗi xảy ra. Vui lòng thử lại sau.";
        }
    }
});