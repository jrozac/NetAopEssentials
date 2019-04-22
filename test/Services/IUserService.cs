using NetAopEssentialsTest.Models;

namespace NetAopEssentialsTest.Services
{

    /// <summary>
    /// User service interface 
    /// </summary>
    interface IUserService
    {

        /// <summary>
        /// Get user 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        User GetUser(int id);

        /// <summary>
        /// Get serializable user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        SerializableUser GetSerializableUser(int id);

        /// <summary>
        /// Update user 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        bool UpdateUser(User user);

        /// <summary>
        /// Delete user by name 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        User DeleteByName(string name);

    }
}
