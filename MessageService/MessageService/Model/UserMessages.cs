using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MessageService.Model
{
    /// <summary>
    /// Модель для сообщений
    /// </summary>
    [DataContract]
    public class UserMessages
    {
        /// <summary>
        /// Тема сообщения
        /// </summary>
        [DataMember(Name = "subject")]
        public string Subject { get; set; } 

        /// <summary>
        /// Сообщение
        /// </summary>
        [DataMember(Name = "message")]
        [Required]
        public string Message { get; set; }

        /// <summary>
        /// Email отправителя
        /// </summary>
        [DataMember(Name = "senderId")]
        [Required]
        public string SenderId { get; set; }

        /// <summary>
        /// Email получателя
        /// </summary>
        [DataMember(Name = "receiverId")]
        [Required]
        public string ReceiverId { get; set; }
    }
}
