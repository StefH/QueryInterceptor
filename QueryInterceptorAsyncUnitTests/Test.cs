﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using QueryInterceptor;
using Xunit;

#if DNXCORE50
using Microsoft.Data.Entity;
#else
using System.Data.Entity;
#endif

namespace QueryInterceptorAsyncUnitTests
{
    public class EqualsToNotEqualsVisitor : ExpressionVisitor
    {
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.Equal)
            {
                // Change == to !=
                return Expression.NotEqual(node.Left, node.Right);
            }

            return base.VisitBinary(node);
        }
    }

    public class Test
    {
        [Fact]
        public void InterceptWith_IQueryable()
        {
            IQueryable<int> query = Enumerable.Range(0, 10).AsQueryable().Where(n => n % 2 == 0);

            var visitor = new EqualsToNotEqualsVisitor();
            IQueryable queryIntercepted = query.InterceptWith(visitor);
            Assert.NotNull(queryIntercepted);

            Assert.Equal(typeof(int), queryIntercepted.ElementType);
            Assert.Equal("QueryInterceptor.QueryTranslatorProviderAsync", queryIntercepted.Provider.ToString());

            Expression<Func<int, bool>> predicate = x => x >= 0;
            var queryCreated = queryIntercepted.Provider.CreateQuery(predicate);
            Assert.NotNull(queryCreated);
        }

        [Fact]
        public void InterceptWith_TestEqualsToNotEqualsVisitor()
        {
            IQueryable<int> query = Enumerable.Range(0, 10).AsQueryable().Where(n => n % 2 == 0);
            List<int> numbersEven = query.ToList();
            Assert.Equal(new List<int> { 0, 2, 4, 6, 8 }, numbersEven);

            var visitor = new EqualsToNotEqualsVisitor();
            List<int> numbersOdd = query.InterceptWith(visitor).Where(x => x >= 0).ToList();
            Assert.Equal(new List<int> { 1, 3, 5, 7, 9 }, numbersOdd);
        }

        [Fact]
        public void FirstAsync()
        {
            var queryEven = Enumerable.Range(0, 10).AsQueryable().Where(n => n % 2 == 0).AsQueryable();

            var visitor = new EqualsToNotEqualsVisitor();
            var queryOdd = queryEven.InterceptWith(visitor);
            //tween the following methods or properties: 'System.Data.Entity.QueryableExtensions.FirstAsync<TSource>(System.Linq.IQueryable<TSource>,
            //System.Linq.Expressions.Expression<System.Func<TSource, bool>>, System.Threading.CancellationToken)' and 'System.Linq.QueryableExtensions.
            var task = queryOdd.FirstAsync(n => n > 5, CancellationToken.None);
            task.ContinueWith(t => t.Result).Wait();

            Assert.Equal(7, task.Result);
        }
    }
}