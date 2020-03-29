using System.Linq;
using Confuser.Core;
using dnlib.DotNet;

namespace Confuser.Protections.Constants
{
    internal class MutationPhase : ProtectionPhase
    {
        public MutationPhase(MutationProtection parent)
            : base(parent) { }

        public override ProtectionTargets Targets
        {
            get { return ProtectionTargets.Modules; }
        }

        public override string Name
        {
            get { return "Constants Mutation"; }
        }

        protected override void Execute(ConfuserContext context, ProtectionParameters parameters)
        {


            foreach (ModuleDef module in parameters.Targets.OfType<ModuleDef>())
            {
                new Arithmetic(module);
            }

        }






    }
}