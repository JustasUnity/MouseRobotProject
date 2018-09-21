using RobotRuntime.Execution;
using RobotRuntime;
using System;
using RobotRuntime.Tests;

namespace CustomNamespace
{
    [Serializable]
    [RunnerType(typeof(CustomCommandRunner))]
	//[PropertyDesignerType("CustomCommandDesigner")] // Optional
    public class CustomCommand : Command
    {
		// This is what will appear in dropdown in inspector under Command Type
        public override string Name { get { return "Custom Command"; } }
        public override bool CanBeNested { get { return true; } }

        public int SomeInt { get; set; } = 5;

		// having an empty constructor is a must, will not work otherwise
        public CustomCommand() { } 
        public CustomCommand(int SomeInt)
        {
            this.SomeInt = SomeInt;
        }

        public override void Run(TestData TestData)
        {
            // Something could be done here, if it's more complex, CustomCommandRunner can handle it
        }

        public override string ToString()
        {
			// This is what hierarchy will show
            return "Custom Command" + SomeInt;
        }
    }
}
