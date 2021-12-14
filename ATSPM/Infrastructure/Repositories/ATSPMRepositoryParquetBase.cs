using ATSPM.Application.Models;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Services;
using ATSPM.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ATSPM.Infrasturcture.Repositories
{
    public static class EntityFrameworkCoreExtensions
    {
        public static string CreateKeyValueName(this DbContext db, ATSPMModelBase item)
        {
            return item.GetType().Name + "_" + string.Join("_", db.Model.FindEntityType(item.GetType()).FindPrimaryKey().Properties.Select(p => String.Format(p.FindAnnotation("KeyNameFormat") != null ? "{0:" + p.FindAnnotation("KeyNameFormat")?.Value.ToString() + "}" : "{0}", p.PropertyInfo.GetValue(item, null))));
        }
    }

    public abstract class ATSPMRepositoryParquetBase<T> : IAsyncRepository<T> where T : ATSPMModelBase
    {
        protected readonly ILogger _log;
        protected readonly DbContext _db;
        protected readonly DbSet<T> table;

        #region ClassSpecific

        protected virtual string GenerateFileName(T item)
        {
            return Path.Combine("C:", "ControlLogs", $"{_db.CreateKeyValueName(item)}.txt");
        }

        protected virtual byte[] SerializeFile(T item)
        {
            //return JsonSerializer.Serialize(item).GZipCompressToByte();
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(item));
        }

        protected virtual T DeserializeFile(byte[] data)
        {
            //return JsonSerializer.Deserialize<T>(stream.GZipDecompressToString(), null);
            return data.ToJson<T>();
        }

        #endregion

        public ATSPMRepositoryParquetBase(DbContext db, ILogger<ATSPMRepositoryParquetBase<T>> log)
        {
            _log = log;
            _db = db;
            table = _db.Set<T>();
        }

        public void Add(T item)
        {
            File.WriteAllBytes(GenerateFileName(item), SerializeFile(item));
        }

        public async Task AddAsync(T item)
        {
            await File.WriteAllBytesAsync(GenerateFileName(item), SerializeFile(item));
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        public async Task AddRangeAsync(IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                await AddAsync(item);
            }
        }

        public IReadOnlyList<T> GetList(Expression<Func<T, bool>> criteria)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.Combine("C:", "ControlLogs"));

            IEnumerable<FileInfo> fileList = dir.GetFiles("*.*", SearchOption.AllDirectories);

            //return table.Where(criteria).ToList();

            throw new NotImplementedException();
        }

        public IReadOnlyList<T> GetList(ISpecification<T> criteria)
        {
            //return table.Where(criteria.Criteria).ToList();

            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> criteria)
        {
            //return await table.Where(criteria).ToListAsync();

            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<T>> GetListAsync(ISpecification<T> criteria)
        {
            //return await table.Where(criteria.Criteria.Compile()).AsQueryable().ToListAsync().ConfigureAwait(false);

            throw new NotImplementedException();
        }

        public T Lookup(T item)
        {
            //return table.Find(_db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.PropertyInfo.GetValue(item, null)).ToArray());

            throw new NotImplementedException();
        }

        public async Task<T> LookupAsync(T item)
        {
            //return await table.FindAsync(_db.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties.Select(p => p.PropertyInfo.GetValue(item, null)).ToArray()).ConfigureAwait(false);

            throw new NotImplementedException();
        }

        public void Remove(T item)
        {
            //table.Remove(item);
            //_db.SaveChanges();

            throw new NotImplementedException();
        }

        public async Task RemoveAsync(T item)
        {
            //table.Remove(item);
            //await _db.SaveChangesAsync().ConfigureAwait(false);

            throw new NotImplementedException();
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            //table.RemoveRange(items);
            //_db.SaveChanges();

            throw new NotImplementedException();
        }

        public async Task RemoveRangeAsync(IEnumerable<T> items)
        {
            //table.RemoveRange(items);
            //await _db.SaveChangesAsync().ConfigureAwait(false);

            throw new NotImplementedException();
        }
    }
}
