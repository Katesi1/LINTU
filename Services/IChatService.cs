using System.Threading.Tasks;

namespace LMS.Services
{
    public interface IChatService
    {
        /// <summary>
        /// Kiểm tra xem người dùng đã đạt giới hạn tin nhắn hàng ngày chưa
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="roomId">ID của phòng chat</param>
        /// <returns>True nếu người dùng chưa đạt giới hạn, False nếu đã đạt giới hạn</returns>
        Task<bool> CanSendMessage(string userId, int roomId);

        /// <summary>
        /// Tăng số đếm tin nhắn của người dùng
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="roomId">ID của phòng chat</param>
        /// <returns>Số lượng tin nhắn còn lại trong giới hạn</returns>
        Task<int> IncrementMessageCount(string userId, int roomId);

        /// <summary>
        /// Lấy số lượng tin nhắn còn lại trong giới hạn của người dùng
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="roomId">ID của phòng chat</param>
        /// <returns>Số lượng tin nhắn còn lại</returns>
        Task<int> GetRemainingMessageCount(string userId, int roomId);
    }
}