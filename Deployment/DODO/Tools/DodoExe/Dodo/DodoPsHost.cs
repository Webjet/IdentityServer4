using System;
using System.Globalization;
using System.Management.Automation.Host;

namespace Dodo
{
    internal class DodoPsHost : PSHost
    {
        private DodoApplication program;
        private CultureInfo originalCultureInfo =
           System.Threading.Thread.CurrentThread.CurrentCulture;
        private CultureInfo originalUICultureInfo =
            System.Threading.Thread.CurrentThread.CurrentUICulture;
        private Guid myId = Guid.NewGuid();

        public DodoPsHost(DodoApplication program)
        {
            this.program = program;
        }
        private DodoUserInterface myHostUserInterface = new DodoUserInterface();


        public override System.Globalization.CultureInfo CurrentCulture
        {
            get { return this.originalCultureInfo; }
        }

        public override System.Globalization.CultureInfo CurrentUICulture
        {
            get { return this.originalUICultureInfo; }
        }

        public override Guid InstanceId
        {
            get { return this.myId; }
        }

        public override string Name
        {
            get { return "DodoConsoleHostImplementation"; }
        }

        public override PSHostUserInterface UI
        {
            get { return this.myHostUserInterface; }
        }

        public override Version Version
        {
            get { return new Version(1, 0, 0, 0); }
        }

        public override void EnterNestedPrompt()
        {
            throw new NotImplementedException(
                "The method or operation is not implemented.");
        }

        public override void ExitNestedPrompt()
        {
            throw new NotImplementedException(
                "The method or operation is not implemented.");
        }

        public override void NotifyBeginApplication()
        {
            return;
        }

        public override void NotifyEndApplication()
        {
            return;
        }

        public override void SetShouldExit(int exitCode)
        {
            this.program.ShouldExit = true;
            this.program.ExitCode = exitCode;
        }
    }
}
