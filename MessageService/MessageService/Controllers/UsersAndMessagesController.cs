using MessageService.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace MessageService.Controllers
{
    /// <summary>
    /// Контроллер пользователей и сообщений.
    /// </summary>
    [Route("[controller]")]
    public class UsersAndMessagesController : Controller
    {
        List<User> users = GetUsers();
        readonly List<UserMessages> messages = GetMessages();

        #region GET запросы

        #region GET запросы на пользователей
        /// <summary>
        /// Получение всех пользователей.
        /// </summary>
        /// <returns>Список всех пользователей</returns>
        [HttpGet("/Users")]
        public IEnumerable<User> GetUs() => users;


        /// <summary>
        /// Получение пользователя по email.
        /// </summary>
        /// <param name="email">Email, который нужно найти</param>
        /// <returns>Пользователь</returns>
        [HttpGet("/Users/{email}")]
        public IActionResult GetUserByEmail(string email)
        {
            var user = users.SingleOrDefault(u => u.Email == email);

            if (user == null)
                return NotFound("There is no such registered user");
            return Ok(user);
        }


        /// <summary>
        /// Получение списка пользователей, пропустив offset (кол-во) пользователей.
        /// </summary>
        /// <param name="offset">Кол-во пользователей с начала</param>
        /// <returns>Список пользователей</returns>
        [HttpGet("/Users/Offset={offset}")]
        public IEnumerable<User> GetUsersSkippedByOffset(int offset)
        {
            if (offset < 0)
            {
                BadRequest("The offset cannot be negative");
                return null;
            }
            var use = new List<User>();
            for (int i = offset; i < users.Count; i++)
            {
                use.Add(users[i]);
            }
            Ok();
            return use;
        }


        /// <summary>
        /// Получение списка пользователей, пропустив offset пользователей и полувив limit (кол-во) пользователей
        /// </summary>
        /// <param name="offset">Кол-во пользователей с начала</param>
        /// <param name="limit">Кол-во пользователей (ограничение)</param>
        /// <returns>Список пользователей</returns>
        [HttpGet("/Users/Offset={offset}/Limit={limit}")]
        public IEnumerable<User> GetUsersSkippedByOffsetAndLimit(int offset, int limit)
        {
            var use = GetUsersSkippedByOffset(offset).ToList();
            if (limit < 0) 
            {
                BadRequest("The limit cannot be negative");
                return null; 
            }
            var result = new List<User>();
            for (int i = 0; i < limit; i++)
            {
                if (i < use.Count)
                    result.Add(use[i]);
            }
            return result;
        }
        #endregion

        #region GET запросы на сообщения
        /// <summary>
        /// Получение всех сообщений.
        /// </summary>
        /// <returns>Список сообщений</returns>
        [HttpGet("/Messages")]
        public IEnumerable<UserMessages> GetMes() => messages;


        /// <summary>
        /// Получение списка сообщений по email отправителя.
        /// </summary>
        /// <param name="sender">email отправителя</param>
        /// <returns>Список сообщений</returns>
        [HttpGet("/Messages/Sender={sender}")]
        public IActionResult GetMessagesBySenderEmail(string sender)
        {
            bool exist = false;
            foreach (var item in users)
            {
                if (item.Email == sender)
                {
                    exist = true;
                    break;
                }
            }
            if (!exist)
                return NotFound("User does not exist");

            List<UserMessages> mess = new();
            foreach (var item in messages)
            {
                if (item.SenderId == sender)
                    mess.Add(item);
            }
            if (mess.Count == 0)
                return NotFound("The user has no messages yet");
            else
                return Ok(mess);
        }


        /// <summary>
        /// Получение списка сообщений по email получателя.
        /// </summary>
        /// <param name="receiver">email получателя</param>
        /// <returns>Список сообщений</returns>
        [HttpGet("/Messages/Receiver={receiver}")]
        public IActionResult GetMessagesByReceiverEmail(string receiver)
        {
            bool exist = false;
            foreach (var item in users)
            {
                if (item.Email == receiver)
                {
                    exist = true;
                    break;
                }
            }
            if (!exist)
                return NotFound("User does not exist");

            List<UserMessages> mess = new();
            foreach (var item in messages)
            {
                if (item.ReceiverId == receiver)
                    mess.Add(item);
            }
            if (mess.Count == 0)
                return NotFound("The user has no messages yet");
            else
                return Ok(mess);
        }


        /// <summary>
        /// Получение списка сообщений по email отправителя и получателя
        /// </summary>
        /// <param name="sender">email отправителя</param>
        /// <param name="receiver">email получателя</param>
        /// <returns>Сообщение</returns>
        [HttpGet("/Messages/{sender}/{receiver}")]
        public IActionResult GetMessagesByEmails(string sender, string receiver)
        {
            List<UserMessages> mess = new();
            foreach (var item in messages)
            {
                Console.WriteLine(item);
                if (item.SenderId == sender && item.ReceiverId == receiver)
                    mess.Add(item);
            }
            if (mess.Count == 0)
                return NotFound("These users have no conversations");
            return Ok(mess);
        }
        #endregion

        #endregion

        #region POST запросы

        #region POST запросы на пользователей
        /// <summary>
        /// Добавление пользователя в список пользователей.
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <returns>Пользователь</returns>
        [HttpPost("/Users")]
        public IActionResult PostUser(User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            foreach (var item in users)
            {
                if (item.Email == user.Email)
                    return BadRequest("This user already exists");
            }
            if (!IsValidEmail(user.Email))
                return BadRequest("Invalid email address");
            users.Add(user);
            users.Sort();
            var sep = Path.DirectorySeparatorChar;
            using (FileStream fs = new(Environment.CurrentDirectory + $"{sep}Data{sep}users.json", FileMode.Create))
            {
                var serialize = new DataContractJsonSerializer(typeof(List<User>));
                serialize.WriteObject(fs, users);
            }
            return Ok(user);
        }


        /// <summary>
        /// Заполнение рандомных пользователей.
        /// </summary>
        /// <returns>Список пользователей</returns>
        [HttpPost("/Users/Random")]
        public IActionResult PostRandom()
        {
            List<User> randomUsers = GenerateUsers();
            users = randomUsers;
            users.Sort();
            var sep = Path.DirectorySeparatorChar;
            using (FileStream fs = new(Environment.CurrentDirectory + $"{sep}Data{sep}users.json", FileMode.Create))
            {
                var serialize = new DataContractJsonSerializer(typeof(List<User>));
                serialize.WriteObject(fs, users);
            }

            return Ok(users);
        }


        /// <summary>
        /// Добавление рандомного Пользователя
        /// </summary>
        /// <returns>Пользователь</returns>
        [HttpPost("/Users/AddRandom")]
        public IActionResult PostAddRandom()
        {
            User randomUser = new();
            randomUser.Name = GenerateName();
            randomUser.Email = GenerateEmail();
            users.Add(randomUser);
            users.Sort();
            var sep = Path.DirectorySeparatorChar;
            using (FileStream fs = new(Environment.CurrentDirectory + $"{sep}Data{sep}users.json", FileMode.Create))
            {
                var serialize = new DataContractJsonSerializer(typeof(List<User>));
                serialize.WriteObject(fs, users);
            }

            return Ok(randomUser);
        }

        #endregion

        #region POST запросы на сообщения

        /// <summary>
        /// Добавление сообщения в список сообщений.
        /// </summary>
        /// <param name="mess">Сообщение</param>
        /// <returns>Сообщение</returns>
        [HttpPost("/Messages/")]
        public IActionResult PostMessage(UserMessages mess)
        {
            if (!ModelState.IsValid)
                return BadRequest(mess);

            bool isSenderOk = false, isRecieverOk = false;

            foreach (var item in users)
            {
                Console.WriteLine(item);
                if (item.Email == mess.SenderId)
                    isSenderOk = true;
                if (item.Email == mess.ReceiverId)
                    isRecieverOk = true;
                if (isRecieverOk && isSenderOk)
                    break;
            }

            if (isSenderOk && isRecieverOk)
            {
                var sep = Path.DirectorySeparatorChar;
                messages.Add(mess);
                using (FileStream fs = new(Environment.CurrentDirectory + $"{sep}Data{sep}messages.json", FileMode.Create))
                {
                    var serialize = new DataContractJsonSerializer(typeof(List<UserMessages>));
                    serialize.WriteObject(fs, messages);
                }
                return Ok(mess);
            }
            else
            {
                return NotFound("No such registered user");
            }

        }


        /// <summary>
        /// Генерация рандомных сообщений
        /// </summary>
        /// <returns>Список сообщений</returns>
        [HttpPost("/Messages/Random")]
        public IActionResult PostRandomMessages()
        {
            if (users.Count == 0)
                return BadRequest("There are no registered users at all");
            var rng = new Random();
            List<UserMessages> messages = new();
            var n = rng.Next(10, 100);
            for (int i = 0; i < n; i++)
            {
                UserMessages mess = new();
                mess.Subject = GenerateSubject();
                mess.Message = GenerateMessage();
                mess.SenderId = users[rng.Next(0, users.Count)].Email;
                mess.ReceiverId = users[rng.Next(0, users.Count)].Email;
                messages.Add(mess);
            }
            var sep = Path.DirectorySeparatorChar;
            using (FileStream fs = new(Environment.CurrentDirectory + $"{sep}Data{sep}messages.json", FileMode.Create))
            {
                var serialize = new DataContractJsonSerializer(typeof(List<UserMessages>));
                serialize.WriteObject(fs, messages);
            }
            return Ok(messages);
        }
        #endregion

        #endregion

        #region Вспомогательные методы
        /// <summary>
        /// Получение пользователей из Json.
        /// </summary>
        /// <returns>Список пользователей</returns>
        private static List<User> GetUsers()
        {
            List<User> users = new();
            try
            {
                var sep = Path.DirectorySeparatorChar;
                using (FileStream fs = new(Environment.CurrentDirectory + $"{sep}Data{sep}users.json", FileMode.Open))
                {
                    var formatter = new DataContractJsonSerializer(typeof(List<User>));
                    users = (List<User>)formatter.ReadObject(fs);
                }
            }
            catch
            {

            }
            return users;
        }


        /// <summary>
        /// Получения списка всех сообщений.
        /// </summary>
        /// <returns>Список сообщений</returns>
        private static List<UserMessages> GetMessages()
        {
            List<UserMessages> messages = new();
            try
            {
                var sep = Path.DirectorySeparatorChar;
                using (FileStream fs = new(Environment.CurrentDirectory + $"{sep}Data{sep}messages.json", FileMode.Open))
                {
                    var formatter = new DataContractJsonSerializer(typeof(List<UserMessages>));
                    messages = (List<UserMessages>)formatter.ReadObject(fs);
                }
            }
            catch
            {

            }
            return messages;
        }


        /// <summary>
        /// Рандомная генерация пользователей.
        /// </summary>
        /// <returns>Список пользователей</returns>
        private static List<User> GenerateUsers()
        {
            var rng = new Random();

            List<User> users = new();
            List<string> emails = new();
            for (int i = 0; i < rng.Next(1, 20); i++)
            {
                string email = GenerateEmail();
                string name = GenerateName();
                if (emails.Contains(email))
                {
                    i--;
                    continue;
                }
                emails.Add(email);
                User user = new User();
                user.Email = email;
                user.Name = name;
                users.Add(user);
            }
            return users;
        }


        /// <summary>
        /// Генерация рандомного email.
        /// </summary>
        /// <returns>email</returns>
        private static string GenerateEmail()
        {
            var rng = new Random();
            string email = "";
            for (int i = 0; i < rng.Next(5, 20); i++)
            {
                email += rng.Next(0, 2) == 0 ? (char)rng.Next(65, 91) : (char)rng.Next(97, 123);
            }
            email += "@" + (rng.Next(0, 2) == 0 ? "yandex.ru" : "mail.ru");
            return email;
        }


        /// <summary>
        /// Генерация рандомного имени.
        /// </summary>
        /// <returns>Имя(string)</returns>
        private static string GenerateName()
        {
            var rng = new Random();
            string name = "";
            for (int i = 0; i < rng.Next(5, 20); i++)
            {
                name += rng.Next(0, 2) == 0 ? (char)rng.Next(65, 91) : (char)rng.Next(97, 123);
            }
            return name;
        }


        /// <summary>
        /// Генерация темы сообщения.
        /// </summary>
        /// <returns>Тема сообщения (string)</returns>
        private static string GenerateSubject()
        {
            var rng = new Random();
            string subject = "";
            for (int i = 0; i < rng.Next(10, 30); i++)
                subject += (char)rng.Next(32, 127);
            return subject;
        }


        /// <summary>
        /// Генерация рандомного сообщения (отправитель и получатель также рандомны).
        /// </summary>
        /// <returns>Сообщение (string)</returns>
        private static string GenerateMessage()
        {
            string message = "";
            var rng = new Random();
            for (int i = 0; i < rng.Next(30, 80); i++)
            {
                message += (char)rng.Next(32, 127);
            }
            return message;
        }

        /// <summary>
        /// Проверка на правильность введенного email.
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>TRUE в случае если правильный email, FALSE в обратном</returns>
        private static bool IsValidEmail(string email)
        {
            if (email.Split("@").Length == 2 && email.Split(".").Length == 2 && !HaveSpecialSymbols(email))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Проверка на наличие в строке спец символов.
        /// </summary>
        /// <param name="str">строка</param>
        /// <returns>TRUE, если есть спец символы, FALSE в противном случае</returns>
        private static bool HaveSpecialSymbols(string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                Console.WriteLine(str[i]);
                if (((char)str[i] >= 33 && (char)str[i] < 46) || 
                    ((char)str[i] > 46 && (char)str[i] <= 47) ||
                    ((char)str[i] >= 58 && (char)str[i] <= 63) || 
                    ((char)str[i] > 91 && (char)str[i] < 96) ||
                    ((char)str[i] >= 123))
                    return true;
            }
            return false;
        }
        #endregion
    }
}
