using System;
using System.Collections.Generic;
using System.Text;

namespace GlobalKiosk.Integrations.QualityAssurance.Evaluations.Criterions
{
	public class TransmitKioskMessageCriteria
	{
		#region Properties

		public Guid KioskId { get; set; }
		public string MessageTypeName { get; set; }
		public Guid? ComponentTypeId { get; set; }
		public Guid? ComponentRuleViolationId { get; set; }
		public int? MessageMaintenanceModeErrorCode { get; set; }
		public string UserName { get; set; }
		public string StageChangeReason { get; set; }
		public Guid? StageId { get; set; }
		public bool InitiatedByIoT { get; set; }

		#endregion Properties

		#region Methods

		public static TransmitKioskMessageCriteria New(Guid kioskId, string messageTypeName,
			Guid? componentTypeId, Guid? componentRuleViolationId, string userName, string stageChangeReason,
			Guid? stageId, bool initiatedByIoT,
			int? maintenanceModeErrorCode = null)
		{
			return new TransmitKioskMessageCriteria
			{
				KioskId = kioskId,
				MessageTypeName = messageTypeName,
				ComponentTypeId = componentTypeId,
				ComponentRuleViolationId = componentRuleViolationId,
				UserName = userName,
				MessageMaintenanceModeErrorCode = maintenanceModeErrorCode,
				StageChangeReason = stageChangeReason,
				StageId = stageId,
				InitiatedByIoT = initiatedByIoT
			};
		}

		#endregion Methods
	}
}
