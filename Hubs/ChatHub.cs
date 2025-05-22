using LMS.Data;
using LMS.Data.Entities;
using LMS.Services;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace LMS.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly IChatService _chatService;

        public ChatHub(ApplicationDbContext context, IChatService chatService)
        {
            _context = context;
            _chatService = chatService;
        }

        public async Task SendMessage(int roomId, string user, string message)
        {
            if (string.IsNullOrEmpty(message)) return; // Không gửi tin rỗng

            // Kiểm tra giới hạn tin nhắn
            bool canSend = await _chatService.CanSendMessage(user, roomId);

            if (!canSend)
            {
                // Gửi thông báo tới người dùng rằng họ đã đạt giới hạn tin nhắn
                await Clients.Caller.SendAsync("MessageLimitReached", "Bạn đã đạt giới hạn số lượng tin nhắn hôm nay (50 tin nhắn). Vui lòng thử lại vào ngày mai.");
                return;
            }

            // Lưu tin nhắn vào database
            var newMessage = new Message
            {
                UserId = user,
                Content = message,
                ChatRoomId = roomId,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(newMessage);
            await _context.SaveChangesAsync();

            // Tăng số đếm tin nhắn
            int remainingMessages = await _chatService.IncrementMessageCount(user, roomId);

            // Gửi tin nhắn đến tất cả user trong phòng
            await Clients.Group(roomId.ToString()).SendAsync("ReceiveMessage", user, message, DateTime.Now.ToString("HH:mm"));

            // Nếu còn 20% tin nhắn trong giới hạn, thông báo cho người dùng (10 tin nhắn còn lại)
            if (remainingMessages <= 10 && remainingMessages > 0)
            {
                await Clients.Caller.SendAsync("MessageLimitWarning", $"Bạn còn {remainingMessages} tin nhắn trong hôm nay.");
            }
        }

        public async Task JoinRoom(int roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Group(roomId.ToString()).SendAsync("UserJoined", $"{Context.User!.Identity!.Name} đã tham gia phòng.");

            // Hiển thị thông tin giới hạn tin nhắn khi người dùng tham gia phòng
            if (Context.User?.Identity?.Name != null)
            {
                int remainingMessages = await _chatService.GetRemainingMessageCount(Context.User.Identity.Name, roomId);
                await Clients.Caller.SendAsync("RemainingMessageCount", remainingMessages);
            }
        }

        public async Task LeaveRoom(int roomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId.ToString());
            await Clients.Group(roomId.ToString()).SendAsync("UserLeft", $"{Context.User!.Identity!.Name} đã rời phòng.");
        }
    }
}
