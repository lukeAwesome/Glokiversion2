namespace GlobalKiosk.Integrations.QualityAssurance.Evaluations.Criterions
{
    using System;

    public class ResultCriteria
    {
        #region Constructors

        public ResultCriteria(Guid evaluationId, DateTime? ended, string endReason, bool hasPassed, string failureReason)
        {
            EvaluationId = evaluationId;
            Ended = ended;
            EndReason = endReason;
            HasPassed = hasPassed;
            FailureReason = failureReason;
        }

        #endregion Constructors

        #region Properties

        public Guid EvaluationId { get; set; }
        public DateTime? Ended { get; set; }
        public string EndReason { get; set; }
        public bool HasPassed { get; set; }
        public string FailureReason { get; set; }

        #endregion Properties
    }
}
