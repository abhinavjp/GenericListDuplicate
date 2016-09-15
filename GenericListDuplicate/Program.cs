using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
                    Type = "Wizard"
                },
                new Hero {
                    Id = 2,
                    Name = "Azarus",
                    Gender = "Male",
                    Origin = "Earth",
                    Type = "Warrior"
                },
                new Hero {
                    Id = 3,
                    Name = "Lympiq",
                    Gender = "Female",
                    Origin = "Water",
                    Type = "Rogue"
                },
                new Hero {
                    Id = 4,
                    Name = "Ria",
                    Gender = "Female",
                    Origin = "Fire",
                    Type = "Warden"
                },
                new Hero {
                    Id = 5,
                    Name = "Azarus",
                    Gender = "Male",
                    Origin = "Earth",
                    Type = "Universalist"
                },
                new Hero {
                    Id = 6,
                    Name = "Ria",
                    Gender = "Female",
                    Origin = "Fire",
                    Type = "Warrior"
                },
                new Hero {
                    Id = 7,
                    Name = "Oret",
                    Gender = "Female",
                    Origin = "Water",
                    Type = "Mage"
                }
            };

            var duplicateList = GenericDuplicateChecker.CheckDuplicateList(heroList, "Id", "Name", "Gender", "Origin");
            Console.ReadKey();

        }
    }

    public class Hero : IEqualityComparer<Hero>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Gender { get; set; }
        public string Origin { get; set; }

        public bool Equals(Hero hero1, Hero hero2)
        {
            if (hero1.Gender == hero2.Gender && hero1.Name == hero2.Name && hero1.Origin == hero2.Origin)
                return true;
            return false;
        }

        public int GetHashCode(Hero obj)
        {
            return obj.GetHashCode();
        }
    }

    public static class GenericDuplicateChecker
    {
        public static List<T> CheckDuplicateList<T>(List<T> objectData, string idParameter, params string[] parameters)
        {
            var groupByExpression = GroupByExpression<T>(parameters).Compile();
            var idProp = typeof(T).GetType().GetProperty(idParameter);
            var groupedList = objectData.GroupBy(groupByExpression).Where(g => g.Skip(1).Any()).Select(s => s.First()).ToList();
            //var selectedList = groupedList.SelectMany(os => os).ToList();
            //var duplicateList = selectedList.DistinctBy(groupByExpression).ToList();
            return objectData;
        }

        static Expression<Func<T, object>> GroupByExpression<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.Property(parameter, propertyName);
            return Expression.Lambda<Func<T, object>>(Expression.PropertyOrField(body, propertyName), parameter);
        }
        static Expression<Func<T, object>> GroupByExpressionAggregate<T>(string[] propertyNames, ParameterExpression parameter)
        {
            var endExpression = default(Expression<Func<T, object>>);
            var expression = default(Expression);
            var propertyExpressions = propertyNames.Select(p => GetDeepPropertyExpression(parameter, p)).ToArray();            

            if (propertyExpressions.Length == 1)
                expression = propertyExpressions[0];
            else
            {
                var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string), typeof(string) });

                var separator = Expression.Constant(",");
                expression = propertyExpressions.Aggregate(
                    (x, y) => Expression.Call(concatMethod, x, separator, y));
            }
            
            endExpression = Expression.Lambda<Func<T, object>>(expression, parameter);
            return endExpression;
        }

        static Expression<Func<T, object>> GroupByExpression<T>(string propertyName, ParameterExpression parameter)
        {
            //var body = Expression.Property(parameter, propertyName);
            return Expression.Lambda<Func<T, object>>(Expression.PropertyOrField(parameter, propertyName), parameter);
        }

        static Expression<Func<T, object>> GroupByExpression<T>(string[] propertyNames)
        {
            var groupedExpression = default(Expression<Func<T, object>>);
            var parameter = Expression.Parameter(typeof(T));
            groupedExpression = GroupByExpressionAggregate<T>(propertyNames, parameter);
            return groupedExpression;
        }

        private static Expression GetDeepPropertyExpression(Expression initialInstance, string property)
        {
            Expression result = null;
            foreach (var propertyName in property.Split('.'))
            {
                Expression instance = result;
                if (instance == null)
                    instance = initialInstance;
                result = Expression.Property(instance, propertyName);
            }
            return result;
        }
    }


}
