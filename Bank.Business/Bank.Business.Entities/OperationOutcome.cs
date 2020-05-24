using System;

namespace Bank.Business.Entities
{
    public class OperationOutcome
    {
        public enum OperationOutcomeResult { Successful, Failure };

        public String Message { get; set; }
        public OperationOutcomeResult Outcome { get; set; }
    }
}
