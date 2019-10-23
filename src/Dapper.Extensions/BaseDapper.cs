﻿using Dapper.Extensions.Caching;
using Dapper.Extensions.MiniProfiler;
using Dapper.Extensions.SQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dapper.Extensions.MasterSlave;

namespace Dapper.Extensions
{
    public abstract partial class BaseDapper : IDapper, IDisposable
    {
        public Lazy<IDbConnection> Conn { get; }

        protected IDbTransaction Transaction { get; set; }

        protected IConfiguration Configuration { get; }

        protected abstract IDbConnection CreateConnection(string connectionName);

        protected CacheConfiguration CacheConfiguration { get; }

        private ICacheProvider Cache { get; }

        private ICacheKeyBuilder CacheKeyBuilder { get; }

        private IDbMiniProfiler DbMiniProfiler { get; }

        private ISQLManager SQLManager { get; }

        private bool ReadOnly { get; }

        private bool EnableMasterSlave { get; }

        private ConnectionConfigureManager ConnectionConfigureManager { get; }

        protected BaseDapper(IServiceProvider serviceProvider, string connectionName = "DefaultConnection", bool enableMasterSlave = false, bool readOnly = false)
        {
            if (!enableMasterSlave && readOnly)
                throw new InvalidOperationException($"The connection with the name '{connectionName}' does not enable the master-slave");
            EnableMasterSlave = enableMasterSlave;
            ReadOnly = readOnly;
            Configuration = serviceProvider.GetRequiredService<IConfiguration>();
            Cache = serviceProvider.GetService<ICacheProvider>();
            CacheConfiguration = serviceProvider.GetService<CacheConfiguration>();
            CacheKeyBuilder = serviceProvider.GetService<ICacheKeyBuilder>();
            DbMiniProfiler = serviceProvider.GetService<IDbMiniProfiler>();
            SQLManager = serviceProvider.GetService<ISQLManager>();
            Conn = new Lazy<IDbConnection>(() => CreateConnection(connectionName));
            if (enableMasterSlave)
                ConnectionConfigureManager = serviceProvider.GetService<ConnectionConfigureManager>();
        }

        protected IDbConnection GetConnection(string connectionName, DbProviderFactory factory)
        {
            var connString = EnableMasterSlave ? ConnectionConfigureManager.GetConnectionString(connectionName, ReadOnly) : Configuration.GetConnectionString(connectionName);
            if (string.IsNullOrWhiteSpace(connString))
                throw new ArgumentNullException(nameof(connString), "The config of " + connectionName + " cannot be null.");
            var conn = factory.CreateConnection();
            if (conn == null)
                throw new ArgumentNullException(nameof(IDbConnection), "Failed to create database connection.");
            conn.ConnectionString = connString;
            conn.Open();
            return DbMiniProfiler == null ? conn : DbMiniProfiler.CreateConnection(conn);
        }


        public virtual List<TReturn> Query<TReturn>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query<TReturn>(sql, param, Transaction, buffered, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public List<TReturn> Query<TReturn>(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return Query<TReturn>(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey, commandType, buffered);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public List<TReturn> Query<TFirst, TSecond, TReturn>(SQLName name, Func<TFirst, TSecond, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null, bool buffered = true)
        {
            return Query(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, commandType, buffered);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null, bool buffered = true)
        {
            return Query(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, commandType, buffered);
        }

        public virtual List<TResult> Query<TFirst, TSecond, TThird, TFourth, TResult>(string sql, Func<TFirst, TSecond, TThird, TFourth, TResult> map, object param = null, string splitOn = "Id",
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return Query(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, commandType, buffered);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return Query(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, commandType, buffered);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null,
            string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return Query(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, commandType, buffered);
        }

        public virtual List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, map, param, Transaction, buffered, splitOn, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public List<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(SQLName name, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map,
            object param = null, string splitOn = "Id", int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return Query(GetSQL(name), map, param, splitOn, commandTimeout, enableCache, cacheExpire, cacheKey, commandType, buffered);
        }


        public virtual List<dynamic> Query(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return CommandExecute(enableCache, () => Conn.Value.Query(sql, param, Transaction, buffered, commandTimeout, commandType).ToList(), sql, param, cacheKey, cacheExpire);
        }

        public List<dynamic> Query(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null, bool buffered = true)
        {
            return Query(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey, commandType, buffered);
        }


        public virtual TReturn QueryFirstOrDefault<TReturn>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            return CommandExecute(enableCache, () => Conn.Value.QueryFirstOrDefault<TReturn>(sql, param, Transaction, commandTimeout, commandType), sql, param, cacheKey, cacheExpire);
        }

        public TReturn QueryFirstOrDefault<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            return QueryFirstOrDefault<TReturn>(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey, commandType);
        }


        public virtual dynamic QueryFirstOrDefault(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            return CommandExecute(enableCache, () => Conn.Value.QueryFirstOrDefault(sql, param, Transaction, commandTimeout, commandType), sql, param, cacheKey, cacheExpire);
        }

        public dynamic QueryFirstOrDefault(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            return QueryFirstOrDefault(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey, commandType);
        }

