﻿using System.Linq.Expressions;

namespace ECommerceApp.Repository.IRepository
{
    public interface IRepository<T> where T :class
    {
        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter,string? includeProperties=null);
        T Get(Expression<Func<T,bool>> filter, string? includeProperties=null);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);
    }
}
