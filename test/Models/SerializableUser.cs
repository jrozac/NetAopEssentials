﻿using System;

namespace NetAopEssentialsTest.Models
{

    /// <summary>
    /// Serializable user 
    /// </summary>
    [Serializable]
    public class SerializableUser
    {

        /// <summary>
        /// Get user id 
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User name 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Random token 
        /// </summary>
        public string RandomToken { get; set; }

    }
}
