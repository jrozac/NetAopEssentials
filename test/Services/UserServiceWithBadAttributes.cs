using NetCoreAopEssentials.Cache.Models;
using NetCoreAopEssentialsTest.Models;

namespace NetCoreAopEssentialsTest.Services
{
    /// <summary>
    /// Service with bad attrbiutes
    /// </summary>
    public class UserServiceWithBadAttributes : UserService
    {
        /// <summary>
        /// Bad attribute
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Cacheable("user-{name}")]
        public new User GetUser(int id)
        {
            return base.GetUser(id);
        }
    }
}
