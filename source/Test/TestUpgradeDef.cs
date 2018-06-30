using BattleTech.UI;
namespace CustomComponents
{
    [Custom("TestUpgrade")]
    public class TestUpgradeDef : CustomUpgradeDef<TestUpgradeDef>, IColorComponent
    {
        public int SomeValue = 0;
        public UIColor Color { get; set; }

        public TestUpgradeDef()
        {
        }
    }

}
