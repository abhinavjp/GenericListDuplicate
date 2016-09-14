using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            var groupedList = objectData.GroupBy(groupByExpression).Where(g => g.Skip(1).Any()).SelectMany(s => s).ToList();
            //var selectedList = groupedList.SelectMany(os => os).ToList();
            //var duplicateList = selectedList.DistinctBy(groupByExpression).ToList();
            return objectData;
        }

        static Expression<Func<T, object>> GroupByExpression<T>(string propertyName)
        {
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.Property(parameter, propertyName);
            return Expression.Lambda<Func<T, object>>(Expression.Convert(body, typeof(object)), parameter);
        }

        static Expression<Func<T, object>> GroupByExpression<T>(string[] propertyNames)
        {
            var groupedExpression = default(Expression<Func<T, object>>);
            foreach (var propertyName in propertyNames)
            {
                var exp = GroupByExpression<T>(propertyName);
                var parameter = Expression.Parameter(typeof(T));
                if (groupedExpression != null)
                {
                    var combinedExpr = TryCombiningExpressions(groupedExpression, exp);
                    groupedExpression = Expression.Lambda<Func<T, object>>(combinedExpr, parameter);
                    Console.Write(groupedExpression.ToString());
                }
                else
                {
                    groupedExpression = Expression.Lambda<Func<T, object>>(exp, parameter);
                }
            }
            return groupedExpression;
        }

        public static Expression<Func<T, object>> TryCombiningExpressions<T>(Expression<Func<T, object>> func1, Expression<Func<T, object>> func2)
        {
            return func1.CombineWithAndAlso(func2);
        }
    }

    public static class CombineExpressions
    {
        public static Expression<Func<TInput, object>> CombineWithAndAlso<TInput>(this Expression<Func<TInput, object>> func1, Expression<Func<TInput, object>> func2)
        {
            return Expression.Lambda<Func<TInput, object>>(
                Expression.AndAlso(
                    func1.Body, new ExpressionParameterReplacer(func2.Parameters, func1.Parameters).Visit(func2.Body)),
                func1.Parameters);
        }

        public static Expression<Func<TInput, object>> CombineWithOrElse<TInput>(this Expression<Func<TInput, object>> func1, Expression<Func<TInput, object>> func2)
        {
            return Expression.Lambda<Func<TInput, object>>(
                Expression.AndAlso(
                    func1.Body, new ExpressionParameterReplacer(func2.Parameters, func1.Parameters).Visit(func2.Body)),
                func1.Parameters);
        }

        private class ExpressionParameterReplacer : ExpressionVisitor
        {
            public ExpressionParameterReplacer(IList<ParameterExpression> fromParameters, IList<ParameterExpression> toParameters)
            {
                ParameterReplacements = new Dictionary<ParameterExpression, ParameterExpression>();
                for (int i = 0; i != fromParameters.Count && i != toParameters.Count; i++)
                    ParameterReplacements.Add(fromParameters[i], toParameters[i]);
            }

            private IDictionary<ParameterExpression, ParameterExpression> ParameterReplacements { get; set; }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                ParameterExpression replacement;
                if (ParameterReplacements.TryGetValue(node, out replacement))
                    node = replacement;
                return base.VisitParameter(node);
            }
        }
    }
}
