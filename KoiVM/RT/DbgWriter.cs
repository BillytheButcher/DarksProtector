﻿#region

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using KoiVM.AST.IL;

#endregion

namespace KoiVM.RT
{
    [Obfuscation(Exclude = false, Feature = "+koi;-ref proxy")]
    internal class DbgWriter
    {
        private byte[] dbgInfo;
        private readonly HashSet<string> documents = new HashSet<string>();

        private readonly Dictionary<ILBlock, List<DbgEntry>> entries = new Dictionary<ILBlock, List<DbgEntry>>();

        public void AddSequencePoint(ILBlock block, uint offset, uint len, string document, uint lineNum)
        {
            List<DbgEntry> entryList;
            if(!entries.TryGetValue(block, out entryList))
                entryList = entries[block] = new List<DbgEntry>();

            entryList.Add(new DbgEntry
            {
                offset = offset,
                len = len,
                document = document,
                lineNum = lineNum
            });
            documents.Add(document);
        }

        public DbgSerializer GetSerializer()
        {
            return new DbgSerializer(this);
        }

        public byte[] GetDbgInfo()
        {
            return dbgInfo;
        }

        private struct DbgEntry
        {
            public uint offset;
            public uint len;

            public string document;
            public uint lineNum;
        }

        internal class DbgSerializer : IDisposable
        {
            private readonly DbgWriter dbg;
            private Dictionary<string, uint> docMap;
            private readonly MemoryStream stream;
            private readonly BinaryWriter writer;

            internal DbgSerializer(DbgWriter dbg)
            {
                this.dbg = dbg;
                stream = new MemoryStream();
                var aes = new AesManaged();
                aes.IV = aes.Key = Convert.FromBase64String("UkVwAyrARLAy4GmQLL860w==");
                writer = new BinaryWriter(
                    new DeflateStream(
                        new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write),
                        CompressionMode.Compress
                    )
                );

                InitStream();
            }

            public void Dispose()
            {
                writer.Dispose();
                dbg.dbgInfo = stream.ToArray();
            }

            private void InitStream()
            {
                docMap = new Dictionary<string, uint>();
                writer.Write(dbg.documents.Count);
                uint docId = 0;
                foreach(var doc in dbg.documents)
                {
                    writer.Write(doc);
                    docMap[doc] = docId++;
                }
            }

            public void WriteBlock(BasicBlockChunk chunk)
            {
                List<DbgEntry> entryList;
                if(chunk == null || !dbg.entries.TryGetValue(chunk.Block, out entryList) ||
                   chunk.Block.Content.Count == 0)
                    return;

                var offset = chunk.Block.Content[0].Offset;
                foreach(var entry in entryList)
                {
                    writer.Write(entry.offset + chunk.Block.Content[0].Offset);
                    writer.Write(entry.len);
                    writer.Write(docMap[entry.document]);
                    writer.Write(entry.lineNum);
                }
            }
        }
    }
}