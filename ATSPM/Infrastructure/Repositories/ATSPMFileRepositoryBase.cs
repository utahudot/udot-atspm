using ATSPM.Application.Configuration;
using ATSPM.Application.Models;
using ATSPM.Domain.Common;
using ATSPM.Domain.Extensions;
using ATSPM.Domain.Services;
using ATSPM.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    public abstract class ATSPMFileRepositoryBase<T> : IAsyncRepository<T> where T : ATSPMModelBase
    {
        protected readonly IFileTranscoder _fileTranscoder;
        protected readonly IOptions<FileRepositoryConfiguration> _options;
        protected readonly ILogger _log;

        #region ClassSpecific

        protected virtual string GenerateFileName(T item)
        {
            return $"{item.GetType().Name}_{item}{_fileTranscoder.FileExtension}";
        }

        protected virtual string GenerateFilePath(T item = null)
        {
            var folder = new DirectoryInfo(Path.Combine(_options.Value.RootPath, typeof(T).Name));

            if (!folder.Exists)
                folder.Create();

            return Path.Combine(folder.FullName);
        }

        #endregion

        public ATSPMFileRepositoryBase(IFileTranscoder fileTranscoder, IOptions<FileRepositoryConfiguration> options, ILogger<ATSPMFileRepositoryBase<T>> log) => 
            (_fileTranscoder, _options, _log) = (fileTranscoder, options, log);

        public void Add(T item)
        {
            File.WriteAllBytes(Path.Combine(GenerateFilePath(item), GenerateFileName(item)), _fileTranscoder.EncodeItem(item));
        }

        public async Task AddAsync(T item)
        {
            await File.WriteAllBytesAsync(Path.Combine(GenerateFilePath(item), GenerateFileName(item)), _fileTranscoder.EncodeItem(item));
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
            var dir = new DirectoryInfo(GenerateFilePath());

            return dir.GetFiles("*.*", SearchOption.AllDirectories).Select(s => _fileTranscoder.DecodeItem<T>(File.ReadAllBytes(s.FullName))).AsQueryable().Where(criteria).ToList();
        }

        public IReadOnlyList<T> GetList(ISpecification<T> criteria)
        {
            var dir = new DirectoryInfo(GenerateFilePath());

            return dir.GetFiles("*.*", SearchOption.AllDirectories).Select(s => _fileTranscoder.DecodeItem<T>(File.ReadAllBytes(s.FullName))).AsQueryable().Where(criteria.Criteria).ToList();
        }

        public async Task<IReadOnlyList<T>> GetListAsync(Expression<Func<T, bool>> criteria)
        {
            var dir = new DirectoryInfo(GenerateFilePath());

            return (await Task.WhenAll(dir.GetFiles("*.*", SearchOption.AllDirectories).Select(async s => _fileTranscoder.DecodeItem<T>(await File.ReadAllBytesAsync(s.FullName))))).AsQueryable().Where(criteria).ToList();
        }

        public async Task<IReadOnlyList<T>> GetListAsync(ISpecification<T> criteria)
        {
            var dir = new DirectoryInfo(GenerateFilePath());

            return (await Task.WhenAll(dir.GetFiles("*.*", SearchOption.AllDirectories).Select(async s => _fileTranscoder.DecodeItem<T>(await File.ReadAllBytesAsync(s.FullName))))).AsQueryable().Where(criteria.Criteria).ToList();
        }

        public T Lookup(T item)
        {
            var file = new FileInfo(Path.Combine(GenerateFilePath(item), GenerateFileName(item)));

            if (file.Exists)
            {
                return _fileTranscoder.DecodeItem<T>(File.ReadAllBytes(file.FullName));
            }

            return null;
        }

        public async Task<T> LookupAsync(T item)
        {
            var file = new FileInfo(Path.Combine(GenerateFilePath(item), GenerateFileName(item)));

            if (file.Exists)
            {
                return _fileTranscoder.DecodeItem<T>(await File.ReadAllBytesAsync(file.FullName));
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
