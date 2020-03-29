using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Confuser.Core
{
    /// <summary>
    ///     Discovers available protection plugins.
    /// </summary>
    // Token: 0x0200004E RID: 78
    public class PluginDiscovery
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Confuser.Core.PluginDiscovery" /> class.
        /// </summary>
        // Token: 0x060001D0 RID: 464 RVA: 0x0000F537 File Offset: 0x0000D737
        protected PluginDiscovery()
        {
        }

        /// <summary>
        ///     Retrieves the available protection plugins.
        /// </summary>
        /// <param name="context">The working context.</param>
        /// <param name="protections">A list of resolved protections.</param>
        /// <param name="packers">A list of resolved packers.</param>
        /// <param name="components">A list of resolved components.</param>
        // Token: 0x060001D1 RID: 465 RVA: 0x0000F53F File Offset: 0x0000D73F
        public void GetPlugins(ConfuserContext context, out IList<Protection> protections, out IList<Packer> packers, out IList<ConfuserComponent> components)
        {
            protections = new List<Protection>();
            packers = new List<Packer>();
            components = new List<ConfuserComponent>();
            this.GetPluginsInternal(context, protections, packers, components);
        }

        /// <summary>
        ///     Determines whether the specified type has an accessible default constructor.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the specified type has an accessible default constructor; otherwise, <c>false</c>.</returns>
        // Token: 0x060001D2 RID: 466 RVA: 0x0000F568 File Offset: 0x0000D768
        public static bool HasAccessibleDefConstructor(Type type)
        {
            ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
            return !(ctor == null) && ctor.IsPublic;
        }

        /// <summary>
        ///     Adds plugins in the assembly to the protection list.
        /// </summary>
        /// <param name="context">The working context.</param>
        /// <param name="protections">The working list of protections.</param>
        /// <param name="packers">The working list of packers.</param>
        /// <param name="components">The working list of components.</param>
        /// <param name="asm">The assembly.</param>
        // Token: 0x060001D3 RID: 467 RVA: 0x0000F594 File Offset: 0x0000D794
        protected static void AddPlugins(ConfuserContext context, IList<Protection> protections, IList<Packer> packers, IList<ConfuserComponent> components, Assembly asm)
        {
            Module[] loadedModules = asm.GetLoadedModules();
            for (int j = 0; j < loadedModules.Length; j++)
            {
                Module module = loadedModules[j];
                Type[] types = module.GetTypes();
                for (int k = 0; k < types.Length; k++)
                {
                    Type i = types[k];
                    if (!i.IsAbstract && PluginDiscovery.HasAccessibleDefConstructor(i))
                    {
                        if (typeof(Protection).IsAssignableFrom(i))
                        {
                            try
                            {
                                protections.Add((Protection)Activator.CreateInstance(i));
                                goto IL_126;
                            }
                            catch (Exception ex)
                            {
                                context.Logger.ErrorException("Failed to instantiate protection '" + i.Name + "'.", ex);
                                goto IL_126;
                            }
                        }
                        if (typeof(Packer).IsAssignableFrom(i))
                        {
                            try
                            {
                                packers.Add((Packer)Activator.CreateInstance(i));
                                goto IL_126;
                            }
                            catch (Exception ex2)
                            {
                                context.Logger.ErrorException("Failed to instantiate packer '" + i.Name + "'.", ex2);
                                goto IL_126;
                            }
                        }
                        if (typeof(ConfuserComponent).IsAssignableFrom(i))
                        {
                            try
                            {
                                components.Add((ConfuserComponent)Activator.CreateInstance(i));
                            }
                            catch (Exception ex3)
                            {
                                context.Logger.ErrorException("Failed to instantiate component '" + i.Name + "'.", ex3);
                            }
                        }
                    }
                IL_126:;
                }
            }
            context.CheckCancellation();
        }

        /// <summary>
        ///     Retrieves the available protection plugins.
        /// </summary>
        /// <param name="context">The working context.</param>
        /// <param name="protections">The working list of protections.</param>
        /// <param name="packers">The working list of packers.</param>
        /// <param name="components">The working list of components.</param>
        // Token: 0x060001D4 RID: 468 RVA: 0x0000F718 File Offset: 0x0000D918
        protected virtual void GetPluginsInternal(ConfuserContext context, IList<Protection> protections, IList<Packer> packers, IList<ConfuserComponent> components)
        {
            try
            {
                Assembly protAsm = Assembly.Load("Confuser.Protections");
                PluginDiscovery.AddPlugins(context, protections, packers, components, protAsm);
            }
            catch (Exception ex)
            {
                context.Logger.ErrorException("Failed to load built-in protections.", ex);
            }
            try
            {
                Assembly renameAsm = Assembly.Load("Confuser.Renamer");
                PluginDiscovery.AddPlugins(context, protections, packers, components, renameAsm);
            }
            catch (Exception ex2)
            {
                context.Logger.ErrorException("Failed to load renamer.", ex2);
            }
            try
            {
                Assembly renameAsm2 = Assembly.Load("Confuser.DynCipher");
                PluginDiscovery.AddPlugins(context, protections, packers, components, renameAsm2);
            }
            catch (Exception ex3)
            {
                context.Logger.ErrorException("Failed to load dynamic cipher library.", ex3);
            }
            foreach (string pluginPath in context.Project.PluginPaths)
            {
                string realPath = Path.Combine(context.BaseDirectory, pluginPath);
                try
                {
                    Assembly plugin = Assembly.LoadFile(realPath);
                    PluginDiscovery.AddPlugins(context, protections, packers, components, plugin);
                }
                catch (Exception ex4)
                {
                    context.Logger.ErrorException("Failed to load plugin '" + pluginPath + "'.", ex4);
                }
            }
        }

        /// <summary>
        ///     The default plugin discovery service.
        /// </summary>
        // Token: 0x0400015F RID: 351
        internal static readonly PluginDiscovery Instance = new PluginDiscovery();
    }
}