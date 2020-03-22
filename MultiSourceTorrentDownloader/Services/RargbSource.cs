﻿using MultiSourceTorrentDownloader.Data;
using MultiSourceTorrentDownloader.Enums;
using MultiSourceTorrentDownloader.Interfaces;
using MultiSourceTorrentDownloader.Mapping;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MultiSourceTorrentDownloader.Services
{
    public class RargbSource : SourceBase, IRargbSource
    {
        private readonly ILogService _logger;
        private readonly IRargbParser _parser;
        public RargbSource(ILogService logger, IRargbParser parser)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));

            _baseUrl = ConfigurationManager.AppSettings["RargbUrl"];
            _searchEndpoint = Path.Combine(_baseUrl, ConfigurationManager.AppSettings["RargbSearchEndpoint"]);
        }

        public string FullTorrentUrl(string uri) => TorrentUrl(uri);

        public async Task<string> GetTorrentDescriptionAsync(string detailsUri)
        {
            return await BaseGetTorrentDescriptionAsync(Path.Combine(_baseUrl, detailsUri), _parser);
        }

        public async Task<string> GetTorrentMagnetAsync(string detailsUri)
        {
            return await BaseGetTorrentMagnetAsync(detailsUri, _parser);
        }

        public async Task<TorrentQueryResult> GetTorrentsAsync(string searchFor, int page, Sorting sorting)
        {
            var mappedSortOption = SortingMapper.SortingToRargbSorting(sorting);
            var fullUrl = Path.Combine(_searchEndpoint, page.ToString(), $"?search={searchFor}&order={mappedSortOption.Order}&by={mappedSortOption.By}");

            var contents = await UrlGetResponseString(fullUrl);
            return await _parser.ParsePageForTorrentEntriesAsync(contents);
        }

        public async Task<TorrentQueryResult> GetTorrentsByCategoryAsync(string searchFor, int page, Sorting sorting, TorrentCategory category)
        {
            var mappedSortOption = SortingMapper.SortingToRargbSorting(sorting);
            var mappedCategory = TorrentCategoryMapper.ToRargbCategory(category);

            var fullUrl = Path.Combine(_searchEndpoint, page.ToString(), $"?search={searchFor}&category[]={mappedCategory}&order={mappedSortOption.Order}&by={mappedSortOption.By}");

            var contents = await UrlGetResponseString(fullUrl);
            return await _parser.ParsePageForTorrentEntriesAsync(contents);
        }
    }
}
