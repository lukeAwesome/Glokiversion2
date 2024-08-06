namespace GlobalKiosk.Integrations.QualityAssurance.Evaluations.Criterions
{
    using System;

    public class EvaluationCriteria
    {

        #region Constructors

        public EvaluationCriteria(string name, DateTime started, string startReason, Guid kioskId)
        {
            Name = name;
            Started = started;
            StartReason = startReason;
            KioskId = kioskId;
        }

        #endregion Constructors

        #region Properties

        public string Name { get; set; }
        public DateTime Started { get; set; }
        public string StartReason { get; set; }
        public Guid KioskId { get; set; }

        #endregion Properties
    }
}
