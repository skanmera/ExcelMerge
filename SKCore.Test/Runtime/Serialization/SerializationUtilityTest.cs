using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SKCore.Runtime.Serialization;

namespace SKCore.Test.Runtime.Serialization
{
    [TestClass]
    public class SerializationUtilityTest
    {
        [TestMethod]
        public void DeepCloneTest()
        {
            var user1 = new User(0, "AAA");
            var user2 = new User(1, "BBB");
            var users = new List<User> { user1, user2 };
            var userCollection = new UserCollection(users);

            var cloned = SerializationUtility.DeepClone(userCollection);

            Assert.AreEqual(userCollection, cloned);
            Assert.AreNotSame(userCollection, cloned);
            Assert.AreNotSame(userCollection.Users, cloned.Users);
        }
    }

    [Serializable]
    class UserCollection : IEquatable<UserCollection>
    {
        public List<User> Users { get; private set; }

        public UserCollection(IEnumerable<User> users)
        {
            Users = users.ToList();
        }

        public override bool Equals(object obj)
        {
            var other = obj as UserCollection;

            if (other == null)
                return false;

            return Equals(other);
        }

        public bool Equals(UserCollection other)
        {
            if (other == null)
                return false;

            return other.Users.SequenceEqual(Users);
        }
    }

    [Serializable]
    class User : IEquatable<User>
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public User(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            var other = obj as User;

            if (other == null)
                return false;

            return Equals(other);
        }

        public bool Equals(User other)
        {
            if (other == null)
                return false;

            return other.Name == Name && other.Id == Id;
        }
    }
}