        public virtual dynamic QuerySingleOrDefault(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            return CommandExecute(enableCache, () => Conn.Value.QuerySingleOrDefault(sql, param, Transaction, commandTimeout, commandType), sql, param, cacheKey, cacheExpire);
        }

        public dynamic QuerySingleOrDefault(SQLName name, object param = null, int? commandTimeout = null, bool? enableCache = default,
            TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            return QuerySingleOrDefault(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey, commandType);
        }


        public virtual TReturn QuerySingleOrDefault<TReturn>(string sql, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default, CommandType? commandType = null)
        {
            return CommandExecute(enableCache, () => Conn.Value.QuerySingleOrDefault<TReturn>(sql, param, Transaction, commandTimeout, commandType), sql, param, cacheKey, cacheExpire);
        }

        public TReturn QuerySingleOrDefault<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default,
            CommandType? commandType = null)
        {
            return QuerySingleOrDefault<TReturn>(GetSQL(name), param, commandTimeout, enableCache, cacheExpire, cacheKey, commandType);
        }


        public virtual void QueryMultiple(string sql, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            using var multi = Conn.Value.QueryMultiple(sql, param, Transaction, commandTimeout, commandType);
            reader(multi);
        }

        public void QueryMultiple(SQLName name, Action<SqlMapper.GridReader> reader, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            QueryMultiple(GetSQL(name), reader, param, commandTimeout, commandType);
        }

        public virtual IDataReader ExecuteReader(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Conn.Value.ExecuteReader(sql, param, Transaction, commandTimeout, commandType);
        }

        public IDataReader ExecuteReader(SQLName name, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return ExecuteReader(GetSQL(name), param, commandTimeout, commandType);
        }


        public virtual PageResult<TReturn> QueryPage<TReturn>(string countSql, string dataSql, int pageindex, int pageSize, object param = null, int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            if (pageindex < 1)
                throw new ArgumentException("The pageindex cannot be less then 1.");
            if (pageSize < 1)
                throw new ArgumentException("The pageSize cannot be less then 1.");
            var pars = new DynamicParameters();
            if (param != null)
                pars.AddDynamicParams(param);

            pars.AddDynamicParams(new
            {
                TakeStart = (pageindex - 1) * pageSize + 1,
                TakeEnd = pageindex * pageSize,
                Skip = (pageindex - 1) * pageSize,
                Take = pageSize
            });
            var sql = $"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}";
            return CommandExecute(enableCache, () =>
            {
                using var multi = Conn.Value.QueryMultiple(sql, pars, Transaction, commandTimeout);
                var count = multi.Read<long>().FirstOrDefault();
                var data = multi.Read<TReturn>().ToList();
                var result = new PageResult<TReturn>
                {
                    TotalCount = count,
                    Page = pageindex,
                    PageSize = pageSize,
                    Contents = data
                };
                result.TotalPage = result.TotalCount % pageSize == 0
                    ? result.TotalCount / pageSize
                    : result.TotalCount / pageSize + 1;
                if (result.Page > result.TotalPage)
                    result.Page = result.TotalPage;
                return result;
            }, sql, pars, cacheKey, cacheExpire, pageindex, pageSize);
        }

        public PageResult<TReturn> QueryPage<TReturn>(SQLName name, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default,
            string cacheKey = default)
        {
            var (countSql, querySql) = GetPagingSQL(name);
            return QueryPage<TReturn>(countSql, querySql, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey);
        }

        public virtual List<TReturn> QueryPlainPage<TReturn>(string sql, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            if (pageindex < 1)
                throw new ArgumentException("The pageindex cannot be less then 1.");
            if (pageSize < 1)
                throw new ArgumentException("The pageSize cannot be less then 1.");
            var pars = new DynamicParameters();
            if (param != null)
                pars.AddDynamicParams(param);

            pars.AddDynamicParams(new
            {
                TakeStart = (pageindex - 1) * pageSize + 1,
                TakeEnd = pageindex * pageSize,
                Skip = (pageindex - 1) * pageSize,
                Take = pageSize
            });

            return CommandExecute(enableCache, () => Conn.Value.Query<TReturn>(sql, pars, Transaction, true, commandTimeout).ToList(), sql, pars, cacheKey, cacheExpire, pageindex, pageSize);
        }

        public List<TReturn> QueryPlainPage<TReturn>(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            return QueryPlainPage<TReturn>(GetSQL(name), pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey);
        }

