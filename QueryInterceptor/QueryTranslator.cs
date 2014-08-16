﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace QueryInterceptor
{
    internal class QueryTranslator<T> : IOrderedQueryable<T>
    {
        private readonly Expression _expression;
        private readonly QueryTranslatorProviderAsync _provider;

        public QueryTranslator(IQueryable source, IEnumerable<ExpressionVisitor> visitors)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (visitors == null)
            {
                throw new ArgumentNullException("visitors");
            }

            _expression = Expression.Constant(this);
            _provider = new QueryTranslatorProviderAsync(source, visitors);
        }

        public QueryTranslator(IQueryable source, Expression expression, IEnumerable<ExpressionVisitor> visitors)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            if (visitors == null)
            {
                throw new ArgumentNullException("visitors");
            }

            _expression = expression;
            _provider = new QueryTranslatorProviderAsync(source, visitors);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_provider.ExecuteEnumerable(_expression)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _provider.ExecuteEnumerable(_expression).GetEnumerator();
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public Expression Expression
        {
            get { return _expression; }
        }

        public IQueryProvider Provider
        {
            get { return _provider; }
        }
    }
}