using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KendoGridCrudController
{
	public interface IAuditableEntity
	{
		Guid CreatedBy { get; set; }
		DateTime CreatedOn { get; set; }
		Guid ModifiedBy { get; set; }
		DateTime ModifiedOn { get; set; }
	}
}
