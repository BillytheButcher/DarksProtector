using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Confuser.Core;
using Confuser.Core.Services;
using Confuser.Renamer.Analyzers;
using dnlib.DotNet;
using Microsoft.VisualBasic;

namespace Confuser.Renamer
{
    public interface INameService
    {
        VTableStorage GetVTables();

        void Analyze(IDnlibDef def);

        bool CanRename(object obj);
        void SetCanRename(object obj, bool val);

        void SetParam(IDnlibDef def, string name, string value);
        string GetParam(IDnlibDef def, string name);

        RenameMode GetRenameMode(object obj);
        void SetRenameMode(object obj, RenameMode val);
        void ReduceRenameMode(object obj, RenameMode val);

        string RandomName();

        void RegisterRenamer(IRenamer renamer);
        T FindRenamer<T>();
        void AddReference<T>(T obj, INameReference<T> reference);

        void SetOriginalName(object obj, string name);
        void SetOriginalNamespace(object obj, string ns);

        void MarkHelper(IDnlibDef def, IMarkerService marker, ConfuserComponent parentComp);
    }

    public class NameService : INameService
    {
        static readonly object CanRenameKey = new object();
        static readonly object RenameModeKey = new object();
        static readonly object ReferencesKey = new object();
        static readonly object OriginalNameKey = new object();
        static readonly object OriginalNamespaceKey = new object();

        readonly ConfuserContext context;
        readonly byte[] nameSeed;
        readonly RandomGenerator random;
        readonly VTableStorage storage;
        AnalyzePhase analyze;

        readonly HashSet<string> identifiers = new HashSet<string>();
        readonly byte[] nameId = new byte[8];
        readonly Dictionary<string, string> nameMap1 = new Dictionary<string, string>();
        readonly Dictionary<string, string> nameMap2 = new Dictionary<string, string>();
        internal ReversibleRenamer reversibleRenamer;

        public NameService(ConfuserContext context)
        {
            this.context = context;
            storage = new VTableStorage(context.Logger);
            random = context.Registry.GetService<IRandomService>().GetRandomGenerator(NameProtection._FullId);
            nameSeed = random.NextBytes(20);

            Renamers = new List<IRenamer> {
                new InterReferenceAnalyzer(),
                new VTableAnalyzer(),
                new TypeBlobAnalyzer(),
                new ResourceAnalyzer(),
                new LdtokenEnumAnalyzer()
            };
        }

        public IList<IRenamer> Renamers { get; private set; }
        public List<string> Names = new List<string>();

        static Random _rnd = new Random();

        private static Random kys = new Random();

        static string charset;

        static string rename;

        private static Random _rand = new Random();

        public static string GetRandomLine(string filename)
        {
            var lines = File.ReadAllLines(filename);

            var lineNumber = _rand.Next(0, lines.Length);

            return lines[lineNumber];
        }

        public static string RandomNameText()
        {
            charset = "他是说汉语的ｱ尺乇你他是说汉语的ｱ尺乇你他是说汉语的ｱ尺乇你abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789123456789123456789123456789123456789123456789ᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠαβγδεζηθικλμνξοπρστυφχψωαβγδεζηθικλμνξοπρστυφχψωαβγδε";
            rename = "dark#5000-protector|";

            char[] chArray = new char[(20 - 1) + 1];
            int num = 20 - 1;
            for (int k = 0; k <= num; k++)
            {
                chArray[k] = charset[(int)Math.Round((double)Conversion.Int((float)(VBMath.Rnd() * charset.Length)))];
            }
            rename += new string(chArray);

            return rename;
        }

        public static string RandomNameCustom()
        {

            return GetRandomLine(Path.Combine(Environment.CurrentDirectory, "Config", "Renamer.txt"));
        }

        string namesdqsdq;

        public string RandomName()
        {

            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Config", "Renamer.txt")))
            {
                namesdqsdq =  RandomNameCustom();
            }
            else
            {
                namesdqsdq = RandomNameText();
            }

            return namesdqsdq;
        }

        static string namesdqsdq1;

        public static string RandomNameStatic()
        {

            if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Config", "Renamer.txt")))
            {
                namesdqsdq1 = RandomNameCustom();
            }
            else
            {
                namesdqsdq1 = RandomNameText();
            }

            return namesdqsdq1;
        }

        public VTableStorage GetVTables()
        {
            return storage;
        }

