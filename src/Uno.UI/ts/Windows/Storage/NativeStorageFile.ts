﻿
namespace Windows.Storage {

	export class NativeStorageFile {
		private static _fileMap: Map<string, FileSystemFileHandle> = new Map<string, FileSystemFileHandle>();

		public static AddHandle(guid: string, handle: FileSystemFileHandle) {
			NativeStorageFile._fileMap.set(guid, handle);
		}

		public static RemoveHandle(guid: string) {
			NativeStorageFile._fileMap.delete(guid);
		}

		public static GetHandle(guid: string): FileSystemFileHandle {
			return NativeStorageFile._fileMap.get(guid);
		}
	}
}