        public virtual PageResult<dynamic> QueryPage(string countSql, string dataSql, int pageindex, int pageSize, object param = null,
            int? commandTimeout = null, bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            if (pageindex < 1)
                throw new ArgumentException("The pageindex cannot be less then 1.");
            if (pageSize < 1)
                throw new ArgumentException("The pageSize cannot be less then 1.");
            var pars = new DynamicParameters();
            if (param != null)
                pars.AddDynamicParams(param);

            pars.AddDynamicParams(new
            {
                TakeStart = (pageindex - 1) * pageSize + 1,
                TakeEnd = pageindex * pageSize,
                Skip = (pageindex - 1) * pageSize,
                Take = pageSize
            });
            var sql = $"{countSql}{(countSql.EndsWith(";") ? "" : ";")}{dataSql}";
            return CommandExecute(enableCache, () =>
            {
                using var multi = Conn.Value.QueryMultiple(sql, pars, Transaction, commandTimeout);
                var count = multi.Read<long>().FirstOrDefault();
                var data = multi.Read().ToList();
                var result = new PageResult<dynamic>
                {
                    TotalCount = count,
                    Page = pageindex,
                    PageSize = pageSize,
                    Contents = data
                };
                result.TotalPage = result.TotalCount % pageSize == 0
                    ? result.TotalCount / pageSize
                    : result.TotalCount / pageSize + 1;
                if (result.Page > result.TotalPage)
                    result.Page = result.TotalPage;
                return result;
            }, sql, pars, cacheKey, cacheExpire, pageindex, pageSize);

        }

        public PageResult<dynamic> QueryPage(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            var (countSql, querySql) = GetPagingSQL(name);
            return QueryPage(countSql, querySql, pageindex, pageSize, param, commandTimeout, enableCache, cacheExpire, cacheKey);
        }

        public virtual List<dynamic> QueryPlainPage(string sql, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            if (pageindex < 1)
                throw new ArgumentException("The pageindex cannot be less then 1.");
            if (pageSize < 1)
                throw new ArgumentException("The pageSize cannot be less then 1.");
            var pars = new DynamicParameters();
            if (param != null)
                pars.AddDynamicParams(param);

            pars.AddDynamicParams(new
            {
                TakeStart = (pageindex - 1) * pageSize + 1,
                TakeEnd = pageindex * pageSize,
                Skip = (pageindex - 1) * pageSize,
                Take = pageSize
            });

            return CommandExecute(enableCache, () => Conn.Value.Query(sql, pars, Transaction, true, commandTimeout).ToList(), sql, pars, cacheKey, cacheExpire, pageindex, pageSize);
        }

        public List<dynamic> QueryPlainPage(SQLName name, int pageindex, int pageSize, object param = null, int? commandTimeout = null,
            bool? enableCache = default, TimeSpan? cacheExpire = default, string cacheKey = default)
        {
            return QueryPlainPage(GetSQL(name), pageindex, pageSize, pageSize, commandTimeout, enableCache, cacheExpire, cacheKey);
        }


        public virtual int Execute(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Conn.Value.Execute(sql, param, Transaction, commandTimeout, commandType);
        }

        public int Execute(SQLName name, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Execute(GetSQL(name), param, commandTimeout, commandType);
        }


        public virtual TReturn ExecuteScalar<TReturn>(string sql, object param = null, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Conn.Value.ExecuteScalar<TReturn>(sql, param, Transaction, commandTimeout, commandType);
        }

        public TReturn ExecuteScalar<TReturn>(SQLName name, object param = null, int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return ExecuteScalar<TReturn>(GetSQL(name), param, commandTimeout, commandType);
        }

        #region Transaction

        public virtual IDbTransaction BeginTransaction()
        {
            return Transaction = Conn.Value.BeginTransaction();
        }

        public virtual IDbTransaction BeginTransaction(IsolationLevel level)
        {
            return Transaction = Conn.Value.BeginTransaction(level);
        }

        public virtual void CommitTransaction()
        {
            if (Transaction == null)
                throw new InvalidOperationException("Please call the BeginTransaction method first.");
            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
        }

        public virtual void RollbackTransaction()
        {
            if (Transaction == null)
                throw new InvalidOperationException("Please call the BeginTransaction method first.");
            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
        }



        #endregion

        public virtual void Dispose()
        {
            if (!Conn.IsValueCreated) return;
            Transaction?.Dispose();
            Conn.Value?.Close();
            Conn.Value?.Dispose();
        }


        #region Cache methods

        protected bool IsEnableCache(bool? enable)
        {
            if (CacheConfiguration == null)
                return false;
            if (enable.HasValue)
                return enable.Value;
            return CacheConfiguration.AllMethodsEnableCache;
        }

        protected TReturn CommandExecute<TReturn>(bool? enableCache, Func<TReturn> execQuery, string sql, object param, string cacheKey, TimeSpan? expire, int? pageIndex = default, int? pageSize = default)
        {
            if (!IsEnableCache(enableCache))
                return execQuery();
            cacheKey = CacheKeyBuilder.Generate(sql, param, cacheKey, pageIndex, pageSize);
            var cache = Cache.TryGet<TReturn>(cacheKey);
            if (cache.HasKey)
                return cache.Value;
            var result = execQuery();
            Cache.TrySet(cacheKey, result, expire ?? CacheConfiguration.Expire);
            return result;
        }

        #endregion


        public string GetSQL(string id)
        {
            if (SQLManager == null)
                throw new InvalidOperationException("Please call the 'AddSQLSeparateForDapper' method to register first.");
            return SQLManager.GetSQL(id);
        }

        public (string CountSQL, string QuerySQL) GetPagingSQL(string id)
        {
            if (SQLManager == null)
                throw new InvalidOperationException("Please call the 'AddSQLSeparateForDapper' method to register first.");
            return SQLManager.GetPagingSQL(id);
        }
    }
}