        public bool CanRename(object obj)
        {
            if (obj is IDnlibDef)
            {
                if (analyze == null)
                    analyze = context.Pipeline.FindPhase<AnalyzePhase>();

                var prot = (NameProtection)analyze.Parent;
                ProtectionSettings parameters = ProtectionParameters.GetParameters(context, (IDnlibDef)obj);
                if (parameters == null || !parameters.ContainsKey(prot))
                    return false;
                return context.Annotations.Get(obj, CanRenameKey, true);
            }
            return false;
        }

        public void SetCanRename(object obj, bool val)
        {
            context.Annotations.Set(obj, CanRenameKey, val);
        }

        public void SetParam(IDnlibDef def, string name, string value)
        {
            var param = ProtectionParameters.GetParameters(context, def);
            if (param == null)
                ProtectionParameters.SetParameters(context, def, param = new ProtectionSettings());
            Dictionary<string, string> nameParam;
            if (!param.TryGetValue(analyze.Parent, out nameParam))
                param[analyze.Parent] = nameParam = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            nameParam[name] = value;
        }

        public string GetParam(IDnlibDef def, string name)
        {
            var param = ProtectionParameters.GetParameters(context, def);
            if (param == null)
                return null;
            Dictionary<string, string> nameParam;
            if (!param.TryGetValue(analyze.Parent, out nameParam))
                return null;
            return nameParam.GetValueOrDefault(name);
        }

        public RenameMode GetRenameMode(object obj)
        {
            return context.Annotations.Get(obj, RenameModeKey, RenameMode.Unicode);
        }

        public void SetRenameMode(object obj, RenameMode val)
        {
            context.Annotations.Set(obj, RenameModeKey, val);
        }

        public void ReduceRenameMode(object obj, RenameMode val)
        {
            RenameMode original = GetRenameMode(obj);
            if (original < val)
                context.Annotations.Set(obj, RenameModeKey, val);
        }

        public void AddReference<T>(T obj, INameReference<T> reference)
        {
            context.Annotations.GetOrCreate(obj, ReferencesKey, key => new List<INameReference>()).Add(reference);
        }

        public void Analyze(IDnlibDef def)
        {
            if (analyze == null)
                analyze = context.Pipeline.FindPhase<AnalyzePhase>();

            SetOriginalName(def, def.Name);
            if (def is TypeDef)
            {
                GetVTables().GetVTable((TypeDef)def);
                SetOriginalNamespace(def, ((TypeDef)def).Namespace);
            }
            analyze.Analyze(this, context, ProtectionParameters.Empty, def, true);
        }

        public void SetNameId(uint id)
        {
            for (int i = nameId.Length - 1; i >= 0; i--)
            {
                nameId[i] = (byte)(id & 0xff);
                id >>= 8;
            }
        }

        void IncrementNameId()
        {
            for (int i = nameId.Length - 1; i >= 0; i--)
            {
                nameId[i]++;
                if (nameId[i] != 0)
                    break;
            }
        }
 
        string ObfuscateNameInternal(byte[] hash, RenameMode mode)
        {
            switch (mode)
            {
                case RenameMode.Empty:
                    return "";
                case RenameMode.Unicode:
                    return RandomName();
                case RenameMode.Letters:
                    return RandomName();
                case RenameMode.ASCII:
                    return RandomName();
                case RenameMode.Decodable:
                    IncrementNameId();
                    return RandomName();
                case RenameMode.Sequential:
                    IncrementNameId();
                    return RandomName();
                default:

                    throw new NotSupportedException("Rename mode '" + mode + "' is not supported.");
            }
        }

        string ParseGenericName(string name, out int? count)
        {
            if (name.LastIndexOf('`') != -1)
            {
                int index = name.LastIndexOf('`');
                int c;
                if (int.TryParse(name.Substring(index + 1), out c))
                {
                    count = c;
                    return name.Substring(0, index);
                }
            }
            count = null;
            return name;
        }

        string MakeGenericName(string name, int? count)
        {
            if (count == null)
                return name;
            else
                return string.Format("{0}`{1}", name, count.Value);
        }

        public void SetOriginalName(object obj, string name)
        {
            identifiers.Add(name);
            context.Annotations.Set(obj, OriginalNameKey, name);
        }

        public void SetOriginalNamespace(object obj, string ns)
        {
            identifiers.Add(ns);
            context.Annotations.Set(obj, OriginalNamespaceKey, ns);
        }

