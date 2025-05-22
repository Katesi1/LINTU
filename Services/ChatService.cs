using LMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace LMS.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const int DEFAULT_DAILY_MESSAGE_LIMIT = 50;

        public ChatService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<bool> CanSendMessage(string userId, int roomId)
        {
            int messageCount = await GetTodayMessageCount(userId, roomId);
            return messageCount < DEFAULT_DAILY_MESSAGE_LIMIT;
        }

        public async Task<int> IncrementMessageCount(string userId, int roomId)
        {
            string cacheKey = GetCacheKey(userId, roomId);

            int currentCount = await GetTodayMessageCount(userId, roomId);

            if (currentCount < DEFAULT_DAILY_MESSAGE_LIMIT)
            {
                // Tăng số đếm tin nhắn
                _cache.Set(cacheKey, currentCount + 1, GetEndOfDayExpiration());
                return DEFAULT_DAILY_MESSAGE_LIMIT - (currentCount + 1);
            }

            return 0;
        }

        public async Task<int> GetRemainingMessageCount(string userId, int roomId)
        {
            int messageCount = await GetTodayMessageCount(userId, roomId);
            return Math.Max(0, DEFAULT_DAILY_MESSAGE_LIMIT - messageCount);
        }

        private async Task<int> GetTodayMessageCount(string userId, int roomId)
        {
            string cacheKey = GetCacheKey(userId, roomId);

            // Kiểm tra trong bộ nhớ cache trước
            if (_cache.TryGetValue(cacheKey, out int cachedCount))
            {
                return cachedCount;
            }

            // Không có trong cache, truy vấn từ cơ sở dữ liệu
            DateTime today = DateTime.Today;
            int count = await _context.Messages
                .CountAsync(m => m.UserId == userId &&
                                m.ChatRoomId == roomId &&
                                m.Timestamp.Date == today);

            // Lưu vào cache
            _cache.Set(cacheKey, count, GetEndOfDayExpiration());

            return count;
        }

        private string GetCacheKey(string userId, int roomId)
        {
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            return $"Chat_Message_Count_{userId}_{roomId}_{today}";
        }

        private TimeSpan GetEndOfDayExpiration()
        {
            DateTime now = DateTime.Now;
            DateTime endOfDay = now.Date.AddDays(1);
            return endOfDay - now;
        }
    }
}