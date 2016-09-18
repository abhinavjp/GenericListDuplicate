using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GenericListDuplicate
{
    class Program
    {
        static void Main(string[] args)
        {
            var heroList = new List<Hero> {
                new Hero {
                    Id = 1,
                    Name = "Azarus",
                    Gender = "Male",
                    Origin = "Earth",
                    Type = "Wizard",
                    DateRecognized = DateTime.Parse("19/05/2145")
                },
                new Hero {
                    Id = 2,
                    Name = "Azarus",
                    Gender = "Male",
                    Origin = "Earth",
                    Type = "Warrior",
                    DateRecognized = DateTime.Parse("19/05/2145")
                },
                new Hero {
                    Id = 3,
                    Name = "Lympiq",
                    Gender = "Female",
                    Origin = "Water",
                    Type = "Rogue",
                    DateRecognized = DateTime.Parse("19/05/2145")
                },
                new Hero {
                    Id = 4,
                    Name = "Ria",
                    Gender = "Female",
                    Origin = "Fire",
                    Type = "Warden",
                    DateRecognized = DateTime.Parse("19/05/2056")
                },
                new Hero {
                    Id = 5,
                    Name = "Azarus",
                    Gender = "Male",
                    Origin = "Earth",
                    Type = "Universalist",
                    DateRecognized = DateTime.Parse("19/05/2145")
                },
                new Hero {
                    Id = 6,
                    Name = "Ria",
                    Gender = "Female",
                    Origin = "Fire",
                    Type = "Warden",
                    DateRecognized = DateTime.Parse("19/05/2056")
                },
                new Hero {
                    Id = 7,
                    Name = "Oret",
                    Gender = "Female",
                    Origin = "Water",
                    Type = "Mage",
                    DateRecognized = DateTime.Parse("01/03/2149")
                }
            };

            var duplicateList = GenericDuplicateChecker.CheckDuplicateList(heroList, "Name", "Gender", "Origin", "DateRecognized");
            Console.ReadKey();

        }
    }

    public class Hero
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Gender { get; set; }
        public string Origin { get; set; }
        public DateTime DateRecognized { get; set; }
    }

    public static class GenericDuplicateChecker
    {
        public static List<T> CheckDuplicateList<T>(List<T> objectData, params string[] parameters)
        {
            var groupByExpression = SelectOrGroupByExpression<T>(parameters).Compile();
            var groupedList = objectData.GroupBy(groupByExpression).Where(g => g.Skip(1).Any()).Select(s => s.First()).ToList();
            return groupedList;
        }

        

        public static Expression<Func<TItem, object>> SelectOrGroupByExpression<TItem>(string[] propertyNames)
        {
            var properties = propertyNames.Select(name => typeof(TItem).GetProperty(name)).ToArray();
            var propertyTypes = properties.Select(p => p.PropertyType).ToArray();
            var tupleTypeDefinition = typeof(Tuple).Assembly.GetType("System.Tuple`" + properties.Length);
            var tupleType = tupleTypeDefinition.MakeGenericType(propertyTypes);
            var constructor = tupleType.GetConstructor(propertyTypes);
            var param = Expression.Parameter(typeof(TItem), "item");
            var body = Expression.New(constructor, properties.Select(p => Expression.Property(param, p)));
            var expr = Expression.Lambda<Func<TItem, object>>(body, param);
            return expr;
        }

        
    }


}
