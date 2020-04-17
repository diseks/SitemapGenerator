using Sitemap.Core.Config;
using Sitemap.Core.Helpers;
using Sitemap.Core.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Sitemap.Core.Processing
{
    public interface IBackgroundJob
    {
        Task<IEnumerable<string>> Process();
    }

    public class BackgroundJob : IBackgroundJob
    {
        private readonly ILogger _logger;
        private readonly IStorage _storage;
        private readonly ISerializer _serializer;
        private readonly IFileHandlerHelper _fileHandlerHelper;
        private readonly SiteMapFilterSettings _siteMapFilterSettings;

        private ActionBlock<int> _actionFetchBlock;
        private ConcurrentBag<string> _generatedFiles;

        public BackgroundJob (IStorage storage, SiteMapFilterSettings siteMapFilterSettings,
            ISerializer serializer, IFileHandlerHelper fileHandlerHelper,
            ILogger logger)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _siteMapFilterSettings = siteMapFilterSettings ?? throw new ArgumentNullException(nameof(siteMapFilterSettings));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _fileHandlerHelper = fileHandlerHelper ?? throw new ArgumentNullException(nameof(fileHandlerHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _generatedFiles = new ConcurrentBag<string>();
            _actionFetchBlock = new ActionBlock<int>(ProcessBlock, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = -1, BoundedCapacity = 5 });
        }

        public async Task<IEnumerable<string>> Process()
        {
            await _actionFetchBlock.SendAsync(0);

            await _actionFetchBlock.Completion;

            return _generatedFiles.ToList();
        }

        private string GenerateFile(int page, string content)
        {
            var pageNumber = page == 0 ? "" : string.Concat(".part", page);

            var fileName = $"sitemap_{_siteMapFilterSettings.FileName.Replace(" ", "").ToLower().Trim()}{pageNumber}.xml";

            _logger.LogInformation($"Generate file {fileName}");

            _fileHandlerHelper.Save(fileName, _serializer.PrepareUrlSetXml(content));

            return fileName;
        }

        private async Task ProcessBlock(int page)
        {
            try
            {
                var skip = page * _siteMapFilterSettings.FetchCount;

                var entries = _storage.GetEntries(_siteMapFilterSettings.Table, _siteMapFilterSettings.FetchCount, skip);
                var entriesCount = entries.Count();

                if (entriesCount == 0)
                {
                    _logger.LogInformation($"Empty page {page + 1}. Start action blocks completing.");

                    if (_actionFetchBlock.InputCount == 0) { _actionFetchBlock.Complete(); }
                    return;
                }

                _logger.LogInformation($"Request data from table {_siteMapFilterSettings.Table}." +
                  $"Take {(skip == 0 ? "all data" : string.Format("take {0} rows", entriesCount))}.");

                var content = string.Join("", entries.Select(x => _serializer.SerializeUrlSet(x, _siteMapFilterSettings.Route)).ToArray());

                var filename = GenerateFile(page, content);

                _generatedFiles.Add(filename);

                if (_siteMapFilterSettings.FetchCount > 0)
                {
                    _logger.LogInformation($"Fetch next page {page + 1}");

                    page++;
                    await _actionFetchBlock.SendAsync(page);
                }
                else
                {
                    if (_actionFetchBlock.InputCount == 0) { _actionFetchBlock.Complete(); }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, nameof(ProcessBlock));
            }
        }
    }
}
