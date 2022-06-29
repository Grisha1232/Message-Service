using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MessageService.Model
{
    /// <summary>
    /// Модель юзеров
    /// </summary>
    [DataContract]
    public class User : IComparable<User>
    {
        /// <summary>
        /// Email пользователя
        /// </summary>
        [DataMember(Name = "email")]
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        [DataMember(Name = "name")]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Сравнение двух пользователей.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(User other)
        {
            return Email.CompareTo(other.Email);
        }

        /// <summary>
        /// Преобразование объекта пользователя в строку
        /// </summary>
        /// <returns>Строка</returns>
        public override string ToString()
        {
            return $"Email: {Email}   Name: {Name}";
        }
    }
}
