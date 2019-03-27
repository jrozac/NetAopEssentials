using System;
using NetAopEssentials.Cache;
using NetAopEssentialsTest.Models;

namespace NetAopEssentialsTest.Services
{

    /// <summary>
    /// User service 
    /// </summary>
    public class UserService : IUserService
    {

        /// <summary>
        /// Method run count
        /// </summary>
        public static int MethodRunCount { get; private set; } = 0;

        /// <summary>
        /// Reset method count
        /// </summary>
        public static void MethodRunCountReset() { MethodRunCount = 0; }

        /// <summary>
        /// Delete user by name 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public User DeleteByName(string name)
        {
            MethodRunCount++;
            if (name == "User1" || name == "User2")
            {
                return GetUser(int.Parse(name.Substring(4)));
            }
            return null;
        }

        /// <summary>
        /// Get serializable user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SerializableUser GetSerializableUser(int id)
        {
            var user = GetUser(id);
            if(user == null)
            {
                return null;
            }
            return new SerializableUser {
                Id = user.Id,
                Name = user.Name,
                RandomToken = user.RandomToken
            };
        }

        /// <summary>
        /// Get user 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public User GetUser(int id)
        {
            MethodRunCount++;
            if (id == 1 || id ==2)
            {
                return new User {
                    Id = id,
                    Name = $"User{id}",
                    RandomToken = Guid.NewGuid().ToString(),
                    TimeouMs = id * CacheTimeout.Second
                };
            }
            return null;
        }

        /// <summary>
        /// User update
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool UpdateUser(User user)
        {
            MethodRunCount++;
            return (user?.Id == 1 || user?.Id == 2);
        }
    }
}
