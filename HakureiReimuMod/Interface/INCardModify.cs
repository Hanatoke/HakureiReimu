using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace HakureiReimu.HakureiReimuMod.Interface
{
    public interface INCardModify
    {
        bool AllowNodePool => false;
        void OnReload(NCard card);
    }
}