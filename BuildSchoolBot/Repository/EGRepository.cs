using BuildSchoolBot.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BuildSchoolBot.Repository
{
    public class EGRepository<T>where T :class
    {
        private TeamsBuyContext _context;
        public TeamsBuyContext context { get { return _context; } }
        public EGRepository(TeamsBuyContext context)
        {
            if(context == null)
            {
                throw new ArgumentException();

            }
            _context = context;
        }
        public void Create(T value)
        {
            _context.Entry(value).State = EntityState.Added;
        }
        public void Update(T value)
        {
            _context.Entry(value).State = EntityState.Modified;
        }
        public void Delete(T value)
        {
            _context.Entry(value).State = EntityState.Deleted;
        }
        public IQueryable<T> GetAll()
        {
            return _context.Set<T>();
        }
    }
}
