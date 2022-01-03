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

        protected virtual string FileExtension { get; set; } = ".json";

        protected abstract string GenerateFileName(T item);

        protected abstract string GenerateFilePath(T item);

        protected virtual byte[] SerializeFile(T item)
        {
            //return JsonSerializer.Serialize(item).GZipCompressToByte();
            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(item));
        }

        protected virtual T DeserializeFile(byte[] data)
        {
            //return JsonSerializer.Deserialize<T>(stream.GZipDecompressToString(), null);
            return data.FromEncodedJson<T>();
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
            File.WriteAllBytes(Path.Combine(GenerateFilePath(item), GenerateFileName(item)), SerializeFile(item));
        }

        public async Task AddAsync(T item)
        {
            await File.WriteAllBytesAsync(Path.Combine(GenerateFilePath(item), GenerateFileName(item)), SerializeFile(item));
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
            var dir = new DirectoryInfo(Path.Combine("C:", "ControlLogs", typeof(T).Name));

            return dir.GetFiles("*.*", SearchOption.AllDirectories).Select(s => DeserializeFile(File.ReadAllBytes(s.FullName))).AsQueryable().Where(criteria).ToList();
        }

        public IReadOnlyList<T> GetList(ISpecification<T> criteria)
        {
            var dir = new DirectoryInfo(Path.Combine("C:", "ControlLogs", typeof(T).Name));

            return dir.GetFiles("*.*", SearchOption.AllDirectories).Select(s => DeserializeFile(File.ReadAllBytes(s.FullName))).AsQueryable().Where(criteria.Criteria).ToList();
        }

        public async Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> criteria)
        {
            var dir = new DirectoryInfo(Path.Combine("C:", "ControlLogs", typeof(T).Name));

            return (await Task.WhenAll(dir.GetFiles("*.*", SearchOption.AllDirectories).Select(async s => DeserializeFile(await File.ReadAllBytesAsync(s.FullName))))).AsQueryable().Where(criteria).ToList();
        }

        public async Task<IReadOnlyList<T>> GetListAsync(ISpecification<T> criteria)
        {
            var dir = new DirectoryInfo(Path.Combine("C:", "ControlLogs", typeof(T).Name));

            return (await Task.WhenAll(dir.GetFiles("*.*", SearchOption.AllDirectories).Select(async s => DeserializeFile(await File.ReadAllBytesAsync(s.FullName))))).AsQueryable().Where(criteria.Criteria).ToList();
        }

        public T Lookup(T item)
        {
            var file = new FileInfo(Path.Combine(GenerateFilePath(item), GenerateFileName(item)));

            if (file.Exists)
            {
                return DeserializeFile(File.ReadAllBytes(file.FullName));
            }

            return null;
        }

        public async Task<T> LookupAsync(T item)
        {
            var file = new FileInfo(Path.Combine(GenerateFilePath(item), GenerateFileName(item)));

            if (file.Exists)
            {
                return DeserializeFile(await File.ReadAllBytesAsync(file.FullName));
            }

            return null;
        }

        public void Remove(T item)
        {
            var file = new FileInfo(Path.Combine(GenerateFilePath(item), GenerateFileName(item)));

            if (file.Exists)
            {
                file.Delete();
            }
        }

        public async Task RemoveAsync(T item)
        {
            await Task.Run(() => Remove(item));
        }

        public void RemoveRange(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                Remove(item);
            }
        }

        public async Task RemoveRangeAsync(IEnumerable<T> items)
        {
            await Task.Run(() => RemoveRange(items));
        }
    }
}
