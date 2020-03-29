﻿#region

using What_a_great_VM;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

#endregion

namespace KoiVM.Runtime.Data
{
    internal unsafe class DarksVMData
    {
        private static readonly Dictionary<Module, DarksVMData> moduleVMData = new Dictionary<Module, DarksVMData>();
        private readonly Dictionary<uint, DarksVMExportInfo> exports;

        private readonly Dictionary<uint, RefInfo> references;
        private readonly Dictionary<uint, string> strings;

        public DarksVMData(Module module, void* data)
        {
            var header = (VMDAT_HEADER*) data;
            if(header->MAGIC != 0x68736966)
                throw new InvalidProgramException();

            references = new Dictionary<uint, RefInfo>();
            strings = new Dictionary<uint, string>();
            exports = new Dictionary<uint, DarksVMExportInfo>();

            var ptr = (byte*) (header + 1);
            for(var i = 0; i < header->MD_COUNT; i++)
            {
                var id = Utils.ReadCompressedUInt(ref ptr);
                var token = (int) Utils.FromCodedToken(Utils.ReadCompressedUInt(ref ptr));
                references[id] = new RefInfo
                {
                    module = module,
                    token = token
                };
            }
            for(var i = 0; i < header->STR_COUNT; i++)
            {
                var id = Utils.ReadCompressedUInt(ref ptr);
                var len = Utils.ReadCompressedUInt(ref ptr);
                strings[id] = new string((char*) ptr, 0, (int) len);
                ptr += len << 1;
            }
            for(var i = 0; i < header->EXP_COUNT; i++) exports[Utils.ReadCompressedUInt(ref ptr)] = new DarksVMExportInfo(ref ptr, module);

            KoiSection = (byte*) data;

            Module = module;
            moduleVMData[module] = this;
        }

        public Module Module
        {
            get;
        }

        public byte* KoiSection
        {
            get;
            set;
        }

        public static DarksVMData Instance(Module module)
        {
            DarksVMData data;
            lock(moduleVMData)
            {
                if(!moduleVMData.TryGetValue(module, out data))
                    data = moduleVMData[module] = DarksVMDataInitializer.GetData(module);
            }
            return data;
        }

        public MemberInfo LookupReference(uint id)
        {
            return references[id].Member;
        }

        public string LookupString(uint id)
        {
            if(id == 0)
                return null;
            return strings[id];
        }

        public DarksVMExportInfo LookupExport(uint id)
        {
            return exports[id];
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct VMDAT_HEADER
        {
            public readonly uint MAGIC;
            public readonly uint MD_COUNT;
            public readonly uint STR_COUNT;
            public readonly uint EXP_COUNT;
        }

        private class RefInfo
        {
            public Module module;
            public MemberInfo resolved;
            public int token;

            public MemberInfo Member => resolved ?? (resolved = module.ResolveMember(token));
        }
    }
}