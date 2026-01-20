// File: Program.cs
using MiniOrm.Core;
using MiniOrm.Database;
using MiniOrm.Entities;

namespace MiniOrm
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbInit = new DatabaseInitializer();
            dbInit.Initialize();
            var db = new MiniDbContext();

            Console.WriteLine("=== Tests MiniOrm début ===\n");

            Console.WriteLine("Utilisateurs dont le nom commence par 'A':");
            var usersStartsWithA = db.Set<User>()
                                      .Where(static u => u.Name.StartsWith("A"))
                                      .ToList();
            Console.WriteLine($"-> {string.Join(", ", usersStartsWithA.Select(u => u.Name))}\n");

            Console.WriteLine("Utilisateurs avec 'li' dans le nom (limité à 1):");
            var usersContainsLi = db.Set<User>()
                                     .Where(u => u.Name.Contains("li"))
                                     .Select(u => new { u.Id, u.Name })
                                     .Take(1)
                                     .ToList();
            Console.WriteLine($"-> {string.Join(", ", usersContainsLi.Select(u => u.Name))}\n");

            Console.WriteLine("Utilisateurs majeurs (age > 18):");
            var adultUsers = db.Set<User>()
                               .Where(u => u.Age > 18)
                               .ToList();
            foreach (var user in adultUsers)
            {
                Console.WriteLine($"-> {user.Id} - {user.Name} ({user.Age})");
            }
            Console.WriteLine();

            Console.WriteLine("Projection Nom + Age pour les utilisateurs >= 20:");
            var nameAndAge = db.Set<User>()
                               .Where(u => u.Age >= 20)
                               .Select(u => new { u.Name, u.Age })
                               .ToList();
            foreach (var item in nameAndAge)
            {
                Console.WriteLine($"-> {item.Name} ({item.Age})");
            }
            Console.WriteLine();

            Console.WriteLine("Premier utilisateur majeur:");
            var firstAdult = db.Set<User>()
                               .Where(u => u.Age >= 18)
                               .Take(1)
                               .ToList();
            if (firstAdult.Any())
            {
                Console.WriteLine($"-> {firstAdult.First().Name} ({firstAdult.First().Age})");
            }
            Console.WriteLine();

            Console.WriteLine("Tous les utilisateurs:");
            var allUsers = db.Set<User>().ToList();
            foreach (var user in allUsers)
            {
                Console.WriteLine($"-> {user.Id} - {user.Name} ({user.Age})");
            }

            Console.WriteLine("\n=== Tests MiniOrm finis ===\n");
        }
    }
}