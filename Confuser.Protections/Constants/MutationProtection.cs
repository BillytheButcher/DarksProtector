using System;
using System.Linq;
using Confuser.Core;
using Confuser.Protections.Constants;
using dnlib.DotNet;

namespace Confuser.Protections
{

    [AfterProtection("Ki.Constants", "Ki.ControlFlow")]
    internal class MutationProtection : Protection
    {
        public const string _Id = "Mutations";
        public const string _FullId = "Ki.Mutations";

        public override string Name
        {
            get { return "Mutation Protection"; }
        }

        public override string Description
        {
            get { return "This protection marks the module with a attribute that discourage ILDasm from disassembling it."; }
        }

        public override string Id
        {
            get { return _Id; }
        }

        public override string FullId
        {
            get { return _FullId; }
        }

        public override ProtectionPreset Preset
        {
            get { return ProtectionPreset.Minimum; }
        }

        protected override void Initialize(ConfuserContext context)
        {
            //
        }

        protected override void PopulatePipeline(ProtectionPipeline pipeline)
        {

            pipeline.InsertPostStage(PipelineStage.ProcessModule, new MutationPhase(this));
            pipeline.InsertPostStage(PipelineStage.ProcessModule, new TestPhase(this));



        }


    }
}