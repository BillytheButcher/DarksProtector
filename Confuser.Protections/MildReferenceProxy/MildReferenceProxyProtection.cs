namespace Confuser.Protections
{
    using Confuser.Core;
    using Confuser.Protections.MildReferenceProxy;
    using dnlib.DotNet;
    using ReferenceProxy;
    using System;
    using System.Runtime.InteropServices;

    [BeforeProtection(new string[] { "Ki.ControlFlow","Ki.RefProxy" }), AfterProtection(new string[] { "Ki.AntiDebug"/*, "Ki.AntiDump"*/ })]
    internal class MildReferenceProxyProtection : Protection, IMildReferenceProxyService
    {
        public const string _FullId = "Ki.MildRefProxy";
        public const string _Id = "Mid ref proxy";
        public const string _ServiceId = "Ki.MildRefProxy";

        public void ExcludeMethod(ConfuserContext context, MethodDef method)
        {
            ProtectionParameters.GetParameters(context, method).Remove(this);
        }

        protected override void Initialize(ConfuserContext context)
        {
            context.Registry.RegisterService("Ki.MildRefProxy", typeof(IMildReferenceProxyService), this);
        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {
            pipeline.InsertPreStage(PipelineStage.ProcessModule, new MildReferenceProxy.MildReferenceProxyPhase(this));
         
        }

        public override string Description =>
            "Encodes and hides references to type/method/fields with indirection method as proxy.";

        public override string FullId =>
            "Ki.MildRefProxy";

        public override string Id =>
            "Clean ref proxy";

        public override string Name =>
            "∂αякsρяσтεcтσя";

        public override ProtectionPreset Preset =>
            ProtectionPreset.Normal;
    }
}

