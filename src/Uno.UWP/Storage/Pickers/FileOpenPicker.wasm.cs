﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Uno.Foundation;

namespace Windows.Storage.Pickers
{
	public partial class FileOpenPicker
	{
		private const string JsType = "Windows.Storage.Pickers.FileOpenPicker";
		private const string FileSeparator = "\\\\";
		private const char FileInfoSeparator = '\\';

		private async Task<StorageFile?> PickSingleFileTaskAsync(CancellationToken token)
		{
			var files = await PickFilesAsync(false, token);
			return files.FirstOrDefault();
		}

		private async Task<IReadOnlyList<StorageFile>> PickMultipleFilesTaskAsync(CancellationToken token)
		{
			return await PickFilesAsync(true, token);
		}

		private async Task<FilePickerSelectedFilesArray> PickFilesAsync(bool multiple, CancellationToken token)
		{
			var showAllEntryParameter = FileTypeFilter.Contains("*") ? "true" : "false";
			var multipleParameter = multiple ? "true" : "false";
			var fileTypeMapParameter = BuildFileTypesMap();

			var returnValue = await WebAssemblyRuntime.InvokeAsync($"{JsType}.pickFilesAsync({multipleParameter},{showAllEntryParameter},{fileTypeMapParameter})");
			var files = returnValue.Split(new string[] { FileSeparator }, StringSplitOptions.RemoveEmptyEntries);
			var results = new List<StorageFile>();
			foreach (var file in files)
			{
				var fileInfoParts = file.Split(FileInfoSeparator);
				if (!Guid.TryParse(fileInfoParts[0], out var guid))
				{
					throw new InvalidOperationException("Selected file ID is not valid.");
				}
				var name = fileInfoParts[1];
				var contentType = fileInfoParts[2];
				var storageFile = StorageFile.GetFileFromNativePath(guid, name, contentType);
				results.Add(storageFile);
			}
			return new FilePickerSelectedFilesArray(results.ToArray());
		}

		private string BuildFileTypesMap()
		{
			var mimeTypeMap = new Dictionary<string, List<string>>();
			foreach (var fileType in FileTypeFilter)
			{
				if (fileType == "*")
				{
					continue;
				}

				var mimeType = MimeTypeService.GetFromExtension(fileType);

				if (!mimeTypeMap.TryGetValue(mimeType, out var extensionList))
				{
					extensionList = new List<string>();
					mimeTypeMap[mimeType] = extensionList;
				}
				extensionList.Add(fileType);
			}

			if (mimeTypeMap.Count == 0)
			{
				return "{}";
			}

			// Build JSON object with the extensions/MIME types
			var builder = new StringBuilder();
			builder.Append("{");
			bool firstItem = true;
			foreach (var mimeType in mimeTypeMap)
			{
				if (!firstItem)
				{
					builder.Append(",");
				}
				firstItem = false;

				builder.Append("'");
				builder.Append(mimeType.Key.Replace("'", "\'"));
				builder.Append("':[");
				builder.Append(string.Join(",", mimeType.Value.Select(extension => "'" + extension.Replace("'", "\'") + "'")));
				builder.Append("]");
			}
			builder.Append("}");
			return builder.ToString();
		}
	}
}