        public void RegisterRenamer(IRenamer renamer)
        {
            Renamers.Add(renamer);
        }

        public T FindRenamer<T>()
        {
            return Renamers.OfType<T>().Single();
        }

        public static void MarkHelperStatic(IDnlibDef def, IMarkerService marker, ConfuserComponent parentComp)
        {
            if (marker.IsMarked(def))
                return;
            if (def is MethodDef)
            {
                var method = (MethodDef)def;
                method.Access = MethodAttributes.Assembly;
                if (!method.IsSpecialName && !method.IsRuntimeSpecialName && !method.DeclaringType.IsDelegate())
                    method.Name = RandomNameStatic();
            }
            else if (def is FieldDef)
            {
                var field = (FieldDef)def;
                field.Access = FieldAttributes.Assembly;
                if (!field.IsSpecialName && !field.IsRuntimeSpecialName)
                    field.Name = RandomNameStatic();
            }
            else if (def is TypeDef)
            {
                var type = (TypeDef)def;
                type.Visibility = type.DeclaringType == null ? TypeAttributes.NotPublic : TypeAttributes.NestedAssembly;
                type.Namespace = RandomNameStatic();
                if (!type.IsSpecialName && !type.IsRuntimeSpecialName)
                    type.Name = RandomNameStatic();
            }
            
            marker.Mark(def, parentComp);
        }

        public void MarkHelper(IDnlibDef def, IMarkerService marker, ConfuserComponent parentComp)
        {
            if (marker.IsMarked(def))
                return;
            if (def is MethodDef)
            {
                var method = (MethodDef)def;
                method.Access = MethodAttributes.Assembly;
                if (!method.IsSpecialName && !method.IsRuntimeSpecialName && !method.DeclaringType.IsDelegate())
                    method.Name = RandomName();
            }
            else if (def is FieldDef)
            {
                var field = (FieldDef)def;
                field.Access = FieldAttributes.Assembly;
                if (!field.IsSpecialName && !field.IsRuntimeSpecialName)
                    field.Name = RandomName();
            }
            else if (def is TypeDef)
            {
                var type = (TypeDef)def;
                type.Visibility = type.DeclaringType == null ? TypeAttributes.NotPublic : TypeAttributes.NestedAssembly;
                type.Namespace = RandomName();
                if (!type.IsSpecialName && !type.IsRuntimeSpecialName)
                    type.Name = RandomName();
            }
            SetCanRename(def, false);
            Analyze(def);
            marker.Mark(def, parentComp);
        }

        #region Charsets

        static readonly char[] asciiCharset = Enumerable.Range(32, 95)
                                                        .Select(ord => (char)ord)
                                                        .Except(new[] { '.' })
                                                        .ToArray();

        static readonly char[] letterCharset = Enumerable.Range(0, 26)
                                                         .SelectMany(ord => new[] { (char)('a' + ord), (char)('A' + ord) })
                                                         .ToArray();

        static readonly char[] alphaNumCharset = Enumerable.Range(0, 26)
                                                           .SelectMany(ord => new[] { (char)('a' + ord), (char)('A' + ord) })
                                                           .Concat(Enumerable.Range(0, 10).Select(ord => (char)('0' + ord)))
                                                           .ToArray();

        // Especially chosen, just to mess with people.
        // Inspired by: http://xkcd.com/1137/ :D
        static readonly char[] unicodeCharset = new char[] { }
            .Concat(Enumerable.Range(0x200b, 5).Select(ord => (char)ord))
            .Concat(Enumerable.Range(0x2029, 6).Select(ord => (char)ord))
            .Concat(Enumerable.Range(0x206a, 6).Select(ord => (char)ord))
            .Except(new[] { '\u2029' })
            .ToArray();

        #endregion

        public RandomGenerator GetRandom()
        {
            return random;
        }

        public IList<INameReference> GetReferences(object obj)
        {
            return context.Annotations.GetLazy(obj, ReferencesKey, key => new List<INameReference>());
        }

        public string GetOriginalName(object obj)
        {
            return context.Annotations.Get(obj, OriginalNameKey, "");
        }

        public string GetOriginalNamespace(object obj)
        {
            return context.Annotations.Get(obj, OriginalNamespaceKey, "");
        }

        public ICollection<KeyValuePair<string, string>> GetNameMap()
        {
            return nameMap2;
        }
    }
}