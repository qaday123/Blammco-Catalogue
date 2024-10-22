using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TF2Items
{
    public class TF2PlayerExtension : MonoBehaviour
    {
        public delegate void ModifyMaximumRollDepthDelegate(ref int maxDepth);
        public delegate void ModifyCanAttackProperty(ref bool canAttack);

        public ModifyMaximumRollDepthDelegate ModifyMaxRollDepth;
        public ModifyCanAttackProperty ModifyCanAttack;
    }
}
