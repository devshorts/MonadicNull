using System;
using System.Collections.Generic;

namespace NoNulls.Tests.SampleData
{    
    public sealed class User
    {
        public School School { get; set; }

        public School GetSchool()
        {
            return School;
        }

        public int Number { get; set; }

        public User Field;

        public IEnumerable<User> ClassMatesEnumerable { get; set; }

        public List<User> ClassMatesList { get; set; }

        public Dictionary<User, User> ClassMatesDict { get; set; }

        public HashSet<User> ClassMatesHash { get; set; }

        public DateTime DateOfBirth { get; set; }

        public DateTime? GraduationDate { get; set; }

        public int? ScholarshipReceived { get; set; }
    }

    public sealed class School
    {
        public District District { get; set; }

        public IEnumerable<User> ClassMatesEnumerable { get; set; }

        public List<User> ClassMatesList { get; set; }

        public Dictionary<User, User> ClassMatesDict { get; set; }

        public HashSet<User> ClassMatesHash { get; set; }

        public District GetDistrict()
        {
            return District;
        }
    }

    public sealed class District
    {
        public Street Street { get; set; }

        public Street GetStreet()
        {
            return Street;
        }
    }

    public class Street
    {
        public String Name { get; set; }

        public int Number { get; set; }

        public String GetName(int i)
        {
            return Name + i;
        }

        public String GetNameSideEffect(int i)
        {
            Name += i;

            return Name;
        }

        public String GetName(Func<int> action)
        {
            return GetName(action());
        }
    }
}


    



