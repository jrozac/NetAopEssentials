using System;
using System.Collections.Generic;
using System.Text;
using NetCoreAopEssentials.Cache.Models;
using NetCoreAopEssentialsTest.Models;

namespace NetCoreAopEssentialsTest.Services
{

    /// <summary>
    /// User service decorated with cache attributes
    /// </summary>
    public class UserServiceWithAttributes : IUserService
    {
        /// <summary>
        /// Default service
        /// </summary>
        private readonly UserService _service;

        /// <summary>
        /// Constructor
        /// </summary>
        public UserServiceWithAttributes()
        {
            _service = new UserService();
        }

        /// <summary>
        /// Delete by name 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [CacheRemove("user-{_ret.Id}")]
        public User DeleteByName(string name)
        {
            return _service.DeleteByName(name);
        }

        /// <summary>
        /// Get serializable user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SerializableUser GetSerializableUser(int id)
        {
            return _service.GetSerializableUser(id);
        }

        /// <summary>
        /// Get user 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Cacheable("user-{id}")]
        public User GetUser(int id)
        {
            return _service.GetUser(id);
        }

        /// <summary>
        /// Update user 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [CacheRemove("user-{user.Id}")]
        public bool UpdateUser(User user)
        {
            return _service.UpdateUser(user);
        }
    }
}
