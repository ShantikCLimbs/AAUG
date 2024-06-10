﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using AAUG.Context.Context;
using AAUG.DataAccess.Implementations.General;
using AAUG.DataAccess.Implementations;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using AAUG.DataAccess.Interfaces.General;


namespace AAUG.DataAccess.Implementations.UnitOfWork
{
    public class AaugUnitOfWork : IAaugUnitOfWork
    {

        private AaugContext dbContext;
        private IDbContextTransaction transaction;
        private bool disposed = false;
        private readonly IMapper mapper;
        public AaugUnitOfWork(AaugContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }
        //public AaugContext Context { get; private set; }
        public AaugContext Context => dbContext;

        #region Transaction methods
        public void SaveChanges()
        {
            dbContext.SaveChanges();
        }
        public async Task SaveChangesAsync()
        {
            await dbContext.SaveChangesAsync();
        }
        public void BeginTransaction()
        {
            if (transaction == null)
            {
                dbContext.Database.BeginTransaction();
            }
        }
        public void BeginTransactionAsync()
        {
            if (transaction == null)
            {
                dbContext.Database.BeginTransactionAsync();
            }
        }
        public void CommitTransaction()
        {
            try
            {
                if (transaction != null)
                {
                    dbContext.SaveChanges();
                    transaction.Commit();
                }
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                    transaction = null;
                }
            }
        }
        public async Task CommitTransactionAsync()
        {
            try
            {
                if (transaction != null)
                {
                    await dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync(); // Ensure RollbackTransactionAsync method is defined in the same class
                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                    transaction = null;
                }
            }
        }
        public void RollbackTransaction()
        {
            try
            {
                transaction?.Rollback();
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                    transaction = null;
                }
            }
        }
        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (transaction != null)
                {
                    await transaction.RollbackAsync();
                }
            }
            finally
            {
                if (transaction != null)
                {
                    await transaction.DisposeAsync();
                    transaction = null;
                }
            }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    dbContext.Dispose();
                    transaction?.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Repositories
        public required INewsRepository _newsRepository;
        public INewsRepository NewsRepository => _newsRepository ??= new NewsRepository(this, mapper);

        #endregion
    }
}